using Asteroids.Math;

namespace Asteroids.Game.Match.Components
{
    public class Transform : Component
    {
        public virtual Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public virtual long Rotation
        {
            get => rotation;
            set => rotation = value % FixedMath.TwoPi;
        }

        public uint TeleportationCount { get; private set; }

        protected Vector2 position;
        protected long rotation;

        internal Transform(Entity entity) : base(entity)
        {
        }

        internal override void OnCreate()
        {
            position = Vector2.Zero;

            rotation = 0;

            TeleportationCount = 0;
        }

        internal void Teleport()
        {
            TeleportationCount++;
        }
    }
}