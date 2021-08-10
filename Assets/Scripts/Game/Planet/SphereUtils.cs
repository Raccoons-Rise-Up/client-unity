using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KRU.Game 
{
    public static class SphereUtils
    {
        /// <summary>
        /// Get center point of three vertices.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns>Returns center point given three vertices</returns>
        public static Vector3 GetCenterPoint(Vector3 a, Vector3 b, Vector3 c) => (a + b + c) / 3;

        /// <summary>
        /// Get midpoint of two vertices.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>Returns midpoint given two vertices</returns>
        public static Vector3 GetMidPointVertex(Vector3 a, Vector3 b) => (a + b) / 2;
    }
}

