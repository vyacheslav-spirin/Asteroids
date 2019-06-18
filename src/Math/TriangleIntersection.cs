namespace Asteroids.Math
{
    public static class TriangleIntersection
    {
        public static bool IsTrianglesIntersects(Vector2 first1, Vector2 first2, Vector2 first3, Vector2 second1, Vector2 second2, Vector2 second3)
        {
            return !Cross2(first1, first2, first3, second1, second2, second3) && !Cross2(second1, second2, second3, first1, first2, first3);
        }

        private static bool Cross2(Vector2 first1, Vector2 first2, Vector2 first3, Vector2 second1, Vector2 second2, Vector2 second3)
        {
            var pa = first1;
            var pb = first2;
            var pc = first3;
            var p0 = second1;
            var p1 = second2;
            var p2 = second3;

            var dXa = pa.x - p2.x;
            var dYa = pa.y - p2.y;
            var dXb = pb.x - p2.x;
            var dYb = pb.y - p2.y;
            var dXc = pc.x - p2.x;
            var dYc = pc.y - p2.y;
            var dX21 = p2.x - p1.x;
            var dY12 = p1.y - p2.y;

            var D = (dY12 * (p0.x - p2.x) + dX21 * (p0.y - p2.y)) >> FixedMath.SHIFT_AMOUNT;

            var sa = (dY12 * dXa + dX21 * dYa) >> FixedMath.SHIFT_AMOUNT;
            var sb = (dY12 * dXb + dX21 * dYb) >> FixedMath.SHIFT_AMOUNT;
            var sc = (dY12 * dXc + dX21 * dYc) >> FixedMath.SHIFT_AMOUNT;

            var ta = ((p2.y - p0.y) * dXa + (p0.x - p2.x) * dYa) >> FixedMath.SHIFT_AMOUNT;
            var tb = ((p2.y - p0.y) * dXb + (p0.x - p2.x) * dYb) >> FixedMath.SHIFT_AMOUNT;
            var tc = ((p2.y - p0.y) * dXc + (p0.x - p2.x) * dYc) >> FixedMath.SHIFT_AMOUNT;

            if (D < 0)
                return sa >= 0 && sb >= 0 && sc >= 0 ||
                       ta >= 0 && tb >= 0 && tc >= 0 ||
                       sa + ta <= D && sb + tb <= D && sc + tc <= D;

            return sa <= 0 && sb <= 0 && sc <= 0 ||
                   ta <= 0 && tb <= 0 && tc <= 0 ||
                   sa + ta >= D && sb + tb >= D && sc + tc >= D;
        }
    }
}