using System;

namespace Asteroids.Game.Match
{
    public sealed class Player
    {
        public EntityPointer SpaceshipPointer { get; internal set; }

        public int Score { get; internal set; }

        public int LivesLeft { get; internal set; } = Config.DefaultPlayerLives;

        public readonly uint id;

        internal Player(uint id)
        {
            if (id == 0) throw new ArgumentException(nameof(id));

            this.id = id;
        }
    }
}