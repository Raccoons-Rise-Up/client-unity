using UnityEngine;

namespace KRU.Game
{
    public class Icosahedron
    {
        private readonly Vector3[] vertices;
        private readonly int[] triangles;

        public Icosahedron(float radius = 1)
        {
            var t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

            vertices = new Vector3[]
            {
            new Vector3(-1, t, 0).normalized  * radius,
            new Vector3(1, t, 0).normalized   * radius,
            new Vector3(-1, -t, 0).normalized * radius,
            new Vector3(1, -t, 0).normalized  * radius,
            new Vector3(0, -1, t).normalized  * radius,
            new Vector3(0, 1, t).normalized   * radius,
            new Vector3(0, -1, -t).normalized * radius,
            new Vector3(0, 1, -t).normalized  * radius,
            new Vector3(t, 0, -1).normalized  * radius,
            new Vector3(t, 0, 1).normalized   * radius,
            new Vector3(-t, 0, -1).normalized * radius,
            new Vector3(-t, 0, 1).normalized  * radius
            };

            triangles = new int[] {
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,
            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,
            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,
            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };
        }

        public Vector3[] GetVertices() => vertices;

        public int[] GetTriangles() => triangles;

        public int GetFaceCount() => triangles.Length / 3;
    }
}

