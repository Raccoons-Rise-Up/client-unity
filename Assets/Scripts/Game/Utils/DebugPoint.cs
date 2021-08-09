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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace KRU.Utils
{
    // Useful when debugging visual geometry
    public class DebugPoint
    {
        private static bool parentCreated;
        private static Transform debugParent;

        private readonly GameObject go;

        public DebugPoint(Vector3 _pos, string _name = "Debug Point")
        {
            var prefab = Resources.Load<GameObject>("Prefabs/Debug Object");

            if (prefab == null)
            {
                Debug.LogWarning("Debug prefab has not been setup properly. No debug points will be shown until this is fixed.");
                return;
            }

            go = Object.Instantiate(prefab);
            go.transform.position = _pos;
            go.name = _name;
            SetColor(Color.white);

            SetUniversalParent();
        }

        public DebugPoint SetName(string name)
        {
            go.name = name;
            return this;
        }

        public DebugPoint SetColor(Color color)
        {
            go.GetComponent<Renderer>().sharedMaterial.color = color;
            return this;
        }

        public DebugPoint SetSize(float size)
        {
            go.transform.localScale *= size;
            return this;
        }

        private DebugPoint SetUniversalParent()
        {
            // For organization
            if (!parentCreated)
            {
                debugParent = new GameObject("Debug Points").transform;
                parentCreated = true;
            }

            go.transform.parent = debugParent;
            return this;
        }
    }
}
