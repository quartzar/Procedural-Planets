using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ShapeSettings : ScriptableObject
{
    [Range(1, 500)]
    public float planetRadius = 10;
    public NoiseLayer[] noiseLayers;
    public float oceanFloorDepth = 1;
    public float oceanFloorSmoothing = .5f;
    public float oceanDepthMultiplier = 1;
    public float mountainBlend = 1;
    
    [System.Serializable]
    public class NoiseLayer
    {
        public bool enabled = true;
        public bool useFirstLayerAsMask;
        // public NoiseSettings noiseSettings;
    }
}
