using Asteroids.Math;

namespace Asteroids.Game.Match.Components
{
    internal sealed class UfoController : Component
    {
        private const long Acceleration = FixedMath.One / 80;
        private const long MaxMoveSpeed = FixedMath.One / 20;
        private const long RotationSpeed = FixedMath.Pi / 50;

        private readonly Transform transform;

        private Vector2 moveSpeed;

        internal UfoController(Entity entity) : base(entity)
        {
            transform = entity.GetComponent<Transform>();
        }

        internal override void OnCreate()
        {
            moveSpeed = Vector2.Zero;
        }

        internal override void Update()
        {
            Vector2 moveDir;

            if (entity.entityManager.GetEntityPerNameCount("Spaceship") == 0)
            {
                moveDir = new Vector2(FixedMath.Cos(transform.Rotation), FixedMath.Sin(transform.Rotation));
            }
            else
            {
                var spaceship = entity.entityManager.Find(e => e.name == "Spaceship");

                moveDir = (spaceship.GetComponent<Transform>().Position - transform.Position).Normalized();
            }

            moveSpeed += moveDir * Acceleration;

            var moveSpeedNormalized = moveSpeed.Normalized(out var mag);
            if (mag > MaxMoveSpeed) moveSpeed = moveSpeedNormalized * MaxMoveSpeed;

            transform.Position += moveSpeed;

            transform.Rotation += RotationSpeed;
        }
    }
}