using UnityEngine;

namespace Wompipomp.ImageEffects.Components
{
    [ExecuteInEditMode]
    [AddComponentMenu("Wompipomp/Image Effects/Components/KawaseBlur")]
    public class KawaseBlur : MonoBehaviour
    {
        private Material _material;

        public Shader ImageShader;
        [Range(0, 3)] public float BlurSpread = 1;
        [Range(0, 10)] public int Iterations = 3;
        [Range(1, 10)] public int Downsample = 4;

        public Material Material
        {
            get
            {
                if (_material == null)
                {
                    _material = new Material(ImageShader);
                    _material.hideFlags = HideFlags.HideAndDontSave;
                }
                return _material;
            }
        }

        private void Start()
        {
            if (!SystemInfo.supportsImageEffects || ImageShader == null || !Material.shader.isSupported)
            {
                enabled = false;
            }
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            RenderImage(source, destination);
        }

        private void RenderImage(RenderTexture source, RenderTexture destination)
        {
            int smallScreenWidth = Screen.width/Downsample;
            var smallScreenHeight = Screen.height/Downsample;
            var renderTex = RenderTexture.GetTemporary(smallScreenWidth, smallScreenHeight, 0);
            source.filterMode = FilterMode.Bilinear;
            Material.SetFloat("_BlurSpread", BlurSpread);
            Graphics.Blit(source, renderTex, Material);
            for (var i = 0; i < Iterations; i++)
            {
                Material.SetInt("_Iteration", i);
                var renderTex1 = RenderTexture.GetTemporary(smallScreenWidth, smallScreenHeight, 0);
                Graphics.Blit(renderTex, renderTex1, Material);
                RenderTexture.ReleaseTemporary(renderTex);
                renderTex = renderTex1;
            }
            Graphics.Blit(renderTex, destination);
            RenderTexture.ReleaseTemporary(renderTex);
        }

        public void ProcessOffline(RenderTexture source, RenderTexture destination)
        {
            RenderImage(source, destination);
        }

        public void OnDisable()
        {
            if (_material != null)
            {
                DestroyImmediate(_material);
            }
        }
    }
}