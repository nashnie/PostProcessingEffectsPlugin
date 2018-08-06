Shader "BlurShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Bloom ("Bloom (RGB)", 2D) = "black" {}
	}
	SubShader
	{
		ZTest Off
		Cull Off
		ZWrite Off
		Blend Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert4Tap
			#pragma fragment fragDownsample
			#include "Blur.cginc"
			ENDCG
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vertBlurVertical
			#pragma fragment fragBlur8
			#include "Blur.cginc"
			ENDCG
		}

		Pass
		{
			ZTest Always
			Cull Off

			CGPROGRAM

			#pragma vertex vertBlurHorizontal
			#pragma fragment fragBlur8
			#include "Blur.cginc"
			ENDCG
		}

		// alternate blur
		// 3
		Pass
		{
			ZTest Always
			Cull Off

			CGPROGRAM

			#pragma vertex vertBlurVerticalSGX
			#pragma fragment fragBlurSGX
			#include "Blur.cginc"
			ENDCG
		}

		// 4
		Pass
		{
			ZTest Always
			Cull Off

			CGPROGRAM

			#pragma vertex vertBlurHorizontalSGX
			#pragma fragment fragBlurSGX
			#include "Blur.cginc"
			ENDCG
		}
	}	
		
	FallBack Off
}
