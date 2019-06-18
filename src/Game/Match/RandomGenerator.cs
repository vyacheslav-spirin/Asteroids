namespace Asteroids.Game.Match
{
    internal sealed class RandomGenerator
    {
        private const uint Y = 842502087, Z = 3579807591, W = 273326509;

        private uint y = Y, z = Z, w = W, s;

        internal void SetSeed(uint value)
        {
            s = value;
        }

        internal int GetRandom(int maxNonInclusiveValue = int.MaxValue)
        {
            if (maxNonInclusiveValue == 0) return 0;

            var t = s ^ (s << 11);
            s = y;
            y = z;
            z = w;

            return (int) ((0x7FFFFFFF & (w = w ^ (w >> 19) ^ t ^ (t >> 8))) % maxNonInclusiveValue);
        }
    }
}