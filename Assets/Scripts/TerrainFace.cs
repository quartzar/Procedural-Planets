using System.Collections;
using System.Collections.Generic;
using NoiseSettings;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainFace
{
    private Planet _planetScript;
    private readonly CelestialBodySettings _body;
    private readonly Mesh _mesh;
    private readonly int _resolution;
    private readonly int _subdivisions;
    private readonly float _radius;
    private readonly Vector3 _localUp;
    private readonly Vector3 _axisA;
    private readonly Vector3 _axisB;
    
    public TriangleNode parentNode;
    
    List<Vector3> _vertices;
    List<int> _triangles;
    
    // List<Vector3> vertices;

    public TerrainFace(Planet planetScript, CelestialBodySettings body, Mesh mesh, int resolution, int subdivisions, float radius, Vector3 localUp)
    {
        this._planetScript = planetScript;
        this._body = body;
        this._mesh = mesh;
        this._resolution = resolution;
        this._subdivisions = subdivisions;
        this._radius = radius;
        this._localUp = localUp;
        
        _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        _axisB = Vector3.Cross(localUp, _axisA);
        
    }
    

    public void ConstructTree()
    {
        Vector3[] baseVertices = new Vector3[] { 
            (_localUp + _axisA + _axisB).normalized * _radius,
            (_localUp + _axisA - _axisB).normalized * _radius,
            (_localUp - _axisA + _axisB).normalized * _radius,
            // (_localUp - _axisA - _axisB).normalized * _radius 
        };
        int[] baseTriangles = new int[] { 0, 2, 1 };
        
        // _mesh.vertices = baseVertices;
        // _mesh.triangles = baseTriangles;
        
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // This is needed for meshes with more than 65535 [256*256] vertices
        
        parentNode = new TriangleNode(_planetScript, null, _localUp.normalized * Planet.size, _radius, 0, _localUp, _axisA, _axisB, baseVertices, baseTriangles);
        parentNode.GenerateChildren();
        
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        
        int triOffset = 0;
        foreach (TriangleNode child in parentNode.GetVisibleChildren())
        {
            var (childVertices, childTriangles) = child.ConstructLEB(triOffset);
            _vertices.AddRange(childVertices);
            _triangles.AddRange(childTriangles);
            
            triOffset += childVertices.Length;
        }
        
        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();
    }

    public void ConstructTreeOld()
    {
        // Clear mesh data
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // This is needed for meshes with more than 65535 [256*256] vertices
        
        // parentNode = new TriangleNode(_planetScript, null, _localUp.normalized * Planet.size, _radius, 0, _localUp, _axisA, _axisB);
        parentNode.GenerateChildren();
        
        int triOffset = 0;
        foreach (TriangleNode child in parentNode.GetVisibleChildren())
        {
            var (childVertices, childTriangles) = child.ConstructLEB(triOffset);
            _vertices.AddRange(childVertices);
            _triangles.AddRange(childTriangles);
            
            triOffset += childVertices.Length;
        }
        
        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();
    }
    
    public void UpdateTree()
    {
        // Clear mesh data
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        
        parentNode.UpdateQuad();
        
        
    }
    
    public void ConstructMesh()
    {
        // Initialize the mesh with a single square
        _mesh.Clear();
        
        Vector3[] baseQuadVertices = new Vector3[] { 
            (_localUp + _axisA + _axisB).normalized * _radius,
            (_localUp + _axisA - _axisB).normalized * _radius,
            (_localUp - _axisA + _axisB).normalized * _radius,
            (_localUp - _axisA - _axisB).normalized * _radius 
        };
        int[] baseQuadTriangles = new int[] { 0, 2, 1, 1, 2, 3 };
        
        _mesh.vertices = baseQuadVertices;
        _mesh.triangles = baseQuadTriangles;
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
        _mesh.Clear();
        _mesh.vertices = newVertices.ToArray();
        _mesh.triangles = newTriangles.ToArray();
    }
    
    
    public void ConstructMeshOlder()
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


// public void ConstructMesh()
// {
//     // Reset mesh
//     _mesh.Clear();
//
//     // Start with one "big" triangle
//     List<Vector3> vertices = new List<Vector3>
//     {
//         // Define vertices for your initial big triangle here
//         new Vector3(0, 0, 0),
//         new Vector3(1, 0, 0),
//         new Vector3(0.5f, Mathf.Sqrt(3)/2f, 0)
//     };
//
//     // Index buffer for triangles
//     List<int> triangles = new List<int>();
//
//     // Recursive subdivision
//     SubdivideTriangle(vertices, triangles, 0, 1, 2, _resolution);
//
//     // Set the vertices and triangles in the mesh
//     _mesh.vertices = vertices.ToArray();
//     _mesh.triangles = triangles.ToArray();
//
//     // Other mesh properties like normals, if needed
//     _mesh.RecalculateNormals();
// }
//
// void SubdivideTriangle(List<Vector3> vertices, List<int> triangles, int i0, int i1, int i2, int depth)
// {
//     if (depth == 0)
//     {
//         // Base case: add indices to draw this triangle
//         triangles.Add(i0);
//         triangles.Add(i1);
//         triangles.Add(i2);
//         return;
//     }
//
//     // Calculate midpoints on each edge
//     Vector3 m01 = Vector3.Lerp(vertices[i0], vertices[i1], 0.5f);
//     Vector3 m12 = Vector3.Lerp(vertices[i1], vertices[i2], 0.5f);
//     Vector3 m20 = Vector3.Lerp(vertices[i2], vertices[i0], 0.5f);
//
//     // Add midpoints to vertices list
//     int i01 = vertices.Count;
//     vertices.Add(m01);
//     int i12 = vertices.Count;
//     vertices.Add(m12);
//     int i20 = vertices.Count;
//     vertices.Add(m20);
//
//     // Recursive subdivision for 4 new triangles
//     SubdivideTriangle(vertices, triangles, i0, i01, i20, depth - 1);
//     SubdivideTriangle(vertices, triangles, i1, i12, i01, depth - 1);
//     SubdivideTriangle(vertices, triangles, i2, i20, i12, depth - 1);
//     SubdivideTriangle(vertices, triangles, i01, i12, i20, depth - 1);
// }