using UnityEngine;

namespace KRU.Game
{
    public class RidgidNoiseFilter : INoiseFilter
    {
        private NoiseSettings.RidgidNoiseSettings settings;
        private Noise noise = new Noise();

        public RidgidNoiseFilter(NoiseSettings.RidgidNoiseSettings settings)
        {
            this.settings = settings;
        }

        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0;
            float frequency = settings.frequency;
            float amplitude = 1;
            float weight = 1;

            for (int i = 0; i < settings.octaves; i++)
            {
                float v = 1 - Mathf.Abs(noise.Evaluate(point * frequency + settings.centre));
                v *= v;
                v *= weight;
                weight = Mathf.Clamp01(v * settings.weightMultiplier);

                frequency *= 2;
                amplitude *= settings.amplitude;

                noiseValue += v * amplitude;
            }

            noiseValue -= settings.minValue;
            return noiseValue * settings.strength;
        }
    }
}