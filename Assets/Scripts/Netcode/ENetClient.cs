using ENet;
using EventType = ENet.EventType;  // fixes CS0104 ambigous reference between the same thing in UnityEngine
using Event = ENet.Event;          // fixes CS0104 ambigous reference between the same thing in UnityEngine

using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

using Common.Networking.Packet;

namespace GameClient.Networking 
{
    public class ENetClient : MonoBehaviour
    {
        public ConcurrentQueue<ClientPacket> m_Outgoing = new ConcurrentQueue<ClientPacket>();

        public const byte m_ChannelID = 0; // The channel all networking traffic will be going through
        private const int m_MaxFrames = 30; // The games FPS cap

        public uint m_PingInterval = 1000; // Pings are used both to monitor the liveness of the connection and also to dynamically adjust the throttle during periods of low traffic so that the throttle has reasonable responsiveness during traffic spikes.
        public uint m_Timeout = 5000; // Will be ignored if maximum timeout is exceeded
        public uint m_TimeoutMinimum = 5000; // The timeout for server not sending the packet to the client sent from the server
        public uint m_TimeoutMaximum = 5000; // The timeout for server not receiving the packet sent from the client

        public static Peer Peer { get; set; }

        public string m_IP = "127.0.0.1";
        public ushort m_Port = 8888;

        private Thread m_WorkerThread;
        public bool m_RunningNetCode;
        public bool m_TryingToConnect;
        public bool m_ConnectedToServer;

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

            Peer = client.Connect(address);
            Peer.PingInterval(m_PingInterval);
            Peer.Timeout(m_Timeout, m_TimeoutMinimum, m_TimeoutMaximum);
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

                            Send(Peer, clientPacket, PacketFlags.Reliable);

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

            if (Input.GetKeyDown(KeyCode.R))
            {
                var data = new PacketPurchaseItem(0);
                var clientPacket = new ClientPacket(ClientPacketType.PurchaseItem, data);

                m_Outgoing.Enqueue(clientPacket);
            }
        }

        private void Send(Peer peer, GamePacket gamePacket, PacketFlags packetFlags)
        {
            var packet = default(Packet);
            packet.Create(gamePacket.Data, packetFlags);
            peer.Send(ENetClient.m_ChannelID, ref packet);
        }
    }
}
