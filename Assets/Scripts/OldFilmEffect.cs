using UnityEngine;
using System.Collections;
/// <summary>
/// OldFilmEffect
/// Nash
/// </summary>
public class OldFilmEffect : BasePostEffect 
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
	
	public  Texture2D dustTexture;
	public float dustYSpeed = 10.0f;
	public float dustXSpeed = 10.0f;
	
	private Material oldFilmMaterial;
	private float randomValue;

    public OldFilmEffectModel oldFilmEffectModel;

    public override void OnDisable()
    {
        if (oldFilmMaterial)
        {
            GameObject.DestroyImmediate(oldFilmMaterial);
            oldFilmMaterial = null;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        //Copy data
        oldFilmEffectModel = GetPostEffectModel<OldFilmEffectModel>();
        oldFilmShader = oldFilmEffectModel.oldFilmShader;
        contrast = oldFilmEffectModel.contrast;
        distortion = oldFilmEffectModel.distortion;
        cubicDistortion = oldFilmEffectModel.cubicDistortion;
        scale = oldFilmEffectModel.scale;

        sepiaColor = oldFilmEffectModel.sepiaColor;
        vignetteTexture = oldFilmEffectModel.vignetteTexture;
        vignetteAmount = oldFilmEffectModel.vignetteAmount;

        scratchesTexture = oldFilmEffectModel.scratchesTexture;
        scratchesYSpeed = oldFilmEffectModel.scratchesYSpeed;
        scratchesXSpeed = oldFilmEffectModel.scratchesXSpeed;

        dustTexture = oldFilmEffectModel.dustTexture;
        dustYSpeed = oldFilmEffectModel.dustYSpeed;
        dustXSpeed = oldFilmEffectModel.dustXSpeed;

        randomValue = oldFilmEffectModel.randomValue;
    }

    public override void CheckShaderAndCreateMaterial()
    {
        oldFilmMaterial = CheckShaderAndCreateMaterial(oldFilmShader, oldFilmMaterial);
    }

    public override void RenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if(oldFilmShader != null)
		{
            oldFilmMaterial.SetColor("_SepiaColor", sepiaColor);
            oldFilmMaterial.SetFloat("_VignetteAmount", vignetteAmount);
            oldFilmMaterial.SetFloat("_EffectAmount", OldFilmEffectAmount);
            oldFilmMaterial.SetFloat("_Contrast", contrast);
            oldFilmMaterial.SetFloat("_cubicDistortion", cubicDistortion);
            oldFilmMaterial.SetFloat("_distortion", distortion);
            oldFilmMaterial.SetFloat("_scale", scale);
			
			if (vignetteTexture)
			{
                oldFilmMaterial.SetTexture("_VignetteTex", vignetteTexture);
			}
			
			if (scratchesTexture)
			{
                oldFilmMaterial.SetTexture("_ScratchesTex", scratchesTexture);
                oldFilmMaterial.SetFloat("_ScratchesYSpeed", scratchesYSpeed);
                oldFilmMaterial.SetFloat("_ScratchesXSpeed", scratchesXSpeed);
			}
			
			if (dustTexture)
			{
                oldFilmMaterial.SetTexture("_DustTex", dustTexture);
                oldFilmMaterial.SetFloat("_dustYSpeed", dustYSpeed);
                oldFilmMaterial.SetFloat("_dustXSpeed", dustXSpeed);
                oldFilmMaterial.SetFloat("_RandomValue", randomValue);
			}
			
			Graphics.Blit(sourceTexture, destTexture, oldFilmMaterial);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}
	
	void Update()
	{
		vignetteAmount = Mathf.Clamp01(vignetteAmount);
		OldFilmEffectAmount = Mathf.Clamp(OldFilmEffectAmount, 0f, 1.5f);
		randomValue = Random.Range(-1f,1f);
		contrast = Mathf.Clamp(contrast, 0f, 4f);
		distortion = Mathf.Clamp(distortion, -1f,1f);
		cubicDistortion = Mathf.Clamp(cubicDistortion, -1f, 1f);
		scale = Mathf.Clamp(scale, 0f, 1f);
	}
}
