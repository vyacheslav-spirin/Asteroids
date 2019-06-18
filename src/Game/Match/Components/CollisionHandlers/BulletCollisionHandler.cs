namespace Asteroids.Game.Match.Components.CollisionHandlers
{
    internal sealed class BulletCollisionHandler : CollisionHandler
    {
        internal BulletCollisionHandler(Entity entity) : base(entity)
        {
        }

        internal override void OnCollision(Collider other)
        {
            var player = entity.entityManager.matchManager.playerManager.GetPlayerById(entity.Owner);

            if (player != null) player.Score += 100;

            other.entity.Destroy();

            entity.Destroy();
        }
    }
}