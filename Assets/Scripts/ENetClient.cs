using ENet;

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

using Common.Networking.Packet;

namespace GameClient.Networking 
{
    public class ENetClient : MonoBehaviour
    {
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

        private void Start()
        {
            Application.targetFrameRate = m_MaxFrames;
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);
        }

        public void Connect() 
        {
            m_WorkerThread = new Thread(ThreadWorker);
            m_WorkerThread.Start();
        }

        private void ThreadWorker() 
        {
            ENet.Library.Initialize();

            using Host client = new Host();
            var address = new Address();
            address.SetHost(m_IP);
            address.Port = m_Port;
            client.Create();

            Peer = client.Connect(address);
            Peer.PingInterval(m_PingInterval);
            Peer.Timeout(m_Timeout, m_TimeoutMinimum, m_TimeoutMaximum);
            Debug.Log("Attempting to connect...");

            var runningNetCode = true;
            while (runningNetCode)
            {
                var polled = false;

                while (!polled)
                {
                    if (client.CheckEvents(out ENet.Event netEvent) <= 0)
                    {
                        if (client.Service(15, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case ENet.EventType.None:
                            Debug.Log("Nothing");
                            break;

                        case ENet.EventType.Connect:
                            Debug.Log("Client connected to server");
                            break;

                        case ENet.EventType.Disconnect:
                            Debug.Log("Client disconnected from server");
                            break;

                        case ENet.EventType.Timeout:
                            Debug.Log("Client connection timeout");
                            break;

                        case ENet.EventType.Receive:
                            var incomingPacket = netEvent.Packet;
                            Debug.Log("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + incomingPacket.Length);
                            incomingPacket.Dispose();
                            break;
                    }
                }
            }

            client.Flush();
            client.Dispose();

            ENet.Library.Deinitialize();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) 
            {
                var sendPacket = new ClientPacket(ClientPacketType.Disconnect);
                ENetNetwork.Send(sendPacket, PacketFlags.Reliable);
            }
        }
    }
}
