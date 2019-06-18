using System;
using Asteroids.Math;

namespace Asteroids.Game.Match.Components
{
    public sealed class BorderTransform : Transform
    {
        public override Vector2 Position
        {
            set
            {
                position = value;

                if (position.x > border.x)
                {
                    position.x = -border.x;

                    Teleport();
                }
                else if (position.x < -border.x)
                {
                    position.x = border.x;

                    Teleport();
                }

                if (position.y > border.y)
                {
                    position.y = -border.y;

                    Teleport();
                }
                else if (position.y < -border.y)
                {
                    position.y = border.y;

                    Teleport();
                }
            }
        }

        private readonly Vector2 border;

        internal BorderTransform(Entity entity, Vector2 border) : base(entity)
        {
            if (border.x < 0 || border.y < 0) throw new ArgumentOutOfRangeException(nameof(border));

            this.border = border;
        }
    }
}