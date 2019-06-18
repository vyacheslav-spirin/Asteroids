namespace Asteroids.Game.Match.Components.CollisionHandlers
{
    internal abstract class CollisionHandler : Component
    {
        protected CollisionHandler(Entity entity) : base(entity)
        {
        }

        internal abstract void OnCollision(Collider other);
    }
}