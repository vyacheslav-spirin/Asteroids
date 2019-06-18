namespace Asteroids.Game.Unity
{
    //Temp. Can replaced by visual editor

    internal static class ExamplePoolConfig
    {
        internal struct PoolItem
        {
            internal readonly string name;

            internal readonly int count;

            internal PoolItem(string name, int count)
            {
                this.name = name;

                this.count = count;
            }
        }

        internal static readonly PoolItem[] Pools =
        {
            new PoolItem("Asteroids/Asteroid1", 15),
            new PoolItem("Asteroids/Asteroid2", 40),
            new PoolItem("Spaceship", Game.Config.MaxPlayers),
            new PoolItem("Bullet", 10 * Game.Config.MaxPlayers),
            new PoolItem("Laser", Game.Config.MaxPlayers),
            new PoolItem("Ufo", 3)
        };
    }
}