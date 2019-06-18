using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Asteroids.Game.Match;
using Asteroids.Game.Unity.Match.Multiplayer.Protocol;
using Asteroids.Game.Unity.Network;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Asteroids.Game.Unity.Match.Multiplayer
{
    internal sealed class UnityMatchManagerServer : UnityMatchManager
    {
        private class Client
        {
            internal bool IsLocal => lastDataReceiveTime <= 0;

            internal readonly uint id;

            internal readonly IPEndPoint remoteIpEndPoint;

            internal uint lastPacketIndex;

            internal uint lastReceivedTick;

            private float lastDataReceiveTime;

            internal Client(uint id, IPEndPoint remoteIpEndPoint, bool isLocal = false)
            {
                this.id = id;

                this.remoteIpEndPoint = remoteIpEndPoint;

                lastDataReceiveTime = isLocal ? -1 : Time.unscaledTime;
            }

            internal void UpdateDataReceiveTime()
            {
                if (IsLocal) return;

                lastDataReceiveTime = Time.unscaledTime;
            }

            internal bool IsAlive()
            {
                return IsLocal || Time.unscaledTime - lastDataReceiveTime < ProtocolConfig.ClientTimeout;
            }
        }

        private readonly UdpNetwork udpNetwork;

        private readonly byte[] receiveBuffer = new byte[UdpNetwork.Mtu];
        private readonly byte[] sendBuffer = new byte[UdpNetwork.Mtu];
        private readonly BinaryWriter packetWriter;
        private readonly BinaryReader packetReader;

        //TODO: global history buffer can be replaced to last snapshot + last history buffer
        private readonly byte[] commandsHistoryBuffer = new byte[Config.CommandsHistoryBufferSize];
        private readonly List<uint> commandsHistoryFragmentsStart = new List<uint>((int) Game.Config.MaxMatchDurationTicks);
        private readonly BinaryWriter historyWriter;

        private readonly List<PlayerCommand> futureCommands = new List<PlayerCommand>(10 + Game.Config.MaxPlayers);

        private readonly List<PlayerCommand> nextTickCommands = new List<PlayerCommand>(10 + Game.Config.MaxPlayers);

        private readonly List<Client> clients = new List<Client>(Game.Config.MaxPlayers);

        private uint playerIdSequence;

        private float elapsedTimeAfterLastTick = TimeBetweenTicks;

        internal UnityMatchManagerServer()
        {
            udpNetwork = new UdpNetwork(1024 * Game.Config.MaxPlayers, 1024 * Game.Config.MaxPlayers);

            udpNetwork.InitAsServer(ProtocolConfig.Port);

            localPlayerId = ++playerIdSequence;

            var client = new Client(localPlayerId, null, true);
            clients.Add(client);

            packetWriter = new BinaryWriter(new MemoryStream(sendBuffer), Encoding.UTF8);
            packetReader = new BinaryReader(new MemoryStream(receiveBuffer), Encoding.UTF8);

            historyWriter = new BinaryWriter(new MemoryStream(commandsHistoryBuffer), Encoding.UTF8);

            futureCommands.Add(new PlayerCommand
            {
                playerId = 0,
                command = new Commands.SetRandomSeed {seed = (uint) Random.Range(0, 1000000)}
            });

            futureCommands.Add(new PlayerCommand
            {
                playerId = 0,
                command = new Commands.AddPlayer {id = localPlayerId}
            });
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();

            udpNetwork.Dispose();

            packetWriter.Dispose();
            packetReader.Dispose();

            historyWriter.Dispose();
        }

        private Client ReadIncomingPacketSyncBodyAndGetValidClient()
        {
            Client client = null;

            foreach (var c in clients)
            {
                if (c.IsLocal) continue;

                if (c.remoteIpEndPoint.Equals(udpNetwork.lastReadRemoteIpEndPoint))
                {
                    client = c;

                    break;
                }
            }

            if (client == null) return null;

            var packetIndex = packetReader.ReadUInt32();

            if (client.lastPacketIndex >= packetIndex) return null;

            client.lastPacketIndex = packetIndex;

            var lastReceivedTick = packetReader.ReadUInt32();

            if (lastReceivedTick < client.lastReceivedTick || lastReceivedTick > commandsHistoryFragmentsStart.Count)
            {
                //TODO: add kick

                return null;
            }

            client.lastReceivedTick = lastReceivedTick;

            client.UpdateDataReceiveTime();

            return client;
        }

        private void SendCommandsToClient(Client client)
        {
            if (client.lastReceivedTick == commandsHistoryFragmentsStart.Count) return;

            packetWriter.BaseStream.Position = 0;

            packetWriter.Write((byte) PacketType.Sync);

            //start tick in packet
            packetWriter.Write(client.lastReceivedTick + 1);

            var fragmentsCountPos = packetWriter.BaseStream.Position;
            packetWriter.BaseStream.Position += sizeof(ushort);

            ushort fragmentsCount = 0;

            for (var i = (int) (client.lastReceivedTick + 1); i <= commandsHistoryFragmentsStart.Count; i++)
            {
                var fragmentStartPos = commandsHistoryFragmentsStart[i - 1];

                var fragmentEndPos = i == commandsHistoryFragmentsStart.Count ? historyWriter.BaseStream.Position : commandsHistoryFragmentsStart[i];

                var fragmentSize = fragmentEndPos - fragmentStartPos;

                if (fragmentSize > sendBuffer.Length - packetWriter.BaseStream.Position) break;

                Buffer.BlockCopy(commandsHistoryBuffer, (int) fragmentStartPos, sendBuffer, (int) packetWriter.BaseStream.Position, (int) fragmentSize);

                packetWriter.BaseStream.Position += fragmentSize;

                fragmentsCount++;
            }

            var endPos = packetWriter.BaseStream.Position;

            packetWriter.BaseStream.Position = fragmentsCountPos;

            packetWriter.Write(fragmentsCount);

            udpNetwork.ApplyEndPointToWriter(client.remoteIpEndPoint);
            udpNetwork.Send(sendBuffer, 0, (int) endPos);
        }

        private void ProcessIncomingPackets()
        {
            while (udpNetwork.TryRead(receiveBuffer, 0, out var length))
            {
                packetReader.BaseStream.Position = 0;
                //TODO: add read limit

                try
                {
                    var packetType = (PacketType) packetReader.ReadByte();

                    Client existsClient;

                    switch (packetType)
                    {
                        case PacketType.JoinToMatch:

                            packetWriter.BaseStream.Position = 0;

                            packetWriter.Write((byte) PacketType.JoinToMatch);

                            packetWriter.Write(0u);

                            var isClientExists = false;

                            foreach (var client in clients)
                            {
                                if (client.IsLocal) continue;

                                if (client.remoteIpEndPoint.Equals(udpNetwork.lastReadRemoteIpEndPoint))
                                {
                                    packetWriter.BaseStream.Position -= sizeof(uint);
                                    packetWriter.Write(client.id);

                                    isClientExists = true;

                                    break;
                                }
                            }

                            if (clients.Count < Game.Config.MaxPlayers && !isClientExists)
                            {
                                var client = new Client(++playerIdSequence,
                                    new IPEndPoint(new IPAddress(udpNetwork.lastReadRemoteIpEndPoint.Address.GetAddressBytes()),
                                        udpNetwork.lastReadRemoteIpEndPoint.Port));

                                clients.Add(client);

                                packetWriter.BaseStream.Position -= sizeof(uint);
                                packetWriter.Write(client.id);

                                futureCommands.Add(new PlayerCommand
                                {
                                    playerId = 0,
                                    command = new Commands.AddPlayer {id = client.id}
                                });
                            }

                            udpNetwork.SendToLastClient(sendBuffer, 0, (int) packetWriter.BaseStream.Position);

                            break;
                        case PacketType.Sync:

                            existsClient = ReadIncomingPacketSyncBodyAndGetValidClient();
                            if (existsClient == null) continue;

                            SendCommandsToClient(existsClient);

                            break;
                        case PacketType.PlayerCommand:

                            existsClient = ReadIncomingPacketSyncBodyAndGetValidClient();
                            if (existsClient == null) continue;

                            SendCommandsToClient(existsClient);

                            ParseIncomingPlayerCommand(existsClient);

                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Incoming packet processing error! Details: " + e);
                }
            }
        }

        private void ParseIncomingPlayerCommand(Client client)
        {
            foreach (var futureCommand in futureCommands)
            {
                if (futureCommand.playerId == client.id) return;
            }

            var command = Commands.UnpackCommand(packetReader);

            futureCommands.Add(new PlayerCommand
            {
                playerId = client.id,
                command = command
            });
        }

        private void PrepareAndSendNextTickCommands()
        {
            commandsHistoryFragmentsStart.Add((uint) historyWriter.BaseStream.Position);

            var commandsCountPos = historyWriter.BaseStream.Position;
            historyWriter.BaseStream.Position++;

            foreach (var futureCommand in futureCommands)
            {
                historyWriter.Write(futureCommand.playerId);

                Commands.PackCommand(futureCommand.command, historyWriter);

                nextTickCommands.Add(futureCommand);
            }

            commandsHistoryBuffer[commandsCountPos] = (byte) nextTickCommands.Count;

            futureCommands.Clear();

            foreach (var client in clients)
            {
                if (!client.IsLocal) SendCommandsToClient(client);
            }
        }

        protected override void SendLocalPlayerCommand(Commands.Command command, bool isReliable)
        {
            foreach (var futureCommand in futureCommands)
            {
                if (futureCommand.playerId == localPlayerId) return;
            }

            futureCommands.Add(new PlayerCommand
            {
                playerId = localPlayerId,
                command = command
            });
        }

        internal override void Update()
        {
            ProcessIncomingPackets();

            for (var i = 0; i < clients.Count; i++)
            {
                var client = clients[i];

                if (!client.IsAlive())
                {
                    futureCommands.Add(new PlayerCommand
                    {
                        playerId = 0,
                        command = new Commands.RemovePlayer {id = client.id}
                    });

                    clients.RemoveAt(i);
                }
            }

            elapsedTimeAfterLastTick += Time.deltaTime;

            while (elapsedTimeAfterLastTick >= TimeBetweenTicks && !matchManager.IsFinished)
            {
                elapsedTimeAfterLastTick -= TimeBetweenTicks;

                //apply commands for current tick then update current tick

                ApplyCurrentTickCommands();

                PrepareAndSendNextTickCommands();

                matchManager.Update();
            }

            if (matchManager.IsFinished) return;

            ticksLerp = elapsedTimeAfterLastTick / TimeBetweenTicks;

            base.Update();
        }

        private void ApplyCurrentTickCommands()
        {
            foreach (var nextTickCommand in nextTickCommands)
            {
                matchManager.ApplyCommand(nextTickCommand.playerId, nextTickCommand.command);
            }

            nextTickCommands.Clear();
        }
    }
}