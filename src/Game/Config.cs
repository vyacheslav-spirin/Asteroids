namespace Asteroids.Game
{
    public static class Config
    {
        public const int DefaultPlayerLives = 5;

        public const int MaxActiveEntities = 500;

        public const int MaxPlayers = 4;

        public const int TicksPerSecond = 20;

        public const uint MaxMatchDurationTicks = TicksPerSecond * 3600;
    }
}