using System;

namespace Asteroids.Math
{
    public static class FixedMath
    {
        public const int SHIFT_AMOUNT = 16;

        public const long One = 1 << SHIFT_AMOUNT;

        public const long Half = One / 2;

        public const double OneD = One;

        public const long Pi = 355 * One / 113;

        public const long TwoPi = Pi * 2;

        public const long HalfPi = Pi / 2;

        public const long Epsilon = 1 << (SHIFT_AMOUNT - 10);

        public static long Sin(long theta)
        {
            theta = theta.Normalized(TwoPi);


            if (theta == HalfPi)
            {
                return One;
            }

            if (theta == 0 || theta == Pi)
            {
                return 0;
            }

            if (theta == Pi + HalfPi)
            {
                return -One;
            }

            if (theta == Pi / 6 || theta == Pi * 5 / 6)
            {
                return Half;
            }

            if (theta == Pi * 11 / 6 || theta == Pi * 7 / 6)
            {
                return -Half;
            }

            var mirror = false;
            var flip = false;
            var quadrant = theta.Div(HalfPi).ToInt();
            switch (quadrant)
            {
                case 0:
                    break;
                case 1:
                    mirror = true;
                    break;
                case 2:
                    flip = true;
                    break;
                case 3:
                    mirror = true;
                    flip = true;
                    break;
            }

            theta = theta.Normalized(HalfPi);
            if (mirror)
                theta = HalfPi - theta;

            var thetaSquared = theta.Mul(theta);

            var result = theta;
            const int shift = SHIFT_AMOUNT;
            //2 shifts for 2 multiplications but there's a division so only 1 shift
            var n = (theta * theta * theta) >> (shift * 1);
            const long Factorial3 = 3 * 2 * One;
            result -= n / Factorial3;

            n *= thetaSquared;
            n >>= shift;
            const long Factorial5 = Factorial3 * 4 * 5;
            result += n / Factorial5;

            n *= thetaSquared;
            n >>= shift;
            const long Factorial7 = Factorial5 * 6 * 7;
            result -= n / Factorial7;

            if (flip)
                result *= -1;

            return result;
        }

        public static long Cos(long theta)
        {
            theta += theta > 0 ? -Pi - HalfPi : HalfPi;

            return Sin(theta);
        }

        public static long Asin(long F)
        {
            var isNegative = F < 0;

            F = Abs(F);

            if (F > One) throw new ArithmeticException("Bad Asin Input:" + F.ToDouble());

            //Magic numbers from: http://stackoverflow.com/questions/605124/fixed-point-math-in-c
            //Converting steps: fixed (shift 12) -> double -> fixed (shift 16)

            var f1 = Mul(Mul(Mul(Mul(560, F) -
                                 2336, F) +
                             5536, F) -
                         14032, F) +
                     HalfPi;

            var f2 = HalfPi - Mul(Sqrt(One - F), f1);

            return isNegative ? -f2 : f2;
        }

        public static long Atan(long F)
        {
            return Asin(Div(F, Sqrt(One + Mul(F, F))));
        }

        public static long Atan2(long F1, long F2)
        {
            if (F1 > 0 && F1 < Epsilon || F1 < 0 && F1 > -Epsilon) F1 = 0;
            if (F2 > 0 && F2 < Epsilon || F2 < 0 && F2 > -Epsilon) F2 = 0;

            if (F2 == 0 && F1 == 0) return 0;

            long result;
            if (F2 > 0)
                result = Atan(Div(F1, F2));
            else if (F2 < 0)
                if (F1 >= 0)
                    result = Pi - Atan(Abs(Div(F1, F2)));
                else
                    result = -(Pi - Atan(Abs(Div(F1, F2))));
            else
                result = (F1 >= 0 ? Pi : -Pi) / 2;

            if (result < -TwoPi) result += TwoPi;
            else if (result > TwoPi) result -= TwoPi;

            return result;
        }

        public static long Mul(this long f1, long f2)
        {
            return (f1 * f2) >> SHIFT_AMOUNT;
        }

        public static long Div(this long f1, long f2)
        {
            return (f1 << SHIFT_AMOUNT) / f2;
        }

        public static long Sqrt(long f1)
        {
            if (f1 == 0)
                return 0;

            var n = (f1 >> 1) + 1;
            var n1 = (n + f1 / n) >> 1;
            while (n1 < n)
            {
                n = n1;
                n1 = (n + f1 / n) >> 1;
            }

            return n << (SHIFT_AMOUNT / 2);
        }


        public static long Abs(this long f1)
        {
            return f1 < 0 ? -f1 : f1;
        }

        public static long Normalized(this long f1, long range)
        {
            while (f1 < 0)
                f1 += range;
            if (f1 >= range)
                f1 = f1 % range;
            return f1;
        }

        public static int ToInt(this long f1)
        {
            return (int) (f1 >> SHIFT_AMOUNT);
        }

        public static double ToDouble(this long f1)
        {
            return f1 / OneD;
        }

        public static float ToFloat(this long f1)
        {
            return (float) (f1 / OneD);
        }

        public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var s = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y) >> SHIFT_AMOUNT;
            var t = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y) >> SHIFT_AMOUNT;

            if (s < 0 != t < 0) return false;

            var a = (-p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y) >> SHIFT_AMOUNT;

            return a < 0 ? s <= 0 && s + t >= a : s >= 0 && s + t <= a;
        }
    }
}