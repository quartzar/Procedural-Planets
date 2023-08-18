using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Planet : MonoBehaviour
{
    [Range(2, 128)]
    public int resolution = 10;
    [Range(1, 24)]
    public int quadResolution = 8;
    public static float cullingMinAngle = 1.45f;
    public bool autoUpdate = true;
    public enum FaceRenderMask { All, Top, Bottom, Left, Right, Front, Back };
    public FaceRenderMask faceRenderMask;
    
    public ShapeSettings shapeSettings;
    public ColourSettings colourSettings;
    
    [HideInInspector]
    public bool shapeSettingsFoldout;
    [HideInInspector]
    public bool colourSettingsFoldout;
    
    ShapeGenerator _shapeGenerator = new ShapeGenerator();
    ColourGenerator _colourGenerator = new ColourGenerator();
    
    [SerializeField, HideInInspector]
    MeshFilter[] meshFilters;
    TerrainFace[] terrainFaces;
    
    // Basic implementation
    public Transform player;
    // public static Vector3 playerPosition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
    [HideInInspector]
    public float distanceToPlayer;
    [HideInInspector]
    public float distanceToPlayerSqr;
    public static float size;
    [HideInInspector]
    public static bool isEditor = true;
    // [HideInInspector]
    // public float dstToPlayer;
    // [HideInInspector]
    // public float dstToPlayerSqr;
    
    // First value is level, second is distance from player. Finding the right values can be a little tricky
    // public static float[] detailLevelDistances = new float[] {
    //     Mathf.Infinity,
    //     3000f,
    //     1100f,
    //     500f,
    //     210f,
    //     100f,
    //     40f,
    // };
    
    public static readonly float[] DetailLevelDistances = new float[] {
        Mathf.Infinity,
        1000f,
        500f,
        200f,
        100f,
        75f,
        50f,
        40f,
        30f,
        20f,
        10f,
        5f,
        2f,
        1f
    };
    
    public static readonly int MaxDetailLevel = DetailLevelDistances.Length - 1;
    
    
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
    }

    
    // private void FixedUpdate()
    // {
    //     distanceToPlayer = Vector3.Distance(transform.position, player.position);
    //     distanceToPlayerSqr = distanceToPlayer * distanceToPlayer;
    // }
    
    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.position);
        distanceToPlayerSqr = distanceToPlayer * distanceToPlayer;
    }
    
    private void Start()
    {
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
    
    
    void Initialise()
    {
        _shapeGenerator.UpdateSettings(shapeSettings);
        _colourGenerator.UpdateSettings(colourSettings);
        size = shapeSettings.planetRadius;
        
        if (meshFilters == null || meshFilters.Length == 0)
        {
            meshFilters = new MeshFilter[6];
        }
        terrainFaces = new TerrainFace[6];
        
        Vector3[] directions = {Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back};
        string[] directionStrings = {"Up", "Down", "Left", "Right", "Forward", "Back"};

        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i] == null)
            {
                GameObject meshObj = new GameObject("Mesh " + directionStrings[i]);  //"mesh");
                meshObj.transform.parent = transform;
                
                meshObj.AddComponent<MeshRenderer>();
                meshFilters[i] = meshObj.AddComponent<MeshFilter>();
                meshFilters[i].sharedMesh = new Mesh();
            }
            meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = new Material(colourSettings.planetMaterial);
            
            terrainFaces[i] = new TerrainFace(_shapeGenerator, _colourGenerator, meshFilters[i].sharedMesh, resolution, quadResolution, directions[i], shapeSettings.planetRadius, this);
            bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i;
            meshFilters[i].gameObject.SetActive(renderFace);
        }
    }
    
    public void GeneratePlanet() // general regeneration method
    {
        isEditor = true;
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

    // private void GenerateMesh()
    // {
    //     for (int i = 0; i < 6; i++)
    //     {
    //         if (meshFilters[i].gameObject.activeSelf)
    //         {
    //             terrainFaces[i].ConstructMesh();
    //         }
    //     }
    //     _colourGenerator.UpdateElevation(_shapeGenerator.elevationMinMax);
    // }
    
    private void GenerateMesh() // generates the mesh
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].ConstructTree();
            }
        }
        _colourGenerator.UpdateElevation(_shapeGenerator.elevationMinMax);
    }
    
    private void UpdateMesh() // updates the mesh
    {
        for (int i = 0; i < 6; i++)
        {
            if (meshFilters[i].gameObject.activeSelf)
            {
                terrainFaces[i].UpdateTree();
            }
        }
        _colourGenerator.UpdateElevation(_shapeGenerator.elevationMinMax);
        // _colourGenerator.UpdateColours();
    }
    
    private void GenerateColours()
    {
       _colourGenerator.UpdateColours();
       for (int i = 0; i < 6; i++)
       {
           if (meshFilters[i].gameObject.activeSelf)
           {
               // terrainFaces[i].UpdateUVs(_colourGenerator);
           }
       }
    }

    
}
