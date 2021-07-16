using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

public class PacketPurchaseItem : IWritable
{
    private readonly byte m_ID;
    private readonly ushort m_ItemID;

    public PacketPurchaseItem(ushort m_ItemID) 
    {
        this.m_ID = (byte)ClientPacketType.PurchaseItem;
        this.m_ItemID = m_ItemID;
    }

    public void Write(PacketWriter writer)
    {
        writer.Write(m_ID);
        writer.Write(m_ItemID);
    }
}
