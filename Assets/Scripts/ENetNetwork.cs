using ENet;
using Common.Networking.Packet;

namespace GameClient.Networking
{
    public class ENetNetwork
    {
        public static void Send(GamePacket gamePacket, PacketFlags packetFlags)
        {
            var packet = default(ENet.Packet);
            packet.Create(gamePacket.Data, packetFlags);
            ENetClient.Peer.Send(ENetClient.m_ChannelID, ref packet);
        }
    }
}
