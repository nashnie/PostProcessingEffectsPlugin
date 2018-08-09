using UnityEngine;

namespace Wompipomp.ImageEffects.NightVision
{

    public enum ScreenAlignment
    {
        Horizontal = 0,
        Vertical
    }

    [ExecuteInEditMode]
    [AddComponentMenu("Wompipomp/Image Effects/NightVision")]
    public class NightVisionImageEffect : MonoBehaviour
    {
        private Material _material;
        private Material _blurMaterial;


        [Header("Shader")] public Shader ImageShader;
        public Shader BlurShader;

        [Header("Resize")]
        public ScreenAlignment MainAxis = ScreenAlignment.Vertical;

        [Header("Night Vision")] public float Exposure = 3f;
        public Color MainColor = Color.white;
        public Texture2D RampTex;

        [Header("Distortion")] public float CubicDistortionCoeff = 0.5f;
        public float DistortionCoeff = -0.15f;

        [Header("Noise")] [Range(0, 1)] public float NoiseIntensity = 0.7f;

        [Header("Vignette")] public float VignetteSoftness = 0.03f;
        public float VignetteRadius = 0.41f;


        [Header("Blur")] public bool IsBlurActive = true;
        public int BlurIterations = 4;
        public float BlurSpread = 0.7f;
        public float BlurSoftness = 0.2f;
        public float BlurRadius = 0.45f;

        [Header("Final Downsampling")] [Range(1, 6)] public int DownsampleRate = 1;



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

        public Material BlurMaterial
        {
            get
            {
                if (_blurMaterial == null)
                {
                    _blurMaterial = new Material(BlurShader);
                    _blurMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
                return _blurMaterial;
            }
        }

        private void Start()
        {
            if (!SystemInfo.supportsImageEffects || !ImageShader || !Material.shader.isSupported)
            {
                enabled = false;
            }
        }

        private void BlurImage(RenderTexture source, RenderTexture destination)
        {
            var smallScreenWidth = Screen.width/4;
            var smallScreenHeight = Screen.height/4;
            var renderTex = RenderTexture.GetTemporary(smallScreenWidth, smallScreenHeight, 0);
            source.filterMode = FilterMode.Bilinear;
            BlurMaterial.SetFloat("_BlurSpread", BlurSpread);
            Graphics.Blit(source, renderTex, BlurMaterial);
            for (var i = 0; i < BlurIterations; i++)
            {
                BlurMaterial.SetInt("_Iteration", i);
                var renderTex1 = RenderTexture.GetTemporary(smallScreenWidth, smallScreenHeight, 0);
                Graphics.Blit(renderTex, renderTex1, BlurMaterial);
                RenderTexture.ReleaseTemporary(renderTex);
                renderTex = renderTex1;
            }
            Graphics.Blit(renderTex, destination);
            RenderTexture.ReleaseTemporary(renderTex);
        }

        public void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Material.SetTexture("_RampTex", RampTex);
            Material.SetFloat("_Intensity", Exposure);
            Material.SetColor("_MainTint", MainColor);
            Material.SetFloat("_NoiseIntensity", NoiseIntensity);
            Material.SetFloat("_NoiseRandomOffset", Random.Range(0.1f, 0.4f));
            Material.SetFloat("_VignetteRadius", VignetteRadius);
            Material.SetFloat("_VignetteSoftness", VignetteSoftness);
            Material.SetFloat("_DistortionCoeff", DistortionCoeff);
            Material.SetFloat("_CubicDistortionCoeff", CubicDistortionCoeff);
            Material.SetFloat("_BlurRadius", BlurRadius);
            Material.SetFloat("_BlurSoftness", BlurSoftness);
            Material.SetFloat("_ScreenFactorX", MainAxis == ScreenAlignment.Vertical ? (float)Screen.width / Screen.height : 1);
            Material.SetFloat("_ScreenFactorY", MainAxis == ScreenAlignment.Horizontal ? (float)Screen.height / Screen.width : 1);

            //Color
            var rtPass0 = RenderTexture.GetTemporary(Screen.width, Screen.height, source.depth, source.format);
            Graphics.Blit(source, rtPass0, Material, 0);

            //Noise
            var rtPass1 = RenderTexture.GetTemporary(Screen.width, Screen.height, source.depth, source.format);
            Graphics.Blit(rtPass0, rtPass1, Material, 1);

            //Blur
            var rtPass2 = RenderTexture.GetTemporary(Screen.width, Screen.height, source.depth, source.format);
            if (IsBlurActive)
            {
                var rtBlur = RenderTexture.GetTemporary(Screen.width, Screen.height, source.depth, source.format);
                BlurImage(rtPass0, rtBlur);
                Material.SetTexture("_BlurredImage", rtBlur);

                Graphics.Blit(rtPass1, rtPass2, Material, 2);
                RenderTexture.ReleaseTemporary(rtBlur);
            }
            else
            {
                Graphics.Blit(rtPass1, rtPass2);
            }

            //Vignette
            var rtPass3 = RenderTexture.GetTemporary(Screen.width, Screen.height, source.depth, source.format);
            Graphics.Blit(rtPass2, rtPass3, Material, 3);

            //Final downsampling of image. Could be integrated in last blit but for better readability
            var rtDownsample = RenderTexture.GetTemporary(Screen.width/DownsampleRate, Screen.height/DownsampleRate,
                source.depth, source.format);
            Graphics.Blit(rtPass3, rtDownsample);

            Graphics.Blit(rtDownsample, destination);
            RenderTexture.ReleaseTemporary(rtPass0);
            RenderTexture.ReleaseTemporary(rtPass1);
            RenderTexture.ReleaseTemporary(rtPass2);
            RenderTexture.ReleaseTemporary(rtPass3);
            RenderTexture.ReleaseTemporary(rtDownsample);


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