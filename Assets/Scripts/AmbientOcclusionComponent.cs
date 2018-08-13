using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class AmbientOcclusionEffect : BasePostEffect
{
    static class Uniforms
    {
        internal static readonly int _Intensity = Shader.PropertyToID("_Intensity");
        internal static readonly int _Radius = Shader.PropertyToID("_Radius");
        internal static readonly int _Downsample = Shader.PropertyToID("_Downsample");
        internal static readonly int _SampleCount = Shader.PropertyToID("_SampleCount");
        internal static readonly int _OcclusionTexture1 = Shader.PropertyToID("_OcclusionTexture1");
        internal static readonly int _OcclusionTexture2 = Shader.PropertyToID("_OcclusionTexture2");
        internal static readonly int _OcclusionTexture = Shader.PropertyToID("_OcclusionTexture");
        internal static readonly int _MainTex = Shader.PropertyToID("_MainTex");
        internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
    }

    const string k_BlitShaderString = "Hidden/Post FX/Blit";
    const string k_ShaderString = "Hidden/Post FX/Ambient Occlusion";

    readonly RenderTargetIdentifier[] m_MRT =
    {
            BuiltinRenderTextureType.GBuffer0, // Albedo, Occ
            BuiltinRenderTextureType.CameraTarget // Ambient
        };

    enum OcclusionSource
    {
        DepthTexture,
        DepthNormalsTexture,
    }

    private float intensity;
    private float radius;
    private bool downsampling;
    private int sampleCount;
    private int width;
    private int height;
    private bool isHdr;
    public AmbientOcclusionModel ambientOcclusionModel;

    public Shader k_BlitShader;
    public Shader k_Shader;

    public override void OnEnable()
    {
        base.OnEnable();
        ambientOcclusionModel = GetPostEffectModel<AmbientOcclusionModel>();
        intensity = ambientOcclusionModel.intensity;
        radius = ambientOcclusionModel.radius;
        downsampling = ambientOcclusionModel.downsampling;
        sampleCount = ambientOcclusionModel.sampleCount;
        width = ambientOcclusionModel.width;
        height = ambientOcclusionModel.height;
        isHdr = ambientOcclusionModel.isHdr;

        k_BlitShader = ambientOcclusionModel.k_BlitShader;
        k_Shader = ambientOcclusionModel.k_Shader;
    }

    public override string GetName()
    {
        return "Ambient Occlusion";
    }

    public override void PopulateCommandBuffer(CommandBuffer cb)
    {
        // Material setup
        var blitMaterial = CheckShaderAndCreateMaterial(k_BlitShader, null);
        var material = CheckShaderAndCreateMaterial(k_Shader, null);

        material.shaderKeywords = null;
        material.SetFloat(Uniforms._Intensity, intensity);
        material.SetFloat(Uniforms._Radius, radius);
        material.SetFloat(Uniforms._Downsample, downsampling ? 0.5f : 1f);
        material.SetInt(Uniforms._SampleCount, (int)sampleCount);

        int tw = width;
        int th = height;
        int ts = downsampling ? 2 : 1;
        const RenderTextureFormat kFormat = RenderTextureFormat.ARGB32;
        const RenderTextureReadWrite kRWMode = RenderTextureReadWrite.Linear;
        const FilterMode kFilter = FilterMode.Bilinear;

        // AO buffer
        var rtMask = Uniforms._OcclusionTexture1;
        cb.GetTemporaryRT(rtMask, tw / ts, th / ts, 0, kFilter, kFormat, kRWMode);

        // AO estimation
        cb.Blit((Texture)null, rtMask, material, (int)OcclusionSource.DepthTexture);

        // Blur buffer
        var rtBlur = Uniforms._OcclusionTexture2;

        // Separable blur (horizontal pass)
        cb.GetTemporaryRT(rtBlur, tw, th, 0, kFilter, kFormat, kRWMode);
        cb.SetGlobalTexture(Uniforms._MainTex, rtMask);
        cb.Blit(rtMask, rtBlur, material, 3);
        cb.ReleaseTemporaryRT(rtMask);

        // Separable blur (vertical pass)
        rtMask = Uniforms._OcclusionTexture;
        cb.GetTemporaryRT(rtMask, tw, th, 0, kFilter, kFormat, kRWMode);
        cb.SetGlobalTexture(Uniforms._MainTex, rtBlur);
        cb.Blit(rtBlur, rtMask, material, 5);
        cb.ReleaseTemporaryRT(rtBlur);

        var fbFormat = isHdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;

        int tempRT = Uniforms._TempRT;
        cb.GetTemporaryRT(tempRT, tw, th, 0, FilterMode.Bilinear, fbFormat);
        cb.Blit(BuiltinRenderTextureType.CameraTarget, tempRT, blitMaterial, 0);
        cb.SetGlobalTexture(Uniforms._MainTex, tempRT);
        cb.Blit(tempRT, BuiltinRenderTextureType.CameraTarget, material, 6);
        cb.ReleaseTemporaryRT(tempRT);

        cb.ReleaseTemporaryRT(rtMask);
    }
}