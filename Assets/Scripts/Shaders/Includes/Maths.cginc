/** Helper functions for math operations.
 *
 *
 **/

/**
 * \brief Smooth maximum between a and b, with a smoothness of k.
 * When k = 0, this is equivalent to max(a, b).
 * \param a value a
 * \param b value b
 * \param k smoothing factor
 * \return Smooth maximum between a and b.
 */
float smoothMax(float a, float b, float k)
{
    k = min(0, -k);
    float h = max(0, min(1, (b - a + k) / (2 * k)));
    return a * h + b * (1 - h) - k * h * (1 - h);
}

/**
 * \brief Blends between two values based on a height.
 * \param startHeight starting height
 * \param blendDst blend height
 * \param height height to blend
 * \return Blended value.
 */
float Blend(float startHeight, float blendDst, float height)
{
    return smoothstep(startHeight - blendDst / 2, startHeight + blendDst / 2, height);
}