using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
    private ShapeGenerator _shapeGenerator;
    private ColourGenerator _colourGenerator;
    private Mesh _mesh;
    private int _resolution;
    private int _quadResolution;
    private Vector3 _localUp;
    private Vector3 _axisA;
    private Vector3 _axisB;
    private float _radius;
    
    public Quadtree parentQuad;
    private Planet _planetScript;
    
    List<Vector3> _vertices;
    List<int> _triangles;
    List<Vector2> _uvs;
    
    public TerrainFace(ShapeGenerator shapeGenerator, ColourGenerator colourGenerator, Mesh mesh, int resolution, int quadResolution, Vector3 localUp, float radius, Planet planetScript)
    {
        this._shapeGenerator = shapeGenerator;
        this._colourGenerator = colourGenerator;
        this._mesh = mesh;
        this._resolution = resolution;
        this._quadResolution = quadResolution;
        this._localUp = localUp;
        this._radius = radius;
        this._planetScript = planetScript;
        
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
        
        _mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // This is needed for meshes with more than 65535 [256*256] vertices
        
        // Generate quadtree
        parentQuad = new Quadtree(_planetScript, null, _localUp.normalized * Planet.size, _radius, 0, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator, _colourGenerator);
        parentQuad.GenerateChildren();
        
        // Retrieve mesh data from quadtree
        int triOffset = 0;
        foreach (Quadtree child in parentQuad.GetVisibleChildren())
        {
            Quadtree.Quad quad = child.CalculateQuad(triOffset);
            _vertices.AddRange(quad.vertices);
            _triangles.AddRange(quad.triangles);
            _uvs.AddRange(quad.uvs);
            
            triOffset += quad.vertices.Length;
        }
        
        // Reset mesh and apply new data
        _mesh.Clear();
        _mesh.vertices = _vertices.ToArray();
        _mesh.triangles = _triangles.ToArray();
        _mesh.RecalculateNormals();
        _mesh.uv = _uvs.ToArray();
    }
    
    // Updates the quadtree
    public void UpdateTree()
    {
        // Clear mesh data
        _vertices.Clear();
        _triangles.Clear();
        _uvs.Clear();
        
        parentQuad.UpdateQuad();
        
        // Retrieve mesh data from quadtree
        int triOffset = 0;
        int generationCounter = 0;
        foreach (Quadtree child in parentQuad.GetVisibleChildren())
        {
            Quadtree.Quad quad = new Quadtree.Quad();
            if (child.vertices == null || child.vertices.Length == 0)
            {
                quad = child.CalculateQuad(triOffset);
                generationCounter++;
            }
            else 
            {
                quad.vertices = child.vertices;
                quad.triangles = child.GetTrianglesWithOffset(triOffset);
                quad.uvs = child.uvs;
            }
            
            _vertices.AddRange(quad.vertices);
            _triangles.AddRange(quad.triangles);
            _uvs.AddRange(quad.uvs);
            
            triOffset += quad.vertices.Length;
        }
        
        // Reset mesh and apply new data, only if the mesh has been updated
        if (generationCounter > 0)
        {
            _mesh.Clear();
            _mesh.vertices = _vertices.ToArray();
            _mesh.triangles = _triangles.ToArray();
            _mesh.RecalculateNormals();
            _mesh.uv = _uvs.ToArray();
        }
    }
    
    // public void UpdateUVs(ColourGenerator colourGenerator)
    // {
    //     foreach (Quadtree child in parentTree.GetVisibleChildren())
    //     {
    //         Vector2[] uv = _mesh.uv;
    //         for (int y = 0; y < _quadResolution; y++)
    //         {
    //             for (int x = 0; x < _quadResolution; x++)
    //             {
    //                 int i = x + y * _quadResolution;
    //
    //                 Vector3 pointOnUnitSphere = _vertices.ToArray()[i].normalized;
    //
    //                 uv[i].x = colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);
    //             }
    //         }
    //         _mesh.uv = uv;
    //     }
    //     // _mesh.uv = uv;
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
