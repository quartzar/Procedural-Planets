using System.Collections;
using System.Collections.Generic;
using Shape;
using UnityEngine;

[CreateAssetMenu (menuName = "Celestial Body/Settings Holder")]
public class CelestialBodySettings : ScriptableObject
{
    public CelestialBodyShape shape;
    // shader settings here
}
