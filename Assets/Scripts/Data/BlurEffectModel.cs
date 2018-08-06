using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlurEffectModel : BasePostEffectModel
{
    [Range(0, 2)]
    public int downsample = 1;

    [Range(0.0f, 10.0f)]
    public float blurSize = 1.0f;

    [Range(1, 4)]
    public int blurIterations = 1;

    public BlurType blurType = BlurType.StandardGauss;

    public Shader blur;
}

public enum BlurType
{
    StandardGauss = 0,
    SgxGauss = 1,
}