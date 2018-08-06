using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Nash
/// </summary>
public class RenderTextureFactory
{
    private HashSet<RenderTexture> tempRTMap;

    public RenderTextureFactory()
    {
        tempRTMap = new HashSet<RenderTexture>();
    }

    public RenderTexture Get(RenderTexture baseRT)
    {
        return Get(baseRT.width, baseRT.height, baseRT.depth, baseRT.format, baseRT.sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
    }

    public RenderTexture Get(RenderTexture baseRT, int downSample)
    {
        return Get(baseRT.width >> downSample, baseRT.height >> downSample, baseRT.depth, baseRT.format, baseRT.sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear);
    }

    public RenderTexture Get(RenderTexture baseRT, int downSample, FilterMode filterMode = FilterMode.Bilinear)
    {
        return Get(baseRT.width >> downSample, baseRT.height >> downSample, baseRT.depth, baseRT.format, baseRT.sRGB ? RenderTextureReadWrite.sRGB : RenderTextureReadWrite.Linear, filterMode);
    }

    public RenderTexture Get(int width, int height, int depth, RenderTextureFormat format, RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, FilterMode filterMode = FilterMode.Bilinear, string name = "RenderTextureFactoryRT")
    {
        RenderTexture temporary = RenderTexture.GetTemporary(width, height, depth, format, readWrite);
        temporary.filterMode = filterMode;
        temporary.name = name;
        tempRTMap.Add(temporary);
        return temporary;
    }

    public void Release(RenderTexture tempRT)
    {
        tempRTMap.Remove(tempRT);
        RenderTexture.ReleaseTemporary(tempRT);
    }

    public void ReleaseAll()
    {
        var enumerator = tempRTMap.GetEnumerator();
        while (enumerator.MoveNext())
        {
            RenderTexture.ReleaseTemporary(enumerator.Current);
        }
        tempRTMap.Clear();
    }

    public void Dispose()
    {
        ReleaseAll();
    }
}
