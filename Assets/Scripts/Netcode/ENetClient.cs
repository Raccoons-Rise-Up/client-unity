/*
 * Kittens Rise Up is a long term progression MMORPG.
 * Copyright (C) 2021  valkyrienyanko
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 * 
 * Contact valkyrienyanko by joining the Kittens Rise Up discord at
 * https://discord.gg/cDNf8ja or email sebastianbelle074@protonmail.com
 */

using ENet;
using EventType = ENet.EventType;  // fixes CS0104 ambigous reference between the same thing in UnityEngine
using Event = ENet.Event;          // fixes CS0104 ambigous reference between the same thing in UnityEngine

using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

using Common.Networking.Packet;
using Common.Networking.IO;
using TMPro;
using KRU.Game;

namespace KRU.Networking 
{
    public class ENetClient : MonoBehaviour
    {
        // Unity Inspector Variables
        public string ip = "127.0.0.1";
        public ushort port = 25565;

        public Transform menuTranform;
        private UIMenu menuScript;

        public Transform loginTransform;
        private UILogin loginScript;

        public Transform terminalTransform;
        private UITerminal terminalScript;

        private readonly ConcurrentQueue<UnityInstruction> unityInstructions = new ConcurrentQueue<UnityInstruction>(); // Need a way to communicate with the Unity thread from the ENet thread
        private readonly ConcurrentQueue<ENetInstruction> ENetInstructions = new ConcurrentQueue<ENetInstruction>(); // Need a way to communicate with the ENet thread from the Unity thread
        private readonly ConcurrentQueue<ClientPacket> outgoing = new ConcurrentQueue<ClientPacket>(); // The packets that are sent to the server

        private const byte channelID = 0; // The channel all networking traffic will be going through
        private const int maxFrames = 30; // The games FPS cap

        private readonly uint pingInterval = 1000; // Pings are used both to monitor the liveness of the connection and also to dynamically adjust the throttle during periods of low traffic so that the throttle has reasonable responsiveness during traffic spikes.
        private readonly uint timeout = 5000; // Will be ignored if maximum timeout is exceeded
        private readonly uint timeoutMinimum = 5000; // The timeout for server not sending the packet to the client sent from the server
        private readonly uint timeoutMaximum = 5000; // The timeout for server not receiving the packet sent from the client

        private Peer peer;

        private Thread workerThread;
        private bool runningNetCode;
        private bool tryingToConnect;
        private bool connectedToServer;

        private TMP_InputField inputField;

        private void Start()
        {
            Application.targetFrameRate = maxFrames;
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);

            menuScript = menuTranform.GetComponent<UIMenu>();
            loginScript = loginTransform.GetComponent<UILogin>();
            terminalScript = terminalTransform.GetComponent<UITerminal>();

            // Make sure queues are completely drained before starting
            if (outgoing != null) while (outgoing.TryDequeue(out _)) ;
            if (unityInstructions != null) while (unityInstructions.TryDequeue(out _)) ;
            if (ENetInstructions != null) while (ENetInstructions.TryDequeue(out _)) ;
        }

        public void Connect() 
        {
            if (tryingToConnect || connectedToServer)
                return;

            tryingToConnect = true;
            workerThread = new Thread(ThreadWorker);
            workerThread.Start();
        }

        public void Disconnect() 
        {
            ENetInstructions.Enqueue(ENetInstruction.CancelConnection);
        }

        public bool IsConnected() => connectedToServer;

