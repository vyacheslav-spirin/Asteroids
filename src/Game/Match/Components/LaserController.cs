using Asteroids.Game.Match.Physics;
using Asteroids.Math;

namespace Asteroids.Game.Match.Components
{
    public sealed class LaserController : Component
    {
        public const uint Lifetime = Config.TicksPerSecond / 5;

        private const long LaserLength = FixedMath.One * 10;

        public uint CreationTime { get; private set; }

        public Vector2 StartPoint { get; private set; }

        public Vector2 EndPoint { get; private set; }

        private EntityPointer root;

        internal LaserController(Entity entity) : base(entity)
        {
        }

        internal override void OnCreate()
        {
            CreationTime = entity.entityManager.matchManager.CurrentTick;

            StartPoint = Vector2.Zero;

            EndPoint = Vector2.Zero;

            root = EntityPointer.Null;
        }

        internal void Init(EntityPointer rootEntityPointer, Vector2 dir)
        {
            root = rootEntityPointer;

            var entityManager = entity.entityManager;

            var rootEntity = entityManager.TryGetEntity(root);

            StartPoint = rootEntity == null ? Vector2.Zero : rootEntity.GetComponent<Transform>().Position;

            EndPoint = StartPoint + dir * LaserLength;

            var physicsManager = entityManager.matchManager.physicsManager;

            var intersectionsCount = physicsManager.FindLineSegmentIntersections(StartPoint, EndPoint, CollisionLayersConfig.WeaponLayer);

            if (intersectionsCount == 0) return;

            var player = entityManager.matchManager.playerManager.GetPlayerById(entity.Owner);


            for (var i = 0; i < intersectionsCount; i++)
            {
                var e = entityManager.TryGetEntity(physicsManager.GetLastLineSegmentIntersectionEntityPointer(i));
                if (e == null) continue;

                if (player != null) player.Score += 200;

                e.Destroy();
            }
        }

        internal override void Update()
        {
            if (entity.entityManager.matchManager.CurrentTick - CreationTime > Lifetime) entity.Destroy();
            else
            {
                var rootEntity = entity.entityManager.TryGetEntity(root);

                if (rootEntity != null) StartPoint = rootEntity.GetComponent<Transform>().Position;
            }
        }
    }
}