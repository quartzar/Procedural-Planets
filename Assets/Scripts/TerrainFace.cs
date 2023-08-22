using System.Collections;
using System.Collections.Generic;
using NoiseSettings;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainFace
{
    private readonly CelestialBodySettings _body;
    private readonly Mesh _mesh;
    private readonly int _resolution;
    private readonly float _radius;
    private readonly Vector3 _localUp;
    private readonly Vector3 _axisA;
    private readonly Vector3 _axisB;
    

    public TerrainFace(CelestialBodySettings body, Mesh mesh, int resolution, float radius, Vector3 localUp)
    {
        this._body = body;
        this._mesh = mesh;
        this._resolution = resolution;
        this._radius = radius;
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
                Vector3 pointOnUnitSphere = pointOnCube.normalized * _radius;
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
    
    
    public void NoiseShader()
    {
        ComputeBuffer vertexBuffer = new ComputeBuffer(_mesh.vertexCount, 3 * sizeof(float)); // 3 for x, y, z
        vertexBuffer.SetData(_mesh.vertices);
        float[] heights = _body.shape.ComputeHeights(vertexBuffer);
        
        // Modify the mesh vertices using the heights
        Vector3[] modifiedVertices = _mesh.vertices;
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            float height = heights[i];
            modifiedVertices[i] *= height; 
            Planet.elevationMinMax.AddValue(height);
        }
        _mesh.vertices = modifiedVertices;
        _mesh.RecalculateNormals();
    }
    
    // public void AddNoiseWithComputeShader(ComputeShader noiseShader)
    // {
    //     // 1. Get the kernel id
    //     int kernelHandle = noiseShader.FindKernel("CSMain");
    //
    //     // 2. Create a compute buffer to hold the vertices
    //     ComputeBuffer vertexBuffer = new ComputeBuffer(_mesh.vertexCount, 3 * sizeof(float)); // 3 for x, y, z
    //
    //     // 3. Create a compute buffer to hold the heights
    //     ComputeBuffer heightBuffer = new ComputeBuffer(_mesh.vertexCount, sizeof(float));
    //
    //     // 4. Set the buffer data from the mesh vertices
    //     vertexBuffer.SetData(_mesh.vertices);
    //
    //     // 5. Set the buffer to the compute shader
    //     noiseShader.SetBuffer(kernelHandle, Vertices, vertexBuffer);
    //     noiseShader.SetBuffer(kernelHandle, Heights, heightBuffer);
    //     noiseShader.SetInt(Shader.PropertyToID("numVertices"), _mesh.vertexCount);
    //     
    //     noiseShader.SetFloat(Shader.PropertyToID("numLayers"), _continentSettings.numLayers);
    //     noiseShader.SetFloat(Shader.PropertyToID("scale"), _continentSettings.scale/10f);
    //     noiseShader.SetFloat(Shader.PropertyToID("persistence"), _continentSettings.persistence);
    //     noiseShader.SetFloat(Shader.PropertyToID("lacunarity"), _continentSettings.lacunarity);
    //     noiseShader.SetFloat(Shader.PropertyToID("multiplier"), _continentSettings.elevation);
    //     noiseShader.SetFloat(Shader.PropertyToID("oceanDepthMultiplier"), _shapeSettings.oceanDepthMultiplier);
    //     noiseShader.SetFloat(Shader.PropertyToID("oceanFloorDepth"), _shapeSettings.oceanFloorDepth);
    //     noiseShader.SetFloat(Shader.PropertyToID("oceanFloorSmoothing"), _shapeSettings.oceanFloorSmoothing);
    //     noiseShader.SetFloat(Shader.PropertyToID("mountainBlend"), _shapeSettings.mountainBlend);
    //     
    //     float[] maskParams = {
    //         // [0]
    //         _maskSettings.scale/10f,
    //         _maskSettings.persistence,
    //         _maskSettings.lacunarity,
    //         _maskSettings.elevation,
    //         // [1]
    //         _maskSettings.numLayers,
    //         _maskSettings.verticalShift
    //     };
    //     noiseShader.SetFloats(Shader.PropertyToID("noiseParams_mask"), maskParams);
    //     
    //     float[] ridgeParams = {
    //         // [0]
    //         _ridgeSettings.offset.x,
    //         _ridgeSettings.offset.y,
    //         _ridgeSettings.offset.z,
    //         _ridgeSettings.numLayers,
    //         // [1]
    //         _ridgeSettings.persistence,
    //         _ridgeSettings.lacunarity,
    //         _ridgeSettings.scale,
    //         _ridgeSettings.elevation,
    //         // [2]
    //         _ridgeSettings.power,
    //         _ridgeSettings.gain,
    //         _ridgeSettings.verticalShift,
    //         _ridgeSettings.peakSmoothing
    //     };
    //     noiseShader.SetFloats(Shader.PropertyToID("noiseParams_mountains"), ridgeParams);
    //
    //     // 6. Dispatch the compute shader
    //     int threadsPerGroup = 512;
    //     int threadGroups = Mathf.CeilToInt(_mesh.vertexCount / (float)threadsPerGroup);
    //     noiseShader.Dispatch(kernelHandle, threadGroups, 1, 1);
    //     
    //     // 7. Retrieve the heights from the buffer
    //     float[] heights = new float[_mesh.vertexCount];
    //     heightBuffer.GetData(heights);
    //     
    //     
    //
    //     // 8. Modify the mesh vertices using the heights
    //     Vector3[] modifiedVertices = _mesh.vertices;
    //     for (int i = 0; i < modifiedVertices.Length; i++)
    //     {
    //         float height = heights[i];
    //         modifiedVertices[i] *= height; 
    //         Planet.elevationMinMax.AddValue(height);
    //     }
    //     _mesh.vertices = modifiedVertices;
    //     _mesh.RecalculateNormals();
    //
    //     // 9. Cleanup
    //     vertexBuffer.Release();
    //     heightBuffer.Release();
    // }
    
    
    
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
