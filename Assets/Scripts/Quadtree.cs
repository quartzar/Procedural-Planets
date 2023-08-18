using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quadtree
{
    ShapeGenerator _shapeGenerator;
    ColourGenerator _colourGenerator;
    
    public Quadtree[] _children;
    public Vector3 _position;
    public float _radius;
    public int _detailLevel;
    public Vector3 _localUp;
    public Vector3 _axisA;
    public Vector3 _axisB;
    public int _quadResolution;
    public Vector3 _normalisedPos;
    
    public Planet planetScript;
    
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    
    // Constructor
    public Quadtree(Planet planetScript, Quadtree[] children, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, int quadResolution, ShapeGenerator shapeGenerator, ColourGenerator colourGenerator)
    {
        this.planetScript = planetScript;
        this._children = children;
        this._position = position;
        this._radius = radius;
        this._detailLevel = detailLevel;
        this._localUp = localUp;
        this._axisA = axisA;
        this._axisB = axisB;
        this._quadResolution = quadResolution;
        this._shapeGenerator = shapeGenerator;
        this._colourGenerator = colourGenerator;
        this._normalisedPos = position.normalized;
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
        if (_detailLevel <= Planet.MaxDetailLevel && _detailLevel >= 0)
        {
            //
            //
            if (Vector3.Distance(planetScript.transform.TransformDirection(_normalisedPos * Planet.size) + planetScript.transform.position, planetScript.player.position) <= Planet.DetailLevelDistances[_detailLevel])
            {
                // Assign the quadtree children
                _children = new Quadtree[4];
                _children[0] = new Quadtree(planetScript, new Quadtree[0], _position + _axisA * _radius / 2 + _axisB * _radius / 2, _radius / 2, _detailLevel + 1, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator, _colourGenerator);
                _children[1] = new Quadtree(planetScript, new Quadtree[0], _position + _axisA * _radius / 2 - _axisB * _radius / 2, _radius / 2, _detailLevel + 1, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator, _colourGenerator);
                _children[2] = new Quadtree(planetScript, new Quadtree[0], _position - _axisA * _radius / 2 + _axisB * _radius / 2, _radius / 2, _detailLevel + 1, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator, _colourGenerator);
                _children[3] = new Quadtree(planetScript, new Quadtree[0], _position - _axisA * _radius / 2 - _axisB * _radius / 2, _radius / 2, _detailLevel + 1, _localUp, _axisA, _axisB, _quadResolution, _shapeGenerator, _colourGenerator);
                
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
            // Check if quad is within culling angle, if so add to list of quads to be rendered
            if (Mathf.Acos((Mathf.Pow(Planet.size, 2) + Mathf.Pow(planetScript.distanceToPlayer, 2) - 
                            Mathf.Pow(Vector3.Distance(planetScript.transform.TransformDirection(_normalisedPos * Planet.size), planetScript.player.position), 2)) / 
                           (2 * Planet.size * planetScript.distanceToPlayer)) < Planet.cullingMinAngle || Planet.isEditor)
            {
                toBeRendered.Add(this);
            }
            
            // toBeRendered.Add(this);
        }
        
        return toBeRendered.ToArray();
    }
    
    
    // Updates the quadtree
    public void UpdateQuad()
    {
        // get distance from player to quadtree with direction
        float dstToPlayer = Vector3.Distance(planetScript.transform.TransformDirection(_normalisedPos * Planet.size) + planetScript.transform.position, planetScript.player.position);
        // Debug.Log("Max detail: " + Planet.MaxDetailLevel);
        if (_detailLevel <= Planet.MaxDetailLevel)
        {
            if (dstToPlayer > Planet.DetailLevelDistances[_detailLevel])
            {
                _children = new Quadtree[0];
            }
            else
            {
                if (_children.Length > 0)
                {
                    foreach (Quadtree child in _children)
                    {
                        child.UpdateQuad();
                    }
                }
                else
                {
                    GenerateChildren();
                }
            }
        }
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
                
                // Vector3 pointOnUnitSphere = pointOnCube.normalized; /////
                // Vector3 pointOnUnitSphere = pointOnCube.normalized * Planet.size;
                // quad.vertices[i] = pointOnUnitSphere;
                
                // removed while debugging
                Vector3 pointOnUnitSphere = pointOnCube.normalized;
                float unscaledElevation = _shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere);
                quad.vertices[i] = pointOnUnitSphere * _shapeGenerator.GetScaledElevation(unscaledElevation);
                quad.uvs[i].y = unscaledElevation;
                quad.uvs[i].x = _colourGenerator.BiomePercentFromPoint(pointOnUnitSphere);
                
                if (x < _quadResolution - 1 && y < _quadResolution - 1)
                {
                    quad.triangles[triIndex++] = i;
                    quad.triangles[triIndex++] = i + _quadResolution + 1;
                    quad.triangles[triIndex++] = i + _quadResolution;
                    
                    quad.triangles[triIndex++] = i;
                    quad.triangles[triIndex++] = i + 1;
                    quad.triangles[triIndex++] = i + _quadResolution + 1;
                }
            }
        }
        // Debug.Log("Planet radius:   " + settings.planetRadius)
        this.vertices = quad.vertices;
        this.triangles = quad.triangles;
        this.uvs = quad.uvs;
        
        quad.triangles = GetTrianglesWithOffset(triOffset);
        
        return quad;
    }
    
    
    // Return triangles including offset
    public int[] GetTrianglesWithOffset(int triOffset)
    {
        int[] newTriangles = new int[this.triangles.Length];

        for (int i = 0; i < this.triangles.Length; i++)
        {
            newTriangles[i] = this.triangles[i] + triOffset;
        }

        return newTriangles;
    }
}