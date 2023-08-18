using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quadtree
{
    ShapeSettings settings;
    
    ShapeGenerator _shapeGenerator;
    
    Quadtree[] _children;
    Vector3 _position;
    float _radius;
    int _detailLevel;
    Vector3 _localUp;
    Vector3 _axisA;
    Vector3 _axisB;
    int _quadResolution;
    
    public void UpdateSettings(ShapeSettings settings)
    {
        this.settings = settings;
    }
    
    
    // Constructor
    public Quadtree(Quadtree[] children, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, int quadResolution, ShapeGenerator shapeGenerator)
    {
        this._children = children;
        this._position = position;
        this._radius = radius;
        this._detailLevel = detailLevel;
        this._localUp = localUp;
        this._axisA = axisA;
        this._axisB = axisB;
        this._quadResolution = quadResolution;
        this._shapeGenerator = shapeGenerator;
    }
    
    // Quad struct
    public struct Quad 
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
    }
    

    // Primary function to generate the quadtree
    public void GenerateChildren()
    {
        // Check detail level between 0 and max level. Max level depends on how many detail levels are defined
        if (_detailLevel is <= 6 and >= 0)
        {
            // log the below values
            // Debug.Log("Position: " + _position + " Radius: " + _radius + " Detail level: " + _detailLevel + " Local up: " + _localUp + " Axis A: " + _axisA + " Axis B: " + _axisB + " Quad resolution: " + _quadResolution);// + " Planet radius: " + settings.planetRadius);
            // Debug.Log("Planet Radius: "+Planet.size);
            Debug.Log("PlayerPosition: "+Planet.playerPosition);
            if (Vector3.Distance(_position.normalized * Planet.size, Planet.playerPosition) <= Planet.detailLevelDistances[_detailLevel])
            {
                // Assign the quadtree children
                _children = new Quadtree[4];
                _children[0] = new Quadtree(new Quadtree[0], _position + _axisA * _radius / 2 + _axisB * _radius / 2, _radius / 2, _detailLevel + 1, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator);
                _children[1] = new Quadtree(new Quadtree[0], _position + _axisA * _radius / 2 - _axisB * _radius / 2, _radius / 2, _detailLevel + 1, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator);
                _children[2] = new Quadtree(new Quadtree[0], _position - _axisA * _radius / 2 + _axisB * _radius / 2, _radius / 2, _detailLevel + 1, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator);
                _children[3] = new Quadtree(new Quadtree[0], _position - _axisA * _radius / 2 - _axisB * _radius / 2, _radius / 2, _detailLevel + 1, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator);
                
                // Generate the grandchildren
                foreach (Quadtree child in _children)
                {
                    child.GenerateChildren();
                }
            }
        }
    }
    
    // Returns the most recent quad in every branch of the quadtree, i.e. quad to be rendered
    public Quadtree[] GetVisibleChildren()
    {
        List<Quadtree> toBeRendered = new List<Quadtree>();
        
        if (_children.Length > 0)
        {
            foreach (Quadtree child in _children)
            {
                toBeRendered.AddRange(child.GetVisibleChildren());
            }
        }
        else // Leaf node, i.e. quadtree with no children
        {
            toBeRendered.Add(this);
        }
        
        return toBeRendered.ToArray();
    }
    
    // Mostly from Sebastian Lague
    public Quad CalculateQuad(int triOffset)
    {
        Quad quad = new Quad();
        quad.vertices = new Vector3[_quadResolution * _quadResolution];
        quad.triangles = new int[(_quadResolution - 1) * (_quadResolution - 1) * 6];
        // quad.uvs = (_mesh.uv.Length == quad.vertices.Length) ? _mesh.uv : new Vector2[quad.vertices.Length];
        quad.uvs = new Vector2[quad.vertices.Length];
        
        int triIndex = 0;
        
        // Normalised Cube
        for (int y = 0; y < _quadResolution; y++)
        {
            for (int x = 0; x < _quadResolution; x++)
            {
                int i = x + y * _quadResolution;
                Vector2 percent = new Vector2(x, y) / (_quadResolution - 1);
                // Vector3 pointOnCube = _localUp + (percent.x - 0.5f) * 2 * _axisA + (percent.y - 0.5f) * 2 * _axisB;
                Vector3 pointOnCube = _position + ((percent.x - 0.5f) * 2 * _axisA + (percent.y - 0.5f) * 2 * _axisB) * _radius;
                
                // Vector3 pointOnUnitSphere = pointOnCube.normalized;
                Vector3 pointOnUnitSphere = pointOnCube.normalized * Planet.size;
                
                quad.vertices[i] = pointOnUnitSphere;
                
                // removed while debugging
                // float unscaledElevation = _shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
                // quad.vertices[i] = pointOnUnitSphere * _shapeGenerator.GetScaledElevation(unscaledElevation);
                // quad.uvs[i].y = unscaledElevation;
                
                if (x != _quadResolution - 1 && y != _quadResolution - 1)
                {
                    quad.triangles[triIndex++] = i + triOffset;
                    quad.triangles[triIndex++] = i + triOffset + _quadResolution + 1;
                    quad.triangles[triIndex++] = i + triOffset + _quadResolution;
                    
                    quad.triangles[triIndex++] = i + triOffset;
                    quad.triangles[triIndex++] = i + triOffset + 1;
                    quad.triangles[triIndex++] = i + triOffset + _quadResolution + 1;
                }
            }
        }
        // Debug.Log("Planet radius:   " + settings.planetRadius);
        return quad;
    }
    
}