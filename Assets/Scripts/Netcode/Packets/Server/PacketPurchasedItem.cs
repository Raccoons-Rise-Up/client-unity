using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Networking.Packet;
using Common.Networking.IO;
using Common.Networking.Message;

public class PacketPurchasedItem : IReadable
{
    public ServerPacketType m_ID;
    public uint m_ItemID;

    public void Read(PacketReader reader)
    {
        m_ID = (ServerPacketType)reader.ReadByte();
        m_ItemID = reader.ReadUInt16();
    }
}
