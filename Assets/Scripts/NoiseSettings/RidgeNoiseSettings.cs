using UnityEngine;

namespace NoiseSettings
{
    [System.Serializable]
    public class RidgeNoiseSettings 
    {
        public Vector3 offset = new Vector3(0, 0, 0);
        [Range(1, 8)] public int numLayers = 1;
        [Range(0, 0.75f)] public float scale = 0.5f;
        public float persistence = 0.5f;
        public float lacunarity = 2;
        public float elevation = 1;
        public float power = 2;
        public float gain = 1;
        public float verticalShift = 0;
        public float peakSmoothing = 0;
        
        public void SetComputeValues(ComputeShader shader, string varSuffix)
        {
            float[] noiseParams = {
                // [0]
                offset.x,
                offset.y,
                offset.z,
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