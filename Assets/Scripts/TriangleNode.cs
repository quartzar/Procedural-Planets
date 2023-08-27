using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleNode 
{
    private Planet _planetScript;
    private TriangleNode[] _children;
    private Vector3 _position;
    private readonly float _radius;
    private int _detailLevel;
    private readonly Vector3 _localUp;
    private readonly Vector3 _axisA;
    private readonly Vector3 _axisB;
    
    private readonly Vector3 _normalisedPos;
    
    public Vector3[] _baseVertices;
    public int[] _baseTriangles;
    
    public Vector3[] vertices;
    public int[] triangles;
    
    
    public TriangleNode(Planet planetScript, TriangleNode[] children, Vector3 position, float radius, int detailLevel, Vector3 localUp, Vector3 axisA, Vector3 axisB, Vector3[] baseVertices, int[] baseTriangles)
    {
        _planetScript = planetScript;
        _children = children;
        _position = position;
        _radius = radius;
        _detailLevel = detailLevel;
        _localUp = localUp;
        _axisA = axisA;
        _axisB = axisB;
        
        _baseVertices = baseVertices;
        _baseTriangles = baseTriangles;
        
        _normalisedPos = _position.normalized;
    }
    
    
    
    // Primary function to generate the quadtree
    public void GenerateChildren()
    {
        // Check detail level between 0 and max level. Max level depends on how many detail levels are defined
        if (_detailLevel <= Planet.MaxDetailLevel && _detailLevel >= 0)
        {
            //
            //
            if (Vector3.Distance(_planetScript.transform.TransformDirection(_normalisedPos * Planet.size) + _planetScript.transform.position, _planetScript.player.position) <= Planet.DetailLevelDistances[_detailLevel])
            {
                // Assign the quadtree children
                _children = new TriangleNode[2];
                // _children[0] = new TriangleNode(_planetScript, new TriangleNode[0], _position + _axisA * _radius / 2 + _axisB * _radius / 2, _radius / 2f, _detailLevel + 1, _localUp, _axisA, _axisB);
                // _children[1] = new TriangleNode(_planetScript, new TriangleNode[0], _position + _axisA * _radius / 2 - _axisB * _radius / 2, _radius / 2f, _detailLevel + 1, _localUp, _axisA, _axisB);
               
                Vector3 midPoint = (_position + _axisA * _radius + _axisB * _radius) / 2;
                midPoint = midPoint.normalized * _radius;
                
                // triangle 1
                // Vector3[] vertices1 = new Vector3[]
                // {
                //     vertices[0], vertices[1]
                // }
                
                
                
                Vector3[] oldVertices = _baseVertices;
                int[] oldTriangles = _baseTriangles;
        
        
                List<Vector3> newVertices = new List<Vector3>(oldVertices);
                List<int> newTriangles = new List<int>();

                // For each old triangle, subdivide and add new triangles to the newTriangles list
                // for (int i = 0; i < baseQuadTriangles.Length; i += 3)
                // {
                Vector3 v0 = oldVertices[oldTriangles[0]];
                Vector3 v1 = oldVertices[oldTriangles[1]];
                Vector3 v2 = oldVertices[oldTriangles[2]];

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
                    newTriangles.AddRange(new int[] { oldTriangles[0], midIndex, oldTriangles[2], midIndex, oldTriangles[1], oldTriangles[2] });
                }
                else if (d12 > d01 && d12 > d20)
                {
                    // The longest edge is v1-v2
                    Vector3 m = ((v1 + v2) / 2).normalized * _radius;
                    newVertices.Add(m);
                    int midIndex = newVertices.Count - 1;

                    // Add new triangles
                    newTriangles.AddRange(new int[] { oldTriangles[0], oldTriangles[1], midIndex, oldTriangles[0], midIndex, oldTriangles[2] });
                }
                else
                {
                    // The longest edge is v2-v0
                    Vector3 m = ((v2 + v0) / 2).normalized * _radius;
                    newVertices.Add(m);
                    int midIndex = newVertices.Count - 1;

                    // Add new triangles
                    newTriangles.AddRange(new int[] { oldTriangles[0], oldTriangles[1], midIndex, midIndex, oldTriangles[1], oldTriangles[2] });
                
                }
                

                _children[0] = new TriangleNode(_planetScript, new TriangleNode[0], _position /2f, _radius , _detailLevel + 1, _localUp, _axisA, _axisB, newVertices.ToArray(), newTriangles.ToArray());
                _children[1] = new TriangleNode(_planetScript, new TriangleNode[0], _position /2f, _radius , _detailLevel + 1, _localUp, _axisA, _axisB, newVertices.ToArray(), newTriangles.ToArray());
                
                // Generate the grandchildren
                foreach (TriangleNode child in _children)
                {
                    child.GenerateChildren();
                }
            }
        }
    }
    
    
    // Returns the most recent quad in every branch of the quadtree, i.e. quad to be rendered
    public TriangleNode[] GetVisibleChildren()
    {
        List<TriangleNode> toBeRendered = new List<TriangleNode>();
        
        if (_children.Length > 0)
        {
            foreach (TriangleNode child in _children)
            {
                toBeRendered.AddRange(child.GetVisibleChildren());
            }
        }
        else // Leaf node, i.e. quadtree with no children
        {
            // Check if quad is within culling angle, if so add to list of quads to be rendered
            // if (Mathf.Acos((Mathf.Pow(Planet.size, 2) + Mathf.Pow(_planetScript.distanceToPlayer, 2) - 
            //                 Mathf.Pow(Vector3.Distance(_planetScript.transform.TransformDirection(_normalisedPos * Planet.size), _planetScript.player.position), 2)) / 
            //                (2 * Planet.size * _planetScript.distanceToPlayer)) < Planet.CullingMinAngle || Planet.isEditor)
            // {
            //     toBeRendered.Add(this);
            // }
            
            toBeRendered.Add(this);
        }
        
        return toBeRendered.ToArray();
    }
    
    
    // Updates the quadtree
    public void UpdateQuad()
    {
        // get distance from player to quadtree with direction
        float dstToPlayer = Vector3.Distance(_planetScript.transform.TransformDirection(_normalisedPos * Planet.size) + _planetScript.transform.position, _planetScript.player.position);
        // Debug.Log("Max detail: " + Planet.MaxDetailLevel);
        if (_detailLevel <= Planet.MaxDetailLevel)
        {
            if (dstToPlayer > Planet.DetailLevelDistances[_detailLevel])
            {
                _children = new TriangleNode[0];
            }
            else
            {
                if (_children.Length > 0)
                {
                    foreach (TriangleNode child in _children)
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
    
    public (Vector3[], int[]) ConstructLEB(int triOffset) // Longest Edge Bisection
    {
        // Vector3[] baseQuadVertices = new Vector3[] { 
        //     (_position + _axisA + _axisB).normalized * _radius,
        //     (_position + _axisA - _axisB).normalized * _radius,
        //     (_position - _axisA + _axisB).normalized * _radius,
        //     (_position - _axisA - _axisB).normalized * _radius 
        // };
        // int[] baseQuadTriangles = new int[] { 0, 2, 1, 1, 2, 3 };
        
        
        // Vector3[] oldVertices = _mesh.vertices;
        // int[] oldTriangles = _mesh.triangles;
        
        // Create new arrays to hold subdivided mesh data
        // List<Vector3> newVertices = new List<Vector3>(baseQuadVertices);
        
        Vector3[] oldVertices = _baseVertices;
        int[] oldTriangles = _baseTriangles;
        
        
        List<Vector3> newVertices = new List<Vector3>(oldVertices);
        List<int> newTriangles = new List<int>();

        // For each old triangle, subdivide and add new triangles to the newTriangles list
        // for (int i = 0; i < baseQuadTriangles.Length; i += 3)
        // {
        Vector3 v0 = oldVertices[oldTriangles[0]];
        Vector3 v1 = oldVertices[oldTriangles[1]];
        Vector3 v2 = oldVertices[oldTriangles[2]];

        // Find the longest edge and its opposite vertex
        float d01 = Vector3.Distance(v0, v1);
        float d12 = Vector3.Distance(v1, v2);
        float d20 = Vector3.Distance(v2, v0);

        if (d01 > d12 && d01 > d20)
        {
            // The longest edge is v0-v1
            Vector3 m = ((v0 + v1) / 2).normalized;// * _radius;
            newVertices.Add(m);
            int midIndex = newVertices.Count - 1;

            // Add new triangles
            newTriangles.AddRange(new int[] { oldTriangles[0], midIndex, oldTriangles[2], midIndex, oldTriangles[1], oldTriangles[2] });
        }
        else if (d12 > d01 && d12 > d20)
        {
            // The longest edge is v1-v2
            Vector3 m = ((v1 + v2) / 2).normalized;// * _radius;
            newVertices.Add(m);
            int midIndex = newVertices.Count - 1;

            // Add new triangles
            newTriangles.AddRange(new int[] { oldTriangles[0], oldTriangles[1], midIndex, oldTriangles[0], midIndex, oldTriangles[2] });
        }
        else
        {
            // The longest edge is v2-v0
            Vector3 m = ((v2 + v0) / 2).normalized;// * _radius;
            newVertices.Add(m);
            int midIndex = newVertices.Count - 1;

            // Add new triangles
            newTriangles.AddRange(new int[] { oldTriangles[0], oldTriangles[1], midIndex, midIndex, oldTriangles[1], oldTriangles[2] });
                
        }
        // }
        // Update node data
        this.vertices = newVertices.ToArray();
        this.triangles = newTriangles.ToArray();
        
        return (vertices, GetTrianglesWithOffset(triOffset));
    }
    
    // Return triangles including offset
    private int[] GetTrianglesWithOffset(int triOffset)
    {
        int[] newTriangles = new int[this.triangles.Length];

        for (int i = 0; i < this.triangles.Length; i++)
        {
            newTriangles[i] = this.triangles[i] + triOffset;
        }

        return newTriangles;
    }
}
