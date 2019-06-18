using System;
using Asteroids.Game.Match.Physics.Shapes;
using Asteroids.Math;

namespace Asteroids.Game.Match.Physics
{
    public static class CollisionDetector
    {
        private delegate bool IntersectionMethod(Shape first, Vector2 firstPos, long firstRot, Shape second, Vector2 secondPos, long secondRot);

        private delegate bool LineIntersectionMethod(Vector2 lineSegment1, Vector2 lineSegment2, Shape shape, Vector2 shapePos, long shapeRot);

        private static readonly IntersectionMethod[] IntersectionMethodMatrix;
        private static readonly int ShapeTypeCount;

        private static readonly LineIntersectionMethod[] LineIntersectionMethods;

        static CollisionDetector()
        {
            var values = Enum.GetValues(typeof(ShapeType));

            ShapeTypeCount = values.Length;

            IntersectionMethodMatrix = new IntersectionMethod[ShapeTypeCount * ShapeTypeCount];

            IntersectionMethodMatrix[(int) ShapeType.Polygon * ShapeTypeCount + (int) ShapeType.Polygon] = IsPolygonAndPolygonIntersects;

            LineIntersectionMethods = new LineIntersectionMethod[ShapeTypeCount];

            LineIntersectionMethods[(int) ShapeType.Polygon] = IsLineAndPolygonIntersects;
        }

        public static bool IsLineSegmentIntersectShape(Vector2 lineSegmentVertex1, Vector2 lineSegmentVertex2, Shape shape, Vector2 shapePos, long shapeRot)
        {
            var intersectionMethod = LineIntersectionMethods[(int) shape.shapeType];

            return intersectionMethod(lineSegmentVertex1, lineSegmentVertex2, shape, shapePos, shapeRot);
        }

        public static bool IsShapesIntersects(Shape first, Vector2 firstPos, long firstRot, Shape second, Vector2 secondPos, long secondRot)
        {
            var fBoundMin = first.BoundingBoxMin + firstPos;
            var fBoundMax = first.BoundingBoxMax + firstPos;

            var sBoundMin = second.BoundingBoxMin + secondPos;
            var sBoundMax = second.BoundingBoxMax + secondPos;

            //bounding boxes not intersects
            if (sBoundMin.x > fBoundMax.x || sBoundMax.x < fBoundMin.x || sBoundMax.y < fBoundMin.y || sBoundMin.y > fBoundMax.y) return false;

            var intersectionMethod = IntersectionMethodMatrix[(int) first.shapeType * ShapeTypeCount + (int) second.shapeType];

            return intersectionMethod(first, firstPos, firstRot, second, secondPos, secondRot);
        }

        //TODO: MODIFY! NOT WORKING IF LINE SEGMENT INSIDE POLYGON

        private static bool IsLineAndPolygonIntersects(Vector2 lineSegmentVertex1, Vector2 lineSegmentVertex2, Shape shape, Vector2 shapePos, long shapeRot)
        {
            var polygonShape = (PolygonShape) shape;

            var cos = FixedMath.Cos(shapeRot);
            var sin = FixedMath.Sin(shapeRot);

            //TODO: replace to better algorithm

            var prevPolygonVertex = Rotate(polygonShape.vertices[0], cos, sin) + shapePos;

            for (var i = polygonShape.vertices.Length - 1; i >= 0; i--)
            {
                var curPolygonVertex = Rotate(polygonShape.vertices[i], cos, sin) + shapePos;

                if (LineSegmentIntersection.IsLineSegmentsIntersects(lineSegmentVertex1, lineSegmentVertex2, prevPolygonVertex, curPolygonVertex)) return true;

                prevPolygonVertex = curPolygonVertex;
            }

            return false;
        }

        private static bool IsPolygonAndPolygonIntersects(Shape first, Vector2 firstPos, long firstRot, Shape second, Vector2 secondPos, long secondRot)
        {
            var polygonShapeFirst = (PolygonShape) first;
            var polygonShapeSecond = (PolygonShape) second;

            var firstCos = FixedMath.Cos(firstRot);
            var firstSin = FixedMath.Sin(firstRot);

            var secondCos = FixedMath.Cos(secondRot);
            var secondSin = FixedMath.Sin(secondRot);

            for (var i = 0; i < polygonShapeFirst.triangleCount; i++)
            {
                var fa = Rotate(polygonShapeFirst.triangleVertices[i * 3], firstCos, firstSin) + firstPos;
                var fb = Rotate(polygonShapeFirst.triangleVertices[i * 3 + 1], firstCos, firstSin) + firstPos;
                var fc = Rotate(polygonShapeFirst.triangleVertices[i * 3 + 2], firstCos, firstSin) + firstPos;

                for (var j = 0; j < polygonShapeSecond.triangleCount; j++)
                {
                    var sa = Rotate(polygonShapeSecond.triangleVertices[j * 3], secondCos, secondSin) + secondPos;
                    var sb = Rotate(polygonShapeSecond.triangleVertices[j * 3 + 1], secondCos, secondSin) + secondPos;
                    var sc = Rotate(polygonShapeSecond.triangleVertices[j * 3 + 2], secondCos, secondSin) + secondPos;

                    if (TriangleIntersection.IsTrianglesIntersects(fa, fb, fc, sa, sb, sc)) return true;
                }
            }

            return false;
        }

        private static Vector2 Rotate(Vector2 source, long cos, long sin)
        {
            var x = (source.x * cos - source.y * sin) >> FixedMath.SHIFT_AMOUNT;
            var y = (source.y * cos + source.x * sin) >> FixedMath.SHIFT_AMOUNT;

            return new Vector2(x, y);
        }
    }
}