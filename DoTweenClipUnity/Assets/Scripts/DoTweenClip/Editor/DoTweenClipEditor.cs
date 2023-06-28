using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Carotaa.Code.Editor
{
	[CustomEditor(typeof(DoTweenClip))]
	public class DoTweenClipEditor : UnityEditor.Editor
	{
		private UnityEngine.AnimationClip _clip;
		private DoTweenClip ClipTarget => target as DoTweenClip;

		private SerializedProperty _duration;
		private SerializedProperty _frameRate;
		private SerializedProperty _clipGuid;
		private SerializedProperty _curves;

		private void OnEnable()
		{
			var path = AssetDatabase.GUIDToAssetPath(ClipTarget.ClipGuid);
			_clip = AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);

			_duration = serializedObject.FindProperty("m_Duration");
			_frameRate = serializedObject.FindProperty("m_FrameRate");
			_clipGuid = serializedObject.FindProperty("m_ClipGuid");
			_curves = serializedObject.FindProperty("m_Curves");
		}


		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			var clip = EditorGUILayout.ObjectField("Clip", _clip, typeof(UnityEngine.AnimationClip), false) as UnityEngine.AnimationClip;
			EditorGUILayout.LabelField("Curves:");
			
			if (EditorGUI.EndChangeCheck() && clip)
			{
				Refresh(clip);
			}

			if (clip && GUILayout.Button("Re-Import All Form Animation"))
			{
				var path = AssetDatabase.GUIDToAssetPath(ClipTarget.ClipGuid);
				clip = AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
				
				Refresh(clip);
			}
			
			if (clip && GUILayout.Button("Re-Import Curve Form Animation"))
			{
				var path = AssetDatabase.GUIDToAssetPath(ClipTarget.ClipGuid);
				clip = AssetDatabase.LoadAssetAtPath<UnityEngine.AnimationClip>(path);
				
				RefreshCurve(clip);
			}
			
			GUI.enabled = false;
			EditorGUILayout.FloatField("Duration", ClipTarget.Duration);
			EditorGUILayout.FloatField("Frame Rate", ClipTarget.FrameRate);
			EditorGUILayout.TextField("GUID",ClipTarget.ClipGuid);
			GUI.enabled = true;
			
			base.OnInspectorGUI();
		}

		private void Refresh(UnityEngine.AnimationClip clip)
		{
			if (!clip) return;
			
			_clip = clip;
			
			AssetDatabase.TryGetGUIDAndLocalFileIdentifier(clip, out var guid, out long localId);
			var list = ListPool<DoTweenClipCurve>.Get();
				
			_duration.floatValue = clip.length;
			_frameRate.floatValue = clip.frameRate;
			_clipGuid.stringValue = guid;

			foreach (var binding in AnimationUtility.GetCurveBindings(clip))
			{
				AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
				var clipCurve = new DoTweenClipCurve() {
					Name = GenCurveName(binding),
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
				FullCopy(ele, curve);
			}
			
			ListPool<DoTweenClipCurve>.Release(list);

			serializedObject.ApplyModifiedProperties();
		}
		
		private void RefreshCurve(UnityEngine.AnimationClip clip)
		{
			if (!clip) return;

			try
			{
				var lut = new Dictionary<string, DoTweenClipCurve>();

				foreach (var binding in AnimationUtility.GetCurveBindings(clip))
				{
					AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
					var clipCurve = new DoTweenClipCurve() {
						Name = GenCurveName(binding),
						Curve = curve,
						Path = binding.path,
						PropertyName = binding.propertyName,
						TargetType = binding.type,
					};
				
					if (IsCurveEmpty(curve, 1f / clip.frameRate)) continue;

					lut.Add(clipCurve.Name, clipCurve);
				}
			
				for (var i = 0; i < _curves.arraySize; i++)
				{
					var ele = _curves.GetArrayElementAtIndex(i);
					var curveName = ele.FindPropertyRelative("m_Name").stringValue;
					if (lut.TryGetValue(curveName, out var clipCurve))
					{
						ele.FindPropertyRelative("m_Curve").animationCurveValue = clipCurve.Curve;
						lut.Remove(curveName);
					}
				}
				
				// new curves
				var newCurves = lut.Values.ToArray();
				var currentSize = _curves.arraySize;
				_curves.arraySize = currentSize + newCurves.Length;
				for (var i = 0; i < newCurves.Length; i++)
				{
					var ele = _curves.GetArrayElementAtIndex(currentSize + i);
					var curve = newCurves[i];
					FullCopy(ele, curve);
				}
				

				serializedObject.ApplyModifiedProperties();
			}
			catch (Exception e)
			{
				Debug.LogError($"Refresh Curve Failed with Exception {e}");
			}
		}

		private static void FullCopy(SerializedProperty ele, DoTweenClipCurve curve)
		{
			ele.FindPropertyRelative("m_Name").stringValue = curve.Name;
			ele.FindPropertyRelative("m_Path").stringValue = curve.Path;
			ele.FindPropertyRelative("m_TypeAssemblyQualifiedName").stringValue = curve.TargetType.AssemblyQualifiedName;
			ele.FindPropertyRelative("m_PropertyName").stringValue = curve.PropertyName;
			ele.FindPropertyRelative("m_Curve").animationCurveValue = curve.Curve;
		}

		// Unity Store some empty curve in animation clips
		public static bool IsCurveEmpty(AnimationCurve curve, float deltaTime)
		{
			var keys = curve.keys;
			if (keys.Length <= 1) return false;

			var startTime = keys[0].time;
			var endTime = keys.Last().time;
			var valueLast = curve.Evaluate(startTime);
			endTime += deltaTime;

			for (var time = startTime + deltaTime; time <= endTime; time += deltaTime)
			{
				var value = curve.Evaluate(time);
				if (!Mathf.Approximately(valueLast, value)) return false;

				valueLast = value;
			}

			return true;
		}
		
		public static string GenCurveName(EditorCurveBinding binding)
		{
			return $"{binding.path}.{binding.propertyName}";
		}

		public static DoTweenClip GenDoTweenClip(UnityEngine.AnimationClip aClip)
		{
			var clip = CreateInstance<DoTweenClip>();
			var list = ListPool<DoTweenClipCurve>.Get();
				
			clip.Duration = aClip.length;
			clip.FrameRate = aClip.frameRate;

			foreach (var binding in AnimationUtility.GetCurveBindings(aClip))
			{
				AnimationCurve curve = AnimationUtility.GetEditorCurve(aClip, binding);
				var clipCurve = new DoTweenClipCurve() {
					Name = GenCurveName(binding),
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