using System;

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
}