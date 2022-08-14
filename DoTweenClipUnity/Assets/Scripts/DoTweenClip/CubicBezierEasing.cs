using System;
using System.Collections.Generic;
using DG.Tweening;

namespace Carotaa.Code
{
    // https://github.com/gre/bezier-easing/blob/master/src/index.js
    public class CubicBezierEasing
    {
        private const int NewtonIterations = 5;
        private const double NewtonMinSlope = 0.001;
        private const double SubdivisionPrecision = 0.000001;
        private const double SubdivisionMaxIterations = 8;
        private const int KSplineTableSize = 15;
        private const double KSampleStepSize = 1.0 / (KSplineTableSize - 1.0);

        private readonly List<double> _sampleValues;
        private readonly double _mX1;
        private readonly double _mY1;
        private readonly double _mX2;
        private readonly double _mY2;

        public delegate double CustomEaseFunction(double x);
        private readonly CustomEaseFunction _customEaseFunction;

        public CubicBezierEasing()
        {
            _mX1 = 0.0f;
            _mY1 = 0.0f;
            _mX2 = 1.0f;
            _mY2 = 1.0f;
            _customEaseFunction = LinearEasing;
        }

        public CubicBezierEasing(double mX1, double mY1, double mX2, double mY2)
        {
            _mX1 = mX1;
            _mY1 = mY1;
            _mX2 = mX2;
            _mY2 = mY2;

            if (mX1 <= 0.001 || mX2 >= 0.999)
            {
                _customEaseFunction = LinearEasing;
            } 
            else if (Math.Abs(mX1 - mY1) < 0.01f && Math.Abs(mX2 - mY2) < 0.01f)
            {
                _customEaseFunction = LinearEasing;
            }
            else
            {
                _customEaseFunction = BezierEasing;
                
                // Precompute samples table
                _sampleValues = new List<double>(KSplineTableSize);
                for (var i = 0; i < KSplineTableSize; ++i) {
                    _sampleValues.Add(CalcBezier(i * KSampleStepSize, _mX1, _mX2));
                }
            }
        }

        private float CustomCurve(float time, float duration, float overshootOrAmplitude, float period)
        {
            return (float) GetEaseFunction()(time / duration);
        }

        public CustomEaseFunction GetEaseFunction()
        {
            return _customEaseFunction;
        }

        public EaseFunction CustomEase() {
            return CustomCurve;
        }

        private static double A(double aA1, double aA2)
        {
            return 1.0 - 3.0 * aA2 + 3.0 * aA1;
        }

        private static double B(double aA1, double aA2)
        {
            return 3.0 * aA2 - 6.0 * aA1;
        }

        private static double C(double aA1)
        {
            return 3.0 * aA1;
        }
        
        public static double CalcBezier(double aT, double aA1, double aA2)
        {
            return ((A(aA1, aA2) * aT + B(aA1, aA2)) * aT + C(aA1)) * aT;
        }

        /// <summary>
        /// Returns dx/dt given t, x1, and x2, or dy/dt given t, y1, and y2.
        /// </summary>
        /// <param name="aT"></param>
        /// <param name="aA1"></param>
        /// <param name="aA2"></param>
        /// <returns></returns>
        public static double GetSlope(double aT, double aA1, double aA2)
        {
            return 3.0 * A(aA1, aA2) * aT * aT + 2.0 * B(aA1, aA2) * aT + C(aA1);
        }

        private static double BinarySubdivide (double aX, double aA, double aB, double mX1, double mX2)
        {
            double currentX, currentT;
            int i = 0;
            do {
                currentT = aA + (aB - aA) / 2.0;
                currentX = CalcBezier(currentT, mX1, mX2) - aX;
                if (currentX > 0.0) {
                    aB = currentT;
                } else {
                    aA = currentT;
                }
            } while (Math.Abs(currentX) > SubdivisionPrecision && ++i < SubdivisionMaxIterations);
            return currentT;
        }

        // ReSharper disable once IdentifierTypo
        private static double NewtonRaphsonIterate (double aX, double aGuessT, double mX1, double mX2) {
            for (var i = 0; i < NewtonIterations; ++i) {
                double currentSlope = GetSlope(aGuessT, mX1, mX2);
                if (currentSlope < NewtonMinSlope) {
                    return aGuessT;
                }
                var currentX = CalcBezier(aGuessT, mX1, mX2) - aX;
                aGuessT -= currentX / currentSlope;
            }
            return aGuessT;
        }

        private static double LinearEasing(double x) {
            return x;
        }

        private double GetTForX (double aX) {
            var intervalStart = 0.0;
            var currentSample = 1;
            const int lastSample = KSplineTableSize - 1;

            for (; currentSample != lastSample && _sampleValues[currentSample] <= aX; ++currentSample) {
                intervalStart += KSampleStepSize;
            }
            --currentSample;

            // Interpolate to provide an initial guess for t
            var dist = (aX - _sampleValues[currentSample]) / (_sampleValues[currentSample + 1] - _sampleValues[currentSample]);
            var guessForT = intervalStart + dist * KSampleStepSize;

            var initialSlope = GetSlope(guessForT, _mX1, _mX2);
            if (initialSlope >= NewtonMinSlope) {
                return NewtonRaphsonIterate(aX, guessForT, _mX1, _mX2);
            } else if (Math.Abs(initialSlope) < NewtonMinSlope / 10) {
                return guessForT;
            } else {
                return BinarySubdivide(aX, intervalStart, intervalStart + KSampleStepSize, _mX1, _mX2);
            }
        }

        private double BezierEasing(double x) {
            if (x <= 0 || x >= 1) {
                return x;
            }
            return CalcBezier(GetTForX(x), _mY1, _mY2);
        }
    }
}