        private void ThreadWorker() 
        {
            Library.Initialize();

            using Host client = new Host();
            var address = new Address();
            address.SetHost(ip);
            address.Port = port;
            client.Create();

            peer = client.Connect(address);
            peer.PingInterval(pingInterval);
            peer.Timeout(timeout, timeoutMinimum, timeoutMaximum);
            Debug.Log("Attempting to connect...");

            runningNetCode = true;
            while (runningNetCode)
            {
                var polled = false;

                // ENet Instructions (from Unity Thread)
                while (ENetInstructions.TryDequeue(out ENetInstruction result))
                {
                    if (result == ENetInstruction.CancelConnection)
                    {
                        Debug.Log("Cancel connection");
                        connectedToServer = false;
                        tryingToConnect = false;
                        runningNetCode = false;
                        break;
                    }
                }

                // Sending data
                while (outgoing.TryDequeue(out ClientPacket clientPacket)) 
                {
                    switch (clientPacket.Opcode) 
                    {
                        case ClientPacketType.Login:
                            Debug.Log("Sending login request to game server..");

                            Send(clientPacket, PacketFlags.Reliable);

                            break;
                        case ClientPacketType.PurchaseItem:
                            Debug.Log("Sending purchase item request to game server..");

                            Send(clientPacket, PacketFlags.Reliable);

                            break;
                    }
                }

                // Receiving Data
                while (!polled)
                {
                    if (client.CheckEvents(out Event netEvent) <= 0)
                    {
                        if (client.Service(15, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            Debug.Log("Nothing");
                            break;

                        case EventType.Connect:
                            // Successfully connected to the game server
                            Debug.Log("Client connected to game server");

                            // Send login request
                            var clientPacket = new ClientPacket(ClientPacketType.Login, new PacketLogin(loginScript.username));

                            outgoing.Enqueue(clientPacket);

                            // Keep track of networking logic
                            tryingToConnect = false;
                            connectedToServer = true;

                            // Load the main game 'scene'
                            unityInstructions.Enqueue(new UnityInstruction { type = UnityInstruction.Type.LoadMainScene });
                            break;

                        case EventType.Disconnect:
                            Debug.Log("Client disconnected from server");
                            connectedToServer = false;
                            break;

                        case EventType.Timeout:
                            Debug.Log("Client connection timeout to game server");
                            tryingToConnect = false;
                            connectedToServer = false;
                            unityInstructions.Enqueue(new UnityInstruction { type = UnityInstruction.Type.NotifyUserOfTimeout });
                            unityInstructions.Enqueue(new UnityInstruction { type = UnityInstruction.Type.LoadSceneForDisconnectTimeout });
                            break;

                        case EventType.Receive:
                            var packet = netEvent.Packet;
                            Debug.Log("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + packet.Length);

                            var readBuffer = new byte[1024];
                            var readStream = new MemoryStream(readBuffer);
                            var reader = new BinaryReader(readStream);

                            readStream.Position = 0;
                            netEvent.Packet.CopyTo(readBuffer);

                            var opcode = (ServerPacketType)reader.ReadByte();

                            if (opcode == ServerPacketType.PurchasedItem) 
                            {
                                var data = new PacketPurchasedItem();
                                var packetReader = new PacketReader(readBuffer);
                                data.Read(packetReader);

                                unityInstructions.Enqueue(new UnityInstruction { 
                                    type = UnityInstruction.Type.LogMessage,
                                    Message = $"You purchased item: {data.itemId} for x gold." 
                                });
                            }

                            packet.Dispose();
                            break;
                    }
                }
            }

            client.Flush();
            client.Dispose();

            Library.Deinitialize();
        }

        private void Update()
        {
            if (!runningNetCode)
                return;

            while (unityInstructions.TryDequeue(out UnityInstruction result)) 
            {
                switch (result.type) 
                {
                    case UnityInstruction.Type.NotifyUserOfTimeout:
                        loginScript.btnConnect.interactable = true;
                        loginScript.webServerResponseText.text = "Client connection timeout to game server";
                        break;
                    case UnityInstruction.Type.LogMessage:
                        terminalScript.Log(result.Message);
                        break;
                    case UnityInstruction.Type.LoadSceneForDisconnectTimeout:
                        menuScript.LoadTimeoutDisconnectScene();
                        menuScript.gameScript.inGame = false;
                        break;
                    case UnityInstruction.Type.LoadMainScene:
                        menuScript.FromConnectingToMainScene();
                        loginScript.webServerResponseText.text = "";
                        loginScript.btnConnect.interactable = true;
                        menuScript.gameScript.inGame = true;
                        break;
                }
            }
        }

        public void PurchaseItem(int itemId) 
        {
            var data = new PacketPurchaseItem((ushort)itemId);
            var clientPacket = new ClientPacket(ClientPacketType.PurchaseItem, data);

            outgoing.Enqueue(clientPacket);
        }

        private void Send(GamePacket gamePacket, PacketFlags packetFlags)
        {
            var packet = default(Packet);
            packet.Create(gamePacket.Data, packetFlags);
            peer.Send(channelID, ref packet);
        }
    }

    public class UnityInstruction 
    {
        public enum Type 
        {
            LoadSceneForDisconnectTimeout,
            LoadMainScene,
            LogMessage,
            NotifyUserOfTimeout
        }

        public Type type;
        public string Message;
    }

    public enum ENetInstruction 
    {
        CancelConnection
    }
}
