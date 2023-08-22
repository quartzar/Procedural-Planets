using UnityEngine;

namespace NoiseSettings
{
    [System.Serializable]
    public class SimpleNoiseSettings 
    {
        public Vector3 offset = new Vector3(0, 0, 0);
        [Range(1, 8)] public int numLayers = 1;
        [Range(0, 0.75f)] public float scale = 0.5f;
        public float persistence = 0.5f;
        public float lacunarity = 2;
        public float elevation = 1;
        public float verticalShift = 0;
        
        public void SetComputeValues(ComputeShader shader, string varSuffix)
        {
            // Debug.Log("Elevation is " + elevation + " and vertical shift is " + verticalShift + " for " + varSuffix + "");
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
                verticalShift
            };
            shader.SetFloats(Shader.PropertyToID("noiseParams" + varSuffix), noiseParams);
        }
    }
}