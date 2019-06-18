namespace Asteroids.Game.Unity.Match.Multiplayer.Protocol
{
    internal enum PacketType : byte
    {
        JoinToMatch,

        Sync,

        PlayerCommand
    }
}