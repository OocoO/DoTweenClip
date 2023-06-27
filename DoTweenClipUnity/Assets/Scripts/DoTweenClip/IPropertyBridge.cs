using System;

namespace Carotaa.Code
{
    public interface IPropertyBridge
    {
        float Evaluate(float time);
        float Value { get; set; }
    }
}