using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Carotaa.Code
{
	public static class DoTweenModuleAnimationClip
	{
		// Similar with Animation.Play()
		public static Tweener DoAnimationClipAbsolute (this Transform root, DoTweenClip clip)
		{
			var bridges = clip.GetPropertyBridges(root);

			return Internal_DoAnimationClip(bridges, clip.Duration);
		}
		
		/// <summary>
		/// Create a Relative Position Animation Tweener
		/// </summary>
		/// <param name="root"></param>
		/// <param name="clip"></param>
		/// <param name="refTime">Is used to define how "Relative works"</param>
		/// <returns></returns>
		public static Tweener DoAnimationClipRelative (this Transform root, DoTweenClip clip, float refTime = 0f)
		{
			var bridges = clip.GetPropertyBridges(root);
			foreach (var bridge in bridges)
			{
				if (bridge is AnchoredPosition ||
				    bridge is LocalPosition)
				{
					// offset
					bridge.OffSet = bridge.Value - bridge.Curve.Evaluate(refTime);
				}
			}
			
			return Internal_DoAnimationClip(bridges, clip.Duration);
		}
		
		public static List<PropertyBridge> GetPropertyBridges(this DoTweenClip clip,  Transform root)
		{
			var list = new List<PropertyBridge>();
			foreach (var curve in clip.Curves)
			{
				var success = PropertyBridge.TryGetPropertyBridge(root, curve, out var bridge);
				if (success)
				{
					list.Add(bridge);
				}
			}
			return list;
		}

		private static Tweener Internal_DoAnimationClip (List<PropertyBridge> bridges, float time)
		{
			var progress = 0f;
			var tweener = DOTween.To(() => progress, x =>
			{
				progress = x;
				foreach (var bridge in bridges)
				{
					bridge.Value = bridge.Curve.Evaluate(x) + bridge.OffSet;
				}
			}, time, time).SetEase(Ease.Linear);

			return tweener;
		}

		// current support limited curve only.
		public abstract class PropertyBridge
		{
			private DoTweenClipCurve _curve;

			public abstract float Value { get; set; }
			
			public AnimationCurve Curve => _curve.Curve;
			public string PropertyName => _curve.PropertyName;
			public string PropertyPath => _curve.Path;
			public Type TargetType => _curve.TargetType;

			// some helper memory buffer
			public float OffSet;
			
			// maybe: extension use
			// public float[] FloatBuffer;
			// public object Buffer;

			protected virtual void Init (DoTweenClipCurve curve, object o)
			{
				_curve = curve;
			}

			protected virtual void LateInit()
			{
				//  Value is accessible
			}

			// return if this bridge is ready to work
			protected virtual bool IsLegal()
			{
				return _curve != null;
			}

			public static bool TryGetPropertyBridge (Transform root, DoTweenClipCurve curve, out PropertyBridge setter)
			{
				// reference check
				setter = null;
				var target = curve.FindRefTarget(root);

				if (!target) return false;
				
				// maybe: all field have as setter property
				switch (curve.PropertyName)
				{
					case "m_AnchoredPosition.x":
						setter = new AnchoredPosition(0);
						break;
					case "m_AnchoredPosition.y":
						setter = new AnchoredPosition(1);
						break;
					case "m_LocalScale.x":
						setter = new LocalScale(0);
						break;
					case "m_LocalScale.y":
						setter = new LocalScale(1);
						break;
					case "m_LocalScale.z":
						setter = new LocalScale(2);
						break;
					case "localEulerAnglesRaw.x":
						setter = new LocalEulerAngle(0);
						break;
					case "localEulerAnglesRaw.y":
						setter = new LocalEulerAngle(1);
						break;
					case "localEulerAnglesRaw.z":
						setter = new LocalEulerAngle(2);
						break;
					case "m_Color.r":
						setter = new GraphicColor(0);
						break;
					case "m_Color.g":
						setter = new GraphicColor(1);
						break;
					case "m_Color.b":
						setter = new GraphicColor(2);
						break;
					case "m_Color.a":
						setter = new GraphicColor(3);
						break;
					case "m_Alpha":
						setter = new CanvasGroupAlpha();
						break;
					case "m_SizeDelta.x":
						setter = new SizeDelta(0);
						break;
					case "m_SizeDelta.y":
						setter = new SizeDelta(1);
						break;
					case "m_AnchorMin.x":
						setter = new AnchorMin(0);
						break;
					case "m_AnchorMin.y":
						setter = new AnchorMin(1);
						break;
					case "m_AnchorMax.x":
						setter = new AnchorMax(0);
						break;
					case "m_AnchorMax.y":
						setter = new AnchorMax(1);
						break;
					case "m_Pivot.x":
						setter = new Pivot(0);
						break;
					case "m_Pivot.y":
						setter = new Pivot(1);
						break;
					case "m_LocalPosition.x":
						setter = new LocalPosition(0);
						break;
					case "m_LocalPosition.y":
						setter = new LocalPosition(1);
						break;
					case "m_LocalPosition.z":
						setter = new LocalPosition(2);
						break;
					case "m_IsActive":
						setter = new Active();
						break;
					case "m_Enabled":
						setter = new Enable();
						break;
					default:
						setter = new ReflectionBridge(curve.PropertyName);
						break;
				}

				setter.Init(curve, target);
				
				if (!setter.IsLegal()) return false;

				setter.LateInit();
				
				return true;

			}
		}

		public abstract class PropertyBridge<T> : PropertyBridge where T :Object
		{
			protected readonly int Index;

			protected PropertyBridge (int index)
			{
				Index = index;
			}

			public T Target { get; private set; }

			protected override void Init (DoTweenClipCurve curve, object o)
			{
				base.Init(curve, o);
				Target = o as T;
			}

			protected override bool IsLegal()
			{
				return base.IsLegal() && Target != null;
			}
		}

		public class AnchoredPosition : PropertyBridge<RectTransform>
		{
			public AnchoredPosition (int index) : base(index)
			{
			}

			public override float Value
			{
				get => Target.anchoredPosition[Index];
				set
				{
					var pos = Target.anchoredPosition;
					pos[Index] = value;
					Target.anchoredPosition = pos;
				}
			}
		}

		public class SizeDelta : PropertyBridge<RectTransform>
		{
			public SizeDelta(int index) : base(index)
			{
			}

			public override float Value
			{
				get => Target.sizeDelta[Index];
				set
				{
					var size = Target.sizeDelta;
					size[Index] = value;
					Target.sizeDelta = size;
				}
			}
		}

		public class AnchorMin : PropertyBridge<RectTransform>
		{
			public AnchorMin(int index) : base(index)
			{
			}

			public override float Value
			{
				get => Target.anchorMin[Index];
				set
				{
					var anchorMin = Target.anchorMin;
					anchorMin[Index] = value;
					Target.anchorMin = anchorMin;
				}
			}
		}
		
		public class AnchorMax : PropertyBridge<RectTransform>
		{
			public AnchorMax(int index) : base(index)
			{
			}

			public override float Value
			{
				get => Target.anchorMax[Index];
				set
				{
					var anchorMin = Target.anchorMax;
					anchorMin[Index] = value;
					Target.anchorMax = anchorMin;
				}
			}
		}

		public class Pivot : PropertyBridge<RectTransform>
		{

			public Pivot(int index) : base(index)
			{
			}

			public override float Value
			{
				get => Target.pivot[Index];
				set
				{
					var pivot = Target.pivot;
					pivot[Index] = value;
					Target.pivot = pivot;
				}
			}
		}

		public class LocalPosition : PropertyBridge<Transform>
		{
			public LocalPosition(int index) : base(index)
			{
			}
			
			public override float Value
			{
				get => Target.localPosition[Index];
				set
				{
					var pos = Target.localPosition;
					pos[Index] = value;
					Target.localPosition = pos;
				}
			}
		}

		public class LocalScale : PropertyBridge<Transform>
		{
			public LocalScale(int index) : base(index)
			{
			}
			
			public override float Value
			{
				get => Target.localScale[Index];
				set
				{
					var scale = Target.localScale;
					scale[Index] = value;
					Target.localScale = scale;
				}
			}
		}

		public class LocalEulerAngle : PropertyBridge<Transform>
		{
			private float _angle;
			public LocalEulerAngle(int index) : base(index)
			{
			}
			
			public override float Value
			{
				get => Target.rotation.eulerAngles[Index];
				set
				{
					// Use to fix quaternion to eulerAngle problem
					var dAngle = Vector3.zero;
					dAngle[Index] = value - _angle;
					_angle = value;
					var dRotate = Quaternion.Euler(dAngle);
					Target.rotation = dRotate * Target.rotation;
				}
			}

			protected override void LateInit()
			{
				base.LateInit();

				_angle = Value;
			}
		}

		public class GraphicColor : PropertyBridge<Graphic>
		{
			public GraphicColor(int index) : base(index)
			{
			}

			public override float Value
			{
				get => Target.color[Index];
				set
				{
					var color = Target.color;
					color[Index] = value;
					Target.color = color;
				}
			}
		}
		
		public class CanvasGroupAlpha : PropertyBridge<CanvasGroup>
		{
			public CanvasGroupAlpha() : base(0)
			{
			}

			public override float Value
			{
				get => Target.alpha;
				set => Target.alpha = value;
			}
		}


		public class Active : PropertyBridge<GameObject>
		{
			public Active() : base(0)
			{
			}

			public override float Value
			{
				get => Target.activeSelf ? 1f : 0f;
				set => Target.SetActive(!Mathf.Approximately(value, 0f));
			}
		}

		public class Enable : PropertyBridge<Behaviour>
		{
			public Enable() : base(0)
			{
			}

			public override float Value
			{
				get => Target.enabled ? 1f : 0f;
				set => Target.enabled = (!Mathf.Approximately(value, 0f));
			}
		}

		// fieldName -> propertyName;
		// remove preFix "m_";
		// The fallback solution
		public class ReflectionBridge : PropertyBridge<Component>
		{
			private readonly string _filedName;

			private FieldInfo _fieldInfo;
			private PropertyInfo _propertyInfo;
			
			public ReflectionBridge(string fieldName) : base(0)
			{
				_filedName = fieldName;
			}

			protected override void Init (DoTweenClipCurve curve, object o)
			{
				base.Init(curve, o);

				try
				{
					var type = o.GetType();
					_propertyInfo = FindProperty(type, _filedName);

					if (_propertyInfo != null) return;
					_fieldInfo = type.GetField(_filedName, 
						BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				}
				catch (Exception e)
				{
					Debug.LogError($"Init Failed With Exception {e}");
				}
			}

			public override float Value
			{
				get
				{
					if (_propertyInfo != null)
					{
						return (float) _propertyInfo.GetValue(Target);
					}

					if (_fieldInfo != null)
					{
						return (float) _fieldInfo.GetValue(Target);
					}

					throw new Exception($"Unable to get Value for {_filedName}");
				}

				set
				{
					if (_propertyInfo != null)
					{
						_propertyInfo.SetValue(Target, value);
						return;
					}

					if (_fieldInfo != null)
					{
						_fieldInfo.SetValue(Target, value);
						return;
					}

					throw new Exception($"Unable to set Value of {_filedName}");
				}
			}

			protected override bool IsLegal()
			{
				var baseLegal = base.IsLegal();

				if (!baseLegal) return false;

				// check the 'connection' of bridge.
				var success = false;
				try
				{
					var value = Value;
					Value = value;

					success = true;
				}
				catch(Exception e)
				{
					Debug.LogError($"Bridge Legal Check Failed with exception {e}");
				}

				return success;
			}

			private static PropertyInfo FindProperty (Type type, string filedName)
			{
				if (filedName.StartsWith("m_"))
				{
					filedName = filedName.Substring(2);
				}

				var info = type.GetProperty(filedName);
				if (info != null) return info;
				
				// try upper case
				var start = filedName[0];
				var end = filedName.Substring(1);
				info = type.GetProperty(char.ToUpper(start) + end, 
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (info != null) return info;
				
				// try lower case property
				info = type.GetProperty(char.ToLower(start) + end, 
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

				return info;
			}
		}

		public class PropertyName : IEquatable<PropertyName>
		{
			public readonly string Name;
			public PropertyName(string name)
			{
				Name = name;
			}
			
			public static readonly PropertyName AnchoredPositionX = new PropertyName("m_AnchoredPosition.x");
			public static readonly PropertyName AnchoredPositionY = new PropertyName("m_AnchoredPosition.y");
			public static readonly PropertyName LocalScaleX = new PropertyName("m_LocalScale.x");
			public static readonly PropertyName LocalScaleY = new PropertyName("m_LocalScale.y");
			public static readonly PropertyName LocalScaleZ = new PropertyName("m_LocalScale.z");
			public static readonly PropertyName LocalRotateX = new PropertyName("localEulerAnglesRaw.x");
			public static readonly PropertyName LocalRotateY = new PropertyName("localEulerAnglesRaw.y");
			public static readonly PropertyName LocalRotateZ = new PropertyName("localEulerAnglesRaw.z");
			public static readonly PropertyName ColorR = new PropertyName("m_Color.r");
			public static readonly PropertyName ColorG = new PropertyName("m_Color.g");
			public static readonly PropertyName ColorB = new PropertyName("m_Color.b");
			public static readonly PropertyName ColorA = new PropertyName("m_Color.a");
			public static readonly PropertyName Alpha = new PropertyName("m_Alpha");
			public static readonly PropertyName SizeDeltaX = new PropertyName("m_SizeDelta.x");
			public static readonly PropertyName SizeDeltaY = new PropertyName("m_SizeDelta.y");
			public static readonly PropertyName AnchorMinX = new PropertyName("m_AnchorMin.x");
			public static readonly PropertyName AnchorMinY = new PropertyName("m_AnchorMin.y");
			public static readonly PropertyName AnchorMaxX = new PropertyName("m_AnchorMax.x");
			public static readonly PropertyName AnchorMaxY = new PropertyName("m_AnchorMax.y");
			public static readonly PropertyName PivotX = new PropertyName("m_Pivot.x");
			public static readonly PropertyName PivotY = new PropertyName("m_Pivot.y");
			public static readonly PropertyName LocalPositionX = new PropertyName("m_LocalPosition.x");
			public static readonly PropertyName LocalPositionY = new PropertyName("m_LocalPosition.y");
			public static readonly PropertyName LocalPositionZ = new PropertyName("m_LocalPosition.z");
			public static readonly PropertyName GameObjectActive = new PropertyName("m_IsActive");
			public static readonly PropertyName ComponentEnable = new PropertyName("m_Enabled");
			public static readonly PropertyName Other = new PropertyName(string.Empty);

			private static PropertyName[] _cache;
			public static PropertyName[] GetAll()
			{
				if (_cache != null) return _cache;

				var type = typeof(PropertyName);
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
				var list = new List<PropertyName>();
				foreach (var field in fields)
				{
					var value = field.GetValue(null);
					if (value is PropertyName pn)
					{
						list.Add(pn);
					}
				}
				
				_cache = list.ToArray();
				return _cache;
			}

			public static PropertyName Find(string name)
			{
				var propertyNames = GetAll();
				foreach (var propertyName in propertyNames)
				{
					if (propertyName.Name == name)
					{
						return propertyName;
					}
				}

				return null;
			}

#if UNITY_EDITOR
			private string _fieldName;
			public override string ToString()
			{
				
				if (_fieldName != null) return _fieldName;
				var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Static);
				
				foreach (var field in fields)
				{
					var value = field.GetValue(null);
					if (value.Equals(this))
					{
						_fieldName = field.Name;
						return _fieldName;
					}
				}

				_fieldName = base.ToString();
				
				return _fieldName;
			}
#endif

			public bool Equals(PropertyName other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return Name == other.Name;
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;
				return Equals((PropertyName) obj);
			}

			public override int GetHashCode()
			{
				return Name != null ? Name.GetHashCode() : 0;
			}
		}
	}
}