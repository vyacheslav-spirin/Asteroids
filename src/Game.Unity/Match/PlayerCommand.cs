using Asteroids.Game.Match;

namespace Asteroids.Game.Unity.Match
{
    internal struct PlayerCommand
    {
        internal uint playerId;

        internal Commands.Command command;
    }
}