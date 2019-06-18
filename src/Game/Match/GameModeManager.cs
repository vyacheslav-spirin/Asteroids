using System;
using Asteroids.Game.Match.Components;
using Asteroids.Game.Match.Physics;
using Asteroids.Math;

namespace Asteroids.Game.Match
{
    internal sealed class GameModeManager
    {
        private readonly MatchManager matchManager;

        private uint lastUfoSpawnTick;

        internal GameModeManager(MatchManager matchManager)
        {
            this.matchManager = matchManager;

            matchManager.physicsManager.SetLayerCollision(CollisionLayersConfig.AsteroidLayer, CollisionLayersConfig.SpaceshipLayer, true);

            matchManager.physicsManager.SetLayerCollision(CollisionLayersConfig.WeaponLayer, CollisionLayersConfig.AsteroidLayer, true);
        }

        internal void OnPlayerConnect(Player player)
        {
        }

        internal void OnPlayerDisconnect(Player player)
        {
            var entity = matchManager.entityManager.TryGetEntity(player.SpaceshipPointer);

            entity?.Destroy();
        }

        internal void Respawn(Player player)
        {
            var entityManager = matchManager.entityManager;

            if (entityManager.TryGetEntity(player.SpaceshipPointer) != null) return;

            if (player.LivesLeft == 0)
            {
                player.LivesLeft = Config.DefaultPlayerLives - 1;
                player.Score = 0;
            }
            else player.LivesLeft--;

            var entity = entityManager.CreateEntity("Spaceship", player.id);

            entity.GetComponent<Collider>().SetCollisionLayer(CollisionLayersConfig.SpaceshipLayer);

            player.SpaceshipPointer = entity.Pointer;
        }

        internal void SpawnAsteroid(Vector2 spawnPos, int type)
        {
            string entityName;

            switch (type)
            {
                case 1:
                    entityName = "Asteroids/Asteroid1";
                    break;
                case 2:
                    entityName = "Asteroids/Asteroid2";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            var asteroid = matchManager.entityManager.CreateEntity(entityName, 0);

            var t = asteroid.GetComponent<Transform>();

            t.Position = spawnPos;

            asteroid.GetComponent<Collider>().SetCollisionLayer(CollisionLayersConfig.AsteroidLayer);
        }

        private void SpawnUfo(Vector2 pos)
        {
            var entity = matchManager.entityManager.CreateEntity("Ufo", 0);

            entity.GetComponent<Transform>().Position = pos;

            entity.GetComponent<Collider>().SetCollisionLayer(CollisionLayersConfig.AsteroidLayer);
        }

        internal void Update()
        {
            if (matchManager.entityManager.GetEntityPerNameCount("Ufo") < 3 &&
                matchManager.CurrentTick > Config.TicksPerSecond * 5 &&
                matchManager.CurrentTick - lastUfoSpawnTick > Config.TicksPerSecond * 10)
            {
                const long maxWidth = 271973;
                const long maxHeight = 157285;

                var rnd = matchManager.random;

                var spawnVariant = rnd.GetRandom(4);

                Vector2 spawnPos;

                if (spawnVariant == 0) //top
                {
                    spawnPos = new Vector2(rnd.GetRandom((int) maxWidth * 2) - maxWidth, maxHeight);
                }
                else if (spawnVariant == 1) //bottom
                {
                    spawnPos = new Vector2(rnd.GetRandom((int) maxWidth * 2) - maxWidth, -maxHeight);
                }
                else if (spawnVariant == 2) //left
                {
                    spawnPos = new Vector2(-maxWidth, rnd.GetRandom((int) maxHeight * 2) - maxHeight);
                }
                else //right
                {
                    spawnPos = new Vector2(maxWidth, rnd.GetRandom((int) maxHeight * 2) - maxHeight);
                }

                lastUfoSpawnTick = matchManager.CurrentTick;

                SpawnUfo(spawnPos);
            }

            //spawn new asteroids once a second
            if (matchManager.CurrentTick % Config.TicksPerSecond == 0)
            {
                if (matchManager.entityManager.GetEntityPerNameCount("Asteroids/Asteroid1") == 0 &&
                    matchManager.entityManager.GetEntityPerNameCount("Asteroids/Asteroid2") < 15)
                {
                    var rnd = matchManager.random;

                    var maxCount = (int) matchManager.CurrentTick / (Config.TicksPerSecond * 10) + 2;
                    if (maxCount > 5) maxCount = 5;

                    var asteroidsToCreateCount = rnd.GetRandom(maxCount) + 1;

                    for (var i = 0; i < asteroidsToCreateCount; i++)
                    {
                        const long maxWidth = 271973;
                        const long maxHeight = 157285;

                        var spawnVariant = rnd.GetRandom(4);

                        Vector2 spawnPos;

                        if (spawnVariant == 0) //top
                        {
                            spawnPos = new Vector2(rnd.GetRandom((int) maxWidth * 2) - maxWidth, maxHeight);
                        }
                        else if (spawnVariant == 1) //bottom
                        {
                            spawnPos = new Vector2(rnd.GetRandom((int) maxWidth * 2) - maxWidth, -maxHeight);
                        }
                        else if (spawnVariant == 2) //left
                        {
                            spawnPos = new Vector2(-maxWidth, rnd.GetRandom((int) maxHeight * 2) - maxHeight);
                        }
                        else //right
                        {
                            spawnPos = new Vector2(maxWidth, rnd.GetRandom((int) maxHeight * 2) - maxHeight);
                        }

                        SpawnAsteroid(spawnPos, 1);
                    }
                }
            }
        }
    }
}