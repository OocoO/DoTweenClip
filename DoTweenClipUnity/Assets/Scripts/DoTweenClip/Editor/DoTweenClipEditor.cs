using System.Collections.Generic;
using System.Linq;
using Carotaa.Code;
using UnityEditor;
using UnityEngine;

namespace Carotaa.Code.Editor
{
	[CustomEditor(typeof(DoTweenClip))]
	public class DoTweenClipEditor : UnityEditor.Editor
	{
		private AnimationClip _clip;
		private DoTweenClip ClipTarget => target as DoTweenClip;

		private SerializedProperty _duration;
		private SerializedProperty _frameRate;
		private SerializedProperty _clipGuid;
		private SerializedProperty _curves;

		private void OnEnable()
		{
			var path = AssetDatabase.GUIDToAssetPath(ClipTarget.ClipGuid);
			_clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

			_duration = serializedObject.FindProperty("m_Duration");
			_frameRate = serializedObject.FindProperty("m_FrameRate");
			_clipGuid = serializedObject.FindProperty("m_ClipGuid");
			_curves = serializedObject.FindProperty("m_Curves");
		}


		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			
			EditorGUI.BeginChangeCheck();
			var clip = EditorGUILayout.ObjectField("Clip", _clip, typeof(AnimationClip), false) as AnimationClip;
			EditorGUILayout.LabelField("Curves:");
			
			if (EditorGUI.EndChangeCheck() && clip)
			{
				_clip = clip;
			}

			if (clip && GUILayout.Button("Read Form Animation"))
			{
				ReadFrom(clip);
			}

			if (clip && GUILayout.Button("Write Back Animation"))
			{
				WriteBack(clip);
			}

			EditorGUILayout.LabelField($"Duration {ClipTarget.Duration}");
			EditorGUILayout.LabelField($"Frame Rate: {ClipTarget.FrameRate}");
			EditorGUILayout.LabelField($"GUID: {ClipTarget.ClipGuid}");
			EditorGUILayout.LabelField($"Curve Count {ClipTarget.Curves.Length}");

			EditorGUI.indentLevel++;
			foreach(var curve in ClipTarget.Curves)
			{
				EditorGUILayout.LabelField($"{curve.Path}/{curve.TargetType}/{curve.PropertyName}, Keys {curve.Curve.keys.Length}");
			}
			EditorGUI.indentLevel--;
		}

		private void ReadFrom(AnimationClip clip)
		{
			if (!clip) return;

			AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out var guid, out long localId);
			var list = ListPool<DoTweenClipCurve>.Get();
				
			_duration.floatValue = clip.length;
			_frameRate.floatValue = clip.frameRate;
			_clipGuid.stringValue = guid;

			foreach (var binding in AnimationUtility.GetCurveBindings(clip))
			{
				AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
				var clipCurve = new DoTweenClipCurve() {
					Name = GenCurveName(binding.path, binding.propertyName),
					Curve = curve,
					Path = binding.path,
					PropertyName = binding.propertyName,
					TargetType = binding.type,
				};
				
				if (IsCurveEmpty(curve, 1f / clip.frameRate)) continue;

				list.Add(clipCurve);
			}

			var array = list.ToArray();
			_curves.arraySize = array.Length;
			for (var i = 0; i < _curves.arraySize; i++)
			{
				var ele = _curves.GetArrayElementAtIndex(i);
				var curve = array[i];
				ele.FindPropertyRelative("m_Name").stringValue = curve.Name;
				ele.FindPropertyRelative("m_Path").stringValue = curve.Path;
				ele.FindPropertyRelative("m_TypeAssemblyQualifiedName").stringValue = curve.TargetType.AssemblyQualifiedName;
				ele.FindPropertyRelative("m_PropertyName").stringValue = curve.PropertyName;
				ele.FindPropertyRelative("m_Curve").animationCurveValue = curve.Curve;
			}
			
			ListPool<DoTweenClipCurve>.Release(list);

			serializedObject.ApplyModifiedProperties();
		}

		private void WriteBack(AnimationClip clip)
		{
			if (!clip) return;
			
			foreach (var curve in ClipTarget.Curves)
			{
				var binding = EditorCurveBinding.FloatCurve(curve.Path, curve.TargetType, curve.PropertyName);
				AnimationUtility.SetEditorCurve(clip, binding, curve.Curve);
			}

			var path = AssetDatabase.GetAssetPath(clip);
			AssetDatabase.SaveAssets();
		}

		// Unity Store some empty curve in animation clips
		public static bool IsCurveEmpty(AnimationCurve curve, float deltaTime)
		{
			var keys = curve.keys;
			if (keys.Length <= 1) return false;

			var startTime = keys[0].time;
			var endTime = keys.Last().time;
			var valueLast = curve.Evaluate(startTime);

			for (var time = startTime + deltaTime; time <= endTime; time += deltaTime)
			{
				var value = curve.Evaluate(time);
				if (!Mathf.Approximately(valueLast, value)) return false;

				valueLast = value;
			}

			return true;
		}

		public static string GenCurveName(string path, string propertyName)
		{
			// simply path
			var name = path.Split('/');
			return $"Animation: {name.Last()}.{propertyName}";
		}

		public static DoTweenClip GenDoTweenClip(AnimationClip aClip)
		{
			var clip = CreateInstance<DoTweenClip>();
			var list = ListPool<DoTweenClipCurve>.Get();
				
			clip.Duration = aClip.length;
			clip.FrameRate = aClip.frameRate;

			foreach (var binding in AnimationUtility.GetCurveBindings(aClip))
			{
				AnimationCurve curve = AnimationUtility.GetEditorCurve(aClip, binding);
				var clipCurve = new DoTweenClipCurve() {
					Name = GenCurveName(binding.path, binding.propertyName),
					Curve = curve,
					Path = binding.path,
					PropertyName = binding.propertyName,
					TargetType = binding.type,
				};
				
				if (IsCurveEmpty(curve, 1f / aClip.frameRate)) continue;

				list.Add(clipCurve);
			}

			clip.Curves = list.ToArray();

			ListPool<DoTweenClipCurve>.Release(list);

			return clip;
		}
	}
}