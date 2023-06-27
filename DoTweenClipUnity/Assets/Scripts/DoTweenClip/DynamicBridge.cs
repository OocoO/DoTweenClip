using System;
using UnityEngine;

namespace Carotaa.Code
{
    public class DynamicBridge : IPropertyBridge
    {
        private Func<float> _getter;
        private Action<float> _setter;
        private Func<float, float> _curve;
        
        
        public DynamicBridge(Func<float> getter, Action<float> setter, Func<float, float> curve)
        {
            _getter = getter;
            _setter = setter;
            _curve = curve;
        }

        public float Evaluate(float time)
        {
            return _curve.Invoke(time);
        }

        public float Value
        {
            get => _getter.Invoke();

            set => _setter.Invoke(value);
        }
    }

    // play a animation in native way
    public class NativeAnimation
    {
        private const string AnimationName = "DoTweenClipAnimation";
        
        private Transform _root;
        private Animation _animation;
        private AnimationClip _clip;
        private bool _init;

        public NativeAnimation(Transform root)
        {
            _root = root;
        }
        
        public void AddCurve(DoTweenClipCurve curve)
        {
            if (!_init)
            {
                var animation = _root.GetComponent<Animation>();
                if (!animation)
                {
                    animation = _root.gameObject.AddComponent<Animation>();
                }

                _animation = animation;
            
                var clip = new AnimationClip();
                clip.legacy = true;

                _clip = clip;
                _init = true;
            }
            _clip.SetCurve(curve.Path, curve.TargetType, curve.PropertyName, curve.Curve);
        }

        public void SetLoop(bool isLoop)
        {
            if (!_init) return;
            
            _clip.wrapMode = isLoop ? WrapMode.Loop : WrapMode.Once;
        }

        public void Play()
        {
            if (!_init) return;
            
            _animation.AddClip(_clip, AnimationName);
            _animation.Play(AnimationName);
        }

        public void Stop()
        {
            if (!_init) return;
            
            _animation.Stop(AnimationName);
        }
    }
}