using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Networking.Packet;
using Common.Networking.IO;
using Common.Networking.Message;

public class PacketPurchasedItem : IReadable
{
    public ServerPacketType id;
    public uint itemId;

    public void Read(PacketReader reader)
    {
        id = (ServerPacketType)reader.ReadByte();
        itemId = reader.ReadUInt16();
    }
}
