Shader "Custom/ScanningTransparent" {
Properties {
	_Cycle ("Cycle", Float) = 10.0
	_MaxRange ("MaxRange", Float) = 250.0
	_Width ("Width", Float) = 1.0
	_LineColor ("Line Color", Color) = (0.0,1.0,0.0,1.0)
}
SubShader
{

Pass { 
	Tags {"Queue"="Transparent" "RenderType"="Transparent"}
	
	ZWrite Off
	Cull Off

	CGPROGRAM
	#pragma target 3.0
	#pragma vertex vert
	#pragma fragment frag
	
	#include "UnityCG.cginc"
	
	half4 _LineColor;
	float _Cycle;
	float _MaxRange;
	float _Width;
	
	struct VertInput
	{	
		float4 vertex	: POSITION;
		half3 normal	: NORMAL;
	};

struct VertOutput
	{
		float4 pos      : SV_POSITION;
		float4 wPos      : TEXCOORD1;
	};

	VertOutput vert (VertInput v)
	{
		VertOutput o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.wPos = mul(unity_ObjectToWorld, v.vertex);
		return o;
	}
	
	half4 frag (VertOutput i) : COLOR
	{
		half4 col = _LineColor;
		
		float timeRate = fmod(_Time.y, _Cycle) / _Cycle;
		float3 c2v = (normalize(i.wPos.xyz - _WorldSpaceCameraPos));
		float3 rayPos = _WorldSpaceCameraPos + c2v * _MaxRange * timeRate;
		float diff = length(i.wPos.xyz - rayPos);
		clip(_Width - diff);
		col.w = diff / _Width;
		return col;
	}
	
	ENDCG
	}
}

Fallback "Transparent/VertexLit"

}
