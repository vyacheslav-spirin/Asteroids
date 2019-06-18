using Asteroids.Math;

namespace Asteroids.Game.Match.Physics.Shapes
{
    public abstract class Shape
    {
        internal abstract Vector2 BoundingBoxMin { get; }
        internal abstract Vector2 BoundingBoxMax { get; }


        public readonly ShapeType shapeType;

        protected Shape(ShapeType shapeType)
        {
            this.shapeType = shapeType;
        }
    }
}