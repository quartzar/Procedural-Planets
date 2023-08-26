using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shape {
    public abstract class CelestialBodyShape : ScriptableObject
    {
        public int seed;
        
        public ComputeShader heightMapCompute;
        public ComputeShader perturbCompute;
        
        public bool perturbVertices;
        [Range(0, 1)]
        public float perturbStrength = 0.75f;
        
        
        private ComputeBuffer _heightBuffer;
        
        // Cached shader property ID's for performance
        private static readonly int NumVertices = Shader.PropertyToID("numVertices");
        private static readonly int Vertices = Shader.PropertyToID("vertices");
        private static readonly int Heights = Shader.PropertyToID("heights");
        private static readonly int PerturbStrength = Shader.PropertyToID("perturbStrength");
        
        public virtual float[] ComputeHeights(ComputeBuffer vertexBuffer)
        {
            SetShapeData();
            
            int kernelHandle = heightMapCompute.FindKernel("CSMain");
            int numVertices = vertexBuffer.count;
            
            _heightBuffer = new ComputeBuffer(numVertices, sizeof(float));
            
            heightMapCompute.SetInt(NumVertices, numVertices);
            heightMapCompute.SetBuffer(kernelHandle, Vertices, vertexBuffer);
            heightMapCompute.SetBuffer(kernelHandle, Heights, _heightBuffer);
            
            // TODO: create a ComputeHelper class to handle this
            int threadsPerGroup = 512;
            int numThreadGroups = Mathf.CeilToInt(numVertices / (float)threadsPerGroup);
            heightMapCompute.Dispatch(kernelHandle, numThreadGroups, 1, 1);
            
            float[] heights = new float[numVertices];
            _heightBuffer.GetData(heights);
            
            if (!perturbVertices) { vertexBuffer.Release(); }
            // vertexBuffer.Release();
            _heightBuffer.Release();
            
            return heights;
        }
        
        public virtual void PerturbVertices(ComputeBuffer vertexBuffer, ref Vector3[] vertices)
        {
            if (!perturbVertices) return;
            
            int kernelHandle = perturbCompute.FindKernel("CSMain");
            int numVertices = vertexBuffer.count;
            
            perturbCompute.SetInt(NumVertices, numVertices);
            perturbCompute.SetFloat(PerturbStrength, perturbStrength * 0.1f);
            perturbCompute.SetBuffer(kernelHandle, Vertices, vertexBuffer);
            
            int threadsPerGroup = 512;
            int numThreadGroups = Mathf.CeilToInt(numVertices / (float)threadsPerGroup);
            perturbCompute.Dispatch(kernelHandle, numThreadGroups, 1, 1);
            
            vertexBuffer.GetData(vertices);
            vertexBuffer.Release();
        }
        
        protected virtual void SetShapeData() {}
    }
}
