Shader "Custom/ShadowModel" {
Properties {
	_ModelColor("Model Color", Color) = (1,1,1,1)
	_AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
	_ColorCutoff("Color Cutoff", Range(0.0, 1.0)) = 0.5
	_SI ("Sampling Interval", Range(0,1)) = 0.01
	
	_MainTex("Albedo", 2D) = "white" {}
	_BumpMap("Normal Map", 2D) = "bump" {}

	[HideInInspector] _CameraRange ("Camera Range", Float) = 20.0
	[HideInInspector] _ShadowMode ("Shadow Mode", Int) = 0
	[KeywordEnum(Less,Greater,LEqual,GEqual,Equal,NotEqual,Always)]
										_ZTest("Z Test", Int) = 2
	[KeywordEnum(Back, Front, Off)]	_Cull("Culling", Int) = 2
	[KeywordEnum(On, Off)]				_ZWrite("Z Write", Int) = 0
}

SubShader {
	
Pass {
	Tags { "LightMode" = "ShadowCaster" }
	ZWrite [_ZWrite]
	ZTest [_ZTest]
	Cull [_Cull]

	CGPROGRAM
	#pragma target 3.0
	#pragma multi_compile_shadowcaster
	#pragma vertex vertShadowCaster
	#pragma fragment fragShadowCaster
	
	#include "UnityCG.cginc"
	#include "UnityShaderVariables.cginc"
	#include "UnityInstancing.cginc"
	#include "UnityStandardConfig.cginc"
	#include "UnityStandardUtils.cginc"
	#include "CustomStandardInput.cginc"
	#include "TextureFilter.cginc"
	
	
	float GetContourFromLight(float3 pos, float3 normal){
		float4 VertToLight = float4(normalize(_WorldSpaceLightPos0 - pos),0.0);
		float4 wNormal = mul(unity_ObjectToWorld, float4(normal,0.0));
		return (12.0 - abs(dot(VertToLight, wNormal))) / 12.0;
	}
	
	struct VertexInputShadowCaster {
		float4	vertex		: POSITION;
		float3	normal		: NORMAL;
		float2	uv0			: TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct VertexOutputShadowCaster {
		V2F_SHADOW_CASTER_NOPOS
		float2	uv			: TEXCOORD0;
		float4	sPos		: TEXCOORD1;
		float3	normal		: TEXCOORD2;
		float4	pos			: SV_POSITION;
	};

	void vertShadowCaster (VertexInputShadowCaster v,
							out VertexOutputShadowCaster o) {
		o.uv = TRANSFORM_TEX(v.uv0, _MainTex);
		TRANSFER_SHADOW_CASTER_NOPOS(o,o.pos)
		o.sPos = o.pos;
		o.normal = v.normal;
	}

	half4 fragShadowCaster (VertexOutputShadowCaster i) : SV_Target {
		
		if(_ShadowMode == 0) discard;
		else if(_ShadowMode == 1){
			clip (GetColorDiff(i.uv, _SI) - 0.2 - _ColorCutoff);
		}
		else if(_ShadowMode == 2){
			clip (GetColorDiffMono(i.uv.x, _SI) - 0.2 - _ColorCutoff);
		}
		else if(_ShadowMode == 3){
			clip (GetColorDiffMono(i.uv.y, _SI) - 0.2 - _ColorCutoff);
		}
		else if(_ShadowMode == 4){
			float alpha = CalcAlpha(float4(1.0, i.uv, 0.0));
			clip (alpha - _ColorCutoff);
		}
		else if(_ShadowMode == 5){
			float alpha = CalcAlpha(float4(i.sPos));
			clip (alpha - _ColorCutoff);
		}
		else if(_ShadowMode == 6){
			float contour = GetContourFromLight(i.sPos.xyz, i.normal);
			clip(contour - _ColorCutoff);
		}
		else if(_ShadowMode == 7){
			half sampling = _CosTime.x * 0.49 + 0.5;
			if(_SinTime.x < 0.0) sampling = -1.0 * sampling;
			clip (GetAlphaAverage(i.uv, sampling) - tex2D(_MainTex, i.uv).a);
		}
		
		return 0;
	}
	
	ENDCG
}
	
}

}

