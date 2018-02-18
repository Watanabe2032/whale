#ifndef CUSTOM_STANDARD_INPUT_INCLUDED
#define CUSTOM_STANDARD_INPUT_INCLUDED

#include "UnityCG.cginc"
//#include "UnityShaderVariables.cginc"
//#include "UnityInstancing.cginc"
//#include "UnityStandardConfig.cginc"
#include "UnityPBSLighting.cginc" // TBD: remove
//#include "UnityStandardUtils.cginc"
#include "CommonConstant.cginc"

//---------------------------------------
// Directional lightmaps & Parallax require tangent space too
#if (_NORMALMAP || DIRLIGHTMAP_COMBINED || DIRLIGHTMAP_SEPARATE || _PARALLAXMAP)
	#define _TANGENT_TO_WORLD 1 
#endif

#if (_DETAIL_MULX2 || _DETAIL_MUL || _DETAIL_ADD || _DETAIL_LERP)
	#define _DETAIL 1
#endif


float	_EdgeWidth;
float	_SI;
half	_AlphaCutoff;
half	_ColorCutoff;
int		_ShadowMode;
int		_Cull;
int		_ZWrite;
int		_ZTest;

half4		_Color;
half		_Cutoff;
sampler2D	_MainTex;
float4		_MainTex_ST;


struct VertexInput
{	
	float4 vertex	: POSITION;
	half3 normal	: NORMAL;
	float2 uv0		: TEXCOORD0;
	//float2 uv1		: TEXCOORD1;
	//UNITY_VERTEX_INPUT_INSTANCE_ID
};

float4 TexCoords(VertexInput v)
{
	float4 texcoord;
	texcoord.xy = TRANSFORM_TEX(v.uv0, _MainTex); // Always source from uv0
	texcoord.z = 0.0;
	texcoord.w = 0.0;
	return texcoord;
}		

half3 Albedo(float4 texcoords)
{
	half3 albedo = _Color.rgb * tex2D (_MainTex, texcoords.xy).rgb;
	return albedo;
}

half Alpha(float2 uv)
{
	return tex2D(_MainTex, uv).a * _Color.a;
}		

half4 SpecularGloss(float2 uv)
{
	half4 sg;
	sg.rgb = _SpecColor.rgb;
	sg.a = 0.0;
	return sg;
}

half3 Emission(float2 uv)
{
	return 0;
}

#endif // CUSTOM_STANDARD_INPUT_INCLUDED
