using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// OldFilmEffectModel
/// </summary>

[System.Serializable]
public class OldFilmEffectModel : BasePostEffectModel
{
    public Shader oldFilmShader;

    public float OldFilmEffectAmount = 1.0f;
    public float contrast = 3.0f;
    public float distortion = 0.2f;
    public float cubicDistortion = 0.6f;
    public float scale = 0.8f;

    public Color sepiaColor = Color.white;
    public Texture2D vignetteTexture;
    public float vignetteAmount = 1.0f;

    public Texture2D scratchesTexture;
    public float scratchesYSpeed = 10.0f;
    public float scratchesXSpeed = 10.0f;

    public Texture2D dustTexture;
    public float dustYSpeed = 10.0f;
    public float dustXSpeed = 10.0f;

    public float randomValue;
}
