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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

using Common.Networking.Packet;
using TMPro;

namespace KRU.Networking 
{
    public class ENetClient : MonoBehaviour
    {
        // Unity Inspector Variables
        public string m_IP = "127.0.0.1";
        public ushort m_Port = 1234;

        private readonly ConcurrentQueue<UnityInstruction> m_UnityInstructions = new ConcurrentQueue<UnityInstruction>(); // Need a way to communicate with the Unity thread from the ENet thread
        private readonly ConcurrentQueue<ClientPacket> m_Outgoing = new ConcurrentQueue<ClientPacket>(); // The packets that are sent to the server

        private const byte m_ChannelID = 0; // The channel all networking traffic will be going through
        private const int m_MaxFrames = 30; // The games FPS cap

        private readonly uint m_PingInterval = 1000; // Pings are used both to monitor the liveness of the connection and also to dynamically adjust the throttle during periods of low traffic so that the throttle has reasonable responsiveness during traffic spikes.
        private readonly uint m_Timeout = 5000; // Will be ignored if maximum timeout is exceeded
        private readonly uint m_TimeoutMinimum = 5000; // The timeout for server not sending the packet to the client sent from the server
        private readonly uint m_TimeoutMaximum = 5000; // The timeout for server not receiving the packet sent from the client

        private Peer m_Peer;

        private Thread m_WorkerThread;
        private bool m_RunningNetCode;
        private bool m_TryingToConnect;
        private bool m_ConnectedToServer;

        private TMP_InputField m_InputField;

        private void Start()
        {
            Application.targetFrameRate = m_MaxFrames;
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);

            // Make sure queues are completely drained before starting
            if (m_Outgoing != null) while (m_Outgoing.TryDequeue(out _)) ;
        }

        public void Connect() 
        {
            if (m_TryingToConnect || m_ConnectedToServer)
                return;

            m_TryingToConnect = true;
            m_WorkerThread = new Thread(ThreadWorker);
            m_WorkerThread.Start();
        }

        private void ThreadWorker() 
        {
            Library.Initialize();

            using Host client = new Host();
            var address = new Address();
            address.SetHost(m_IP);
            address.Port = m_Port;
            client.Create();

            m_Peer = client.Connect(address);
            m_Peer.PingInterval(m_PingInterval);
            m_Peer.Timeout(m_Timeout, m_TimeoutMinimum, m_TimeoutMaximum);
            Debug.Log("Attempting to connect...");

            m_RunningNetCode = true;
            while (m_RunningNetCode)
            {
                var polled = false;

                // Sending data
                while (m_Outgoing.TryDequeue(out ClientPacket clientPacket)) 
                {
                    switch (clientPacket.Opcode) 
                    {
                        case ClientPacketType.PurchaseItem:
                            Debug.Log("Sending purchase item request to server..");

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
                            Debug.Log("Client connected to server");
                            m_TryingToConnect = false;
                            m_ConnectedToServer = true;

                            m_UnityInstructions.Enqueue(UnityInstruction.LoadMainScene);
                            break;

                        case EventType.Disconnect:
                            Debug.Log("Client disconnected from server");
                            m_ConnectedToServer = false;
                            break;

                        case EventType.Timeout:
                            Debug.Log("Client connection timeout");
                            m_TryingToConnect = false;
                            m_ConnectedToServer = false;
                            break;

                        case EventType.Receive:
                            var incomingPacket = netEvent.Packet;
                            Debug.Log("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + incomingPacket.Length);
                            incomingPacket.Dispose();
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
            if (!m_RunningNetCode)
                return;

            while (m_UnityInstructions.TryDequeue(out UnityInstruction result)) 
            {
                if (result == UnityInstruction.LoadMainScene) 
                {
                    StartCoroutine(LoadMainScene());
                }
            }
        }

        private IEnumerator LoadMainScene()
        {
            var asyncLoad = SceneManager.LoadSceneAsync("Main");

            // Loading
            while (!asyncLoad.isDone)
                yield return null;
            // Finished

            m_InputField = GameObject.Find("Canvas").transform.Find("input_PurchaseItem").GetComponent<TMP_InputField>();
        }

        public void PurchaseItem() 
        {
            if (ushort.TryParse(m_InputField.text, out ushort itemId))
            {
                var data = new PacketPurchaseItem(itemId);
                var clientPacket = new ClientPacket(ClientPacketType.PurchaseItem, data);

                m_Outgoing.Enqueue(clientPacket);
            }
            else 
            {
                Debug.LogWarning("Not a valid number");
            }
        }

        private void Send(GamePacket gamePacket, PacketFlags packetFlags)
        {
            var packet = default(Packet);
            packet.Create(gamePacket.Data, packetFlags);
            m_Peer.Send(m_ChannelID, ref packet);
        }
    }

    public enum UnityInstruction
    {
        LoadMainScene
    }
}
