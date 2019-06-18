namespace Asteroids.Game.Match.Components.CollisionHandlers
{
    internal sealed class SpaceshipCollisionHandler : CollisionHandler
    {
        internal SpaceshipCollisionHandler(Entity entity) : base(entity)
        {
        }

        internal override void OnCollision(Collider other)
        {
            other.entity.Destroy();

            entity.Destroy();
        }
    }
}