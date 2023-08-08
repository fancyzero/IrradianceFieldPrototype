using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyVisibilityProbeData : ScriptableObject
{
    //a struct contains position and 9 floats in a float array

    [Serializable]
    public struct SkyVisibilityProbe
    {
        public float[] coeffs;
    }
    [SerializeField]
    public List<SkyVisibilityProbe> probes;
    public int xSize;
    public int ySize;
    public int zSize;


}
