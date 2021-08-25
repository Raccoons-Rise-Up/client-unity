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

using System;
using System.IO;
using System.Collections.Generic;
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
        [Header("Client")]
        public string ip = "127.0.0.1";
        public ushort port = 25565;

        [Header("Linked Transforms")]
        public Transform menuTranform;
        public Transform loginTransform;
        public Transform terminalTransform;
        public Transform gameTransform;

        private static UIMenu MenuScript { get; set; }
        private static UILogin LoginScript { get; set; }
        private static UITerminal TerminalScript { get; set; }
        private static KRUGame GameScript { get; set; }

        // Non-Inspector
        public static byte ClientVersionMajor { get; private set; }
        public static byte ClientVersionMinor { get; private set; }
        public static byte ClientVersionPatch { get; private set; }

        private static ConcurrentQueue<UnityInstructions> UnityInstructions { get; set; } 
        private static ConcurrentQueue<ENetInstructionOpcode> ENetInstructions { get; set; }
        private static ConcurrentQueue<ClientPacket> Outgoing { get; set; }

        private static Peer Peer { get; set; }
        private static bool TryingToConnect { get; set; }
        private static bool ConnectedToServer { get; set; }
        private static bool RunningNetCode { get; set; }
        private static bool ReadyToQuitUnity { get; set; }

        private static DateTime LastLogin { get; set; }
        private static DateTime LastHutPurchase { get; set; }

        private void Start()
        {
            // Client version
            ClientVersionMajor = 0;
            ClientVersionMinor = 1;
            ClientVersionPatch = 0;

            // Need a way to communicate with the Unity thread from the ENet thread
            UnityInstructions = new ConcurrentQueue<UnityInstructions>();

            // Need a way to communicate with the ENet thread from the Unity thread
            ENetInstructions = new ConcurrentQueue<ENetInstructionOpcode>();

            // The packets that are sent to the server
            Outgoing = new ConcurrentQueue<ClientPacket>();

            DontDestroyOnLoad(gameObject);
            Application.wantsToQuit += OnWantsToQuit;

            MenuScript = menuTranform.GetComponent<UIMenu>();
            LoginScript = loginTransform.GetComponent<UILogin>();
            TerminalScript = terminalTransform.GetComponent<UITerminal>();
            GameScript = gameTransform.GetComponent<KRUGame>();

            // Make sure queues are completely drained before starting
            if (Outgoing != null) while (Outgoing.TryDequeue(out _)) ;
            if (UnityInstructions != null) while (UnityInstructions.TryDequeue(out _)) ;
            if (ENetInstructions != null) while (ENetInstructions.TryDequeue(out _)) ;
        }

        public void Connect() 
        {
            if (TryingToConnect || ConnectedToServer)
                return;

            TryingToConnect = true;
            new Thread(ThreadWorker).Start();
        }

        public void Disconnect() 
        {
            ENetInstructions.Enqueue(ENetInstructionOpcode.CancelConnection);
        }

        public bool IsConnected() => ConnectedToServer;

        private void ThreadWorker() 
        {
            Library.Initialize();

            using Host client = new Host();
            var address = new Address();
            address.SetHost(ip);
            address.Port = port;
            client.Create();

            uint pingInterval = 1000; // Pings are used both to monitor the liveness of the connection and also to dynamically adjust the throttle during periods of low traffic so that the throttle has reasonable responsiveness during traffic spikes.
            uint timeout = 5000; // Will be ignored if maximum timeout is exceeded
            uint timeoutMinimum = 5000; // The timeout for server not sending the packet to the client sent from the server
            uint timeoutMaximum = 5000; // The timeout for server not receiving the packet sent from the client

            Peer = client.Connect(address);
            Peer.PingInterval(pingInterval);
            Peer.Timeout(timeout, timeoutMinimum, timeoutMaximum);
            Debug.Log("Attempting to connect...");

            bool wantsToQuit = false;

            RunningNetCode = true;
            while (RunningNetCode)
            {
                var polled = false;

                // ENet Instructions (from Unity Thread)
                while (ENetInstructions.TryDequeue(out ENetInstructionOpcode result))
                {
                    if (result == ENetInstructionOpcode.CancelConnection)
                    {
                        Debug.Log("Cancel connection");
                        ConnectedToServer = false;
                        TryingToConnect = false;
                        RunningNetCode = false;
                        break;
                    }

                    if (result == ENetInstructionOpcode.UserWantsToQuit) 
                    {
                        Peer.Disconnect(0);
                        wantsToQuit = true;
                        RunningNetCode = false;
                        break;
                    }
                }

                // Sending data
                while (Outgoing.TryDequeue(out ClientPacket clientPacket)) 
                {
                    switch ((ClientPacketOpcode)clientPacket.Opcode) 
                    {
                        case ClientPacketOpcode.Login:
                            Debug.Log("Sending login request to game server..");

                            Send(clientPacket, PacketFlags.Reliable);

                            break;
                        case ClientPacketOpcode.PurchaseItem:
                            Debug.Log("Sending purchase item request to game server..");
                            LastHutPurchase = DateTime.Now;

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
                            var clientPacket = new ClientPacket((byte)ClientPacketOpcode.Login, new WPacketLogin { 
                                Username = LoginScript.username,
                                VersionMajor = ClientVersionMajor,
                                VersionMinor = ClientVersionMinor,
                                VersionPatch = ClientVersionPatch
                            });

                            Outgoing.Enqueue(clientPacket);

                            // Keep track of networking logic
                            TryingToConnect = false;
                            ConnectedToServer = true;
                            break;

                        case EventType.Disconnect:
                            //Debug.Log(netEvent.Data);
                            Debug.Log($"{(int)(DateTime.Now - LastLogin).TotalSeconds} seconds passed since last login");
                            Debug.Log($"{(int)(DateTime.Now - LastHutPurchase).TotalSeconds} seconds passed since last hut purchase");
                            Debug.Log("Client disconnected from server");
                            ConnectedToServer = false;
                            break;

                        case EventType.Timeout:
                            Debug.Log("Client connection timeout to game server");
                            TryingToConnect = false;
                            ConnectedToServer = false;
                            UnityInstructions.Enqueue(new UnityInstructions(UnityInstructionOpcode.Timeout));
                            break;

                        case EventType.Receive:
                            var packet = netEvent.Packet;
                            Debug.Log("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + packet.Length);

                            var packetSizeMax = 1024;
                            var readBuffer = new byte[packetSizeMax];
                            var packetReader = new PacketReader(readBuffer);
                            packetReader.BaseStream.Position = 0;

                            netEvent.Packet.CopyTo(readBuffer);

                            var opcode = (ServerPacketOpcode)packetReader.ReadByte();

                            if (opcode == ServerPacketOpcode.LoginResponse) 
                            {
                                var data = new RPacketLogin();
                                data.Read(packetReader);

                                if (data.LoginOpcode == LoginResponseOpcode.VersionMismatch)
                                {
                                    var serverVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                                    var clientVersion = $"{ClientVersionMajor}.{ClientVersionMinor}.{ClientVersionPatch}";

                                    var cmd = new UnityInstructions();
                                    cmd.Set(UnityInstructionOpcode.ServerResponseMessage, 
                                        $"Version mismatch. Server ver. {serverVersion} Client ver. {clientVersion}");

                                    UnityInstructions.Enqueue(cmd);
                                }

                                if (data.LoginOpcode == LoginResponseOpcode.LoginSuccess)
                                {
                                    // Load the main game 'scene'
                                    UnityInstructions.Enqueue(new UnityInstructions(UnityInstructionOpcode.LoadMainScene));

                                    // Update player values
                                    MenuScript.gameScript.Player = new Player
                                    {
                                        Gold = data.Gold,
                                        StructureHuts = data.StructureHut
                                    };

                                    UnityInstructions.Enqueue(new UnityInstructions (UnityInstructionOpcode.LoginSuccess));

                                    LastLogin = DateTime.Now;
                                }
                            }

                            if (opcode == ServerPacketOpcode.PurchasedItem) 
                            {
                                var data = new RPacketPurchaseItem();
                                data.Read(packetReader);

                                var itemResponseOpcode = data.PurchaseItemResponseOpcode;

                                if (itemResponseOpcode == PurchaseItemResponseOpcode.NotEnoughGold) 
                                {
                                    var cmd = new UnityInstructions();
                                    cmd.Set(UnityInstructionOpcode.LogMessage, $"You do not have enough gold for {(ItemType)data.ItemId}.");

                                    UnityInstructions.Enqueue(cmd);

                                    // Update the player gold
                                    GameScript.Player.Gold = data.Gold;
                                }

                                if (itemResponseOpcode == PurchaseItemResponseOpcode.Purchased) 
                                {
                                    var cmd = new UnityInstructions();
                                    cmd.Set(UnityInstructionOpcode.LogMessage, $"Bought {(ItemType)data.ItemId} for 25 gold.");

                                    UnityInstructions.Enqueue(cmd);

                                    // Update the player gold
                                    GameScript.Player.Gold = data.Gold;

                                    // Update the items
                                    switch ((ItemType)data.ItemId) 
                                    {
                                        case ItemType.Hut:
                                            GameScript.Player.StructureHuts++;
                                            break;
                                        case ItemType.Farm:
                                            break;
                                    }
                                }
                            }

                            packetReader.Dispose();
                            packet.Dispose();
                            break;
                    }
                }
            }

            client.Flush();
            client.Dispose();

            Library.Deinitialize();

            if (wantsToQuit) 
            {
                ReadyToQuitUnity = true;
                UnityInstructions.Enqueue(new UnityInstructions(UnityInstructionOpcode.Quit));
            }    
        }

        private static bool OnWantsToQuit()
        {
            ENetInstructions.Enqueue(ENetInstructionOpcode.UserWantsToQuit);

            return ReadyToQuitUnity || !RunningNetCode;
        }

        private void Update()
        {
            while (UnityInstructions.TryDequeue(out UnityInstructions result)) 
            {
                foreach (var cmd in result.Instructions) 
                {
                    var opcode = cmd.Key;

                    if (opcode == UnityInstructionOpcode.ServerResponseMessage) 
                    {
                        LoginScript.loginFeedbackText.text = (string)cmd.Value[0];
                    }

                    if (opcode == UnityInstructionOpcode.LogMessage) 
                    {
                        TerminalScript.Log((string)cmd.Value[0]);
                    }

                    if (opcode == UnityInstructionOpcode.Timeout) 
                    {
                        // Load timeout scene
                        LoginScript.btnConnect.interactable = true;
                        LoginScript.loginFeedbackText.text = "Client connection timeout to game server";

                        MenuScript.LoadTimeoutDisconnectScene();
                        MenuScript.gameScript.InGame = false;

                        // Reset player values
                        MenuScript.gameScript.Player = null;

                        // Clear terminal output
                        //TerminalScript.
                    }

                    if (opcode == UnityInstructionOpcode.LoadMainScene) 
                    {
                        MenuScript.FromConnectingToMainScene();
                        LoginScript.loginFeedbackText.text = "";
                        LoginScript.btnConnect.interactable = true;
                        MenuScript.gameScript.InGame = true;
                    }

                    if (opcode == UnityInstructionOpcode.LoginSuccess) 
                    {
                        GameScript.UILoopRunning = true;
                        StartCoroutine(GameScript.UILoop);
                    }

                    if (opcode == UnityInstructionOpcode.Quit) 
                    {
                        Application.Quit();
                    }
                }
            }
        }

        public void PurchaseItem(int itemId) 
        {
            var data = new WPacketPurchaseItem { ItemID = (ushort)itemId };
            var clientPacket = new ClientPacket((byte)ClientPacketOpcode.PurchaseItem, data);

            Outgoing.Enqueue(clientPacket);
        }

        private void Send(GamePacket gamePacket, PacketFlags packetFlags)
        {
            byte channelID = 0; // The channel all networking traffic will be going through
            var packet = default(Packet);
            packet.Create(gamePacket.Data, packetFlags);
            Peer.Send(channelID, ref packet);
        }
    }

    public class UnityInstructions 
    {
        public Dictionary<UnityInstructionOpcode, List<object>> Instructions { get; set; }

        public UnityInstructions() 
        {
            Instructions = new Dictionary<UnityInstructionOpcode, List<object>>();
        }

        public UnityInstructions(UnityInstructionOpcode opcode) 
        {
            Instructions = new Dictionary<UnityInstructionOpcode, List<object>>
            {
                [opcode] = null
            };
        }

        public void Set(UnityInstructionOpcode opcode, params object[] data) 
        {
            Instructions[opcode] = new List<object>(data);
        }
    }

    public enum UnityInstructionOpcode
    {
        LoadMainScene,
        LogMessage,
        ServerResponseMessage,
        Timeout,
        LoginSuccess,
        Quit
    }

    public enum ENetInstructionOpcode 
    {
        CancelConnection,
        UserWantsToQuit
    }
}
