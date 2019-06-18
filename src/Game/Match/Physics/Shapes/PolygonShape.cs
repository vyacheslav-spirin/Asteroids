using System;
using System.Collections.Generic;
using Asteroids.Math;

namespace Asteroids.Game.Match.Physics.Shapes
{
    public sealed class PolygonShape : Shape
    {
        internal override Vector2 BoundingBoxMin { get; }
        internal override Vector2 BoundingBoxMax { get; }


        internal readonly Vector2[] vertices;

        internal readonly int triangleCount;

        internal readonly Vector2[] triangleVertices;

        internal PolygonShape(Vector2[] vertices) : base(ShapeType.Polygon)
        {
            if (vertices == null) throw new ArgumentNullException(nameof(vertices));
            if (vertices.Length < 3) throw new ArgumentException(nameof(vertices));

            if (!PolygonIsOrientedClockwise(vertices))
            {
                var tmp = new Vector2[vertices.Length];

                for (var i = 0; i < vertices.Length; i++)
                {
                    tmp[vertices.Length - i - 1] = vertices[i];
                }

                vertices = tmp;
            }

            this.vertices = vertices;

            //triangulation

            var verticesCopy = new List<Vector2>(vertices);

            var triangleVerticesList = new List<Vector2>(vertices.Length * 3);

            while (verticesCopy.Count > 3)
            {
                RemoveEar(verticesCopy, triangleVerticesList);
            }

            triangleVerticesList.Add(verticesCopy[0]);
            triangleVerticesList.Add(verticesCopy[1]);
            triangleVerticesList.Add(verticesCopy[2]);

            triangleVertices = triangleVerticesList.ToArray();

            triangleCount = triangleVertices.Length / 3;

            if (triangleCount * 3 != triangleVertices.Length) throw new Exception("Invalid triangle vertices length!");

            //calc bounding box

            var lastLen = 0L;

            foreach (var vertex in vertices)
            {
                var len = vertex.Magnitude();

                if (len > lastLen) lastLen = len;
            }

            BoundingBoxMin = new Vector2(-lastLen, -lastLen);
            BoundingBoxMax = new Vector2(lastLen, lastLen);
        }

        private static bool PolygonIsOrientedClockwise(Vector2[] vertices)
        {
            return SignedPolygonArea(vertices) < 0;
        }

        private static long SignedPolygonArea(Vector2[] vertices)
        {
            var prev = vertices[0];

            long area = 0;
            for (var i = 1; i < vertices.Length + 1; i++)
            {
                var now = vertices[i % vertices.Length];

                area += (now.x - prev.x) * (now.y + prev.y) / (FixedMath.One * 2);

                prev = now;
            }

            return area;
        }

        private static void RemoveEar(List<Vector2> vertices, List<Vector2> triangleVertices)
        {
            int a = 0, b = 0, c = 0;
            FindEar(vertices, ref a, ref b, ref c);

            triangleVertices.Add(vertices[a]);
            triangleVertices.Add(vertices[b]);
            triangleVertices.Add(vertices[c]);

            vertices.RemoveAt(b);
        }

        private static void FindEar(List<Vector2> vertices, ref int A, ref int B, ref int C)
        {
            var numPoints = vertices.Count;

            for (A = 0; A < numPoints; A++)
            {
                B = (A + 1) % numPoints;
                C = (B + 1) % numPoints;

                if (FormsEar(vertices, A, B, C)) return;
            }

            throw new Exception("Invalid behavior!");
        }

        private static long Dot(Vector2 a, Vector2 b, Vector2 c)
        {
            var ba = new Vector2(a.x - b.x, a.y - b.y);
            var bc = new Vector2(c.x - b.x, c.y - b.y);

            return ba.Dot(bc);
        }

        private static long Cross(Vector2 a, Vector2 b, Vector2 c)
        {
            var ba = new Vector2(a.x - b.x, a.y - b.y);
            var bc = new Vector2(c.x - b.x, c.y - b.y);

            return ba.Cross(bc);
        }

        private static long GetAngle(Vector2 a, Vector2 b, Vector2 c)
        {
            var dotProduct = Dot(a, b, c);

            var crossProduct = Cross(a, b, c);

            return FixedMath.Atan2(crossProduct, dotProduct);
        }

        // Return true if the three points form an ear.
        private static bool FormsEar(List<Vector2> vertices, int A, int B, int C)
        {
            if (GetAngle(vertices[A], vertices[B], vertices[C]) > 0)
            {
                return false;
            }

            var triangleVertexA = vertices[A];
            var triangleVertexB = vertices[B];
            var triangleVertexC = vertices[C];

            for (var i = 0; i < vertices.Count; i++)
            {
                if (i != A && i != B && i != C)
                {
                    if (FixedMath.PointInTriangle(vertices[i], triangleVertexA, triangleVertexB, triangleVertexC))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}