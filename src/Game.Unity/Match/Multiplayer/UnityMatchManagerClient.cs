using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Asteroids.Game.Match;
using Asteroids.Game.Unity.Match.Multiplayer.Activities;
using Asteroids.Game.Unity.Match.Multiplayer.Protocol;
using Asteroids.Game.Unity.Network;
using UnityEngine;

namespace Asteroids.Game.Unity.Match.Multiplayer
{
    internal sealed class UnityMatchManagerClient : UnityMatchManager
    {
        internal float LastDataReceiveTime { get; private set; }

        internal bool ServerIsFull { get; private set; }

        internal readonly string serverAddress;

        private readonly UdpNetwork udpNetwork;

        private readonly byte[] receiveBuffer = new byte[UdpNetwork.Mtu];
        private readonly byte[] sendBuffer = new byte[UdpNetwork.Mtu];
        private readonly BinaryWriter packetWriter;
        private readonly BinaryReader packetReader;

        private readonly List<PlayerCommand> nextTickCommands = new List<PlayerCommand>(Game.Config.MaxPlayers + 10);
        private bool isReceivedNextTickCommands = true; //first commands is fake (always empty on client and server)

        private float packetLastSendTime;

        private uint lastPacketIndex;

        private float elapsedTimeAfterLastTick = TimeBetweenTicks;

        internal UnityMatchManagerClient(string serverAddress)
        {
            this.serverAddress = serverAddress;

            udpNetwork = new UdpNetwork(1024, 100 * 1024);

            udpNetwork.lastWriteRemoteIpEndPoint.Address = IPAddress.Parse(serverAddress);
            udpNetwork.lastWriteRemoteIpEndPoint.Port = ProtocolConfig.Port;
            udpNetwork.InitAsClient();

            packetWriter = new BinaryWriter(new MemoryStream(sendBuffer), Encoding.UTF8);
            packetReader = new BinaryReader(new MemoryStream(receiveBuffer), Encoding.UTF8);

            userInterfaceManager.RegisterActivity(new NetworkActivity(this, userInterfaceManager.canvasObject.transform.Find("NetworkActivity").gameObject));
        }

        internal override void OnDestroy()
        {
            base.OnDestroy();

            udpNetwork.Dispose();

            packetWriter.Dispose();
            packetReader.Dispose();
        }

        private void SendJoinToMatchPacket()
        {
            packetWriter.BaseStream.Position = 0;

            packetWriter.Write((byte) PacketType.JoinToMatch);

            udpNetwork.Send(sendBuffer, 0, (int) packetWriter.BaseStream.Position);
        }

        private uint GetLastReceivedTick()
        {
            return isReceivedNextTickCommands ? matchManager.CurrentTick : matchManager.CurrentTick - 1;
        }

        private void AddSyncBodyToOutgoingPacket()
        {
            packetWriter.Write(++lastPacketIndex);

            packetWriter.Write(GetLastReceivedTick());
        }

        private void SendSyncPacket()
        {
            packetWriter.BaseStream.Position = 0;

            packetWriter.Write((byte) PacketType.Sync);

            AddSyncBodyToOutgoingPacket();

            udpNetwork.Send(sendBuffer, 0, (int) packetWriter.BaseStream.Position);

            packetLastSendTime = Time.unscaledTime;
        }

        protected override void SendLocalPlayerCommand(Commands.Command command, bool isReliable)
        {
            //TODO: add reliable commands support

            packetWriter.BaseStream.Position = 0;

            packetWriter.Write((byte) PacketType.PlayerCommand);

            AddSyncBodyToOutgoingPacket();

            Commands.PackCommand(command, packetWriter);

            udpNetwork.Send(sendBuffer, 0, (int) packetWriter.BaseStream.Position);

            packetLastSendTime = Time.unscaledTime;
        }

        private void ParseIncomingSyncPacket()
        {
            var startTickInPacket = packetReader.ReadUInt32();

            var fragmentsCount = packetReader.ReadUInt16();

            var endTickInPacket = startTickInPacket + fragmentsCount - 1;

            var lastReceivedTick = GetLastReceivedTick();

            var minRequiredTick = lastReceivedTick + 1;

            if (endTickInPacket <= lastReceivedTick || startTickInPacket > minRequiredTick) return;

            //apply nextTickCommands and update match manager if commands exists
            if (isReceivedNextTickCommands)
            {
                //elapsedTimeAfterLastTick = 0;

                foreach (var nextTickCommand in nextTickCommands)
                {
                    matchManager.ApplyCommand(nextTickCommand.playerId, nextTickCommand.command);
                }

                nextTickCommands.Clear();

                matchManager.Update();
            }

            isReceivedNextTickCommands = true;

            for (var i = 0; i < fragmentsCount; i++)
            {
                var commandsCount = packetReader.ReadByte();

                for (var j = 0; j < commandsCount; j++)
                {
                    var playerId = packetReader.ReadUInt32();

                    var command = Commands.UnpackCommand(packetReader);

                    if (startTickInPacket + i <= lastReceivedTick) continue;

                    //if non last fragment
                    if (i + 1 < fragmentsCount)
                    {
                        matchManager.ApplyCommand(playerId, command);
                    }
                    else
                    {
                        nextTickCommands.Add(new PlayerCommand
                        {
                            playerId = playerId,
                            command = command
                        });
                    }
                }

                if (startTickInPacket + i <= lastReceivedTick) continue;

                //if non last fragment
                if (i + 1 < fragmentsCount)
                {
                    matchManager.Update();
                }
            }
        }

        private void ProcessIncomingPackets()
        {
            while (udpNetwork.TryRead(receiveBuffer, 0, out var length))
            {
                packetReader.BaseStream.Position = 0;
                //TODO: add read limit

                LastDataReceiveTime = Time.unscaledTime;

                try
                {
                    var packetType = (PacketType) packetReader.ReadByte();

                    switch (packetType)
                    {
                        case PacketType.JoinToMatch:

                            if (localPlayerId != 0) continue;

                            localPlayerId = packetReader.ReadUInt32();

                            ServerIsFull = localPlayerId == 0;

                            SendSyncPacket();

                            break;
                        case PacketType.Sync:

                            ParseIncomingSyncPacket();

                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Incoming packet processing error! Details: " + e);
                }
            }
        }

        internal override void Update()
        {
            ProcessIncomingPackets();

            if (localPlayerId == 0)
            {
                if (Time.unscaledTime - packetLastSendTime > 0.5f)
                {
                    packetLastSendTime = Time.unscaledTime;

                    SendJoinToMatchPacket();
                }

                userInterfaceManager.Update();

                return;
            }

            if (Time.unscaledTime - packetLastSendTime > 0.05f)
            {
                packetLastSendTime = Time.unscaledTime;

                SendSyncPacket();
            }

            elapsedTimeAfterLastTick += Time.deltaTime;

            if (elapsedTimeAfterLastTick > TimeBetweenTicks)
            {
                if (isReceivedNextTickCommands)
                {
                    elapsedTimeAfterLastTick -= TimeBetweenTicks;

                    isReceivedNextTickCommands = false;

                    foreach (var nextTickCommand in nextTickCommands)
                    {
                        matchManager.ApplyCommand(nextTickCommand.playerId, nextTickCommand.command);
                    }

                    nextTickCommands.Clear();

                    matchManager.Update();
                }
                else
                {
                    elapsedTimeAfterLastTick = TimeBetweenTicks;
                }
            }

            ticksLerp = elapsedTimeAfterLastTick / TimeBetweenTicks;

            base.Update();
        }
    }
}