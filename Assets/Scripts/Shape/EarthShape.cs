using System.Collections;
using System.Collections.Generic;
using NoiseSettings;
using UnityEngine;

namespace Shape {
    [CreateAssetMenu (menuName = "Celestial Body/Earth-Like/Earth Shape")]
    public class EarthShape : CelestialBodyShape
    {
        [Header ("Continent Settings")]
        public float oceanFloorDepth = 1;
        public float oceanDepthMultiplier = 1;
        public float oceanFloorSmoothing = .5f;
        public float mountainBlend = 1;
        
        [Header ("Noise Settings")]
        public SimpleNoiseSettings continentNoise;
        public SimpleNoiseSettings maskNoise;
        
        public RidgeNoiseSettings ridgeNoise;

        protected override void SetShapeData() // auto set data 
        {
            continentNoise.SetComputeValues(heightMapCompute, "_continents");
            maskNoise.SetComputeValues(heightMapCompute, "_mask");
            ridgeNoise.SetComputeValues(heightMapCompute, "_mountains");
            
            heightMapCompute.SetFloat(Shader.PropertyToID("_oceanFloorDepth"), oceanFloorDepth);
            heightMapCompute.SetFloat(Shader.PropertyToID("_oceanDepthMultiplier"), oceanDepthMultiplier);
            heightMapCompute.SetFloat(Shader.PropertyToID("_oceanFloorSmoothing"), oceanFloorSmoothing);
            heightMapCompute.SetFloat(Shader.PropertyToID("_mountainBlend"), mountainBlend);
        }
    }
}