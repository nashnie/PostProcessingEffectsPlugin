using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PostProcessingEffect
/// </summary>
public class PostProcessingEffect : MonoBehaviour
{
    private bool isSupported = true;
#pragma warning disable 414
    private bool supportHDRTextures = true;
    private bool supportDX11 = false;
#pragma warning restore 414
    private Camera rendererCamera;
    private RenderTextureFactory renderTextureFactory;

    public BlurEffect blurEffect;
    public BlurEffectModel blurEffectModel;

    public List<BasePostEffect> postEffectList = new List<BasePostEffect>();

    public void OnPostRender()
    {
    }

    public void OnPreRender()
    {
    }

    public void OnPreCull()
    {
    }

    private bool CheckSupport(bool needDepth)
    {
        isSupported = true;
        supportHDRTextures = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
        supportDX11 = SystemInfo.graphicsShaderLevel >= 50 && SystemInfo.supportsComputeShaders;

        if (!SystemInfo.supportsImageEffects)
        {
            NotSupported();
            return false;
        }

        if (needDepth && !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth))
        {
            NotSupported();
            return false;
        }

        if (needDepth)
        {
            rendererCamera.depthTextureMode |= DepthTextureMode.Depth;
        }

        return true;
    }

    public void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (CheckResources() == false)
        {
            Graphics.Blit(source, destination);
            return;
        }
        blurEffect.RenderImage(source, destination);
    }

    protected void Start()
    {
        renderTextureFactory = new RenderTextureFactory();
        PostEffectModelContainer postEffectModel = Resources.Load("PostEffectModelContainer") as PostEffectModelContainer;

        blurEffect = new BlurEffect();
        blurEffect.renderTextureFactory = renderTextureFactory;  
        blurEffectModel = postEffectModel.blurEffectModel;
        blurEffect.SetPostEffectModel<BlurEffectModel>(blurEffectModel);
        postEffectList.Add(blurEffect);

        blurEffect.OnEnable();
        CheckResources();
    }

    public void OnEnable()
    {
        rendererCamera = Camera.main;
        isSupported = true;
    }

    void OnDestroy()
    {
        for (int i = 0; i < postEffectList.Count; i++)
        {
            BasePostEffect postEffect = postEffectList[i];
            postEffect.OnDestroy();
        }
        if (renderTextureFactory != null)
        {
            renderTextureFactory.ReleaseAll();
        }
        postEffectList.Clear();
    }

    private bool CheckResources()
    {
        CheckSupport(false);

        blurEffect.CheckShaderAndCreateMaterial();

        if (!isSupported)
        {
            ReportAutoDisable();
        }

        return isSupported;
    }

    private void OnDisable()
    {
        for (int i = 0; i < postEffectList.Count; i++)
        {
            BasePostEffect postEffect = postEffectList[i];
            postEffect.OnDisable();
        }
    }

    private void NotSupported()
    {
        enabled = false;
        isSupported = false;
        return;
    }

    private void ReportAutoDisable()
    {
        Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
    }
}
