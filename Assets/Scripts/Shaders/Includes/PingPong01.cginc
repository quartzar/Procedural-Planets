// PingPong01.hlsl

void PingPong01_float(float In, out float Out)
{
    float v = In;
    if (v < 0.0){ v = -v; }
    int i = (int)v;
    Out = (i & 1) == 1 ? 1.0 - v + i : (v - i);
}
