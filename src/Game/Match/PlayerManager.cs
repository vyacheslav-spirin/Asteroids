using System;
using System.Collections.Generic;

namespace Asteroids.Game.Match
{
    public sealed class PlayerManager
    {
        public int PlayerCount => players.Count;

        internal readonly MatchManager matchManager;

        private readonly List<Player> players = new List<Player>(Config.MaxPlayers);

        internal PlayerManager(MatchManager matchManager)
        {
            this.matchManager = matchManager;
        }

        internal void AddPlayer(uint id)
        {
            foreach (var existsPlayer in players)
            {
                if (existsPlayer.id == id) return;
            }

            var player = new Player(id);

            players.Add(player);

            matchManager.gameModeManager.OnPlayerConnect(player);
        }

        internal void RemovePlayer(uint id)
        {
            for (var i = 0; i < players.Count; i++)
            {
                var player = players[i];

                if (player.id == id)
                {
                    players.RemoveAt(i);

                    matchManager.gameModeManager.OnPlayerDisconnect(player);

                    return;
                }
            }
        }

        public Player GetPlayerByIndex(int index)
        {
            if (index < 0 || index >= players.Count) throw new ArgumentOutOfRangeException(nameof(index));

            return players[index];
        }

        public Player GetPlayerById(uint id)
        {
            foreach (var player in players)
            {
                if (player.id == id) return player;
            }

            return null;
        }
    }
}