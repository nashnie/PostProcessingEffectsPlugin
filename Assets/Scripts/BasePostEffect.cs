using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

/// <summary>
/// BasePostEffect
/// </summary>
public class BasePostEffect
{
    public RenderTextureFactory renderTextureFactory;
    public bool active = false;
    protected bool isSupported = true;
    protected BasePostEffectModel postEffectModel;
    protected List<Material> createdMaterials = new List<Material>();

    public virtual void RenderImage(RenderTexture source, RenderTexture destination)
    {
    }

    public virtual void PopulateCommandBuffer(CommandBuffer cb)
    {
    }

    public virtual string GetName()
    {
        return "";
    }


    protected virtual bool CheckResources()
    {
        return isSupported;
    }

    public virtual void OnEnable()
    {
        active = true;
        Debug.Log("OnEnable " + this.GetType().Name);
        //UnityEngine.Object postEffectModel = Resources.Load(this.GetType().Name);
        //SetPostEffectModel(postEffectModel as BasePostEffectModel);
    }

    public T GetPostEffectModel<T>() where T : BasePostEffectModel
    {
        return (T)postEffectModel;
    }

    public void SetPostEffectModel<T>(T value) where T : BasePostEffectModel
    {
        postEffectModel = value;
    }

    public virtual void OnDisable()
    {
        active = false;
        isSupported = false;
    }

    private void ReportAutoDisable()
    {
        Debug.LogWarning("The image effect " + ToString() + " has been disabled as it's not supported on the current platform.");
    }

    public virtual void OnDestroy()
    {
        RemoveCreatedMaterials();
        if (renderTextureFactory != null)
        {
            renderTextureFactory.ReleaseAll();
        }
    }

    private void RemoveCreatedMaterials()
    {
        while (createdMaterials.Count > 0)
        {
            Material mat = createdMaterials[0];
            createdMaterials.RemoveAt(0);
#if UNITY_EDITOR
           GameObject.DestroyImmediate(mat);
#else
           GameObject.Destroy(mat);
#endif
        }
    }

    public virtual void CheckShaderAndCreateMaterial()
    {
    }

    public virtual Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)
    {
        if (!s)
        {
            Debug.Log("Error,Missing shader in " + ToString());
            isSupported = false;
            return null;
        }

        if (s.isSupported && m2Create && m2Create.shader == s)
        {
            return m2Create;
        }

        if (!s.isSupported)
        {
            isSupported = false;
            Debug.Log("The shader " + s.ToString() + " on effect " + ToString() + " is not supported on this platform!");
            return null;
        }

        m2Create = new Material(s);
        createdMaterials.Add(m2Create);
        m2Create.hideFlags = HideFlags.DontSave;

        return m2Create;
    }
}
