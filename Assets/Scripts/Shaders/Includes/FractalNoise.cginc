#include "SimplexNoise.cginc"

float simpleNoise(float3 position, int numLayers, float scale, float persistence, float lacunarity, float multiplier)
{
    float noiseValue = 0;
    float frequency = scale;
    float amplitude = 1;

    for (int i = 0; i < numLayers; i++)
    {
        float v = snoise(position * frequency);
        noiseValue += (v + 1) * 0.5 * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    return noiseValue * multiplier;
}