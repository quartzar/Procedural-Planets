using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainFace
{
    private readonly NoiseSettings.SimpleNoiseSettings _noiseSettings;
    private readonly ShapeSettings _shapeSettings;
    private readonly Mesh _mesh;
    private readonly int _resolution;
    private readonly Vector3 _localUp;
    private readonly Vector3 _axisA;
    private readonly Vector3 _axisB;
    
    public TerrainFace(NoiseSettings.SimpleNoiseSettings noiseSettings, ShapeSettings shapeSettings, Mesh mesh, int resolution, Vector3 localUp)
    {
        this._noiseSettings = noiseSettings;
        this._shapeSettings = shapeSettings;
        this._mesh = mesh;
        this._resolution = resolution;
        this._localUp = localUp;
        
        _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        _axisB = Vector3.Cross(localUp, _axisA);
    }

    public void ConstructMesh()
    {
        Vector3[] vertices = new Vector3[_resolution * _resolution];
        int[] triangles = new int[(_resolution - 1) * (_resolution - 1) * 6];
        Vector2[] uv = (_mesh.uv.Length == vertices.Length) ? _mesh.uv : new Vector2[vertices.Length];
        int triIndex = 0;
        
        for (int y = 0; y < _resolution; y++)
        {
            for (int x = 0; x < _resolution; x++)
            {
                int i = x + y * _resolution;
                Vector2 percent = new Vector2(x, y) / (_resolution - 1);
                Vector3 pointOnCube = _localUp + (percent.x - 0.5f) * 2 * _axisA + (percent.y - 0.5f) * 2 * _axisB;
                Vector3 pointOnUnitSphere = pointOnCube.normalized * _shapeSettings.planetRadius;
                vertices[i] = pointOnUnitSphere;
                
                if (x != _resolution - 1 && y != _resolution - 1)
                {
                    triangles[triIndex + 0] = i;
                    triangles[triIndex + 1] = i + _resolution + 1;
                    triangles[triIndex + 2] = i + _resolution;
                    
                    triangles[triIndex + 3] = i;
                    triangles[triIndex + 4] = i + 1;
                    triangles[triIndex + 5] = i + _resolution + 1;
                    triIndex += 6;
                }
            }
        }
        _mesh.Clear();
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // This is needed for meshes with more than 65535 [256*256] vertices
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        // _mesh.RecalculateNormals();
        _mesh.uv = uv;
    }
    
    
    public void AddNoiseWithComputeShader(ComputeShader noiseShader)
    {
        // 1. Get the kernel id
        int kernelHandle = noiseShader.FindKernel("CSMain");
        
        // 2. Create a compute buffer to hold the vertices
        ComputeBuffer vertexBuffer = new ComputeBuffer(_mesh.vertexCount, 3 * sizeof(float)); // 3 for x, y, z

        // 3. Set the buffer data from the mesh vertices
        vertexBuffer.SetData(_mesh.vertices);

        // 4. Set the buffer to the compute shader
        noiseShader.SetBuffer(kernelHandle, "VertexBuffer", vertexBuffer);
        noiseShader.SetInt(Shader.PropertyToID("numVertices"), _mesh.vertexCount);
        noiseShader.SetFloat(Shader.PropertyToID("numLayers"), _noiseSettings.numLayers);
        noiseShader.SetFloat(Shader.PropertyToID("scale"), _noiseSettings.scale);
        noiseShader.SetFloat(Shader.PropertyToID("persistence"), _noiseSettings.persistence);
        noiseShader.SetFloat(Shader.PropertyToID("lacunarity"), _noiseSettings.lacunarity);
        noiseShader.SetFloat(Shader.PropertyToID("multiplier"), _noiseSettings.multiplier);
        noiseShader.SetFloat(Shader.PropertyToID("oceanDepthMultiplier"), _shapeSettings.oceanDepthMultiplier);
        noiseShader.SetFloat(Shader.PropertyToID("oceanFloorDepth"), _shapeSettings.oceanFloorDepth);
        noiseShader.SetFloat(Shader.PropertyToID("oceanFloorSmoothing"), _shapeSettings.oceanFloorSmoothing);
        

        // 5. Dispatch the compute shader
        // int threadGroups = Mathf.CeilToInt(_mesh.vertexCount / 1.0f); // Assuming each thread handles 1 vertex
        int threadsPerGroup = 512;
        int threadGroups = Mathf.CeilToInt(_mesh.vertexCount / (float)threadsPerGroup);
        noiseShader.Dispatch(kernelHandle, threadGroups, 1, 1);

        // 6. Retrieve the data from the buffer
        Vector3[] modifiedVertices = new Vector3[_mesh.vertexCount];
        vertexBuffer.GetData(modifiedVertices);
        _mesh.vertices = modifiedVertices;  
        _mesh.RecalculateNormals(); 

        // 7. Cleanup  
        vertexBuffer.Release();
    }
    
    
    
    // public void UpdateUVs(ColourGenerator colourGenerator)
    // {
    //     Vector2[] uv = _mesh.uv;
    //     
    //     for (int y = 0; y < _resolution; y++)
    //     {
    //         for (int x = 0; x < _resolution; x++)
    //         {
    //             int i = x + y * _resolution;
    //             Vector2 percent = new Vector2(x, y) / (_resolution - 1);
    //             Vector3 pointOnUnitCube = _localUp + (percent.x - 0.5f) * 2 * _axisA + (percent.y - 0.5f) * 2 * _axisB;
    //             Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
    //             
    //             uv[i].x = colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);
    //         }
    //     }
    //     _mesh.uv = uv;
    // }
}
