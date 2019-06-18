using System.IO;
using Asteroids.Game.Match.Components;

namespace Asteroids.Game.Match
{
    public static class Commands
    {
        public enum CommandType : byte
        {
            SetRandomSeed,
            AddPlayer,
            RemovePlayer,
            Respawn,
            SetSpaceshipControls
        }

        public abstract class Command
        {
            internal abstract void Apply(Player player, MatchManager matchManager);

            internal abstract void SaveState(BinaryWriter writer);

            internal abstract void LoadState(BinaryReader reader);
        }

        public sealed class SetRandomSeed : Command
        {
            public uint seed;

            internal override void Apply(Player player, MatchManager matchManager)
            {
                if (player == null) matchManager.random.SetSeed(seed);
            }

            internal override void SaveState(BinaryWriter writer)
            {
                writer.Write((byte) CommandType.SetRandomSeed);

                writer.Write(seed);
            }

            internal override void LoadState(BinaryReader reader)
            {
                seed = reader.ReadUInt32();
            }
        }

        public sealed class AddPlayer : Command
        {
            public uint id;

            internal override void Apply(Player player, MatchManager matchManager)
            {
                if (player == null) matchManager.playerManager.AddPlayer(id);
            }

            internal override void SaveState(BinaryWriter writer)
            {
                writer.Write((byte) CommandType.AddPlayer);

                writer.Write(id);
            }

            internal override void LoadState(BinaryReader reader)
            {
                id = reader.ReadUInt32();
            }
        }

        public sealed class RemovePlayer : Command
        {
            public uint id;

            internal override void Apply(Player player, MatchManager matchManager)
            {
                if (player == null) matchManager.playerManager.RemovePlayer(id);
            }

            internal override void SaveState(BinaryWriter writer)
            {
                writer.Write((byte) CommandType.RemovePlayer);

                writer.Write(id);
            }

            internal override void LoadState(BinaryReader reader)
            {
                id = reader.ReadUInt32();
            }
        }

        public sealed class Respawn : Command
        {
            internal override void Apply(Player player, MatchManager matchManager)
            {
                matchManager.gameModeManager.Respawn(player);
            }

            internal override void SaveState(BinaryWriter writer)
            {
                writer.Write((byte) CommandType.Respawn);
            }

            internal override void LoadState(BinaryReader reader)
            {
            }
        }

        public sealed class SetSpaceshipControls : Command
        {
            public EntityPointer shipEntityPointer;

            public byte controls;

            internal override void Apply(Player player, MatchManager matchManager)
            {
                var entity = matchManager.entityManager.TryGetEntity(shipEntityPointer);

                if (entity == null || entity.Owner != player.id) return;

                var spaceshipController = entity.GetComponent<SpaceshipController>();

                spaceshipController?.SetControls(controls);
            }

            internal override void SaveState(BinaryWriter writer)
            {
                writer.Write((byte) CommandType.SetSpaceshipControls);

                EntityPointer.Save(shipEntityPointer, writer);

                writer.Write(controls);
            }

            internal override void LoadState(BinaryReader reader)
            {
                shipEntityPointer = EntityPointer.Load(reader);

                controls = reader.ReadByte();
            }
        }

        public static void PackCommand(Command command, BinaryWriter writer)
        {
            command.SaveState(writer);
        }

        public static Command UnpackCommand(BinaryReader reader)
        {
            var commandType = (CommandType) reader.ReadByte();

            Command command;

            switch (commandType)
            {
                case CommandType.SetRandomSeed:
                    command = new SetRandomSeed();
                    break;
                case CommandType.AddPlayer:
                    command = new AddPlayer();
                    break;
                case CommandType.RemovePlayer:
                    command = new RemovePlayer();
                    break;
                case CommandType.Respawn:
                    command = new Respawn();
                    break;
                case CommandType.SetSpaceshipControls:
                    command = new SetSpaceshipControls();
                    break;
                default:
                    return null;
            }

            command.LoadState(reader);

            return command;
        }
    }
}