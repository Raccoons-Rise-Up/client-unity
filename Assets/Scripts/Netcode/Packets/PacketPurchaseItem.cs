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
