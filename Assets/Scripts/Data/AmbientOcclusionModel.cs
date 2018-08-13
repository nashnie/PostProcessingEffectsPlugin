using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AmbientOcclusionModel : BasePostEffectModel
{
    public float intensity;
    public float radius;
    public bool downsampling;
    public int sampleCount;
    public int width;
    public int height;
    public bool isHdr;

    public Shader k_BlitShader;
    public Shader k_Shader;
}
