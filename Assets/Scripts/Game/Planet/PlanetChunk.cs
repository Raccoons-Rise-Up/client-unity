using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KRU.Game
{
    public class PlanetChunk
    {
        public GameObject GameObject { get; }
        public Mesh Mesh { get; private set; }
        public Edge RedEdge { get; private set; }
        public Edge GreenEdge { get; private set; }
        public Edge BlueEdge { get; private set; }
        public Edge[] Edges { get; private set; }

        public Vector3[] Vertices { get; set; }
        public Color[] Colors { get; set; }
        public Triangle[] Triangles { get; private set; }
        public int Subdivisions { get; }

        public PlanetChunk(GameObject _gameObject, Vector3[] _Vertices, int _Subdivisions)
        {
            GameObject = _gameObject;

            Subdivisions = Mathf.Max(0, _Subdivisions);

            var vertexCount = 1 + (2 + (int)Mathf.Pow(2, Subdivisions) + 1) * ((int)Mathf.Pow(2, Subdivisions)) / 2;

            int triIndexCount = (int)Mathf.Pow(4, Subdivisions);
            if (Subdivisions == 0)
                triIndexCount = 1;

            Triangles = new Triangle[triIndexCount];
            Vertices = new Vector3[vertexCount];
            Colors = new Color[vertexCount];

            var vertexIndex = 0;

            Vertices[vertexIndex++] = _Vertices[0];
            Vertices[vertexIndex++] = _Vertices[1];
            Vertices[vertexIndex++] = _Vertices[2];

            // Create Edges
            var edgeIndex = 0;
            Edges = new Edge[3];
            CreateEdge(ref vertexIndex, ref edgeIndex, 0, 1);
            CreateEdge(ref vertexIndex, ref edgeIndex, 2, 1);
            CreateEdge(ref vertexIndex, ref edgeIndex, 0, 2);

            // Create Inner Points
            var numRows = Edges[0].Vertices.Length - 3;
            var rows = new Row[numRows];

            if (Subdivisions > 1) 
            {
                var rowIndex = 0;
                CreateInnerPoints(ref vertexIndex, ref rowIndex, ref rows);
            }  

            // Triangles
            Triangulate(rows);
        }

        public void GenerateMesh(Material material)
        {
            Mesh = new Mesh();
            Mesh.name = "Chunk";
            Mesh.vertices = Vertices;

            int[] triIndices = new int[Triangles.Length * 3];
            for (int i = 0; i < Triangles.Length; i++) 
            {
                triIndices[i * 3] = Triangles[i].A;
                triIndices[i * 3 + 1] = Triangles[i].B;
                triIndices[i * 3 + 2] = Triangles[i].C;
            }
            Mesh.triangles = triIndices;
            
            Mesh.colors = Colors;

            //Mesh.RecalculateNormals();
            Mesh.normals = Mesh.vertices.Select(s => s.normalized).ToArray();

            GameObject.GetComponent<MeshFilter>().sharedMesh = Mesh;
            GameObject.GetComponent<MeshRenderer>().material = material;

            var collider = GameObject.AddComponent<MeshCollider>();
            collider.sharedMesh = Mesh;
        }

        private void Triangulate(Row[] rows)
        {
            RedEdge = Edges[0];
            GreenEdge = Edges[1];
            BlueEdge = Edges[2];
            var lastEdgeVertex = Edges[0].Vertices.Length - 1; // All Edges have the same vertex count
            var triIndex = 0;

            if (Subdivisions == 0)
            {
                AddTriangle(ref triIndex, 0, 1, 2);
                return;
            }

            // TRIANGLES WITH NO PATTERNS
            // Top Triangle
            AddTriangle(ref triIndex, BlueEdge.Vertices[1], 0, RedEdge.Vertices[1]);

            // First Triangle from Bottom Left
            AddTriangle(ref triIndex, BlueEdge.Vertices[lastEdgeVertex], BlueEdge.Vertices[lastEdgeVertex - 1], GreenEdge.Vertices[1]);
            // First Triangle from Bottom Right
            AddTriangle(ref triIndex, GreenEdge.Vertices[lastEdgeVertex - 1], RedEdge.Vertices[lastEdgeVertex - 1], RedEdge.Vertices[lastEdgeVertex]);

            if (Subdivisions == 1)
                AddTriangle(ref triIndex, GreenEdge.Vertices[1], BlueEdge.Vertices[1], RedEdge.Vertices[1]);

            if (Subdivisions < 2)
                return;

            // Tri just below top tri
            AddTriangle(ref triIndex, rows[0].Vertices[0], BlueEdge.Vertices[1], RedEdge.Vertices[1]); // Not included with pattern because of RedEdge

            // First Inner Row Triangle
            AddTriangle(ref triIndex, rows[1].Vertices[0], rows[0].Vertices[0], rows[1].Vertices[1]);

            // Second Triangle from Bottom Left
            AddTriangle(ref triIndex, GreenEdge.Vertices[1], BlueEdge.Vertices[lastEdgeVertex - 1], rows[rows.Length - 1].Vertices[0]); // Not included with pattern because of BlueEdge
                                                                                                                                     // Second Triangle from Bottom Right
            AddTriangle(ref triIndex, GreenEdge.Vertices[lastEdgeVertex - 1], rows[rows.Length - 1].Vertices[rows[rows.Length - 1].Vertices.Count - 1], RedEdge.Vertices[lastEdgeVertex - 1]); // Not included with pattern because of RedEdge

            // TRIANGLES WITH PATTERNS
            BottomRowTriangles(ref triIndex, rows);
            LeftRowTriangles(ref triIndex, rows);
            RightRowTriangles(ref triIndex, rows);
            InnerRowTriangles(ref triIndex, rows);
        }

        private void LeftRowTriangles(ref int triIndex, Row[] rows)
        {
            for (int i = 0; i < BlueEdge.Vertices.Length - 3; i++)
                AddTriangle(ref triIndex, BlueEdge.Vertices[2 + i], BlueEdge.Vertices[1 + i], rows[i].Vertices[0]); // 1st tri top to bottom

            for (int i = 0; i < BlueEdge.Vertices.Length - 4; i++)
                AddTriangle(ref triIndex, rows[i + 1].Vertices[0], BlueEdge.Vertices[2 + i], rows[i].Vertices[0]); // 2nd tri top to bottom
        }

        private void RightRowTriangles(ref int triIndex, Row[] rows)
        {
            for (int i = 0; i < RedEdge.Vertices.Length - 3; i++) // Upside Triangles
                AddTriangle(ref triIndex, rows[i].Vertices[rows[i].Vertices.Count - 1], RedEdge.Vertices[1 + i], RedEdge.Vertices[2 + i]);

            for (int i = 0; i < RedEdge.Vertices.Length - 4; i++) // Upside Down Triangles
                AddTriangle(ref triIndex, rows[i].Vertices[rows[i].Vertices.Count - 1], RedEdge.Vertices[2 + i], rows[i + 1].Vertices[rows[i + 1].Vertices.Count - 1]);
        }

        private void BottomRowTriangles(ref int triIndex, Row[] rows)
        {
            // Add Triangles from left to right filling in middle
            for (int i = 0; i < rows[rows.Length - 1].Vertices.Count; i++) // Upside Triangles
                AddTriangle(ref triIndex, GreenEdge.Vertices[1 + i], rows[rows.Length - 1].Vertices[i], GreenEdge.Vertices[2 + i]);

            for (int i = 0; i < rows[rows.Length - 1].Vertices.Count - 1; i++) // Upside Down Triangles
                AddTriangle(ref triIndex, GreenEdge.Vertices[i + 2], rows[rows.Length - 1].Vertices[i], rows[rows.Length - 1].Vertices[i + 1]);
        }

        private void InnerRowTriangles(ref int triIndex, Row[] rows)
        {
            // Second Row and beyond
            for (int r = 1; r < rows.Length - 1; r++)
            {
                for (int i = 0; i < rows[r].Vertices.Count; i++) // Upside Triangles
                    AddTriangle(ref triIndex, rows[r + 1].Vertices[i], rows[r].Vertices[i], rows[r + 1].Vertices[1 + i]);

                for (int i = 0; i < rows[r].Vertices.Count - 1; i++) // Upside Down Triangles
                    AddTriangle(ref triIndex, rows[r + 1].Vertices[1 + i], rows[r].Vertices[i], rows[r].Vertices[1 + i]);
            }
        }

        /*!
         * Creates a edge with a start vertex, end vertex and the inner Vertices also
         * known as the number of inner edge divisions.
         */

        private void CreateEdge(ref int vertexIndex, ref int edgeIndex, int start, int end)
        {
            var divisions = Mathf.Max(0, (int)Mathf.Pow(2, Subdivisions) - 1);
            var innerEdgeIndices = new int[divisions];

            for (int i = 0; i < divisions; i++)
            {
                float t = (i + 1f) / (divisions + 1f);
                var vertex = Vector3.Lerp(Vertices[start], Vertices[end], t); // Calculate inner Vertices
                Vertices[vertexIndex++] = (vertex); // Add inner edge Vertices to total array of chunk Vertices
                innerEdgeIndices[i] = vertexIndex - 1; // For later reference when populating edgeIndices
            }

            // Populate edge indices for later reference
            var edgeIndicies = new int[divisions + 2]; // Edge indicies include start + end + inner indices

            edgeIndicies[0] = start; // Populate start vertex

            for (int i = 0; i < divisions; i++) // Populate inner Vertices
                edgeIndicies[i + 1] = innerEdgeIndices[i];

            edgeIndicies[edgeIndicies.Length - 1] = end; // Populate end vertex

            Edges[edgeIndex++] = new Edge(edgeIndicies);
        }

        /*!
         * Creates the Vertices inside the triangle that do not touch any outside edge.
         */

        private void CreateInnerPoints(ref int vertexIndex, ref int rowIndex, ref Row[] rows)
        {
            for (int i = 0; i < rows.Length; i++)
            {
                var sideA = Edges[2]; // Vertices in sideA created from bottom to top
                var sideB = Edges[0]; // Vertices in sideB created from top to bottom

                var row = new Row();
                var numColumns = i + 1;
                for (int j = 0; j < numColumns; j++)
                {
                    var t = (j + 1f) / (numColumns + 1f);

                    // Create inner point
                    // [sideA.vertexIndices.Length - 3 - i] We subtract 3 to skip over "end" vertex and the first row.
                    // [2 + i] to skip over "start" vertex and the first row.
                    Vertices[vertexIndex++] = (Vector3.Lerp(Vertices[sideA.Vertices[2 + i]], Vertices[sideB.Vertices[2 + i]], t));
                    row.AddTriangle(vertexIndex - 1);
                }
                rows[rowIndex++] = row;
            }
        }

        private void AddTriangle(ref int triIndex, int a, int b, int c)
        {
            Triangles[triIndex++] = new Triangle(a, b, c);
        }
    }

    public struct Triangle
    {
        public int A { get; set; }
        public int B { get; set; }
        public int C { get; set; }

        public Triangle(int _a, int _b, int _c)
        {
            A = _a;
            B = _b;
            C = _c;
        }
    }

    // A Edge counts both the start and end vertex as well as all the Vertices in between.
    public class Edge
    {
        public int[] Vertices; // Referenced by index in EdgeChunk.Vertices

        public Edge(int[] _Vertices)
        {
            Vertices = _Vertices;
        }
    }

    // A Row does not count the outer Vertices touching the outer Edges.
    public class Row
    {
        public List<int> Vertices = new List<int>(); // Referenced by index in EdgeChunk.Vertices

        public void AddTriangle(int _vertex)
        {
            Vertices.Add(_vertex);
        }
    }
}

