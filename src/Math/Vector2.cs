using System;

namespace Asteroids.Math
{
    public struct Vector2
    {
        public static readonly Vector2 Up = new Vector2(0, 1);
        public static readonly Vector2 Zero = new Vector2(0, 0);


        public long x;

        public long y;

        public Vector2(long xFixed, long yFixed)
        {
            x = xFixed;
            y = yFixed;
        }

        public long Magnitude()
        {
            var temp1 = x * x + y * y;

            if (temp1 == 0) return 0;

            temp1 >>= FixedMath.SHIFT_AMOUNT;

            return FixedMath.Sqrt(temp1);
        }

        public Vector2 Normalized()
        {
            var mag = Magnitude();

            if (mag == 0 || mag == FixedMath.One) return this;

            var newX = (x << FixedMath.SHIFT_AMOUNT) / mag;
            var newY = (y << FixedMath.SHIFT_AMOUNT) / mag;

            return new Vector2(newX, newY);
        }

        public Vector2 Normalized(out long mag)
        {
            mag = Magnitude();

            if (mag == 0 || mag == FixedMath.One) return this;

            var newX = (x << FixedMath.SHIFT_AMOUNT) / mag;
            var newY = (y << FixedMath.SHIFT_AMOUNT) / mag;

            return new Vector2(newX, newY);
        }

        public long Dot(Vector2 other)
        {
            return (x * other.x + y * other.y) >> FixedMath.SHIFT_AMOUNT;
        }

        public long Cross(Vector2 vec)
        {
            return (x * vec.y - y * vec.x) >> FixedMath.SHIFT_AMOUNT;
        }

        public override string ToString()
        {
            return "(" +
                   System.Math.Round(x.ToDouble(), 2, MidpointRounding.AwayFromZero) +
                   ", " +
                   System.Math.Round(y.ToDouble(), 2, MidpointRounding.AwayFromZero) +
                   ")";
        }

        //generated
        public long GetLongHashCode()
        {
            return x * 31 + y * 7;
        }

        //generated
        public int GetStateHash()
        {
            return (int) (GetLongHashCode() % int.MaxValue);
        }

        //generated
        public override int GetHashCode()
        {
            return GetStateHash();
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2)
                return (Vector2) obj == this;
            return false;
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2 operator -(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x - v2.x, v1.y - v2.y);
        }

        public static Vector2 operator *(Vector2 v1, long mag)
        {
            return new Vector2((v1.x * mag) >> FixedMath.SHIFT_AMOUNT, (v1.y * mag) >> FixedMath.SHIFT_AMOUNT);
        }

        public static Vector2 operator *(Vector2 v1, int mag)
        {
            return new Vector2(v1.x * mag, v1.y * mag);
        }

        public static Vector2 operator /(Vector2 v1, long div)
        {
            return new Vector2((v1.x << FixedMath.SHIFT_AMOUNT) / div, (v1.y << FixedMath.SHIFT_AMOUNT) / div);
        }

        public static Vector2 operator /(Vector2 v1, int div)
        {
            return new Vector2(v1.x / div, v1.y / div);
        }

        public static Vector2 operator >>(Vector2 v1, int shift)
        {
            return new Vector2(v1.x >> shift, v1.y >> shift);
        }

        public static bool operator ==(Vector2 v1, Vector2 v2)
        {
            return v1.x == v2.x && v1.y == v2.y;
        }

        public static bool operator !=(Vector2 v1, Vector2 v2)
        {
            return v1.x != v2.x || v1.y != v2.y;
        }
    }
}