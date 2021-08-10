using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KRU.Game
{
    public class PlanetRenderer : MonoBehaviour
    {
        // Terrain Settings
        public TerrainSettings terrainSettings;
        [HideInInspector] public bool terrainSettingsFoldout;

        // Ocean Settings
        public OceanSettings oceanSettings;
        [HideInInspector] public bool oceanSettingsFoldout;

        // Models
        public Transform treeModel;
        //public Transform grassModel;

        // Tree Settings
        public NoiseSettings treeNoiseSettings;

        private int chunkNameIndex;

        /// <summary>
        /// 
        /// </summary>
        public void GenerateTerrain()
        {
            Destroy();

            var subdividedIcosahedron = new IcosahedronSubdivided(terrainSettings.chunkSubdivisions);
            var chunkData = subdividedIcosahedron.chunkData;

            chunkNameIndex = 0;

            var chunks = new PlanetChunk[chunkData.Length];

            // Populate chunks array
            var chunksIndex = 0;
            var toggle = true;
            for (int i = 0; i < chunkData.Length; i++)
            {
                if (toggle) // For expermenting with edge stitching
                    PrepareChunk(ref chunksIndex, chunks, chunkData[i], terrainSettings.chunkTriangleSubdivisions);
                else
                    PrepareChunk(ref chunksIndex, chunks, chunkData[i], terrainSettings.chunkTriangleSubdivisions);

                toggle = !toggle;
            }


            BiomesAndNoise(chunks);


            IcosahedronSubdivided.StitchEdges(chunks);

            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i].GenerateMesh(terrainSettings.material);
            }

            IcosahedronSubdivided.FixNormalEdges(chunks);

            var treeNoise = NoiseFilterFactory.CreateNoiseFilter(treeNoiseSettings);

            // Spawn Trees
            for (int i = 0; i < chunks.Length; i++)
            {
                for (int j = 0; j < chunks[i].Vertices.Length; j++)
                {
                    var noiseValue = treeNoise.Evaluate(chunks[i].Vertices[j]);

                    // steepness returns value between 0 and 90
                    var steepness = Vector3.Angle((chunks[i].Vertices[j] - new Vector3(0, 0, 0)).normalized, chunks[i].Mesh.normals[j]);

                    // check if on biome with chunks[i].Mesh.colors[j].g == 1
                    if (steepness < 45) // Check steepness
                    {

                        if ((chunks[i].Vertices[j] - new Vector3(0, 0, 0)).magnitude > terrainSettings.radius + 0.25f) // Don't spawn in the water
                        {
                            if (noiseValue > 0.5f)
                            {
                                SpawnTree(chunks, i, j);
                            }
                            else
                            {
                                if (Random.value < 0.03f)
                                {
                                    SpawnTree(chunks, i, j);
                                }
                            }
                        }
                    }
                }
            }

            GenerateOcean();
        }

        private void BiomesAndNoise(PlanetChunk[] chunks) 
        {
            var terrainNoise = new INoiseFilter[terrainSettings.biomes.Length];
            for (int i = 0; i < terrainSettings.biomes.Length; i++)
            {
                terrainNoise[i] = NoiseFilterFactory.CreateNoiseFilter(terrainSettings.biomes[i].terrainNoise);
            }

            var biome = NoiseFilterFactory.CreateNoiseFilter(terrainSettings.biomeNoise);
            var biomeBlendLine = NoiseFilterFactory.CreateNoiseFilter(terrainSettings.biomeNoiseLine);

            if (terrainSettings.biomes.Length == 0)
            {
                Debug.LogWarning("At least one biome must be defined.");
                return;
            }

            if (terrainSettings.biomeStyle == BiomeStyle.Continent && terrainSettings.biomes.Length < 3)
            {
                Debug.LogWarning("Continent biome style requires exactly 3 biomes.");
                return;
            }

            for (int i = 0; i < chunks.Length; i++)
            {
                var vertices = chunks[i].Vertices;
                var colors = chunks[i].Colors;

                if (terrainSettings.biomeStyle == BiomeStyle.Frequency)
                {
                    BiomesSameFrequency(biomeBlendLine, biome, terrainNoise, ref vertices, ref colors, terrainSettings.biomes.Length);
                }

                if (terrainSettings.biomeStyle == BiomeStyle.XY)
                {
                    XYBiomes(biomeBlendLine, terrainNoise, ref vertices, ref colors, terrainSettings.biomes.Length, true);
                }

                if (terrainSettings.biomeStyle == BiomeStyle.Continent)
                {
                    BiomesContinentStyleFade(biomeBlendLine, biome, terrainNoise, ref vertices, ref colors);
                }

                chunks[i].Vertices = vertices;
                chunks[i].Colors = colors;
            }
        }

        public void GenerateOcean() 
        {
            ChunkData[] oceanChunkData;

            // Ocean
            var ocean = new IcosahedronSubdivided(0);
            oceanChunkData = ocean.chunkData;

            for (int i = 0; i < oceanChunkData.Length; i++) 
            {
                var chunkObj = new GameObject();
                chunkObj.name = $"{chunkNameIndex++}";
                chunkObj.transform.SetParent(transform);
                chunkObj.AddComponent<MeshFilter>();
                var meshRenderer = chunkObj.AddComponent<MeshRenderer>();
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                var chunk = new PlanetChunk(chunkObj, oceanChunkData[i].Vertices, terrainSettings.chunkTriangleSubdivisions);

                for (int j = 0; j < chunk.Vertices.Length; j++) 
                {
                    chunk.Vertices[j] = chunk.Vertices[j].normalized * (terrainSettings.radius + oceanSettings.height);
                }

                chunk.GenerateMesh(oceanSettings.material);
                oceanChunkData[i].chunk = chunk;
            }
        }

        /*
         * Same frequency.
         */
        // terrainNoise[0] RED
        // terrainNoise[1] GREEN
        // terrainNoise[2] BLUE
        // terrainNoise[3] BLACK
        private void BiomesSameFrequency(INoiseFilter biomeBlendLine, INoiseFilter biome, INoiseFilter[] terrainNoise, ref Vector3[] vertices, ref Color[] colors, int numBiomes)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = vertices[i].normalized;

                var biomeNoise = biome.Evaluate(vertices[i]);
                var blendLineNoise = biomeBlendLine.Evaluate(vertices[i]);
                var totalNoise = 0f;

                if (numBiomes >= 1)
                {
                    colors[i] = Color.red;
                    totalNoise = terrainNoise[0].Evaluate(vertices[i]);
                }

                if (numBiomes >= 2)
                {
                    if (biomeNoise < 0.6f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.6f + blendLineNoise, 0.5f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.red, Color.green, t);
                        totalNoise = Mathf.Lerp(terrainNoise[0].Evaluate(vertices[i]), terrainNoise[1].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.5f + blendLineNoise)
                    {
                        colors[i] = Color.green;
                        totalNoise = terrainNoise[1].Evaluate(vertices[i]);
                    }
                }

                if (numBiomes >= 3)
                {
                    if (biomeNoise < 0.4f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.4f + blendLineNoise, 0.3f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.green, Color.blue, t);
                        totalNoise = Mathf.Lerp(terrainNoise[1].Evaluate(vertices[i]), terrainNoise[2].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.3f + blendLineNoise)
                    {
                        colors[i] = Color.blue;
                        totalNoise = terrainNoise[2].Evaluate(vertices[i]);
                    }
                }

                if (numBiomes >= 4)
                {
                    if (biomeNoise < 0.2f + blendLineNoise)
                    {
                        var t = Utils.Utils.Remap(biomeNoise, 0.2f + blendLineNoise, 0.1f + blendLineNoise, 0, 1);
                        colors[i] = Color.Lerp(Color.blue, Color.black, t);
                        totalNoise = Mathf.Lerp(terrainNoise[2].Evaluate(vertices[i]), terrainNoise[3].Evaluate(vertices[i]), t);
                    }

                    if (biomeNoise < 0.1f + blendLineNoise)
                    {
                        colors[i] = Color.black;
                        totalNoise = terrainNoise[3].Evaluate(vertices[i]);
                    }
                }

                vertices[i] = vertices[i].normalized * (terrainSettings.radius + totalNoise);
            }
        }

        /*
         * XY Biomes.
         */
        private void XYBiomes(INoiseFilter biomeBlendLine, INoiseFilter[] terrainNoise, ref Vector3[] vertices, ref Color[] colors, int numBiomes, bool vert)
        {
            var biomeColors = new Color[] { Color.red, Color.green, Color.blue, Color.black };

            for (int i = 0; i < vertices.Length; i++)
            {
                var blendLineNoise = biomeBlendLine.Evaluate(vertices[i]);

                float totalNoise;

                var axis = vert ? vertices[i].y : vertices[i].x;

                var height = Utils.Utils.Remap(axis, -1, 1, 0, 0.99f);
                var biomeIndex = Mathf.FloorToInt(height * (numBiomes));
                biomeIndex = Mathf.Clamp(biomeIndex, 0, numBiomes);

                colors[i] = biomeColors[biomeIndex]; // Biome Colors
                totalNoise = terrainNoise[biomeIndex].Evaluate(vertices[i]);

                var biome = height * numBiomes;

                // Blending Zones
                for (int n = 1; n < numBiomes; n++)
                {
                    if (biome > n - blendLineNoise && biome < n + blendLineNoise)
                    {
                        var biomeLine = -1 + ((float)n / numBiomes) * 2;
                        var t = Utils.Utils.Remap(axis, biomeLine - blendLineNoise / numBiomes, biomeLine + blendLineNoise / numBiomes, 0, 1);
                        colors[i] = Color.Lerp(biomeColors[n - 1], biomeColors[n], t);
                        totalNoise = Mathf.Lerp(terrainNoise[n - 1].Evaluate(vertices[i]), terrainNoise[n].Evaluate(vertices[i]), t);
                    }
                }

                vertices[i] = vertices[i].normalized * (terrainSettings.radius + totalNoise);
            }
        }

        /*
         * All red.
         */
        private void OneBiome(INoiseFilter[] terrainNoise, ref Vector3[] vertices, ref Color[] colors)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                colors[i] = Color.red;
                var totalNoise = terrainNoise[0].Evaluate(vertices[i]);
                vertices[i] = vertices[i].normalized * (terrainSettings.radius + totalNoise);
            }
        }

        /*
         * 50% red near top, 50% green near bottom, surrounded by blue.
         */
        private void BiomesContinentStyleFade(INoiseFilter biomeBlendLine, INoiseFilter biome, INoiseFilter[] terrainNoise, ref Vector3[] vertices, ref Color[] colors)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                var blendLineNoise = biomeBlendLine.Evaluate(vertices[i]);

                float totalNoise;

                var biomeNoise = biome.Evaluate(vertices[i]);
                var blendLine = biomeBlendLine.Evaluate(vertices[i]);

                if (biomeNoise > 0.5f)
                {
                    var percentBlend = Mathf.InverseLerp(0.5f, 1.0f, biomeNoise);

                    // 3 Biomes
                    // Blend blue with redGreen
                    var percentBlendLine = Mathf.InverseLerp(-blendLineNoise, blendLineNoise, vertices[i].y + blendLine);
                    var r = 1 - percentBlendLine;
                    var g = percentBlendLine;

                    var redGreen = new Color(r, g, 0, 0);
                    var redGreenNoise = Mathf.Lerp(terrainNoise[0].Evaluate(vertices[i]), terrainNoise[1].Evaluate(vertices[i]), percentBlendLine);

                    totalNoise = Mathf.Lerp(terrainNoise[2].Evaluate(vertices[i]), redGreenNoise, percentBlend);
                    colors[i] = Color.Lerp(Color.blue, redGreen, percentBlend);
                }
                else
                {
                    totalNoise = terrainNoise[2].Evaluate(vertices[i]);
                    colors[i] = Color.blue;
                }

                vertices[i] = vertices[i].normalized * (terrainSettings.radius + totalNoise);
            }
        }

        private void SpawnTree(PlanetChunk[] chunks, int chunkIndex, int vertexIndex)
        {
            var tree = Instantiate(treeModel, chunks[chunkIndex].GameObject.transform);
            tree.position = chunks[chunkIndex].Vertices[vertexIndex];
            tree.rotation = Quaternion.LookRotation(chunks[chunkIndex].Vertices[vertexIndex]);
            tree.Rotate(new Vector3(90, 0, 0));
        }

        private void PrepareChunk(ref int chunksIndex, PlanetChunk[] chunks, ChunkData _chunkData, int _chunkTriangles)
        {
            var chunkObj = new GameObject();
            chunkObj.name = $"{chunkNameIndex++}";
            chunkObj.transform.SetParent(transform);
            chunkObj.AddComponent<MeshFilter>();
            chunkObj.AddComponent<MeshRenderer>();

            var chunk = new PlanetChunk(chunkObj, _chunkData.Vertices, _chunkTriangles);
            _chunkData.chunk = chunk;

            chunks[chunksIndex++] = chunk;
        }

        // Destroy procedurally generated meshes for file size reduction. Called in `DestroyOnSave.cs`
        public void Destroy()
        {
            while (transform.childCount > 0)
                DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
