// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/NightVision"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	CGINCLUDE
#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
		};

		float randTime(half2 co) {
			half offset = frac(_Time.y);
			co = co + offset;
			return frac(sin(dot(co, half2(12.9898, 78.233))) * 43758.5453); 
		}

	

		
		sampler2D _MainTex;
		fixed _Intensity;
		float4 _MainTint;
		sampler2D _RampTex;
		sampler2D _BlurredImage;
		int _BlendBlur;
		fixed _NoiseIntensity;
		fixed _NoiseRandomOffset;
		fixed _VignetteRadius;
		fixed _VignetteSoftness;
		fixed _BlurRadius;
		fixed _BlurSoftness;
		fixed _DistortionCoeff;
		fixed _CubicDistortionCoeff;
		half _ScreenFactorY;
		half _ScreenFactorX;

		v2f vert(appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			return o;
		}

		fixed4 vignette(v2f i) : SV_Target
		{
			fixed4 finalColor = tex2D(_MainTex, i.uv);
			half2 position = i.uv - float2(0.5, 0.5);
			position = float2(position.x * _ScreenFactorX, position.y * _ScreenFactorY);
			fixed len = length(position);
			half blending =  smoothstep(_VignetteRadius, _VignetteRadius - _VignetteSoftness, len);
			return finalColor * blending;
		}

		fixed4 blur(v2f i ) : SV_Target
		{
			fixed4 finalColor = tex2D(_MainTex, i.uv);
			fixed4 blurColor = tex2D(_BlurredImage, i.uv);
			
			half2 position = i.uv - float2(0.5, 0.5);
			position = float2(position.x * _ScreenFactorX, position.y * _ScreenFactorY);
			fixed len = length(position);
			half blending = smoothstep(_BlurRadius, _BlurRadius - _BlurSoftness, len);
			return lerp(finalColor, blurColor, 1-blending);
		}

		fixed4 LensDistortion(half2 uv)
		{
			/*
			Cubic Lens Distortion HLSL Shader

			Original Lens Distortion Algorithm from SSontech (Syntheyes)
			http://www.ssontech.com/content/lensalg.htm

			r2 = image_aspect*image_aspect*u*u + v*v
			f = 1 + r2*(k + kcube*sqrt(r2))
			u' = f*u
			v' = f*v

			*/

			float r2 = (uv.x - 0.5) * (uv.x - 0.5) + (uv.y - 0.5) * (uv.y - 0.5);
			float f = 0;

			f = 1 + r2 * (_DistortionCoeff + _CubicDistortionCoeff * sqrt(r2));

			float x = f*(uv.x - 0.5) + 0.5;
			float y = f*(uv.y - 0.5) + 0.5;
			return tex2D(_MainTex, float2(x, y));
		}

		

		fixed4 colorMapping(v2f i) : SV_Target
		{
			fixed4 finalColor;
			fixed4 col = LensDistortion(i.uv);
			half lum = dot(col, fixed3(0.22, 0.707, 0.071));
			lum = pow(lum, 1 / _Intensity);
			return tex2D(_RampTex, float2(lum, 0.5)) * _MainTint;
		}

		fixed4 noise(v2f i) : SV_Target
		{
			fixed4 finalColor = tex2D(_MainTex, i.uv);
			fixed noise = randTime(i.uv) + _NoiseIntensity;
			noise = saturate(noise);
			finalColor = finalColor * noise;
			return finalColor;
		}


	
	ENDCG

	SubShader
	{
		// No culling or depth
		ZTest Off Cull Off ZWrite Off Blend Off

		// 0
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment colorMapping
			
			
			ENDCG
		}

		// 1
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment noise
			
			
			ENDCG
		}

		//2
		Pass
		{
			CGPROGRAM
#pragma vertex vert
#pragma fragment blur


			ENDCG
		}

		// 3
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment vignette
			
			
			ENDCG
		}


	}
}
