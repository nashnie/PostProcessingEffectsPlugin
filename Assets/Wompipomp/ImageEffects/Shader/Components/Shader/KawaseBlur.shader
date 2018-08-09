// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/KawaseBlur" {
	Properties{ _MainTex("", any) = "" {} }
	
		CGINCLUDE
#include "UnityCG.cginc"
	struct v2f {
		float4 pos : POSITION;
		half2 uv : TEXCOORD0;
		half2 taps[4] : TEXCOORD1;
	};
	sampler2D _MainTex;
	int _Iteration;
	float _BlurSpread;
	half4 _MainTex_TexelSize;
	v2f vert(appdata_img v) {
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord;
		fixed offset = _Iteration * _BlurSpread * _MainTex_TexelSize;
		o.taps[0] = o.uv + offset * half2(1, 1);
		o.taps[1] = o.uv + offset * half2(-1, -1);
		o.taps[2] = o.uv + offset * half2(-1, 1);
		o.taps[3] = o.uv + offset * half2(1, -1);
		return o;
	}
	half4 frag(v2f i) : COLOR{
		fixed4 col = tex2D(_MainTex, i.taps[0]);
		col += tex2D(_MainTex, i.taps[1]);
		col += tex2D(_MainTex, i.taps[2]);
		col += tex2D(_MainTex, i.taps[3]);
		return col / 4;
	}
	ENDCG
	
		SubShader {
		Pass{
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }
			
			CGPROGRAM
		#pragma fragmentoption ARB_precision_hint_fastest
		#pragma vertex vert
		#pragma fragment frag
		ENDCG
		}

		
	}
	Fallback off
}