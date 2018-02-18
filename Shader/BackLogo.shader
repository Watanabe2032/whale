Shader "Custom/BackLogo" {

// http://wordpress.notargs.com

Properties
{
	_MainTex ("Sprite Texture", 2D) = "white" {}
	_Color ("Color", Color) = (1,1,1,1)
	_AlphaCutoff ("Color Mask", Float) = 0.001
}

SubShader
{
Tags
{ 
	"Queue"="Transparent" 
	"IgnoreProjector"="True" 
	"RenderType"="Transparent" 
	"PreviewType"="Plane"
	"CanUseSpriteAtlas"="True"
}

Cull Off
Lighting Off
ZWrite Off
ZTest On
Blend SrcAlpha OneMinusSrcAlpha

Pass
{
	Name "Default"
CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#pragma target 2.0

	#include "UnityCG.cginc"
	#include "UnityUI.cginc"
	
	float2 mod(float2 a, float2 b)
	{
		return a - floor(a / b) * b;
	}
	
	float rand(float2 co) {
		return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
	}
	
	struct appdata_t
	{
		float4 vertex   : POSITION;
		float4 color    : COLOR;
		float2 texcoord : TEXCOORD0;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f
	{
		float4 vertex   : SV_POSITION;
		fixed4 color    : COLOR;
		float2 texcoord  : TEXCOORD0;
		float4 worldPosition : TEXCOORD1;
		UNITY_VERTEX_OUTPUT_STEREO
	};
	
	half _AlphaCutoff;
	fixed4 _Color;
	fixed4 _TextureSampleAdd;
	float4 _ClipRect;

	v2f vert(appdata_t IN)
	{
		v2f OUT;
		UNITY_SETUP_INSTANCE_ID(IN);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
		OUT.worldPosition = IN.vertex;
		OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

		OUT.texcoord = IN.texcoord;
		
		OUT.color = IN.color * _Color;
		return OUT;
	}

	sampler2D _MainTex;

	fixed4 frag(v2f IN) : SV_Target
	{
		float2 inUV = IN.texcoord;
		float2 uv = IN.texcoord - 0.5;
		float vignet = length(uv);
		uv /= 1 - vignet * 0.2;
		float2 texUV = uv + 0.5;
		float noizeScale = 0.1 * ((_SinTime.x-abs(_SinTime.x))*0.2-abs(_CosTime.z)*0.1+_SinTime.w*0.1);
		float noizeWidth = 200.0 + 50.0 * floor(_SinTime.x + _SinTime.w + _CosTime.z);
		texUV.x += sin(texUV.y*abs(_CosTime.y))*100.0*noizeScale*noizeScale;
		texUV.x += (rand(floor(texUV.y * noizeWidth)+1.0) - 0.5) * noizeScale;
		texUV = mod(texUV, 1);
		
		half4 color;
		color = (tex2D(_MainTex, texUV) + _TextureSampleAdd) * IN.color;
		color.r = tex2D(_MainTex, texUV).r;
		color.g = tex2D(_MainTex, texUV - float2(0.0001, 0)).g;
		color.b = tex2D(_MainTex, texUV - float2(0.0002, 0)).b;
		
		color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
		clip (color.a - _AlphaCutoff);
		color.a *= 1.0 + 0.5 * (_SinTime.x*0.5+_SinTime.y*0.5+abs(_CosTime.y)*0.3+_CosTime.w*0.5);
		
		return color;
	}
	
ENDCG
}
	
}
}
