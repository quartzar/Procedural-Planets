using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    private ShapeGenerator _shapeGenerator;
    private Mesh _mesh;
    private int _resolution;
    private int _quadResolution;
    private Vector3 _localUp;
    private Vector3 _axisA;
    private Vector3 _axisB;
    private float _radius;
    
    List<Vector3> _vertices;
    List<int> _triangles;
    List<Vector2> _uvs;
    
    // new Quadtree to avoid static method warnings
    // private Quadtree _quadtree;
    
    public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, int quadResolution, Vector3 localUp, float radius)
    {
        this._shapeGenerator = shapeGenerator;
        this._mesh = mesh;
        this._resolution = resolution;
        this._quadResolution = quadResolution;
        this._localUp = localUp;
        this._radius = radius;
        
        _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        _axisB = Vector3.Cross(localUp, _axisA);
    }

    
    
    // Construct a quadtree of quads 
    public void ConstructTree()
    {
        // Clear mesh data
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _uvs = new List<Vector2>();
        
        // Debug.Log("Before parentTree");
        
        // Generate quadtree
        Quadtree parentTree = new Quadtree(null, _localUp.normalized * Planet.size, _radius, 0, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator);
        // Debug.Log("After parentTree");
        parentTree.GenerateChildren();
        // Debug.Log("After GenerateChildren");
        
        
        
        // Retrieve mesh data from quadtree
        int triOffset = 0;
        foreach (Quadtree child in parentTree.GetVisibleChildren())
        {
            Quadtree.Quad quad = child.CalculateQuad(triOffset);
            _vertices.AddRange(quad.vertices);
            _triangles.AddRange(quad.triangles);
            _uvs.AddRange(quad.uvs);
            
            triOffset += quad.vertices.Length;
        }
        
        // Reset mesh and apply new data
        _mesh.Clear();
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // This is needed for meshes with more than 65535 [256*256] vertices
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();
        _mesh.uv = _uvs.ToArray();
    }

    
    // // Mostly from Sebastian Lague
    // public Quad CalculateQuad(int triOffset)
    // {
    //     Quad quad = new Quad();
    //     quad.vertices = new Vector3[_quadResolution * _quadResolution];
    //     quad.triangles = new int[(_quadResolution - 1) * (_quadResolution - 1) * 6];
    //     quad.uvs = (_mesh.uv.Length == quad.vertices.Length) ? _mesh.uv : new Vector2[quad.vertices.Length];
    //
    //     int triIndex = 0;
    //     
    //     // Normalised Cube
    //     for (int y = 0; y < _quadResolution; y++)
    //     {
    //         for (int x = 0; x < _quadResolution; x++)
    //         {
    //             int i = x + y * _quadResolution;
    //             Vector2 percent = new Vector2(x, y) / (_quadResolution - 1);
    //             // Vector3 pointOnCube = _localUp + (percent.x - 0.5f) * 2 * _axisA + (percent.y - 0.5f) * 2 * _axisB;
    //             Vector3 pointOnCube = _pos
    //             
    //             Vector3 pointOnUnitSphere = pointOnCube.normalized;
    //             float unscaledElevation = _shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
    //             
    //             quad.vertices[i] = pointOnUnitSphere * _shapeGenerator.GetScaledElevation(unscaledElevation);
    //             quad.uvs[i].y = unscaledElevation;
    //             
    //             if (x != _quadResolution - 1 && y != _quadResolution - 1)
    //             {
    //                 quad.triangles[triIndex++] = i + triOffset;
    //                 quad.triangles[triIndex++] = i + triOffset + _quadResolution + 1;
    //                 quad.triangles[triIndex++] = i + triOffset + _quadResolution;
    //                 
    //                 quad.triangles[triIndex++] = i + triOffset;
    //                 quad.triangles[triIndex++] = i + triOffset + 1;
    //                 quad.triangles[triIndex++] = i + triOffset + _quadResolution + 1;
    //             }
    //         }
    //     }
    //     return quad;
    // }
    
    // public void ConstructMesh()
    // {
    //     Vector3[] vertices = new Vector3[_resolution * _resolution];
    //     int[] triangles = new int[(_resolution - 1) * (_resolution - 1) * 6];
    //     Vector2[] uv = (_mesh.uv.Length == vertices.Length) ? _mesh.uv : new Vector2[vertices.Length];
    //     int triIndex = 0;
    //     
    //     // Normalised Cube
    //     for (int y = 0; y < _resolution; y++)
    //     {
    //         for (int x = 0; x < _resolution; x++)
    //         {
    //             int i = x + y * _resolution;
    //             Vector2 percent = new Vector2(x, y) / (_resolution - 1);
    //             Vector3 pointOnCube = _localUp + (percent.x - 0.5f) * 2 * _axisA + (percent.y - 0.5f) * 2 * _axisB;
    //             
    //             Vector3 pointOnUnitSphere = pointOnCube.normalized;
    //             float unscaledElevation = _shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
    //             
    //             vertices[i] = pointOnUnitSphere * _shapeGenerator.GetScaledElevation(unscaledElevation);
    //             uv[i].y = unscaledElevation;
    //             
    //             if (x != _resolution - 1 && y != _resolution - 1)
    //             {
    //                 triangles[triIndex + 0] = i;
    //                 triangles[triIndex + 1] = i + _resolution + 1;
    //                 triangles[triIndex + 2] = i + _resolution;
    //                 
    //                 triangles[triIndex + 3] = i;
    //                 triangles[triIndex + 4] = i + 1;
    //                 triangles[triIndex + 5] = i + _resolution + 1;
    //                 triIndex += 6;
    //             }
    //         }
    //     }
    //     _mesh.Clear();
    //     _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // This is needed for meshes with more than 65535 [256*256] vertices
    //     _mesh.vertices = vertices;
    //     _mesh.triangles = triangles;
    //     _mesh.RecalculateNormals();
    //     _mesh.uv = uv;
    // }
    
    public void UpdateUVs(ColourGenerator colourGenerator)
    {
        Vector2[] uv = _mesh.uv;
        
        // Normalised Cube
        for (int y = 0; y < _resolution; y++)
        {
            for (int x = 0; x < _resolution; x++)
            {
                int i = x + y * _resolution;
                Vector2 percent = new Vector2(x, y) / (_resolution - 1);
                Vector3 pointOnUnitCube = _localUp + (percent.x - 0.5f) * 2 * _axisA + (percent.y - 0.5f) * 2 * _axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                
                uv[i].x = colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);
                
                
            }
        }
        _mesh.uv = uv;
    }
}
