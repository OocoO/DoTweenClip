using System.Collections.Generic;
using UnityEngine;

namespace Carotaa.Code
{
    public class NativeBridge : MonoBehaviour, IPropertyBridge
    {
        private GameObject _root;
        [SerializeField] private AnimationClip _clip;
        private float _playTime;

        public static NativeBridge Create(Transform root)
        {
            var bridge = root.GetComponent<NativeBridge>();
            if (!bridge)
            {
                bridge = root.gameObject.AddComponent<NativeBridge>();
            }

            bridge._root = root.gameObject;
            bridge.BindClip();

            return bridge;
        }

        private void BindClip()
        {
            Clean();
            
            _clip = new AnimationClip();
            _clip.name = "NativeBridge Clip";
            _clip.legacy = true;
        }

        public void AddCurve(DoTweenClipCurve curve)
        {
            _clip.SetCurve(curve.Path, curve.TargetType, curve.PropertyName, curve.Curve);
        }

        public float Evaluate(float time)
        {
            return time;
        }

        public float Value
        {
            get => _playTime;
            set
            {
                _playTime = value;

                if (_clip.empty) return;
                
                _clip.SampleAnimation(_root, _playTime);
            }
        }

        public void OnDestroy()
        {
            Clean();
        }

        private void Clean()
        {
            if (_clip)
            {
                Object.DestroyImmediate(_clip);
            }
        }
    }
}