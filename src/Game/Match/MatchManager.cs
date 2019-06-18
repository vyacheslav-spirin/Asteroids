namespace Asteroids.Game.Match
{
    public sealed class MatchManager
    {
        public bool IsFinished => CurrentTick >= Config.MaxMatchDurationTicks;

        public uint CurrentTick { get; private set; }

        internal readonly RandomGenerator random;

        public readonly EntityManager entityManager;

        public readonly PlayerManager playerManager;

        internal readonly PhysicsManager physicsManager;

        internal readonly GameModeManager gameModeManager;

        public MatchManager()
        {
            random = new RandomGenerator();

            entityManager = new EntityManager(this);

            playerManager = new PlayerManager(this);

            physicsManager = new PhysicsManager(this);

            gameModeManager = new GameModeManager(this);
        }

        public void Update()
        {
            CurrentTick++;

            entityManager.Update();

            physicsManager.Update();

            gameModeManager.Update();
        }

        public void ApplyCommand(uint playerId, Commands.Command command)
        {
            command.Apply(playerManager.GetPlayerById(playerId), this);
        }
    }
}