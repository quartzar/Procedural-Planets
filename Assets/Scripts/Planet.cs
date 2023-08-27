using System;
using System.Collections;
using System.Collections.Generic;
using NoiseSettings;
using Shape;
using Unity.Mathematics;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Range(2, 2048)]
    public int resolution = 10;
    public int subdivisions = 1;
    public float radius = 200f;
    public bool spawnOcean = false;
    public float oceanScale = 1f;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;
    
    public ColourSettings colourSettings;
    
    public GameObject oceanPrefab;
    private GameObject _oceanSphere;
    
    [HideInInspector]
    public bool shapeFoldout;
    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;
    
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    
    public static MinMax elevationMinMax;
    
    // Cached shader property IDs
    private static readonly int ElevationMinMax = Shader.PropertyToID("_ElevationMinMax");
    private static readonly int PlanetTexture = Shader.PropertyToID("_PlanetTexture");
    
    public CelestialBodySettings body;
    private bool _isheightMapComputeNull;

    
    // // //
    
    public static readonly float CullingMinAngle = 1.45f;
    
    public Transform player;
    [HideInInspector]
    public float distanceToPlayer;
    [HideInInspector]
    public float distanceToPlayerSqr;
    public static float size;
    public static bool isEditor = true;
    
    public static readonly float[] DetailLevelDistances = new float[] {
        Mathf.Infinity,
        3000f,
        1000f,
        500f,
        100f,
        50f,
        40f,
        35f,
        30f,
        20f
    };
    
    public static readonly int MaxDetailLevel = DetailLevelDistances.Length - 1;
    
    // // //
    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        distanceToPlayerSqr = distanceToPlayer * distanceToPlayer;
    }
    
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        GeneratePlanet();
        isEditor = false;
        StartCoroutine(PlanetGenerationLoop());
    }
    
    private IEnumerator PlanetGenerationLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateMesh();
        }
    }

    private void Awake()
    {
        _isheightMapComputeNull = body.shape.heightMapCompute == null;
        
        player = GameObject.FindGameObjectWithTag("Player").transform;
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

    void Initialise()
    {
        // Enforce resolution is an odd number
        if (resolution % 2 == 0)
        {
            resolution++;
        }
        
        _isheightMapComputeNull = body.shape.heightMapCompute == null;
        
        elevationMinMax = new MinMax();
        
        size = radius;
        
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];
        
        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back};
        string[] directionStrings = {"Up", "Down", "Left", "Right", "Forward", "Backward"};
        
        for (int i = 0; i < 6; i++)
        {
            // Create mesh filters only if they don't exist
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("Mesh " + directionStrings[i]);
                meshObj.transform.parent = transform;
                
                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colourSettings.planetMaterial;
            
            terrainFaces[i] = new TerrainFace(this, body, meshFilters[i].sharedMesh, resolution, subdivisions, radius, directions[i]);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
        
        // Add the ocean sphere prefab
        if (_oceanSphere == null && spawnOcean)
        {
            _oceanSphere = Instantiate(oceanPrefab, transform.position, Quaternion.identity);
            _oceanSphere.transform.parent = transform;
            float scale = (radius * 100f) + oceanScale;
            _oceanSphere.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
    
    public void GeneratePlanet() // general regeneration method
    {
        Initialise();
        GenerateMesh();
        GenerateColours();
        UpdateOcean();
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
                // terrainFaces[i].ConstructMesh();
                terrainFaces[i].ConstructTree();
                
                // if (!_isheightMapComputeNull)
                // {
                //     terrainFaces[i].NoiseShader();
                // }
                // terrainFaces[i].AddNoiseWithComputeShader(noiseShader);
            }
        }
    }
    
    private void UpdateMesh()
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructTree();
            }
        }
    }
    
    private void GenerateColours()
    {
        colourSettings.planetMaterial.SetVector(ElevationMinMax, new Vector4(elevationMinMax.Min * radius , elevationMinMax.Max * radius));
        // Debug.Log("Elevation Min: " + elevationMinMax.Min + ", Elevation Max: " + elevationMinMax.Max);
    }
    
    private void UpdateOcean()
    {
        if (spawnOcean)
        {
            float scale = (radius * 100f) + oceanScale * 10f;
            _oceanSphere.transform.localScale = new Vector3(scale, scale, scale);
        }
        else if (_oceanSphere != null)
        {
            // check if in edit mode
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Destroy(_oceanSphere);
            }
            else
            {
                DestroyImmediate(_oceanSphere);
            }
        }
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
