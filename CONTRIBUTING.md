# Contributing

## Sending a Packet from the Client to the Server
### Creating the Writer Packet
Create a new packet class under `Assets/Scripts/Netcode/Packets/Client`, the packet class should be called something like `PacketPositionData` or `PacketDisconnect`. (currently there is no easy way to distinguish a reader packet from a writer packet other then its well defined name and the folder group its in, this may change in the future by adding the prefix "Writer" / "Reader")

Use the following code as a template
```cs
using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

namespace KRU.Networking 
{
    public class PacketSendSomething : IWritable
    {
        private readonly ushort ItemID;

        public PacketSendSomething(ushort ItemID)
        {
            this.ItemID = ItemID;
        }

        public void Write(PacketWriter writer)
        {
            writer.Write(ItemID);
        }
    }
}
```

Replace `ItemID` with one or more fields, remember to write them all with `writer.Write(...)`.

### Adding the Opcode
Opcodes are what make packets unique from each other. For example, `ClientPacketType.Login` is a opcode used to identify that this packet holds login information.

The client and server both use a common DLL which contains netcode shared by both the client and server. This is where the opcodes are defined as both the client and server need to know about this. Clone `https://github.com/Kittens-Rise-Up/common`, go to `src/Networking/Packet/ClientPacketType.cs` and add the opcode to the enum. Build the common library through visual studio. A DLL file should have been generated in `obj/Debug/netstandard2.0` called `Common.dll`. Copy and paste the DLL in the client in `Assets/Scripts/Plugins/x86_64` and in the server in `libs/`.

### Sending the Packet
Navigate to `Assets/Scripts/Netcode/ENetClient.cs`, this is the main networking script for the client.

First, create the packet
```cs
var data = new PacketSendSomething((ushort)itemId);
var clientPacket = new ClientPacket(ClientPacketType.PurchaseItem, data);
```

Add the packet to the outgoing concurrent queue
```cs
outgoing.Enqueue(clientPacket);
```

Finally, dequeue the packet from the outgoing concurrent queue and send it to the server. Note that `PacketFlags.Unreliable` should be used for sending data that is not important such as positional data while `PacketFlags.Reliable` should be used when sending important data such as item the client wants to purchase.
```cs
while (outgoing.TryDequeue(out ClientPacket clientPacket)) 
{
    switch (clientPacket.Opcode) 
    {
        case ClientPacketType.PurchaseItem:
            Debug.Log("Sending something to game server..");

            Send(clientPacket, PacketFlags.Reliable);

            break;
    }
}
```

### Creating the Reader Packet Server Side
The server needs to know how to read this packet.



## Formatting Guidelines
- Methods should follow PascalFormat
- Most of the time `{}` should be fully expanded
- Variables should be camelCase regardless if private or public
- Try to use `var` where ever possible

## Creating a Pull Request
1. Always test the application to see if it works as intended with no additional bugs you may be adding!
2. State all the changes you made in the PR, not everyone will understand what you've done!
