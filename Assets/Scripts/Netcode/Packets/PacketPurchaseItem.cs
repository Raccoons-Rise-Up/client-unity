using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

namespace KRU.Networking 
{
    public class PacketPurchaseItem : IWritable
    {
        private readonly ushort m_ItemID;

        public PacketPurchaseItem(ushort m_ItemID)
        {
            this.m_ItemID = m_ItemID;
        }

        public void Write(PacketWriter writer)
        {
            writer.Write(m_ItemID);
        }
    }
}
