using System;
using Asteroids.Game.Match;
using Asteroids.Game.Unity.Match.Components;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Asteroids.Game.Unity.Match
{
    internal sealed partial class UnityEntity
    {
        public Entity Entity { get; private set; }
        public EntityPointer EntityPointer { get; private set; }

        internal readonly string name;

        internal readonly UnityEntityManager unityEntityManager;

        internal readonly GameObject gameObject;

        private readonly UnityComponent[] unityComponents;

        internal UnityEntity(UnityEntityManager unityEntityManager, string name, int componentCount)
        {
            this.unityEntityManager = unityEntityManager;

            this.name = name;

            var prefab = Resources.Load<GameObject>("Entities/" + unityEntityManager.representationMode + "/" + name);

            if (prefab == null) throw new Exception("Could not load prefab by name: " + name + " and representation: " + unityEntityManager.representationMode);

            gameObject = Object.Instantiate(prefab);
            gameObject.name = name + " " + unityEntityManager.representationMode;

            gameObject.SetActive(false);

            unityComponents = new UnityComponent[componentCount];
        }

        internal void Create(Entity entity)
        {
            if (Entity != null) throw new InvalidOperationException("UnityEntity already created!");

            Entity = entity ?? throw new ArgumentNullException(nameof(entity));
            EntityPointer = entity.Pointer;

            gameObject.SetActive(true);

            foreach (var unityComponent in unityComponents)
            {
                unityComponent.OnCreate();
            }
        }

        internal void Destroy()
        {
            if (Entity == null) throw new InvalidOperationException("UnityEntity already destroyed!");

            for (var i = unityComponents.Length - 1; i >= 0; i--)
            {
                unityComponents[i].OnDestroy();
            }

            gameObject.SetActive(false);

            Entity = null;

            EntityPointer = EntityPointer.Null;
        }

        internal void TickChanged()
        {
            foreach (var unityComponent in unityComponents)
            {
                unityComponent.TickChanged();
            }
        }

        internal void Update()
        {
            foreach (var unityComponent in unityComponents)
            {
                unityComponent.Update();
            }
        }
    }
}