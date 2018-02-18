Shader "Custom/ShadowModelDeferred" {

  // http://tips.hecomi.com/entry/2016/09/26/014539

Properties {
	_AlphaCutoff("Alpha Cutoff", Float) = 0.5
	_ColorCutoff("Color Cutoff", Range(0.0, 1.0)) = 0.5
	_SphereSize ("Sphere Size", Range(0,1)) = 0.1
	_Depth ("Depth", Float) = 1.0
	[HideInInspector] _Cull("Culling", Int) = 2 // (Back, Front, Off)
}

SubShader
{

Tags { "RenderType" = "Opaque" "DisableBatching" = "True" }

CGINCLUDE

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "Raymarching.cginc"


ENDCG

Pass
{
	Tags { "LightMode" = "ShadowCaster" }

	CGPROGRAM
	#pragma target 3.0
	#pragma vertex vert_shadow
	#pragma fragment frag_shadow
	#pragma multi_compile_shadowcaster
	#pragma fragmentoption ARB_precision_hint_fastest
	
	
struct VertShadowInput
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 uv1    : TEXCOORD1;
};

struct VertShadowOutput
{
    V2F_SHADOW_CASTER;
    float4 screenPos : TEXCOORD1;
    float4 worldPos  : TEXCOORD2;
	float3 normal    : TEXCOORD3;
};
	
	
	VertShadowOutput vert_shadow(VertShadowInput v)
{
    VertShadowOutput o;
	TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
    o.screenPos = o.pos;
	o.worldPos = mul(unity_ObjectToWorld, v.vertex);
	o.normal = v.normal;
	
    return o;
}


void frag_shadow(
	VertShadowOutput i, 
	out float4 outColor : SV_Target, 
	out float  outDepth : SV_Depth)
{
	
	float3 rayDir = -UNITY_MATRIX_V[2].xyz;
	float3 pos = i.worldPos;
	float distance = 0.0;
	Raymarch(pos, distance, rayDir, 0.001, 20);

	float4 opos = mul(unity_WorldToObject, float4(pos, 1.0));
	opos = UnityClipSpaceShadowCasterPos(opos, i.normal);
	opos = UnityApplyLinearShadowBias(opos);

	outColor = outDepth = opos.z / opos.w;
}
	
	ENDCG
}

}

Fallback Off

}