using UnityEngine;

namespace KRU.Game
{
    [System.Serializable]
    public class NoiseSettings
    {
        public enum FilterType { Simple, Ridgid };

        public FilterType filterType;

        [ConditionalHide("filterType", 0)]
        public SimpleNoiseSettings simpleNoiseSettings;

        [ConditionalHide("filterType", 1)]
        public RidgidNoiseSettings ridgidNoiseSettings;

        [System.Serializable]
        public class SimpleNoiseSettings
        {
            public float strength = 1;
            public int octaves = 1;
            public float frequency = 2;
            public float amplitude = .5f;
            public float minValue;
            public Vector3 centre;
        }

        [System.Serializable]
        public class RidgidNoiseSettings : SimpleNoiseSettings
        {
            public float weightMultiplier = .8f;
        }
    }
}