using Asteroids.Math;

namespace Asteroids.Game.Match.Components
{
    internal sealed class BulletController : Component
    {
        private const uint Lifetime = Config.TicksPerSecond * 3;

        private const long MoveSpeed = FixedMath.One / 8;

        private readonly Transform transform;

        private uint creationTime;

        private Vector2 moveSpeed;

        internal BulletController(Entity entity) : base(entity)
        {
            transform = entity.GetComponent<Transform>();
        }

        internal override void OnCreate()
        {
            creationTime = entity.entityManager.matchManager.CurrentTick;

            RecalculateMoveSpeed();
        }

        internal void RecalculateMoveSpeed()
        {
            moveSpeed = new Vector2(FixedMath.Cos(transform.Rotation), FixedMath.Sin(transform.Rotation)) * MoveSpeed;
        }

        internal override void Update()
        {
            if (entity.entityManager.matchManager.CurrentTick - creationTime > Lifetime)
            {
                entity.Destroy();

                return;
            }

            transform.Position += moveSpeed;
        }
    }
}