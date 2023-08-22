using UnityEngine;

namespace NoiseSettings
{
    [System.Serializable]
    public class RidgeNoiseSettings 
    {
        public Vector3 offset = new Vector3(0, 0, 0);
        [Range(1, 8)] public int numLayers = 1;
        [Range(0, 10)] public float scale = 0.5f;
        public float persistence = 0.5f;
        public float lacunarity = 2;
        public float elevation = 1;
        public float power = 2;
        public float gain = 1;
        public float verticalShift = 0;
        public float peakSmoothing = 0;
        
        public void SetComputeValues(ComputeShader shader, int seed, string varSuffix)
        {
            Random.InitState(seed);
            Vector3 seedOffset = new Vector3(Random.value, Random.value, Random.value) * (Random.value * 10000);
            
            float[] noiseParams = {
                // [0]
                offset.x + seedOffset.x,
                offset.y + seedOffset.y,
                offset.z + seedOffset.z,
                numLayers,
                // [1]
                scale,
                persistence,
                lacunarity,
                elevation,
                // [2]
                power,
                gain,
                verticalShift,
                peakSmoothing
            };
            shader.SetFloats(Shader.PropertyToID("noiseParams" + varSuffix), noiseParams);
        }
    }
}