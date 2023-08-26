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
        noiseValue += snoise(position * frequency) * amplitude;
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
    float frequency = scale * 0.01;
    float amplitude = 1;

    for (int i = 0; i < numLayers; i++)
    {
        // float v = snoise(position * frequency);
        // noiseValue += (v + 1) * 0.5 * amplitude;
        noiseValue += snoise(position * frequency + offset) * amplitude;
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
    float frequency = scale * 0.01;
    float amplitude = 1;
    float weight = 1;

    for (int i = 0; i < numLayers; i++)
    {
        float v = 1 - abs(snoise(position * frequency + offset));
        v = pow(abs(v), power);
        v *= weight;
        weight = saturate(v * gain);

        noiseValue += v * amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }

    return noiseValue * elevation + verticalShift;
}


// Sample the noise several times at small offsets from the centre and average the result
// This reduces some of the harsh jaggedness that can occur
// From Sebastian Lague
float smoothedRidgeNoise(float3 pos, float4 params[3]) {
    float3 sphereNormal = normalize(pos);
    float3 axisA = cross(sphereNormal, float3(0,1,0));
    float3 axisB = cross(sphereNormal, axisA);

    float offsetDst = params[2].w * 0.01;
    float sample0 = ridgeNoise(pos, params);
    float sample1 = ridgeNoise(pos - axisA * offsetDst, params);
    float sample2 = ridgeNoise(pos + axisA * offsetDst, params);
    float sample3 = ridgeNoise(pos - axisB * offsetDst, params);
    float sample4 = ridgeNoise(pos + axisB * offsetDst, params);
    return (sample0 + sample1 + sample2 + sample3 + sample4) / 5;
}