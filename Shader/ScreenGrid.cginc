#ifndef SCREEN_GRID_INCLUDED
#define SCREEN_GRID_INCLUDED

#include "RandomNoise.cginc"

half GetDotGrid(float2 pos){
	float2 rate;
	rate.x = pos.x * 800.0;
	rate.y = pos.y * 800.0;
	if( sin(rate.x) + sin(rate.y) - 0.5 < 0){
		return 1.0;
	}
	return 0.0;
}	
	
half GetCheckGrid(float2 pos, float gridSize){
	float2 rate;
	rate.x = pos.x * gridSize;
	rate.y = pos.y * gridSize;
	if( sin(rate.x) + sin(rate.y) < 0) return 1.0;
	return 0.0;
}

half GetChangeCheckGrid(float2 pos, float gridSize){
	float2 rate = pos * gridSize;
	rate.y *= _ScreenParams.y / _ScreenParams.x;
	if( sin(rate.x) + sin(rate.y) < 0) return 1.0;
	return 0.0;
}

half GetChangeStripeGrid(float2 pos){
	float2 rate = pos.xy / _ScreenParams.xy * 2000.0;
	rate.y *= _ScreenParams.y / _ScreenParams.x;
	float timeRate = (_SinTime.x+1.0)*0.5;
	if( (sin(rate.x)*timeRate + sin(rate.y)*(1.0-timeRate)) < 0) return 1.0;
	return 0.0;
}

half GetNoiseGrid(float4 pos){
	float3 argument = float3(pos.x,pos.y,pos.z);
	half gridRate = noise(argument);
	return gridRate;
}

half GetNoiseGrid(float2 pos){
	float3 argument = float3(pos.x,pos.y,_SinTime.y*0.2+_CosTime.y*0.8);
	return noise(argument);
}

half GetBinaryNoiseGrid(float2 pos){
	float3 argument = float3(pos.x,pos.y,_Time.y);
	if(noise(argument) < 0.5) return 0.0;
	return 1.0;
}

sampler2D	_NoiseTex;

half GetNoiseTextureGrid(float2 pos, float2 nPos){
	float3 argument = float3(pos.x,pos.y,_SinTime.y*0.1+_CosTime.w*0.9);
	float value = noise(argument);
	value += tex2D(_NoiseTex, nPos).a * 0.2;
	return value;
}


#endif // SCREEN_GRID_INCLUDED
