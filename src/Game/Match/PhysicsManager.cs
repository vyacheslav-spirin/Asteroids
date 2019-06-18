using System;
using System.Collections;
using System.Collections.Generic;
using Asteroids.Game.Match.Components;
using Asteroids.Game.Match.Components.CollisionHandlers;
using Asteroids.Game.Match.Physics;
using Asteroids.Math;

namespace Asteroids.Game.Match
{
    //TODO: REQUIRED SPATIAL OPTIMIZATION (octree, etc)

    internal sealed class PhysicsManager
    {
        private struct CollisionPair
        {
            internal EntityPointer first;
            internal EntityPointer second;
        }

        private readonly MatchManager matchManager;

        private readonly BitArray collisionMatrix;

        private readonly List<CollisionPair> collisionPairs = new List<CollisionPair>(100);

        internal PhysicsManager(MatchManager matchManager)
        {
            this.matchManager = matchManager;

            collisionMatrix = new BitArray(CollisionLayersConfig.MaxCollisionLayers * CollisionLayersConfig.MaxCollisionLayers, false);
        }

        internal void SetLayerCollision(int firstLayer, int secondLayer, bool collision)
        {
            collisionMatrix[firstLayer * CollisionLayersConfig.MaxCollisionLayers + secondLayer] = collision;
            collisionMatrix[secondLayer * CollisionLayersConfig.MaxCollisionLayers + firstLayer] = collision;
        }

        internal int FindLineSegmentIntersections(Vector2 lineSegmentVertex1, Vector2 lineSegmentVertex2, byte layer)
        {
            collisionPairs.Clear();

            var entityManager = matchManager.entityManager;

            var entityCount = entityManager.EntityCount;

            for (var i = 0; i < entityCount; i++)
            {
                var entity = entityManager.GetEntityByIndex(i);

                var collider = entity.GetComponent<Collider>();

                if (collider == null) continue;

                if (!collisionMatrix[collider.CollisionLayer * CollisionLayersConfig.MaxCollisionLayers + layer]) continue;

                var transform = entity.GetComponent<Transform>();

                if (!CollisionDetector.IsLineSegmentIntersectShape(
                    lineSegmentVertex1, lineSegmentVertex2, collider.shape, transform.Position, transform.Rotation)) continue;

                collisionPairs.Add(new CollisionPair
                {
                    first = entity.Pointer
                });
            }

            return collisionPairs.Count;
        }

        internal EntityPointer GetLastLineSegmentIntersectionEntityPointer(int index)
        {
            if (index < 0 || index >= collisionPairs.Count) throw new ArgumentOutOfRangeException(nameof(index));

            return collisionPairs[index].first;
        }

        internal void Update()
        {
            var entityManager = matchManager.entityManager;

            var entityCount = entityManager.EntityCount;

            collisionPairs.Clear();

            for (var i = 0; i < entityCount; i++)
            {
                var firstEntity = entityManager.GetEntityByIndex(i);

                var firstCollider = firstEntity.GetComponent<Collider>();
                if (firstCollider == null) continue;

                var firstTransform = firstEntity.GetComponent<Transform>();
                var firstPos = firstTransform.Position;
                var firstRot = firstTransform.Rotation;

                var fCollisionLayer = firstCollider.CollisionLayer;

                for (var j = i + 1; j < entityCount; j++)
                {
                    var secondEntity = entityManager.GetEntityByIndex(j);

                    var secondCollider = secondEntity.GetComponent<Collider>();
                    if (secondCollider == null) continue;

                    var sCollisionLayer = secondCollider.CollisionLayer;

                    if (!collisionMatrix[fCollisionLayer * CollisionLayersConfig.MaxCollisionLayers + sCollisionLayer]) continue;

                    if (firstEntity.GetComponent<CollisionHandler>() == null && secondEntity.GetComponent<CollisionHandler>() == null) continue;

                    foreach (var collisionPair in collisionPairs)
                    {
                        if (collisionPair.first == firstEntity.Pointer && collisionPair.second == secondEntity.Pointer ||
                            collisionPair.first == secondEntity.Pointer && collisionPair.second == firstEntity.Pointer)
                        {
                            //skip current entity iteration
                            goto Next;
                        }
                    }

                    var secondTransform = secondEntity.GetComponent<Transform>();
                    var secondPos = secondTransform.Position;
                    var secondRot = secondTransform.Rotation;

                    if (!CollisionDetector.IsShapesIntersects(firstCollider.shape, firstPos, firstRot, secondCollider.shape, secondPos, secondRot)) continue;

                    collisionPairs.Add(new CollisionPair
                    {
                        first = firstEntity.Pointer,
                        second = secondEntity.Pointer
                    });

                    Next: ;
                }
            }

            //Process collision pairs

            foreach (var collisionPair in collisionPairs)
            {
                var firstEntity = entityManager.TryGetEntity(collisionPair.first);
                if (firstEntity == null) continue;

                var secondEntity = entityManager.TryGetEntity(collisionPair.second);
                if (secondEntity == null) continue;

                var firstCollider = firstEntity.GetComponent<Collider>();
                var secondCollider = secondEntity.GetComponent<Collider>();

                if (!collisionMatrix[firstCollider.CollisionLayer * CollisionLayersConfig.MaxCollisionLayers + secondCollider.CollisionLayer]) continue;

                var collisionHandler = firstEntity.GetComponent<CollisionHandler>();

                if (collisionHandler != null)
                {
                    collisionHandler.OnCollision(secondCollider);

                    collisionHandler = secondEntity.GetComponent<CollisionHandler>();

                    if (collisionHandler == null) continue;

                    if (firstEntity.Pointer != collisionPair.first ||
                        secondEntity.Pointer != collisionPair.second ||
                        !collisionMatrix[firstCollider.CollisionLayer * CollisionLayersConfig.MaxCollisionLayers + secondCollider.CollisionLayer]) continue;
                }
                else
                {
                    collisionHandler = secondEntity.GetComponent<CollisionHandler>();

                    collisionHandler.OnCollision(firstCollider);
                }
            }

            collisionPairs.Clear();
        }
    }
}