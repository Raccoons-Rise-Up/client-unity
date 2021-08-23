using Common.Networking.Packet;
using Common.Networking.IO;
using Common.Networking.Message;

namespace KRU.Networking
{
    public class RPacketLogin : IReadable
    {
        public LoginOpcode LoginOpcode { get; set; }
        public byte VersionMajor { get; set; }
        public byte VersionMinor { get; set; }
        public byte VersionPatch { get; set; }

        public void Read(PacketReader reader)
        {
            LoginOpcode = (LoginOpcode)reader.ReadByte();

            switch (LoginOpcode) 
            {
                case LoginOpcode.VersionMismatch:
                    VersionMajor = reader.ReadByte();
                    VersionMinor = reader.ReadByte();
                    VersionPatch = reader.ReadByte();
                    break;
                case LoginOpcode.LoginSuccess:
                    break;
            }

            reader.Dispose();
        }
    }
}
