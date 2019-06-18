using System;
using System.Collections.Generic;
using Asteroids.Game.Match;
using Object = UnityEngine.Object;

namespace Asteroids.Game.Unity.Match
{
    internal sealed class UnityEntityManager
    {
        public bool isEnabled = true;

        internal readonly UnityMatchManager unityMatchManager;

        internal readonly RepresentationMode representationMode;

        private readonly Dictionary<string, Stack<UnityEntity>> unityEntitiesPools = new Dictionary<string, Stack<UnityEntity>>();

        private readonly UnityEntity[] unityActiveEntities = new UnityEntity[Game.Config.MaxActiveEntities];

        private int unityActiveEntitiesCount;

        private uint lastSyncTick;

        internal UnityEntityManager(UnityMatchManager unityMatchManager, RepresentationMode representationMode)
        {
            this.unityMatchManager = unityMatchManager;

            this.representationMode = representationMode;
        }

        internal void InitPool(string entityName, int count)
        {
            if (unityEntitiesPools.ContainsKey(entityName)) throw new InvalidOperationException("Pool with name " + entityName + " already exists!");

            var pool = new Stack<UnityEntity>(count);

            for (var i = 0; i < count; i++)
            {
                var entity = UnityEntity.CreateNewEntityInstance(this, entityName);

                pool.Push(entity);
            }

            unityEntitiesPools.Add(entityName, pool);
        }

        private void CreateUnityEntity(Entity entity)
        {
            unityEntitiesPools.TryGetValue(entity.name, out var pool);

            UnityEntity unityEntity;

            if (pool == null || pool.Count == 0)
            {
                unityEntity = UnityEntity.CreateNewEntityInstance(this, entity.name);
            }
            else
            {
                unityEntity = pool.Pop();
            }

            unityActiveEntities[entity.EntityBagIndex] = unityEntity;

            unityEntity.Create(entity);
        }

        internal void Update()
        {
            if (lastSyncTick != unityMatchManager.matchManager.CurrentTick)
            {
                lastSyncTick = unityMatchManager.matchManager.CurrentTick;

                SyncUnityEntities();

                for (var i = 0; i < unityActiveEntitiesCount; i++)
                {
                    unityActiveEntities[i].TickChanged();
                }
            }

            if (isEnabled)
            {
                for (var i = 0; i < unityActiveEntitiesCount; i++)
                {
                    unityActiveEntities[i].Update();
                }
            }
        }

        private void SyncUnityEntities()
        {
            for (var i = 0; i < unityActiveEntitiesCount; i++)
            {
                var unityEntity = unityActiveEntities[i];

                if (unityEntity.Entity.IsDestroyed || unityEntity.Entity.Pointer != unityEntity.EntityPointer)
                {
                    unityEntity.Destroy();

                    if (unityEntitiesPools.TryGetValue(unityEntity.name, out var pool)) pool.Push(unityEntity);
                    else Object.Destroy(unityEntity.gameObject);

                    unityActiveEntities[i] = null;

                    continue;
                }

                if (unityEntity.Entity.EntityBagIndex != i)
                {
                    var deprecatedUnityEntity = unityActiveEntities[unityEntity.Entity.EntityBagIndex];

                    if (deprecatedUnityEntity != null)
                    {
                        deprecatedUnityEntity.Destroy();

                        if (unityEntitiesPools.TryGetValue(deprecatedUnityEntity.name, out var pool)) pool.Push(deprecatedUnityEntity);
                        else Object.Destroy(deprecatedUnityEntity.gameObject);
                    }

                    unityActiveEntities[unityEntity.Entity.EntityBagIndex] = unityEntity;

                    unityActiveEntities[i] = null;
                }
            }

            var entityManager = unityMatchManager.matchManager.entityManager;

            var activeEntitiesCount = entityManager.EntityCount;

            for (var i = 0; i < activeEntitiesCount; i++)
            {
                var entity = entityManager.GetEntityByIndex(i);

                var unityEntity = unityActiveEntities[entity.EntityBagIndex];

                if (unityEntity?.Entity == null)
                {
                    CreateUnityEntity(entity);
                }
            }

            unityActiveEntitiesCount = activeEntitiesCount;
        }
    }
}