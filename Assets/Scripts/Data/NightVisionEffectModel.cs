using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NightVisionEffectModel : BasePostEffectModel
{
    public Shader nightVisionShader;

    public float contrast = 2.0f;
    public float brightness = 1.0f;
    public Color nightVisionColor = Color.white;

    public Texture2D vignetteTexture;

    public Texture2D scanLineTexture;
    public float scanLineTileAmount = 4.0f;

    public Texture2D nightVisionNoise;
    public float noiseXSpeed = 100.0f;
    public float noiseYSpeed = 100.0f;

    public float distortion = 0.2f;
    public float scale = 0.8f;

    public float randomValue = 0.0f;
}
