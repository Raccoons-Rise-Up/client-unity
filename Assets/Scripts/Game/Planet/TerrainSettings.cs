using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KRU.Game
{
    [CreateAssetMenu()]
    public class TerrainSettings : ScriptableObject
    {
        [Tooltip("The radius of the planet")]
        public float radius = 1;
        [Tooltip("The number of chunk subdivisions")]
        public int chunkSubdivisions = 1;
        [Tooltip("The number of triangle subdivisions per chunk")]
        public int chunkTriangleSubdivisions = 1;
        [Tooltip("The material of the planet")]
        public Material material;

        public BiomeStyle biomeStyle;

        public NoiseSettings biomeNoise;
        public NoiseSettings biomeNoiseLine;

        public Biome[] biomes;
    }

    [System.Serializable]
    public class Biome 
    {
        public NoiseSettings terrainNoise;
    }

    public enum BiomeStyle
    {
        Frequency,
        XY,
        Continent
    }
}