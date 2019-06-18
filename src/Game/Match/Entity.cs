using System;
using Asteroids.Game.Match.Components;

namespace Asteroids.Game.Match
{
    public sealed partial class Entity
    {
        public EntityPointer Pointer { get; private set; }

        public uint Owner { get; private set; }
        public byte Team { get; private set; }

        public bool IsDestroyed => Pointer.id == 0;

        public readonly string name;

        internal readonly EntityManager entityManager;

        private readonly Component[] components;

        public int EntityBagIndex { get; internal set; }

        private Entity(EntityManager entityManager, string name, int componentCount)
        {
            this.entityManager = entityManager;

            this.name = name;

            components = new Component[componentCount];
        }

        internal void Create(EntityPointer entityPointer, uint owner)
        {
            if (entityPointer.id == 0) throw new ArgumentOutOfRangeException(nameof(entityPointer));
            if (!IsDestroyed) throw new InvalidOperationException("Entity is already created!");

            Pointer = entityPointer;

            Owner = owner;

            foreach (var component in components)
            {
                component.OnCreate();
            }
        }

        internal void Destroy()
        {
            if (IsDestroyed) throw new InvalidOperationException("Entity is already destroyed!");

            for (var i = components.Length - 1; i >= 0; i--)
            {
                components[i].OnDestroy();
            }

            entityManager.ProcessDestroyEntity(this);

            Pointer = EntityPointer.Null;
            Owner = 0;
            Team = 0;
        }

        internal void Update()
        {
            foreach (var component in components)
            {
                component.Update();
            }
        }

        public T GetComponent<T>() where T : Component
        {
            foreach (var component in components)
            {
                if (component is T t) return t;
            }

            return null;
        }
    }
}