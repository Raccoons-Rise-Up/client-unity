using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KRU.Game
{
    public class IcosahedronSubdivided
    {
        public ChunkData[] chunkData;

        /*private readonly int[,] baseFormNeighbors = new int[20, 3]
        {
        { 1,  4, 6   },
        { 0,  2, 5   },
        { 1,  3, 9   },
        { 2,  4, 8   },
        { 0,  3, 7   },
        { 1,  15, 19 },
        { 0,  15, 16 },
        { 4,  16, 17 },
        { 3,  17, 18 },
        { 2,  18, 19 },
        { 11, 14, 15 },
        { 10, 12, 16 },
        { 11, 13, 17 },
        { 12, 14, 18 },
        { 10, 13, 19 },
        { 5,  6,  10 },
        { 6,  7,  11 },
        { 7,  8,  12 },
        { 8,  9,  13 },
        { 5,  9,  14 }
        };*/

        public IcosahedronSubdivided(int _chunkSubdivisions = 0)
        {
            chunkData = new ChunkData[20 * (int)Mathf.Pow(4, _chunkSubdivisions)];
            var Vertices = new Icosahedron().GetVertices();
            var chunkDataIndex = 0;

            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[0], Vertices[11], Vertices[5] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[0], Vertices[5], Vertices[1] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[0], Vertices[1], Vertices[7] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[0], Vertices[7], Vertices[10] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[0], Vertices[10], Vertices[11] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[1], Vertices[5], Vertices[9] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[5], Vertices[11], Vertices[4] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[11], Vertices[10], Vertices[2] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[10], Vertices[7], Vertices[6] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[7], Vertices[1], Vertices[8] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[3], Vertices[9], Vertices[4] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[3], Vertices[4], Vertices[2] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[3], Vertices[2], Vertices[6] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[3], Vertices[6], Vertices[8] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[3], Vertices[8], Vertices[9] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[4], Vertices[9], Vertices[5] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[2], Vertices[4], Vertices[11] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[6], Vertices[2], Vertices[10] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[8], Vertices[6], Vertices[7] }, _chunkSubdivisions);
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { Vertices[9], Vertices[8], Vertices[1] }, _chunkSubdivisions);
        }

        private void InitializeChunks(ref int chunkDataIndex, List<Vector3> _Vertices, int n)
        {
            // No Subdivisions
            if (n <= 0)
            {
                InitializeChunk(ref chunkDataIndex, _Vertices[0], _Vertices[1], _Vertices[2]);

                return;
            }

            _Vertices.Add(SphereUtils.GetMidPointVertex(_Vertices[0], _Vertices[1])); // Right Middle (3)
            _Vertices.Add(SphereUtils.GetMidPointVertex(_Vertices[1], _Vertices[2])); // Bottom Middle (4)
            _Vertices.Add(SphereUtils.GetMidPointVertex(_Vertices[2], _Vertices[0])); // Left middle (5)

            // Only draw the last recursion
            if (n == 1)
            {
                InitializeChunk(ref chunkDataIndex, _Vertices[0], _Vertices[3], _Vertices[5]); // Top
                InitializeChunk(ref chunkDataIndex, _Vertices[5], _Vertices[4], _Vertices[2]); // Bottom Left
                InitializeChunk(ref chunkDataIndex, _Vertices[4], _Vertices[5], _Vertices[3]); // Bottom Middle
                InitializeChunk(ref chunkDataIndex, _Vertices[3], _Vertices[1], _Vertices[4]); // Bottom Right

                return;
            }

            InitializeChunks(ref chunkDataIndex, new List<Vector3> { _Vertices[0], _Vertices[3], _Vertices[5] }, n - 1); // Top
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { _Vertices[5], _Vertices[4], _Vertices[2] }, n - 1); // Bottom Left
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { _Vertices[4], _Vertices[5], _Vertices[3] }, n - 1); // Bottom Middle
            InitializeChunks(ref chunkDataIndex, new List<Vector3> { _Vertices[3], _Vertices[1], _Vertices[4] }, n - 1); // Bottom Right
        }

        private void InitializeChunk(ref int chunkDataIndex, params Vector3[] _Vertices)
        {
            var chunkCenter = SphereUtils.GetCenterPoint(_Vertices[0], _Vertices[1], _Vertices[2]);
            chunkData[chunkDataIndex++] = new ChunkData() { Vertices = _Vertices, center = chunkCenter };
        }

        // When mesh.RecalculateNormals() is used, ugly seams appear on each edge of a chunk
        // This can be fixed by taking the average of the 2 Vertices along the edges
        public static void FixNormalEdges(PlanetChunk[] chunks) 
        {
            /*for (int i = 0; i < chunks.Length; i++) 
            {
                var chunk = chunks[i];

                for (int j = 0; j < chunk.redEdge.Vertices.Length; j++) 
                {
                    var index = chunk.redEdge.Vertices[j];

                    var normal = chunk.mesh.normals[index];
                }
                
            }*/
        }

        #region Edges
        public static void StitchEdges(PlanetChunk[] chunks)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                var redEdgeIndices = chunks[i].RedEdge.Vertices;
                var greenEdgeIndices = chunks[i].GreenEdge.Vertices;
                var blueEdgeIndices = chunks[i].BlueEdge.Vertices;

                var chunkVertices = chunks[i].Vertices;

                var midpointBlue = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length / 2]];
                var midpointGreen = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length / 2]];
                var midpointRed = chunkVertices[redEdgeIndices[redEdgeIndices.Length / 2]];

                for (int k = 0; k < chunks.Length; k++)
                {
                    var redEdgeIndicesOther = chunks[k].RedEdge.Vertices;
                    var greenEdgeIndicesOther = chunks[k].GreenEdge.Vertices;
                    var blueEdgeIndicesOther = chunks[k].BlueEdge.Vertices;

                    var chunkVerticesOther = chunks[k].Vertices;

                    var midpointRedOther = chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length / 2]];
                    var midpointGreenOther = chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length / 2]];
                    var midpointBlueOther = chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length / 2]];

                    // Do not count itself (i != k)
                    // If the neighboring chunk has a higher subdivision count then move that neighboring edge Vertices to the chunk with lower Subdivisions
                    if (i != k && chunks[k].Subdivisions > chunks[i].Subdivisions)
                    {
                        if (chunks[i].Subdivisions == 0 || chunks[k].Subdivisions == 0)
                        {
                            // RR GG BB (For some reason when the edge colors are the same the stitching needs to be done in reverse)
                            // RR
                            var RR_L = chunkVertices[redEdgeIndices[0]] == chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length - 1]];
                            var RR_R = chunkVertices[redEdgeIndices[redEdgeIndices.Length - 1]] == chunkVerticesOther[redEdgeIndicesOther[0]];

                            if (RR_L && RR_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Red, EdgeColor.Red, i, k);
                            }

                            // GG
                            var GG_L = chunkVertices[greenEdgeIndices[0]] == chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length - 1]];
                            var GG_R = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length - 1]] == chunkVerticesOther[greenEdgeIndicesOther[0]];

                            if (GG_L && GG_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Green, EdgeColor.Green, i, k);
                            }

                            // BB
                            var BB_L = chunkVertices[blueEdgeIndices[0]] == chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length - 1]];
                            var BB_R = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length - 1]] == chunkVerticesOther[blueEdgeIndicesOther[0]];

                            if (BB_L && BB_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Blue, EdgeColor.Blue, i, k);
                            }

                            // RB
                            var RB_L = chunkVertices[redEdgeIndices[0]] == chunkVerticesOther[blueEdgeIndicesOther[0]];
                            var RB_R = chunkVertices[redEdgeIndices[redEdgeIndices.Length - 1]] == chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length - 1]];

                            if (RB_L && RB_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Red, EdgeColor.Blue, i, k);
                            }

                            // RG
                            var RG_L = chunkVertices[redEdgeIndices[0]] == chunkVerticesOther[greenEdgeIndicesOther[0]];
                            var RG_R = chunkVertices[redEdgeIndices[redEdgeIndices.Length - 1]] == chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length - 1]];

                            if (RG_L && RG_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Red, EdgeColor.Green, i, k);
                            }

                            // BR
                            var BR_L = chunkVertices[blueEdgeIndices[0]] == chunkVerticesOther[redEdgeIndicesOther[0]];
                            var BR_R = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length - 1]] == chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length - 1]];

                            if (BR_L && BR_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Blue, EdgeColor.Red, i, k);
                            }

                            // BG
                            var BG_L = chunkVertices[blueEdgeIndices[0]] == chunkVerticesOther[greenEdgeIndicesOther[0]];
                            var BG_R = chunkVertices[blueEdgeIndices[blueEdgeIndices.Length - 1]] == chunkVerticesOther[greenEdgeIndicesOther[greenEdgeIndicesOther.Length - 1]];

                            if (BG_L && BG_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Blue, EdgeColor.Green, i, k);
                            }

                            // GR
                            var GR_L = chunkVertices[greenEdgeIndices[0]] == chunkVerticesOther[redEdgeIndicesOther[0]];
                            var GR_R = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length - 1]] == chunkVerticesOther[redEdgeIndicesOther[redEdgeIndicesOther.Length - 1]];

                            if (GR_L && GR_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Green, EdgeColor.Red, i, k);
                            }

                            // GB
                            var GB_L = chunkVertices[greenEdgeIndices[0]] == chunkVerticesOther[blueEdgeIndicesOther[0]];
                            var GB_R = chunkVertices[greenEdgeIndices[greenEdgeIndices.Length - 1]] == chunkVerticesOther[blueEdgeIndicesOther[blueEdgeIndicesOther.Length - 1]];

                            if (GB_L && GB_R)
                            {
                                StitchChunkEdges(chunks, EdgeColor.Green, EdgeColor.Blue, i, k);
                            }
                        }
                        else
                        {
                            // RR GG BB (For some reason when the edge colors are the same the stitching needs to be done in reverse)
                            if (midpointRed == midpointRedOther)
                                StitchChunkEdges(chunks, EdgeColor.Red, EdgeColor.Red, i, k);

                            if (midpointGreen == midpointGreenOther)
                                StitchChunkEdges(chunks, EdgeColor.Green, EdgeColor.Green, i, k);

                            if (midpointBlue == midpointBlueOther)
                                StitchChunkEdges(chunks, EdgeColor.Blue, EdgeColor.Blue, i, k);

                            // RB RG
                            if (midpointRed == midpointBlueOther)
                                StitchChunkEdges(chunks, EdgeColor.Red, EdgeColor.Blue, i, k);

                            if (midpointRed == midpointGreenOther)
                                StitchChunkEdges(chunks, EdgeColor.Red, EdgeColor.Green, i, k);

                            // BR BG
                            if (midpointBlue == midpointRedOther)
                                StitchChunkEdges(chunks, EdgeColor.Blue, EdgeColor.Red, i, k);

                            if (midpointBlue == midpointGreenOther)
                                StitchChunkEdges(chunks, EdgeColor.Blue, EdgeColor.Green, i, k);

                            // GR GB
                            if (midpointGreen == midpointRedOther)
                                StitchChunkEdges(chunks, EdgeColor.Green, EdgeColor.Red, i, k);

                            if (midpointGreen == midpointBlueOther)
                                StitchChunkEdges(chunks, EdgeColor.Green, EdgeColor.Blue, i, k);
                        }
                    }
                }
            }
        }

        private static void StitchChunkEdges(PlanetChunk[] chunks, EdgeColor edgeCurrent, EdgeColor edgeNeighbor, int curIndex, int neighhorIndex)
        {
            var curChunk = chunks[curIndex];
            var neighborChunk = chunks[neighhorIndex];

            var neighborEdgeVertices = neighborChunk.Edges[(int)edgeNeighbor].Vertices;
            var neighborIndex = 0;

            for (int k = 0; k < curChunk.Edges[(int)edgeCurrent].Vertices.Length - 1; k++)
            {
                var A = curChunk.Vertices[curChunk.Edges[(int)edgeCurrent].Vertices[k]];
                var B = curChunk.Vertices[curChunk.Edges[(int)edgeCurrent].Vertices[k + 1]];

                // Get curChunk points
                int midpointCount = (int)Mathf.Pow(2, neighborChunk.Subdivisions - curChunk.Subdivisions);

                neighborIndex++;

                for (int i = 1; i < midpointCount; i++)
                {
                    float t = i / (float)midpointCount;

                    if (edgeCurrent != edgeNeighbor)
                    {
                        if (neighborChunk.Vertices[neighborEdgeVertices[neighborIndex]] != Vector3.Lerp(A, B, t))
                            neighborChunk.Vertices[neighborEdgeVertices[neighborIndex]] = Vector3.Lerp(A, B, t);
                    }
                    else
                    {
                        //new DebugPoint(neighborChunk.Vertices[neighborEdgeVertices[neighborEdgeVertices.Length - 1 - neighborIndex]], "Purple").SetSize(0.4f).SetColor(Color.magenta);
                        //new DebugPoint(Vector3.Lerp(A, B, t), "Green").SetSize(0.4f).SetColor(Color.green);

                        if (neighborChunk.Vertices[neighborEdgeVertices[neighborEdgeVertices.Length - 1 - neighborIndex]] != Vector3.Lerp(A, B, t))
                            neighborChunk.Vertices[neighborEdgeVertices[neighborEdgeVertices.Length - 1 - neighborIndex]] = Vector3.Lerp(A, B, t);
                    }

                    neighborIndex++;
                }
            }
        }

        public enum EdgeColor
        {
            Red,
            Green,
            Blue
        }
        #endregion
    }

    public class ChunkData
    {
        public Vector3[] Vertices;
        public Vector3 center;
        public PlanetChunk chunk;
    }
}
