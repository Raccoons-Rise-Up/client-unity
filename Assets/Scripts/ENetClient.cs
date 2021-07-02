using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using Common.Networking.Packet;

namespace GameClient.Networking 
{
    public class ENetClient : MonoBehaviour
    {
        public const byte CHANNEL_ID = 0;
        private const int TIMEOUT_SEND = 1000 * 5;
        private const int TIMEOUT_RECEIVE = 1000 * 30;
        private const int MAX_FRAMES = 30; // game frames

        public static Peer Peer { get; set; }

        public string ip = "127.0.0.1";
        public ushort port = 8888;

        private Host host;

        private void Start()
        {
            Application.targetFrameRate = MAX_FRAMES;
            Application.runInBackground = true;
            DontDestroyOnLoad(gameObject);

            ENet.Library.Initialize();

            host = new Host();
            var address = new Address();

            address.SetHost(ip);
            address.Port = port;
            host.Create();

            Peer = host.Connect(address);
            Peer.Timeout(0, TIMEOUT_RECEIVE, TIMEOUT_SEND);
            Debug.Log("Attempting to connect...");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) 
            {
                var sendPacket = new ClientPacket(ClientPacketType.Disconnect);
                ENetNetwork.Send(sendPacket, PacketFlags.Reliable);
            }
        }

        private void FixedUpdate()
        {
            if (!host.IsSet)
                return;

            if (host.CheckEvents(out ENet.Event netEvent) <= 0)
            {
                if (host.Service(15, out netEvent) <= 0)
                    return;
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
                    Debug.Log("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                    netEvent.Packet.Dispose();
                    break;
            }

            host.Flush();
            host.Dispose();

            ENet.Library.Deinitialize();
        }
    }
}
