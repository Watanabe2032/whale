#ifndef RANDOM_NOISE_INCLUDED
#define RANDOM_NOISE_INCLUDED

// http://wordpress.notargs.com

float rand(float3 co){
	return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 56.787))) * 43758.5453);
}

float noise(float3 pos){
	float3 ip = floor(pos);
	float3 fp = smoothstep(0, 1, frac(pos));
	float4 a = float4(
	rand(ip + float3(0, 0, 0)),
	rand(ip + float3(1, 0, 0)),
	rand(ip + float3(0, 1, 0)),
	rand(ip + float3(1, 1, 0)));
	float4 b = float4(
	rand(ip + float3(0, 0, 1)),
	rand(ip + float3(1, 0, 1)),
	rand(ip + float3(0, 1, 1)),
	rand(ip + float3(1, 1, 1)));
	a = lerp(a, b, fp.z);
	a.xy = lerp(a.xy, a.zw, fp.y);
	return lerp(a.x, a.y, fp.x);
}

float perlin(float3 pos){
	return (noise(pos)+noise(pos*2)+noise(pos*4)+noise(pos*8)+noise(pos*16)+noise(pos*32))/6;
}

#endif // RANDOM_NOISE_INCLUDED
