using System;

namespace Carotaa.Code
{
    public interface IPropertyBridge
    {
        Func<float, float> Curve { get; }
        float Value { get; set; }
    }
}