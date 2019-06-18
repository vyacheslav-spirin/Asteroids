namespace Asteroids.Math
{
    public static class LineSegmentIntersection
    {
        public static bool IsLineSegmentsIntersects(Vector2 first1, Vector2 first2, Vector2 second1, Vector2 second2)
        {
            var v1 = ((second2.x - second1.x) * (first1.y - second1.y) - (second2.y - second1.y) * (first1.x - second1.x)) >> FixedMath.SHIFT_AMOUNT;
            var v2 = ((second2.x - second1.x) * (first2.y - second1.y) - (second2.y - second1.y) * (first2.x - second1.x)) >> FixedMath.SHIFT_AMOUNT;
            var v3 = ((first2.x - first1.x) * (second1.y - first1.y) - (first2.y - first1.y) * (second1.x - first1.x)) >> FixedMath.SHIFT_AMOUNT;
            var v4 = ((first2.x - first1.x) * (second2.y - first1.y) - (first2.y - first1.y) * (second2.x - first1.x)) >> FixedMath.SHIFT_AMOUNT;

            return v1 * v2 < 0 && v3 * v4 < 0;
        }
    }
}