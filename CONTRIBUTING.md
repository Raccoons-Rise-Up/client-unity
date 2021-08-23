# Contributing
Hello and welcome to the constributing section for the Kittens Rise Up game client! The codebase is constantly changing, if anything comes across as confusing or is unclear please tell me about it in the [Kittens Rise Up Discord](https://discord.gg/cDNf8ja) or create a issue in this repository and I will have a look at it.

## Table of Contents
1. [Setup Project](#setup-project)
2. [Formatting Guidelines](#formatting-guidelines)
3. [Creating a Pull Request](#creating-a-pull-request)
4. [Threads](#threads)
    - [Communicating from Unity Thread to ENet Thread](#communicating-from-unity-thread-to-enet-thread)
    - [Communicating from ENet Thread to Unity Thread](#communicating-from-enet-thread-to-unity-thread)
5. [Networking](#networking)
    - [Sending a Packet from the Client to the Server](#sending-a-packet-from-the-client-to-the-server)
    - [Sending a Packet from the Server to the Client](#sending-a-packet-from-the-server-to-the-client)

## Setup Project
Download [Unity Hub](https://unity.com/download)

Project is currently using `2021.1.7f1`, later versions should work just fine.

## Formatting Guidelines
- Methods should follow PascalFormat
- Most of the time `{}` should be fully expanded
- Variables should be camelCase regardless if private or public
- Try to use `var` where ever possible

## Creating a Pull Request
1. Always test the application to see if it works as intended with no additional bugs you may be adding!
2. State all the changes you made in the PR, not everyone will understand what you've done!

## Threads
The networking library called ENet-CSharp should be executed on a separate thread from the Unity thread otherwise Unity will interfere with ENet. If you are unfamiliar with threads please read [Using threads and threading](https://docs.microsoft.com/en-us/dotnet/standard/threading/using-threads-and-threading).

### Communicating from Unity Thread to ENet Thread
In `Assets/Scripts/Netcode/ENetClient.cs`, add the 'opcode' to the following enum. For example maybe you want to instruct ENet to disconnect from the server entirely, so you would add something like `CancelConnection`.
```cs
public enum ENetInstruction 
{
    CancelConnection
}
```

Enqueue the instruction from the Unity thread
```cs
ENetInstructions.Enqueue(ENetInstruction.CancelConnection);
```

Then dequeue the instruction in the ENet thread
```cs
// ENet Instructions (from Unity Thread)
while (ENetInstructions.TryDequeue(out ENetInstruction result))
{
    if (result == ENetInstruction.CancelConnection)
    {
        // some random code here
        Debug.Log("Cancel connection");
        connectedToServer = false;
        tryingToConnect = false;
        runningNetCode = false;
        break;
    }
}
```

If you wanted to send data along with the opcode then the `ENetInstruction` would have to become a class with a opcode enum inside however the need for such a purpose has not risen yet.

### Communicating from ENet Thread to Unity Thread
Lets say you want to display a message in the Unity UI when something happens in the ENet thread. Since functions like `Debug.Log(...)` and `gameObject.GetComponent<Text>().text = ...` are Unity related they must be executed on the Unity thread otherwise you will run into a runtime error.

First add the opcode `LogMessage` to the `UnityInstruction.Type`
```cs
public class UnityInstruction 
{
    public enum Type 
    {
        LoadSceneForDisconnectTimeout,
        LoadMainScene,
        LogMessage,
        NotifyUserOfTimeout
    }

    public Type type;
    public string Message;
}
```

Enqueue the data in the ENet thread
```cs
// received a purchased item packet from the server
if (opcode == ServerPacketType.PurchasedItem) 
{
    var data = new PacketPurchasedItem();
    var packetReader = new PacketReader(readBuffer);
    data.Read(packetReader);

    // enqueue data
    unityInstructions.Enqueue(new UnityInstruction { 
        type = UnityInstruction.Type.LogMessage,
        Message = $"You purchased item: {data.itemId} for x gold." 
    });
}
```

Dequeue the data in the Unity thread
```cs
while (unityInstructions.TryDequeue(out UnityInstruction result)) 
{
    switch (result.type) 
    {
        case UnityInstruction.Type.LogMessage:
            Debug.Log(result.Message);
            break;
    }
}
```

## Networking
### Sending a Packet from the Client to the Server

1. [Creating the Writer Packet](#creating-the-writer-packet)
2. [Adding the Opcode](#adding-the-opcode)
3. [Sending the Packet](#sending-the-packet)
4. [Creating the Reader Packet Server Side](#creating-the-reader-packet-server-side)

#### Creating the Writer Packet
Create a new packet class under `Assets/Scripts/Netcode/Packets/Client`, the packet class should be called something like `WPacketPositionData` or `WPacketDisconnect`. (note that the W prefix means "write", if a reader packet was being created the R prefix should go in front)

Use the following code as a template
```cs
using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

namespace KRU.Networking 
{
    public class WPacketSendSomething : IWritable
    {
        public ushort ItemId { get; set; }

        public void Write(PacketWriter writer)
        {
            writer.Write(ItemID);
            
            writer.Dispose();
        }
    }
}
```

Replace `ItemID` with one or more fields, remember to write them all with `writer.Write(...)`.

#### Adding the Opcode
Opcodes are what make packets unique from each other. For example, `ClientPacketType.Login` is a opcode used to identify that this packet holds login information.

On the client, navigate to `Assets/Scripts/Netcode/Packets/Opcodes.cs` and add your opcode to the enum `ClientPacketType`.  
On the server, navigate to `src/Server/Packets/Opcodes.cs` and add your opcode to the enum `ClientPacketType`.  

The `Opcodes.cs` client-side should look exactly like that of the `Opcodes.cs` server-side.

#### Sending the Packet
Navigate to `Assets/Scripts/Netcode/ENetClient.cs`, this is the main networking script for the client.

First, create the packet
```cs
var data = new WPacketSendSomething((ushort)itemId);
var clientPacket = new ClientPacket((byte)ClientPacketType.PurchaseItem, data);
```

Add the packet to the outgoing concurrent queue
```cs
outgoing.Enqueue(clientPacket);
```

Finally, dequeue the packet from the outgoing concurrent queue and send it to the server. Note that `PacketFlags.Unreliable` should be used for sending data that is not important such as positional data while `PacketFlags.Reliable` should be used when sending important data such as item the client wants to purchase.
```cs
while (outgoing.TryDequeue(out ClientPacket clientPacket)) 
{
    switch ((ClientPacketType)clientPacket.Opcode) 
    {
        case ClientPacketType.PurchaseItem:
            Debug.Log("Sending something to game server..");

            Send(clientPacket, PacketFlags.Reliable);

            break;
    }
}
```

#### Creating the Reader Packet Server Side
The server needs to know how to read this packet.

In the server, navigate to `src/Server/Packets/Client/` and create a class named `RPacketSendSomething.cs`

Use the following code as a template.

```cs
using Common.Networking.Message;
using Common.Networking.IO;
using Common.Networking.Packet;

namespace GameServer.Server.Packets
{
    // ================================== Sizes ==================================
    // sbyte   -128 to 127                                                   SByte
    // byte       0 to 255                                                   Byte
    // short   -32,768 to 32,767                                             Int16
    // ushort  0 to 65,535                                                   UInt16
    // int     -2,147,483,648 to 2,147,483,647                               Int32
    // uint    0 to 4,294,967,295                                            UInt32
    // long    -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807       Int64
    // ulong   0 to 18,446,744,073,709,551,615                               UInt64

    public class RPacketSendSomething : IReadable
    {
        public ushort ItemId { get; set; }

        public void Read(PacketReader reader)
        {
            itemId = reader.ReadUInt16(); // we sent it as a ushort so we must read it as a ushort (see to the table above)
            
            reader.Dispose();
        }
    }
}
```

Finally in `src/Server/ENetServer.cs` add the following code.
```cs
if (opcode == ClientPacketType.SendSomething) 
{
    var data = new RPacketSendSomething();
    data.Read(packetReader);
    
    ClientPacketHandleSendSomething(data, peer); // create a private static method for readability and organization
}

//...

private static void ClientPacketHandleSendSomething(RPacketSendSomething data, Peer peer) 
{
    Logger.Log($"Item ID: {data.itemId}");
    
    // Optional: Send a response to the client for feedback
    // You will have to create WPacketSendSomething class
    var packetData = new WPacketSendSomething {
        Opcode = SendSomethingOpcode.InvalidItem
    };

    var serverPacket = new ServerPacket((byte)ServerPacketType.SendSomethingResponse, packetData);
    Send(serverPacket, peer, PacketFlags.Reliable);
}
```

### Sending a Packet From the Server to the Client
Sending a packet from the server to the client is much like sending a packet from the client to the server, except instead of creating the packets under `Client/<...>`, create them under `Server/<...>`.
