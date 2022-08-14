using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Carotaa.Code
{
    public static class DOTweenBezier
    {
        public static Tweener DOAnchorPosBezier(this RectTransform rect, CubicBezierCurve xCurve, CubicBezierCurve yCurve, float duration)
        {
            var bridgeX = new DynamicPropertyBridge(() => rect.anchoredPosition.x, rect.SetAnchorPosX,
                xCurve.Evaluate);
            var bridgeY = new DynamicPropertyBridge(() => rect.anchoredPosition.y, rect.SetAnchorPosY,
                yCurve.Evaluate);
            var list = new List<IPropertyBridge>() {bridgeX, bridgeY};

            var tweener = DOPropertyBridges(list, duration);

            return tweener;
        }

        public static Tweener DOPropertyBridges(IEnumerable<IPropertyBridge> bridges, float duration)
        {
            var progress = 0f;
            var tweener = DOTween.To(() => progress, x =>
            {
                progress = x;
                foreach (var bridge in bridges)
                {
                    bridge.Value = bridge.Curve(progress);
                }
            }, 1f, duration);

            return tweener;
        }

        public class DynamicPropertyBridge : IPropertyBridge
        {
            private readonly Func<float> _getter;
            private readonly Action<float> _setter;

            public DynamicPropertyBridge(Func<float> getter, Action<float> setter, Func<float, float> curve)
            {
                _getter = getter;
                _setter = setter;
                Curve = curve;
            }

            public Func<float, float> Curve { get; }

            public float Value
            {
                get => _getter();
                set => _setter(value);
            }
        }

        private static void SetAnchorPosX(this RectTransform rect, float value)
        {
            var anchorPos = rect.anchoredPosition;
            anchorPos.x = value;
            rect.anchoredPosition = anchorPos;
        }

        private static void SetAnchorPosY(this RectTransform rect, float value)
        {
            var anchorPos = rect.anchoredPosition;
            anchorPos.y = value;
            rect.anchoredPosition = anchorPos;
        }

        // Static use please
        public static CubicBezierEasing BuildEasing(float x1, float y1, float x2, float y2)
        {
            return new CubicBezierEasing(x1, y1, x2, y2);
        }
        public static CubicBezierEasing BuildEasing(Vector2 p1, Vector2 p2)
        {
            return new CubicBezierEasing(p1.x, p1.y, p2.x, p2.y);
        }
        public static CubicBezierEasing BuildEasing(Vector4 p)
        {
            return new CubicBezierEasing(p.x, p.y, p.z, p.w);
        }
    }
}