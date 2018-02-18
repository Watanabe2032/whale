#ifndef TEXTURE_FILTER_INCLUDED
#define TEXTURE_FILTER_INCLUDED

#include "CommonConstant.cginc"

half GetAlphaAverage(half2 uv, half _si){
	half sam = tex2D(_MainTex,uv-half2(_si,_si)).a+tex2D(_MainTex,uv-half2(0,_si)).a+tex2D(_MainTex,uv-half2(-_si,_si)).a
				+tex2D(_MainTex,uv-half2(_si,0)).a+tex2D(_MainTex,uv-half2(-_si,0)).a
				+tex2D(_MainTex,uv-half2(_si,-_si)).a+tex2D(_MainTex,uv-half2(0,-_si)).a+tex2D(_MainTex,uv-half2(-_si,-_si)).a;
	return sam / 8.0;
}

half GetColorDiff(half2 uv, half _si){
	half4 sam = tex2D(_MainTex,uv-half2(_si,_si))+tex2D(_MainTex,uv-half2(0,_si))+tex2D(_MainTex,uv-half2(-_si,_si))
				+tex2D(_MainTex,uv-half2(_si,0))+tex2D(_MainTex,uv-half2(-_si,0))
				+tex2D(_MainTex,uv-half2(_si,-_si))+tex2D(_MainTex,uv-half2(0,-_si))+tex2D(_MainTex,uv-half2(-_si,-_si));
	return abs(dot(tex2D(_MainTex,uv).yzw, (sam / 8.0).yzw));
}

half GetColorDiffMono(half uv, half _si){
	half4 sam = tex2D(_MainTex,uv-half2(_si,_si))+tex2D(_MainTex,uv-half2(0,_si))+tex2D(_MainTex,uv-half2(-_si,_si))
				+tex2D(_MainTex,uv-half2(_si,0))+tex2D(_MainTex,uv-half2(-_si,0))
				+tex2D(_MainTex,uv-half2(_si,-_si))+tex2D(_MainTex,uv-half2(0,-_si))+tex2D(_MainTex,uv-half2(-_si,-_si));
	return abs(dot(tex2D(_MainTex,uv).yzw, (sam / 8.0).yzw));
}

float CalcAlpha(float3 pos){
	float rot = atan2(pos.y, pos.x);
	rot = pos.z * 10.0 + _Time.y * 0.1;
	rot = mod(rot * 20.0, PI * 2);
	return (rot / PI) / 2;
}

#endif // TEXTURE_FILTER_INCLUDED
