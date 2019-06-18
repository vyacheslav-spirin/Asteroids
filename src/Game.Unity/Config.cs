namespace Asteroids.Game.Unity
{
    internal static class Config
    {
        internal const uint CommandsHistoryBufferSize = 30 * Game.Config.MaxPlayers * Game.Config.MaxMatchDurationTicks;
    }
}