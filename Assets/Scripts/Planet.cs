using System;
using System.Collections;
using System.Collections.Generic;
using NoiseSettings;
using Shape;
using Unity.Mathematics;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 2048)]
    public int resolution = 10;
    public float radius = 200f;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;
    
    // public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;
    
    // public NoiseSettings noiseSettings;
    // public SimpleNoiseSettings continentNoise;
    // public SimpleNoiseSettings maskNoise;
    // public RidgeNoiseSettings ridgeNoise;
    
    [HideInInspector]
    public bool shapeFoldout;
    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;
    
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    
    // public ComputeShader noiseShader;
    
    public static MinMax elevationMinMax;
    
    // Cached shader property IDs
    private static readonly int ElevationMinMax = Shader.PropertyToID("_ElevationMinMax");
    private static readonly int PlanetTexture = Shader.PropertyToID("_PlanetTexture");
    
    public CelestialBodySettings body;
    
    
    void Initialise()
    {
        elevationMinMax = new MinMax();
        
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];
        
        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back};
        string[] directionStrings = {"Up", "Down", "Left", "Right", "Forward", "Backward"};
        
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("Mesh " + directionStrings[i]);
                meshObj.transform.parent = transform;
                
                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;
            
            terrainFaces[i] = new TerrainFace(body, meshFilters[i].sharedMesh, resolution, radius, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }
    
    public void GeneratePlanet() // general regeneration method
    {
        Initialise();
        GenerateMesh();
        GenerateColours();
    }
    
    public void OnShapeSettingsUpdated() // shape settings specific regeneration method
    {
        if (autoUpdate)
        {
            Initialise();
            GenerateMesh();
            GenerateColours();
        }
    }
    
    public void OnColourSettingsUpdated() // colour settings specific regeneration method
    {
        if (autoUpdate)
        {
            Initialise();
            GenerateColours();
        }
    }

    private void GenerateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructMesh();
                terrainFaces[i].NoiseShader();
                // terrainFaces[i].AddNoiseWithComputeShader(noiseShader);
            }
        }
    }
    
    private void GenerateColours()
    {
        colourSettings.planetMaterial.SetVector(ElevationMinMax, new Vector4(elevationMinMax.Min * radius , elevationMinMax.Max * radius));
        // Debug.Log("Elevation Min: " + elevationMinMax.Min + ", Elevation Max: " + elevationMinMax.Max);
    }
    
    // private void GenerateColours()
    // {
    //    _colourGenerator.UpdateColours();
    //    for (int i = 0; i < 6; i++)
    //    {
    //        if (meshFilters[i].gameObject.activeSelf)
    //        {
    //            terrainFaces[i].UpdateUVs(_colourGenerator);
    //        }
    //    }
    // }
}
