using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcessingEffect : MonoBehaviour
{
    private RenderTextureFactory renderTextureFactory;

    private void OnEnable()
    {
        renderTextureFactory = new RenderTextureFactory();
    }

    private void OnDisable()
    {
        if (renderTextureFactory != null)
        {
            renderTextureFactory.Dispose();
            renderTextureFactory = null;
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

    }
}
