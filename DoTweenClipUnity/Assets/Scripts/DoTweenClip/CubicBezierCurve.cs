using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Carotaa.Code
{
    // see https://www.algosome.com/articles/continuous-bezier-curve-line.html for more detail
    [Serializable]
    public struct CubicBezierCurve
    {
        private const int KeyInsertCount = 15;
            
        [SerializeField] public float P0;
        [SerializeField] public float P1;
        [SerializeField] public float P2;
        [SerializeField] public float P3;

        public static CubicBezierCurve Build(Vector4 points)
        {
            return Build(points.x, points.y, points.z, points.w);
        }

        public static CubicBezierCurve Build(float p0, float p1, float p2, float p3)
        {
            var curve = new CubicBezierCurve {P0 = p0, P1 = p1, P2 = p2, P3 = p3};
            return curve;
        }
        
        private float CustomCurve(float time, float duration, float overshootOrAmplitude, float period)
        {
            return Evaluate(time / duration);
        }
        
        public float Evaluate(float t)
        {
            return A() * t * t * t + B() * t * t + C() * t + D();
        }
        
        /// <summary>
        /// Returns dx/dt
        /// </summary>
        public float GetSlope(float t)
        {
            return 3f * A() * t * t + 2f * B() * t + C();
        }

        public AnimationCurve Cast2AnimationCurve()
        {
            var tanOut = 3f * (P1 - P0);
            var tanIn = 3f * (P3 - P2);
            var key1 = new Keyframe(0f, P0, tanOut, tanOut);
            var key2 = new Keyframe(1f, P3, tanIn, tanIn);
            var curve = new AnimationCurve(key1, key2);

            return curve;
        }

        // lose quality
        public AnimationCurve Cast2AnimationCurve(Vector4 ease)
        {
            // legal check
            if (ease.x <= 0.001f || ease.z >= 0.999f)
            {
                // linear ease
                return Cast2AnimationCurve();
            }

            var curve = new AnimationCurve();
            for (var i = 0; i <= KeyInsertCount; i++)
            {
                // d[By(Reverse(Bx(t)))] / dx
                var t = 1f / KeyInsertCount * i;
                var time = (float) CubicBezierEasing.CalcBezier(t, ease.x, ease.z);
                var progress = (float) CubicBezierEasing.CalcBezier(t, ease.y, ease.w);
                var value = Evaluate(progress);

                var slope = GetSlope(progress);
                slope *= (float) CubicBezierEasing.GetSlope(t, ease.y, ease.w);
                slope /= (float) CubicBezierEasing.GetSlope(t, ease.x, ease.z);
                
                var keyframe = new Keyframe(time, value, slope, slope);
                curve.AddKey(keyframe);
            }

            return curve;
        }
        
        private float A()
        {
            return P3 - 3f * P2 + 3f * P1 - P0;
        }
        
        private float B()
        {
            return 3f * P2 - 6f * P1 + 3f * P0;
        }

        private float C()
        {
            return 3.0f * P1 - 3f * P0;
        }

        private float D()
        {
            return P0;
        }
    }
}