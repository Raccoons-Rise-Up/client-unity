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

using UnityEngine;
using System.Collections;

namespace KRU.Game 
{
    public class KRUGame : MonoBehaviour
    {
        public Player Player { get; set; }

        public IEnumerator GameLoop { get; private set; }

        private void Start()
        {
            Player = new Player();
            GameLoop = GameUpdate();
        }

        private IEnumerator GameUpdate() 
        {
            while (true) 
            {
                Player.Gold += 1;

                UIGame.UpdateGoldText();

                yield return new WaitForSeconds(1);
            }
        }
    }
}
