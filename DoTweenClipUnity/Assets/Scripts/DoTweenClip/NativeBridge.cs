using System.Collections.Generic;
using UnityEngine;

namespace Carotaa.Code
{
    public class NativeBridge : MonoBehaviour, IPropertyBridge
    {
        private GameObject _root;
        private AnimationClip _clip;
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
            if (_clip)
            {
                Object.Destroy(_clip);
            }
            
            _clip = new AnimationClip();
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
                _clip.SampleAnimation(_root, _playTime);
            }
        }

        public void OnDestroy()
        {
            if (_clip)
            {
                Object.Destroy(_clip);
            }
        }
    }
}