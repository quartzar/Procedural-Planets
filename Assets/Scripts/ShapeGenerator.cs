using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeGenerator {

    ShapeSettings settings;
    INoiseFilter[] noiseFilters;
    
    public MinMax elevationMinMax;

    public void UpdateSettings(ShapeSettings settings)
    {
        this.settings = settings;
        noiseFilters = new INoiseFilter[settings.noiseLayers.Length];
        for (int i = 0; i < noiseFilters.Length; i++)
        {
            noiseFilters[i] = NoiseFilterFactory.CreateNoiseFilter(settings.noiseLayers[i].noiseSettings);
        }
        elevationMinMax = new MinMax();
    }

    public float CalculateUnscaledElevation(Vector3 pointOnUnitSphere)
    {
        float firstLayerValue = 0;
        float elevation = 0;

        if (noiseFilters.Length > 0)
        {
            firstLayerValue = noiseFilters[0].Evaluate(pointOnUnitSphere);
            if (settings.noiseLayers[0].enabled)
            {
                elevation = firstLayerValue;
            }
        }

        for (int i = 1; i < noiseFilters.Length; i++)
        {
            if (settings.noiseLayers[i].enabled)
            {
                float mask = (settings.noiseLayers[i].useFirstLayerAsMask) ? firstLayerValue : 1;
                elevation += noiseFilters[i].Evaluate(pointOnUnitSphere) * mask;
            }
        }

        elevationMinMax.AddValue(elevation);
        return elevation;
    }
    
    public float GetScaledElevation(float unscaledElevation)
    {
        // float elevation = Mathf.Max(0, unscaledElevation);
        // elevation = settings.planetRadius * (1 + elevation);
        
        // float oceanFloorDepth = settings.planetRadius - settings.oceanFloorDepth;
        // float oceanFloorShape = -oceanFloorDepth + unscaledElevation * 0.15f;
        // float elevation = settings.planetRadius * (1 + unscaledElevation);
        // float continentShape = Mathf.SmoothStep(elevation, oceanFloorShape, settings.oceanFloorSmoothing);
        //
        // continentShape *= (continentShape < 0) ? 1 + settings.oceanDepthMultiplier : 1;
        
        // float elevation = Mathf.SmoothStep(unscaledElevation, oceanFloorShape, settings.oceanFloorSmoothing);
        // elevation *= (elevation < 0) ? 1 + settings.oceanDepthMultiplier : 1;
        
        float continentShape = unscaledElevation;
        // float oceanFloorDepth = settings.planetRadius - settings.oceanFloorDepth;
        
        float oceanFloorShape = -settings.oceanFloorDepth + continentShape * 0.15f;
        continentShape = Mathf.Lerp(continentShape, oceanFloorShape, settings.oceanFloorSmoothing);
        continentShape *= (unscaledElevation < 0) ? 1 + settings.oceanDepthMultiplier : 1;
        
        
        return (1 + continentShape * 0.1f) + settings.planetRadius * (1 + Mathf.Max(0, unscaledElevation));
    }
}