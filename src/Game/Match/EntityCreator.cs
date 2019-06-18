using System;
using Asteroids.Game.Match.Components;
using Asteroids.Game.Match.Components.CollisionHandlers;
using Asteroids.Game.Match.Physics.Shapes;
using Asteroids.Math;

namespace Asteroids.Game.Match
{
    public sealed partial class Entity
    {
        // Can replaced by visual component editor

        private static readonly PolygonShape ExampleSpaceshipShape = new PolygonShape(new[]
        {
            new Vector2(6311, 0),

            new Vector2(3912, -1762),
            new Vector2(-360, -2162),
            new Vector2(-1841, -4666),
            new Vector2(-4692, -7457),
            new Vector2(-5603, -6435),
            new Vector2(-4921, -2162),

            new Vector2(-6973, 0),

            new Vector2(-4921, 2162),
            new Vector2(-5603, 6435),
            new Vector2(-4692, 7457),
            new Vector2(-1841, 4666),
            new Vector2(-360, 2162),
            new Vector2(3912, 1762)
        });

        private static readonly PolygonShape ExampleUfoShape = new PolygonShape(new[]
        {
            new Vector2(0, 16908),
            new Vector2(11862, 11862),
            new Vector2(16908, 0),
            new Vector2(-1179, -3034),
            new Vector2(11862, -11862),
            new Vector2(0, -16908),
            new Vector2(-11862, -11862),
            new Vector2(-16908, 0),
            new Vector2(-11862, 11862)
        });

        private static readonly PolygonShape ExampleAsteroid1Shape = new PolygonShape(new[]
        {
            new Vector2(5963, 17170),
            new Vector2(14286, 16252),
            new Vector2(18546, 15073),
            new Vector2(23068, 2293),
            new Vector2(9306, -16777),
            new Vector2(-10616, -18939),
            new Vector2(-20578, -8781),
            new Vector2(-18677, 3211),
            new Vector2(-18415, 9961),
            new Vector2(-12845, 18939),
            new Vector2(-6094, 21364)
        });

        private static readonly PolygonShape ExampleBulletShape = new PolygonShape(new[]
        {
            new Vector2(-1179, 3034),
            new Vector2(1179, 3034),
            new Vector2(1179, -3034),
            new Vector2(-1179, -3034)
        });

        private static readonly PolygonShape ExampleAsteroid2Shape;

        //Example shapes.
        static Entity()
        {
            var vertices = new Vector2[ExampleAsteroid1Shape.vertices.Length];

            for (var i = 0; i < vertices.Length; i++)
            {
                vertices[i] = ExampleAsteroid1Shape.vertices[i] * 45875L; //0.4
            }

            ExampleAsteroid2Shape = new PolygonShape(vertices);
        }


        public static Entity CreateNewEntityInstance(EntityManager entityManager, string name)
        {
            Entity entity = null;

            if (name == "Spaceship")
            {
                entity = new Entity(entityManager, name, 4);

                entity.components[0] = new BorderTransform(entity, new Vector2(248381, 140247)); // 3.79, 2.14
                entity.components[1] = new Collider(entity, ExampleSpaceshipShape);
                entity.components[2] = new SpaceshipCollisionHandler(entity);
                entity.components[3] = new SpaceshipController(entity);
            }
            else if (name == "Asteroids/Asteroid1")
            {
                entity = new Entity(entityManager, name, 3);

                entity.components[0] = new BorderTransform(entity, new Vector2(271974, 157286)); // 4.15, 2.4
                entity.components[1] = new Collider(entity, ExampleAsteroid1Shape);
                entity.components[2] = new AsteroidController(entity, FixedMath.One / 30, FixedMath.One / 25, FixedMath.Pi / 200, FixedMath.Pi / 150, 2);
            }
            else if (name == "Asteroids/Asteroid2")
            {
                entity = new Entity(entityManager, name, 3);

                entity.components[0] = new BorderTransform(entity, new Vector2(247726, 146800)); // 3.78, 2.24
                entity.components[1] = new Collider(entity, ExampleAsteroid2Shape);
                entity.components[2] = new AsteroidController(entity, FixedMath.One / 15, FixedMath.One / 10, FixedMath.Pi / 120, FixedMath.Pi / 70, 0);
            }
            else if (name == "Bullet")
            {
                entity = new Entity(entityManager, name, 4);

                entity.components[0] = new BorderTransform(entity, new Vector2(241172, 136970)); // 3.68, 2.09
                entity.components[1] = new Collider(entity, ExampleBulletShape);
                entity.components[2] = new BulletCollisionHandler(entity);
                entity.components[3] = new BulletController(entity);
            }
            else if (name == "Laser")
            {
                entity = new Entity(entityManager, name, 1);
                entity.components[0] = new LaserController(entity);
            }
            else if (name == "Ufo")
            {
                entity = new Entity(entityManager, name, 3);
                entity.components[0] = new BorderTransform(entity, new Vector2(271974, 157286)); // 4.15, 2.4
                entity.components[1] = new Collider(entity, ExampleUfoShape);
                entity.components[2] = new UfoController(entity);
            }

            if (entity == null) throw new Exception("Could not create entity by name: " + name);

            return entity;
        }
    }
}