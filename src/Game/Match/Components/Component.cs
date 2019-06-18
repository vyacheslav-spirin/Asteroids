using System;

namespace Asteroids.Game.Match.Components
{
    public abstract class Component
    {
        internal readonly Entity entity;

        protected Component(Entity entity)
        {
            this.entity = entity ?? throw new ArgumentOutOfRangeException(nameof(entity));
        }

        internal virtual void OnCreate()
        {
        }

        internal virtual void OnDestroy()
        {
        }

        internal virtual void Update()
        {
        }
    }
}