Shader "Custom/ShadowScreen" {
Properties{
	_ScreenColor ("Screen Color", Color) = (1.0,1.0,1.0,1.0)
	_ShadowColor ("Shadow Color", Color) = (1.0,1.0,1.0,1.0)
	_GridColor ("Grid Color", Color) = (0.0,0.0,0.0,0.0)
	_MainTex ("Albedo (RGB)", 2D) = "white" {}
	_NoiseTex ("NoiseTex (RGBA)", 2D) = "white" {}
	_GridSize ("Grid Size", Range(0.0, 10000.0)) = 0.05
	[HideInInspector] _GridAllScreen ("Grid All Screen", Int) = 0
	[HideInInspector] _GridMode ("Grid Mode", Int) = 0
	[HideInInspector] _TileMode ("Tile Mode", Int) = 0
}

CGINCLUDE

#include "UnityCG.cginc"
#include "AutoLight.cginc"
#include "Lighting.cginc"
#include "CustomStandardInput.cginc"
#include "ScreenGrid.cginc"
#include "TruchetTiles.cginc"
#include "GearGrid.cginc"

ENDCG

SubShader{

LOD 200
Tags { "RenderType"="Opaque" }

Pass { 
	Tags {"LightMode" = "ForwardBase"}
	Lighting On
	
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma multi_compile_fwdbase

	half4 _ScreenColor;
	half4 _ShadowColor;
	half4 _GridColor;
	half _GridSize;
	int _GridMode;
	int _TileMode;
	int _GridAllScreen;
	

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
		LIGHTING_COORDS(4,6)
	};
	
	half GetGridByGridMode(vertexOutput i){
		half grid = 0.0;
		if(_GridMode == 0)      grid = 0.0;
		else if(_GridMode == 1) grid = GetDotGrid(i.uv);
		else if(_GridMode == 2) grid = GetChangeCheckGrid(i.sPos.xy, _GridSize);
		else if(_GridMode == 3) grid = GetTile(i.uv, 50.0, _TileMode);
		else if(_GridMode == 4) grid = GetGridQC(i.uv, 50.0);
		else if(_GridMode == 5) grid = GetBinaryNoiseGrid(i.uv*100.0);
		else if(_GridMode == 6) grid = GetNoiseTextureGrid(i.sPos.xy*100.0, i.uv);
		else if(_GridMode == 7) grid = GetSimpleGearScreen(i.uv);
		//else if(_GridMode == 7) grid = GetGearScreen(i.uv);
		return grid;
	}

	vertexOutput vert (vertexInput v) 
	{
		vertexOutput o;
		o.uv = TRANSFORM_TEX(v.uv, _MainTex);
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.sPos = o.pos;
		TRANSFER_VERTEX_TO_FRAGMENT(o);
		return o;
	}

	half4 frag (vertexOutput i) : COLOR
	{
		half4 color;
		if(LIGHT_ATTENUATION(i) > 0.4){
			color = tex2D(_MainTex, i.uv) + _ScreenColor;
			if(_GridAllScreen == 1){
				half grid = GetGridByGridMode(i);
				color = (1.0-_GridColor)*grid + color*(1.0-grid);
			}
			return color;
		}
		half grid = GetGridByGridMode(i);
		color = _ShadowColor*(1.0-grid) + (tex2D(_MainTex,i.uv)+_GridColor)*grid;
		return color;
	}

	ENDCG
}

}
FallBack "Diffuse"
//FallBack "VertexLit"
}