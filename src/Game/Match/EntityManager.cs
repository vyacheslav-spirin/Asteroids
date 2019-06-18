using System;
using System.Collections.Generic;

namespace Asteroids.Game.Match
{
    public sealed class EntityManager
    {
        internal readonly MatchManager matchManager;

        private readonly FastBag<Entity> activeEntities = new FastBag<Entity>(Config.MaxActiveEntities);

        private readonly Entity[] entitiesFastAccessArray = new Entity[Config.MaxActiveEntities];

        private readonly Dictionary<string, Stack<Entity>> entitiesPools = new Dictionary<string, Stack<Entity>>();

        private readonly Dictionary<string, int> entityPerNameCount = new Dictionary<string, int>();

        private readonly Stack<ushort> entitiesArrayFreeIndexes = new Stack<ushort>();

        private readonly Stack<Entity> deferredUpdateEntities = new Stack<Entity>();

        public int EntityCount => activeEntities.Count;

        public uint EntityIdSequence { get; private set; }

        private ushort lastFastAccessId;

        private bool isInUpdate;

        internal EntityManager(MatchManager matchManager)
        {
            this.matchManager = matchManager;
        }

        public void InitPool(string entityName, int count)
        {
            if (entitiesPools.ContainsKey(entityName)) throw new InvalidOperationException("Pool with name " + entityName + " already exists!");

            var pool = new Stack<Entity>(count);

            for (var i = 0; i < count; i++)
            {
                var entity = Entity.CreateNewEntityInstance(this, entityName);

                pool.Push(entity);
            }

            entitiesPools.Add(entityName, pool);
        }

        internal Entity CreateEntity(string entityName, uint owner)
        {
            ushort allocationId;
            if (entitiesArrayFreeIndexes.Count > 0) allocationId = entitiesArrayFreeIndexes.Pop();
            else
            {
                allocationId = lastFastAccessId++;

                if (allocationId >= Config.MaxActiveEntities)
                    throw new Exception("Maximum number of active entities exceeded! Max: " + Config.MaxActiveEntities);
            }

            entitiesPools.TryGetValue(entityName, out var pool);

            Entity entity;

            if (pool == null || pool.Count == 0)
            {
                entity = Entity.CreateNewEntityInstance(this, entityName);
            }
            else
            {
                entity = pool.Pop();
            }

            entity.EntityBagIndex = activeEntities.Add(entity);

            if (!entityPerNameCount.TryGetValue(entityName, out var count)) entityPerNameCount.Add(entityName, 1);
            else entityPerNameCount[entityName] = count + 1;

            entity.Create(new EntityPointer(++EntityIdSequence, allocationId), owner);

            entitiesFastAccessArray[allocationId] = entity;

            return entity;
        }

        internal void ProcessDestroyEntity(Entity entity)
        {
            entityPerNameCount[entity.name]--;

            entitiesFastAccessArray[entity.Pointer.fastAccessId] = null;
            entitiesArrayFreeIndexes.Push(entity.Pointer.fastAccessId);

            var bagIndex = entity.EntityBagIndex;

            var updateMovedEntity = activeEntities.TryReplaceByLastElement(bagIndex);

            if (entitiesPools.TryGetValue(entity.name, out var pool)) pool.Push(entity);

            if (updateMovedEntity)
            {
                entity = activeEntities[bagIndex];
                entity.EntityBagIndex = bagIndex;

                if (isInUpdate)
                {
                    if (!deferredUpdateEntities.Contains(entity)) deferredUpdateEntities.Push(entity);
                }
            }
        }

        internal void Update()
        {
            isInUpdate = true;

            for (var i = 0; i < activeEntities.Count; i++)
            {
                activeEntities[i].Update();
            }

            isInUpdate = false;

            while (deferredUpdateEntities.Count > 0)
            {
                deferredUpdateEntities.Pop().Update();
            }
        }

        public Entity TryGetEntity(EntityPointer pointer)
        {
            if (pointer.id == 0) return null;

            if (pointer.fastAccessId >= entitiesFastAccessArray.Length) return null;

            var entity = entitiesFastAccessArray[pointer.fastAccessId];
            if (entity == null || entity.Pointer.id != pointer.id) return null;

            return entity;
        }

        public Entity GetEntityByIndex(int index)
        {
            if (index < 0 || index >= activeEntities.Count) throw new ArgumentOutOfRangeException(nameof(index));

            return activeEntities[index];
        }

        public Entity Find(Predicate<Entity> predicate)
        {
            for (var i = 0; i < activeEntities.Count; i++)
            {
                var entity = activeEntities[i];

                if (predicate(entity)) return entity;
            }

            return null;
        }

        public int GetEntityPerNameCount(string name)
        {
            if (!entityPerNameCount.TryGetValue(name, out var count)) return 0;

            return count;
        }
    }
}