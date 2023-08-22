#include "SimplexNoise.cginc"



float simpleNoise(float3 position, int numLayers, float scale, float persistence, float lacunarity, float multiplier)
{
    float noiseValue = 0;
    float frequency = scale;
    float amplitude = 1;

    for (int i = 0; i < numLayers; i++)
    {
        // float v = snoise(position * frequency);
        // noiseValue += (v + 1) * 0.5 * amplitude;
        noiseValue = snoise(position * frequency) * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    return noiseValue * multiplier;
}


float simpleNoise(float3 position, float4 params[3])
{
    // Unpack parameters for readability
    const float3 offset = params[0].xyz;
    const float numLayers = params[0].w;
    const float scale = params[1].x;
    const float persistence = params[1].y;
    const float lacunarity = params[1].z;
    const float elevation = params[1].w;
    const float verticalShift = params[2].x;
    /**/

    float noiseValue = 0;
    float frequency = scale;
    float amplitude = 1;

    for (int i = 0; i < numLayers; i++)
    {
        // float v = snoise(position * frequency);
        // noiseValue += (v + 1) * 0.5 * amplitude;
        noiseValue = snoise(position * frequency) * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    return noiseValue * elevation + verticalShift;
}
 

float ridgeNoise(float3 position, float4 params[3])
{
    // Unpack parameters for readability
    const float3 offset = params[0].xyz;
    const float numLayers = params[0].w;
    const float scale = params[1].x;
    const float persistence = params[1].y;
    const float lacunarity = params[1].z;
    const float elevation = params[1].w;
    const float power = params[2].x;
    const float gain = params[2].y;
    const float verticalShift = params[2].z;
    const float peakSmoothing = params[2].w;
    /**/

    float noiseValue = 0;
    float frequency = scale;
    float amplitude = 1;
    float weight = 1;

    for (int i = 0; i < numLayers; i++)
    {
        float v = 1 - abs(snoise(position * frequency * offset));
        v = pow(abs(v), power);
        v *= weight;
        weight = saturate(v* gain);

        noiseValue += v * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }

    return noiseValue * elevation + verticalShift;
}