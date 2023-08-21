// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class ColourGenerator 
// {
//     ColourSettings settings;
//     Texture2D texture;
//     const int textureResolution = 50;
//     INoiseFilter biomeNoiseFilter;
//     
//     // Cached shader property IDs
//     private static readonly int ElevationMinMax = Shader.PropertyToID("_ElevationMinMax");
//     private static readonly int PlanetTexture = Shader.PropertyToID("_PlanetTexture");
//
//     public void UpdateSettings(ColourSettings settings)
//     {
//         this.settings = settings;
//         if (texture == null || texture.height != settings.biomeColourSettings.biomes.Length)
//         {
//             // first half of texture is ocean texture, second half is land texture
//             texture = new Texture2D(textureResolution * 2, settings.biomeColourSettings.biomes.Length, TextureFormat.RGBA32, false);
//         }
//         biomeNoiseFilter = NoiseFilterFactory.CreateNoiseFilter(settings.biomeColourSettings.noise);
//     }
//     
//     public void UpdateElevation(MinMax elevationMinMax)
//     {
//         settings.planetMaterial.SetVector(ElevationMinMax, new Vector4(elevationMinMax.Min, elevationMinMax.Max));
//     }
//     
//     public float BiomePercentFromPoint(Vector3 pointOnUnitSphere)
//     {
//         float heightPercent = (pointOnUnitSphere.y + 1) / 2f;
//         // vary heightPercent by noise
//         heightPercent += (biomeNoiseFilter.Evaluate(pointOnUnitSphere) - settings.biomeColourSettings.noiseOffset) * settings.biomeColourSettings.noiseStrength;
//         int numBiomes = settings.biomeColourSettings.biomes.Length;
//         float blendRange = settings.biomeColourSettings.blendAmount / 2f + .001f;
//         
//         float biomeIndex = 0;
//         for (int i = 0; i < numBiomes; i++)
//         {
//             float dst = heightPercent - settings.biomeColourSettings.biomes[i].startHeight;
//             float weight = Mathf.InverseLerp(-blendRange, blendRange, dst);
//             biomeIndex *= (1 - weight); // blend out previous biome
//             biomeIndex += i * weight;
//         }
//         
//         return biomeIndex / Mathf.Max(1, numBiomes - 1);
//     }
//     
//     public void UpdateColours()
//     {
//         Color[] colours = new Color[texture.width * texture.height];
//         int colourIndex = 0;
//         foreach (var biome in settings.biomeColourSettings.biomes)
//         {
//             for (int i = 0; i < textureResolution * 2; i++)
//             {
//                 Color gradientCol;
//                 if (i < textureResolution)
//                 {
//                     gradientCol = settings.oceanColour.Evaluate(i / (textureResolution - 1f));
//                 }
//                 else
//                 {
//                     gradientCol = biome.gradient.Evaluate((i - textureResolution) / (textureResolution - 1f));
//                 }
//                 Color tintCol = biome.tint;
//                 colours[colourIndex] = gradientCol * (1 - biome.tintPercent) + tintCol * biome.tintPercent;
//                 colourIndex++;
//             }
//         }
//         texture.SetPixels(colours);
//         texture.Apply();
//         settings.planetMaterial.SetTexture(PlanetTexture, texture);
//     }
// }
