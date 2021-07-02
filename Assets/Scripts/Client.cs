using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ENet;
using Common.Networking.Packet;

public class Client : MonoBehaviour
{
    public string ip = "127.0.0.1";
    public ushort port = 3000;

    public bool active = true;

    private void Start()
    {
        ENet.Library.Initialize();

        using Host client = new Host();
        var address = new Address();

        address.SetHost(ip);
        address.Port = port;
        client.Create();

        var peer = client.Connect(address);

        while (active)
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
            }

            client.Flush();
        }
    }

    private void Update()
    {
        var sendPacket = new ClientPacket(ClientPacketType.Disconnect);
        ENetNetwork.Send(sendPacket, PacketFlags.Reliable);
    }
}
