using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shape {
    public abstract class CelestialBodyShape : ScriptableObject
    {
        public ComputeShader heightMapCompute;
        
        private ComputeBuffer _heightBuffer;
        
        // Cached shader property ID's for performance
        private static readonly int NumVertices = Shader.PropertyToID("numVertices");
        private static readonly int Vertices = Shader.PropertyToID("vertices");
        private static readonly int Heights = Shader.PropertyToID("heights");
        
        public virtual float[] ComputeHeights(ComputeBuffer vertexBuffer)
        {
            SetShapeData();
            
            int kernelHandle = heightMapCompute.FindKernel("CSMain");
            int numVertices = vertexBuffer.count;
            
            heightMapCompute.SetInt(NumVertices, numVertices);
            heightMapCompute.SetBuffer(kernelHandle, Vertices, vertexBuffer);
            heightMapCompute.SetBuffer(kernelHandle, Heights, _heightBuffer);
            
            // TODO: create a ComputeHelper class to handle this
            int threadsPerGroup = 512;
            int numThreadGroups = Mathf.CeilToInt(numVertices / (float)threadsPerGroup);
            heightMapCompute.Dispatch(kernelHandle, numThreadGroups, 1, 1);
            
            float[] heights = new float[numVertices];
            _heightBuffer.GetData(heights);
            
            vertexBuffer.Release();
            _heightBuffer.Release();
            
            return heights;
        }
        
        protected virtual void SetShapeData() {}
    }
}
