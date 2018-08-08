using UnityEngine;

public class BlurEffect : BasePostEffect
{
    [Range(0, 2)]
    public int downsample = 1;

    [Range(0.0f, 10.0f)]
    public float blurSize = 1.0f;

    [Range(1, 4)]
    public int blurIterations = 1;

    public BlurType blurType = BlurType.StandardGauss;

    public Shader blurShader = null;
    public Material blurMaterial = null;

    protected BlurEffectModel blurModel;

    public BlurEffectModel blurEffectModel;

    public override void OnDisable()
    {
        if (blurMaterial)
        {
            GameObject.DestroyImmediate(blurMaterial);
            blurMaterial = null;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        blurEffectModel = GetPostEffectModel<BlurEffectModel>();
        downsample = blurEffectModel.downsample;
        blurSize = blurEffectModel.blurSize;
        blurIterations = blurEffectModel.blurIterations;
        blurType = blurEffectModel.blurType;
        blurShader = blurEffectModel.blur;
    }

    public override void CheckShaderAndCreateMaterial()
    {
        blurMaterial = CheckShaderAndCreateMaterial(blurShader, blurMaterial);
    }

    public override void RenderImage(RenderTexture source, RenderTexture destination)
    {
        float widthMod = 1.0f / (1.0f * (1 << downsample));

        blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));
        source.filterMode = FilterMode.Bilinear;

        int rtW = source.width >> downsample;
        int rtH = source.height >> downsample;

        // downsample
        RenderTexture rt = renderTextureFactory.Get(source, downsample, FilterMode.Bilinear);

        Graphics.Blit(source, rt, blurMaterial, 0);

        var passOffs = blurType == BlurType.StandardGauss ? 0 : 2;

        for (int i = 0; i < blurIterations; i++)
        {
            float iterationOffs = (i * 1.0f);
            blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

            // vertical blur
            RenderTexture rt2 = renderTextureFactory.Get(source, downsample, FilterMode.Bilinear);

            Graphics.Blit(rt, rt2, blurMaterial, 1 + passOffs);
            renderTextureFactory.Release(rt);
            rt = rt2;

            // horizontal blur
            rt2 = renderTextureFactory.Get(source, downsample, FilterMode.Bilinear);

            Graphics.Blit(rt, rt2, blurMaterial, 2 + passOffs);
            renderTextureFactory.Release(rt);
            rt = rt2;
        }

        Graphics.Blit(rt, destination);
        renderTextureFactory.Release(rt);
    }
}