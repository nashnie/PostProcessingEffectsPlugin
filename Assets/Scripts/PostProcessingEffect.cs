using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

/// <summary>
/// CommandBuffer OnPostEffect、OnPreEffect、OnPreCull、OnRenderImage
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

    public OldFilmEffect oldFilmEffect;
    public OldFilmEffectModel oldFilmEffectModel;

    public NightVisionEffect nightVisionEffect;
    public NightVisionEffectModel nightVisionEffectModel;

    public AmbientOcclusionEffect ambientOcclusionEffect;
    public AmbientOcclusionModel ambientOcclusionModel;

    public List<BasePostEffect> postEffectList = new List<BasePostEffect>();

    Dictionary<Type, KeyValuePair<CameraEvent, CommandBuffer>> m_CommandBuffers;
    Camera m_Camera;

    public void OnPostRender()
    {
    }

    public void OnPreRender()
    {
        TryExecuteCommandBuffer<BasePostEffect>(ambientOcclusionEffect, CameraEvent.BeforeImageEffectsOpaque);
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
        //oldFilmEffect.RenderImage(source, destination);
        //nightVisionEffect.RenderImage(source, destination);
    }

    public void OnEnable()
    {
        rendererCamera = Camera.main;
        isSupported = true;
        m_CommandBuffers = new Dictionary<Type, KeyValuePair<CameraEvent, CommandBuffer>>();
        renderTextureFactory = new RenderTextureFactory();
        m_Camera = gameObject.GetComponent<Camera>();
        PostEffectModelContainer postEffectModel = Resources.Load("PostEffectModelContainer") as PostEffectModelContainer;

        blurEffect = new BlurEffect();
        blurEffect.renderTextureFactory = renderTextureFactory;
        blurEffectModel = postEffectModel.blurEffectModel;
        blurEffect.SetPostEffectModel<BlurEffectModel>(blurEffectModel);

        oldFilmEffect = new OldFilmEffect();
        oldFilmEffect.renderTextureFactory = renderTextureFactory;
        oldFilmEffectModel = postEffectModel.oldFilmEffectModel;
        oldFilmEffect.SetPostEffectModel<OldFilmEffectModel>(oldFilmEffectModel);

        nightVisionEffect = new NightVisionEffect();
        nightVisionEffect.renderTextureFactory = renderTextureFactory;
        nightVisionEffectModel = postEffectModel.nightVisionEffectModel;
        nightVisionEffect.SetPostEffectModel<NightVisionEffectModel>(nightVisionEffectModel);

        ambientOcclusionEffect = new AmbientOcclusionEffect();
        ambientOcclusionEffect.renderTextureFactory = renderTextureFactory;
        ambientOcclusionModel = postEffectModel.ambientOcclusionModel;
        ambientOcclusionEffect.SetPostEffectModel(ambientOcclusionModel);

        postEffectList.Add(blurEffect);
        postEffectList.Add(oldFilmEffect);
        postEffectList.Add(nightVisionEffect);
        postEffectList.Add(ambientOcclusionEffect);

        for (int i = 0; i < postEffectList.Count; i++)
        {
            BasePostEffect postEffect = postEffectList[i];
            postEffect.OnEnable();
        }
        CheckResources();
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

        for (int i = 0; i < postEffectList.Count; i++)
        {
            BasePostEffect postEffect = postEffectList[i];
            postEffect.CheckShaderAndCreateMaterial();
        }

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
        foreach (var cb in m_CommandBuffers.Values)
        {
            m_Camera.RemoveCommandBuffer(cb.Key, cb.Value);
            cb.Value.Dispose();
        }
        m_CommandBuffers.Clear();
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

    void TryExecuteCommandBuffer<T>(BasePostEffect component, CameraEvent evt)
    {
        if (component.active)
        {
            var cb = GetCommandBuffer<T>(evt, component.GetName());
            cb.Clear();
            component.PopulateCommandBuffer(cb);
        }
        else
        {
            RemoveCommandBuffer<T>();
        }
    }

    CommandBuffer GetCommandBuffer<T>(CameraEvent evt, string name)
    {
        CommandBuffer cb;
        KeyValuePair<CameraEvent, CommandBuffer> kvp;

        if (!m_CommandBuffers.TryGetValue(typeof(T), out kvp))
        {
            cb = AddCommandBuffer<T>(evt, name);
        }
        else if (kvp.Key != evt)
        {
            RemoveCommandBuffer<T>();
            cb = AddCommandBuffer<T>(evt, name);
        }
        else cb = kvp.Value;

        return cb;
    }

    CommandBuffer AddCommandBuffer<T>(CameraEvent evt, string name)
    {
        var cb = new CommandBuffer { name = name };
        var kvp = new KeyValuePair<CameraEvent, CommandBuffer>(evt, cb);
        m_CommandBuffers.Add(typeof(T), kvp);
        m_Camera.AddCommandBuffer(evt, kvp.Value);
        return kvp.Value;
    }

    void RemoveCommandBuffer<T>()
    {
        KeyValuePair<CameraEvent, CommandBuffer> kvp;
        var type = typeof(T);

        if (!m_CommandBuffers.TryGetValue(type, out kvp))
            return;

        m_Camera.RemoveCommandBuffer(kvp.Key, kvp.Value);
        m_CommandBuffers.Remove(type);
        kvp.Value.Dispose();
    }
}
