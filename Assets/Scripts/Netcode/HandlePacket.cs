/*
 * Kittens Rise Up is a long term progression MMORPG.
 * Copyright (C) 2021  valkyrienyanko
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 * 
 * Contact valkyrienyanko by joining the Kittens Rise Up discord at
 * https://discord.gg/cDNf8ja or email sebastianbelle074@protonmail.com
 */

using ENet;
using EventType = ENet.EventType;  // fixes CS0104 ambigous reference between the same thing in UnityEngine
using Event = ENet.Event;          // fixes CS0104 ambigous reference between the same thing in UnityEngine
using Debug = UnityEngine.Debug;

using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

using Common.Networking.Packet;
using Common.Networking.IO;
using TMPro;
using KRU.Game;

namespace KRU.Networking
{
    public static class HandlePacket
    {
        public static void Handle(ref Event netEvent)
        {
            var packetSizeMax = 1024;
            var readBuffer = new byte[packetSizeMax];
            var packetReader = new PacketReader(readBuffer);
            packetReader.BaseStream.Position = 0;

            netEvent.Packet.CopyTo(readBuffer);

            var opcode = (ServerPacketOpcode)packetReader.ReadByte();

            if (opcode == ServerPacketOpcode.LoginResponse)
            {
                var data = new RPacketLogin();
                data.Read(packetReader);

                if (data.LoginOpcode == LoginResponseOpcode.VersionMismatch)
                {
                    var serverVersion = $"{data.VersionMajor}.{data.VersionMinor}.{data.VersionPatch}";
                    var clientVersion = $"{ENetClient.ClientVersionMajor}.{ENetClient.ClientVersionMinor}.{ENetClient.ClientVersionPatch}";
                    var message = $"Version mismatch. Server ver. {serverVersion} Client ver. {clientVersion}";

                    Debug.Log(message);

                    var cmd = new UnityInstructions();
                    cmd.Set(UnityInstructionOpcode.ServerResponseMessage, message);

                    ENetClient.UnityInstructions.Enqueue(cmd);
                }

                if (data.LoginOpcode == LoginResponseOpcode.LoginSuccess)
                {
                    // Load the main game 'scene'
                    ENetClient.UnityInstructions.Enqueue(new UnityInstructions(UnityInstructionOpcode.LoadMainScene));

                    // Update player values
                    ENetClient.MenuScript.gameScript.Player = new Player
                    {
                        Gold = data.Gold,
                        StructureHuts = data.StructureHut
                    };

                    ENetClient.UnityInstructions.Enqueue(new UnityInstructions(UnityInstructionOpcode.LoginSuccess));
                }
            }

            if (opcode == ServerPacketOpcode.PurchasedItem)
            {
                var data = new RPacketPurchaseItem();
                data.Read(packetReader);

                var itemResponseOpcode = data.PurchaseItemResponseOpcode;

                if (itemResponseOpcode == PurchaseItemResponseOpcode.NotEnoughGold)
                {
                    var message = $"You do not have enough gold for {(ItemType)data.ItemId}.";

                    Debug.Log(message);

                    var cmd = new UnityInstructions();
                    cmd.Set(UnityInstructionOpcode.LogMessage, message);

                    ENetClient.UnityInstructions.Enqueue(cmd);

                    // Update the player gold
                    ENetClient.GameScript.Player.Gold = data.Gold;
                }

                if (itemResponseOpcode == PurchaseItemResponseOpcode.Purchased)
                {
                    var message = $"Bought {(ItemType)data.ItemId} for 25 gold.";

                    Debug.Log(message);

                    var cmd = new UnityInstructions();
                    cmd.Set(UnityInstructionOpcode.LogMessage, message);

                    ENetClient.UnityInstructions.Enqueue(cmd);

                    // Update the player gold
                    ENetClient.GameScript.Player.Gold = data.Gold;

                    // Update the items
                    switch ((ItemType)data.ItemId)
                    {
                        case ItemType.Hut:
                            ENetClient.GameScript.Player.StructureHuts++;
                            break;
                        case ItemType.Farm:
                            break;
                    }
                }
            }

            packetReader.Dispose();
        }
    }
}
