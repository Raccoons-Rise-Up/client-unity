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