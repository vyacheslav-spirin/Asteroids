namespace Asteroids.Game.Match.Physics
{
    public static class CollisionLayersConfig
    {
        public const byte DefaultLayer = 0;
        public const byte SpaceshipLayer = 1;
        public const byte AsteroidLayer = 2;
        public const byte WeaponLayer = 3;


        public const int MaxCollisionLayers = 4;
    }
}