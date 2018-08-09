using UnityEngine;
using System.Collections;

public class NightVisionEffect : BasePostEffect 
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
	
	private float randomValue = 0.0f;
	private Material nightVisionMaterial;

    public NightVisionEffectModel nightVisionEffectModel;

    public override void OnDisable()
    {
        if (nightVisionMaterial)
        {
            GameObject.DestroyImmediate(nightVisionMaterial);
            nightVisionMaterial = null;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        nightVisionEffectModel = GetPostEffectModel<NightVisionEffectModel>();
        nightVisionShader = nightVisionEffectModel.nightVisionShader;
        contrast = nightVisionEffectModel.contrast;
        brightness = nightVisionEffectModel.brightness;
        nightVisionColor = nightVisionEffectModel.nightVisionColor;
        vignetteTexture = nightVisionEffectModel.vignetteTexture;
        scanLineTexture = nightVisionEffectModel.scanLineTexture;
        scanLineTileAmount = nightVisionEffectModel.scanLineTileAmount;
        nightVisionNoise = nightVisionEffectModel.nightVisionNoise;
        noiseXSpeed = nightVisionEffectModel.noiseXSpeed;
        noiseYSpeed = nightVisionEffectModel.noiseYSpeed;
        distortion = nightVisionEffectModel.distortion;
        scale = nightVisionEffectModel.scale;
        randomValue = nightVisionEffectModel.randomValue;
    }

    public override void CheckShaderAndCreateMaterial()
    {
        nightVisionMaterial = CheckShaderAndCreateMaterial(nightVisionShader, nightVisionMaterial);
    }

    public override void RenderImage(RenderTexture sourceTexture, RenderTexture destTexture)
	{
		if(nightVisionShader != null)
		{
            nightVisionMaterial.SetFloat("_Contrast", contrast);
            nightVisionMaterial.SetFloat("_Brightness", brightness);
            nightVisionMaterial.SetColor("_NightVisionColor", nightVisionColor);
            nightVisionMaterial.SetFloat("_RandomValue", randomValue);
            nightVisionMaterial.SetFloat("_distortion", distortion);
            nightVisionMaterial.SetFloat("_scale",scale);
			
			if (vignetteTexture)
			{
                nightVisionMaterial.SetTexture("_VignetteTex", vignetteTexture);
			}
			
			if (scanLineTexture)
			{
                nightVisionMaterial.SetTexture("_ScanLineTex", scanLineTexture);
                nightVisionMaterial.SetFloat("_ScanLineTileAmount", scanLineTileAmount);
			}
			
			if (nightVisionNoise)
			{
                nightVisionMaterial.SetTexture("_NoiseTex", nightVisionNoise);
                nightVisionMaterial.SetFloat("_NoiseXSpeed", noiseXSpeed);
                nightVisionMaterial.SetFloat("_NoiseYSpeed", noiseYSpeed);
			}
			
			Graphics.Blit(sourceTexture, destTexture, nightVisionMaterial);
		}
		else
		{
			Graphics.Blit(sourceTexture, destTexture);
		}
	}
	
	void Update()
	{
		contrast = Mathf.Clamp(contrast, 0f,4f);
		brightness = Mathf.Clamp(brightness, 0f, 2f);
		randomValue = Random.Range(-1f,1f);
		distortion = Mathf.Clamp(distortion, -1f,1f);
		scale = Mathf.Clamp(scale, 0f, 3f);
	}
}
