using Asteroids.Math;

namespace Asteroids.Game.Match.Components
{
    internal sealed class AsteroidController : Component
    {
        private readonly Transform transform;

        private readonly long minMoveSpeed;
        private readonly long maxMoveSpeed;

        private readonly long minRotationSpeed;
        private readonly long maxRotationSpeed;

        private readonly int childCreationTypeOnDestroy;

        private long rotationSpeed;

        private Vector2 moveSpeed;

        internal AsteroidController(
            Entity entity,
            long minMoveSpeed, long maxMoveSpeed, long minRotationSpeed, long maxRotationSpeed,
            int childCreationTypeOnDestroy) : base(entity)
        {
            this.minMoveSpeed = minMoveSpeed;
            this.maxMoveSpeed = maxMoveSpeed;

            this.minRotationSpeed = minRotationSpeed;
            this.maxRotationSpeed = maxRotationSpeed;

            this.childCreationTypeOnDestroy = childCreationTypeOnDestroy;

            transform = entity.GetComponent<Transform>();
        }

        internal override void OnCreate()
        {
            var rnd = entity.entityManager.matchManager.random;

            transform.Rotation = rnd.GetRandom((int) (FixedMath.Pi * 2 + 1));

            var diff = maxRotationSpeed - minRotationSpeed;
            rotationSpeed = minRotationSpeed + rnd.GetRandom((int) diff);

            diff = maxMoveSpeed - minMoveSpeed;
            var moveSpeedMultiplier = minMoveSpeed + rnd.GetRandom((int) diff);

            var dirAngle = rnd.GetRandom((int) (FixedMath.Pi * 2 + 1));

            moveSpeed = new Vector2(FixedMath.Cos(dirAngle), FixedMath.Sin(dirAngle)) * moveSpeedMultiplier;
        }

        internal override void OnDestroy()
        {
            if (childCreationTypeOnDestroy <= 0) return;

            var count = entity.entityManager.matchManager.random.GetRandom(2) + 3;

            for (var i = 0; i < count; i++)
            {
                entity.entityManager.matchManager.gameModeManager.SpawnAsteroid(transform.Position, 2);
            }
        }

        internal override void Update()
        {
            transform.Position += moveSpeed;

            transform.Rotation += rotationSpeed;
        }
    }
}