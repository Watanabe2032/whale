Shader "Custom/ShadowModelBR" {

 // https://github.com/i-saint/BatchRenderer

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
	Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }
	
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
	#include "Assets/Ist/BatchRenderer/Shaders/BatchRenderer.cginc"
	
	void ApplyInstanceTransform(int instance_id, inout float4 vertex, inout float3 normal, inout float4 tangent, inout float2 texcoord)
	{
		if(instance_id >= GetNumInstances()) {
			vertex.xyz *= 0.0;
			return;
		}
		vertex.xyz *= GetBaseScale();
		vertex.xyz *= GetInstanceScale(instance_id);
		float3x3 rot = quaternion_to_matrix33(GetInstanceRotation(instance_id));
		vertex.xyz = mul(rot, vertex.xyz);
		normal.xyz = mul(rot, normal.xyz);
		tangent.xyz = mul(rot, tangent.xyz);
		vertex.xyz += GetInstanceTranslation(instance_id);
	}
	
	
	struct VertexInputShadowCaster {
		float4	vertex		: POSITION;
		float3	normal		: NORMAL;
		float4	tangent		: TANGENT;
		float4	uv0			: TEXCOORD0;
		float4	uv1			: TEXCOORD1;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	
	struct VertexOutputShadowCaster {
		V2F_SHADOW_CASTER_NOPOS
		float4	pos			: SV_POSITION;
	};

	void vertShadowCaster (VertexInputShadowCaster v,
							out VertexOutputShadowCaster o) {
		int iid = GetBatchBegin() + v.uv1.x;
		ApplyInstanceTransform(iid, v.vertex, v.normal, v.tangent, v.uv0.xy);
		TRANSFER_SHADOW_CASTER_NOPOS(o,o.pos)
	}
	
	half4 fragShadowCaster (VertexOutputShadowCaster i) : SV_Target {
		
		return 0;
	}
	
	ENDCG
}
	
}

}
