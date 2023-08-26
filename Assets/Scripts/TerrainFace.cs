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
    private readonly int _subdivisions;
    private readonly float _radius;
    private readonly Vector3 _localUp;
    private readonly Vector3 _axisA;
    private readonly Vector3 _axisB;
    

    public TerrainFace(CelestialBodySettings body, Mesh mesh, int resolution, int subdivisions, float radius, Vector3 localUp)
    {
        this._body = body;
        this._mesh = mesh;
        this._resolution = resolution;
        this._subdivisions = subdivisions;
        this._radius = radius;
        this._localUp = localUp;
        
        _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        _axisB = Vector3.Cross(localUp, _axisA);
    }
    
    public void ConstructMesh()
    {
        // Initialize the mesh with a single square
        _mesh.Clear();
        _mesh.vertices = new Vector3[] { 
            (_localUp + _axisA + _axisB).normalized * _radius,
            (_localUp + _axisA - _axisB).normalized * _radius,
            (_localUp - _axisA + _axisB).normalized * _radius,
            (_localUp - _axisA - _axisB).normalized * _radius 
        };
        _mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        for (int i = 0; i < _subdivisions; i++)
        {
            Subdivide();
        }
        // Subdivide();
    }

    private void Subdivide()
    {
        // Get current mesh data
        Vector3[] oldVertices = _mesh.vertices;
        int[] oldTriangles = _mesh.triangles;
        
        // Create new arrays to hold subdivided mesh data
        List<Vector3> newVertices = new List<Vector3>(oldVertices);
        List<int> newTriangles = new List<int>();

        // For each old triangle, subdivide and add new triangles to the newTriangles list
        for (int i = 0; i < oldTriangles.Length; i += 3)
        {
            Vector3 v0 = oldVertices[oldTriangles[i]];
            Vector3 v1 = oldVertices[oldTriangles[i + 1]];
            Vector3 v2 = oldVertices[oldTriangles[i + 2]];

            // Find the longest edge and its opposite vertex
            float d01 = Vector3.Distance(v0, v1);
            float d12 = Vector3.Distance(v1, v2);
            float d20 = Vector3.Distance(v2, v0);

            if (d01 > d12 && d01 > d20)
            {
                // The longest edge is v0-v1
                Vector3 m = ((v0 + v1) / 2).normalized * _radius;
                newVertices.Add(m);
                int midIndex = newVertices.Count - 1;

                // Add new triangles
                newTriangles.AddRange(new int[] { oldTriangles[i], midIndex, oldTriangles[i + 2], midIndex, oldTriangles[i + 1], oldTriangles[i + 2] });
            }
            else if (d12 > d01 && d12 > d20)
            {
                // The longest edge is v1-v2
                Vector3 m = ((v1 + v2) / 2).normalized * _radius;
                newVertices.Add(m);
                int midIndex = newVertices.Count - 1;

                // Add new triangles
                newTriangles.AddRange(new int[] { oldTriangles[i], oldTriangles[i + 1], midIndex, oldTriangles[i], midIndex, oldTriangles[i + 2] });
            }
            else
            {
                // The longest edge is v2-v0
                Vector3 m = ((v2 + v0) / 2).normalized * _radius;
                newVertices.Add(m);
                int midIndex = newVertices.Count - 1;

                // Add new triangles
                newTriangles.AddRange(new int[] { oldTriangles[i], oldTriangles[i + 1], midIndex, midIndex, oldTriangles[i + 1], oldTriangles[i + 2] });
            }
        }

        // Update the mesh with new data
        _mesh.vertices = newVertices.ToArray();
        _mesh.triangles = newTriangles.ToArray();
    }
    
    
    public void ConstructMeshOld()
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
                    // if ((x + y) % 2 != 0)
                    // {
                        triangles[triIndex + 0] = i;
                        triangles[triIndex + 1] = i + _resolution + 1;
                        triangles[triIndex + 2] = i + _resolution;
    
                        triangles[triIndex + 3] = i;
                        triangles[triIndex + 4] = i + 1;
                        triangles[triIndex + 5] = i + _resolution + 1;
                    // }
                    // else
                    // {
                    //     triangles[triIndex + 0] = i;
                    //     triangles[triIndex + 1] = i + 1;
                    //     triangles[triIndex + 2] = i + _resolution;
                    //     
                    //     triangles[triIndex + 3] = i + 1;
                    //     triangles[triIndex + 4] = i + _resolution + 1;
                    //     triangles[triIndex + 5] = i + _resolution;
                    // }
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
        _body.shape.PerturbVertices(vertexBuffer, ref modifiedVertices);
        
        // Vector3[] modifiedVertices = _mesh.vertices;
        // vertexBuffer.SetData(modifiedVertices);
        // _body.shape.PerturbVertices(vertexBuffer, ref modifiedVertices);
        // float[] heights = _body.shape.ComputeHeights(vertexBuffer);
        
        
        for (int i = 0; i < modifiedVertices.Length; i++)
        {
            float height = heights[i];
            modifiedVertices[i] *= height; 
            Planet.elevationMinMax.AddValue(height);
        }
        _mesh.vertices = modifiedVertices;
        _mesh.RecalculateNormals();
    }
    
}
