using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Carotaa.Code
{
	// Do not invoke or save any animation event.
	[CreateAssetMenu(fileName = "newDoTweenClip", menuName = "DoTweenClip", order = 400)]
	public class DoTweenClip : ScriptableObject
	{
		[SerializeField, HideInInspector, FormerlySerializedAs("m_ClipGuid")] 
		private string m_ClipGuid;

		[SerializeField, HideInInspector, FormerlySerializedAs("m_Duration")] 
		private float m_Duration;

		[SerializeField, HideInInspector, FormerlySerializedAs("m_FrameRate")]
		private float m_FrameRate;

		[SerializeField, HideInInspector, FormerlySerializedAs("m_Curves")]
		private DoTweenClipCurve[] m_Curves;

		public float Duration { get => m_Duration; set => m_Duration = value; }

		public float FrameRate { get => m_FrameRate; set => m_FrameRate = value; }

		public string ClipGuid { get => m_ClipGuid; set => m_ClipGuid = value; }

		public DoTweenClipCurve[] Curves { get => m_Curves; set => m_Curves = value; }
	}

	[Serializable]
	public class DoTweenClipCurve : ISerializationCallbackReceiver
	{
		[SerializeField, FormerlySerializedAs("m_Name")] private string m_Name;
		[SerializeField, FormerlySerializedAs("m_Path")] private string m_Path;
		[SerializeField, FormerlySerializedAs("m_TypeName")] private string m_TypeAssemblyQualifiedName;
		[SerializeField, FormerlySerializedAs("m_PropertyName")] private string m_PropertyName;
		[SerializeField, FormerlySerializedAs("m_Curve")] private AnimationCurve m_Curve;
		
		private Type _type;
		
		public void OnBeforeSerialize()
		{
			// do nothing
		}
		
		public void OnAfterDeserialize()
		{
			_type = DeserializeType(m_TypeAssemblyQualifiedName);
		}

		public string Path
		{
			get => m_Path;
			set => m_Path = value;
		}

		// human readable name
		public string Name
		{
			get => m_Name;
			set => m_Name = value;
		}

		public string PropertyName
		{
			get => m_PropertyName;
			set => m_PropertyName = value;
		}

		public Type TargetType
		{
			get => _type;
			set
			{
				_type = value;
				m_TypeAssemblyQualifiedName = _type.AssemblyQualifiedName;
			}
		}

		public AnimationCurve Curve
		{
			get => m_Curve;
			set => m_Curve = value;
		}

		public Object FindRefTarget(Transform root)
		{
			if (TargetType == null)
				return null;

			Transform trans;
			if (string.IsNullOrEmpty(Path))
			{
				trans = root;
			}
			else
			{
				trans = root.Find(Path);
			}
			if (!trans) return null;
			Object target;
			if (TargetType == typeof(GameObject))
			{
				target = trans.gameObject;
			} else
			{
				target = trans.GetComponent(TargetType);
			}

			return target;
		}

		private static Type DeserializeType(string name)
		{
			return string.IsNullOrEmpty(name) ? null : Type.GetType(name);
		}
	}
}