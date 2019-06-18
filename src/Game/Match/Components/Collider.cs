using System;
using Asteroids.Game.Match.Physics;
using Asteroids.Game.Match.Physics.Shapes;

namespace Asteroids.Game.Match.Components
{
    internal sealed class Collider : Component
    {
        internal readonly Shape shape;

        internal byte CollisionLayer { get; private set; }

        internal Collider(Entity entity, Shape shape) : base(entity)
        {
            this.shape = shape;
        }

        internal override void OnCreate()
        {
            CollisionLayer = CollisionLayersConfig.DefaultLayer;
        }

        internal void SetCollisionLayer(byte layer)
        {
            if (layer > CollisionLayersConfig.MaxCollisionLayers) throw new ArgumentOutOfRangeException(nameof(layer));

            CollisionLayer = layer;
        }
    }
}