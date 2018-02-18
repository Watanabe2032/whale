Shader "Custom/RenderTecture" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
}


CGINCLUDE

#include "CustomStandardInput.cginc"

ENDCG

SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 100
	
	
	Pass {
		Lighting Off
		CGPROGRAM
		
		#pragma vertex vert
		#pragma fragment frag
		
		struct vertexInput
	{
		float2		uv		: TEXCOORD0;
		half4		vertex	: POSITION;
	};

	struct vertexOutput
	{
		float2	uv		: TEXCOORD0;
		float4	sPos	: TEXCOORD1;
		half4	pos		: SV_POSITION;
	};
	
	vertexOutput vert (vertexInput v) 
	{
		vertexOutput o;
		//UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.sPos = o.pos;
		return o;
	}

	half4 frag (vertexOutput i) : COLOR
	{
		//half4 color = tex2D(_MainTex, i.uv);
		half4 color = tex2D(_MainTex, i.sPos.xy);
		return color;
	}
	
	ENDCG
	
	}
}



}
