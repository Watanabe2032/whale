#ifndef RAYMARCHING_INCLUDED
#define RAYMARCHING_INCLUDED

//#include "UnityCG.cginc"
#include "CommonConstant.cginc"


float sphere(float3 pos, float radius)
{
    return length(pos) - radius;
}

float roundBox(float3 pos, float3 size, float round)
{
    return length(max(abs(pos) - size * 0.5, 0.0)) - round;
}

float box(float3 pos, float3 size)
{
    return roundBox(pos, size, 0);
}

float torus(float3 pos, float2 radius)
{
    float2 r = float2(length(pos.xy) - radius.x, pos.z);
    return length(r) - radius.y;
}

/*
float floor(float3 pos)
{	
	float value = dot(pos, float3(0.0, 1.0, 0.0)) + 1.0;
    return value;
}
*/

float cylinder(float3 pos, float2 r)
{
    float2 d = abs(float2(length(pos.xy), pos.z)) - r;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - 0.1;
}

inline bool IsInnerBox(float3 pos, float3 scale)
{
	return all(max(scale * 0.5 - abs(pos), 0.0));
}

float3 repeat(float3 pos, float3 span)
{
	return mod(pos, span) - span * 0.5;
}


float _SphereSize;
float _ColorCutoff;
int _ShadowMode;
int _Cull;

float DistanceFunction(float3 pos)
{	
	if(_ShadowMode == 0) return 1.0;
	
	else if(_ShadowMode == 1){
		float3 _pos = float3(pos.x+_Time.x, pos.y, pos.z);
		float3 rePos = repeat(_pos, (_ColorCutoff+0.001)*2.0);
		return length(max(abs(rePos), 0.0)) - 0.2;
	}

	else if(_ShadowMode == 2){
		float3 rePos = repeat(pos, (_ColorCutoff+0.1)*4.0);
		return length(max(abs(rePos) - float3(1,1,1)*0.2, 0.0)) - 0.1;
	}
	
	return 1.0;
}


float3 GetCameraPosition()    { return _WorldSpaceCameraPos;      }
float3 GetCameraForward()     { return -UNITY_MATRIX_V[2].xyz;    }
float3 GetCameraUp()          { return UNITY_MATRIX_V[1].xyz;     }
float3 GetCameraRight()       { return UNITY_MATRIX_V[0].xyz;     }
float  GetCameraFocalLength() { return abs(UNITY_MATRIX_P[1][1]); }
float  GetCameraMaxDistance() { return _ProjectionParams.z - _ProjectionParams.y; }


float GetContourFromLight(float3 pos, float3 normal){
	float4 VertToLight = float4(normalize(_WorldSpaceCameraPos - pos),0.0);
	float4 wNormal = mul(unity_ObjectToWorld, float4(normal,0.0));
	return abs(dot(VertToLight, wNormal));
}


inline float3 ToLocal(float3 pos)
{
    //return mul(unity_WorldToObject, float4(pos, 1.0)).xyz * _Scale;
    return mul(unity_ObjectToWorld, float4(pos, 1.0)).xyz;
}

float2 GetScreenPos(float4 screenPos)
{
#if UNITY_UV_STARTS_AT_TOP
    screenPos.y *= -1.0;
#endif
    screenPos.x *= _ScreenParams.x / _ScreenParams.y;
    return screenPos.xy / screenPos.w;
}

float3 GetRayDirection(float4 screenPos)
{
    float2 sp = GetScreenPos(screenPos);

    float3 camPos      = GetCameraPosition();
    float3 camDir      = GetCameraForward();
    float3 camUp       = GetCameraUp();
    float3 camSide     = GetCameraRight();
    float  focalLen    = GetCameraFocalLength();
    float  maxDistance = GetCameraMaxDistance();

    return normalize((camSide * sp.x) + (camUp * sp.y) + (camDir * focalLen));
}

float3 GetRayDirectionForShadow(float4 screenPos)
{
    float4 sp = screenPos;

#if UNITY_UV_STARTS_AT_TOP
    sp.y *= -1.0;
#endif
    sp.xy /= sp.w;

    float3 camPos      = GetCameraPosition();
    float3 camDir      = GetCameraForward();
    float3 camUp       = GetCameraUp();
    float3 camSide     = GetCameraRight();
    float  focalLen    = GetCameraFocalLength();
    float  maxDistance = GetCameraMaxDistance();

    return normalize((camSide * sp.x) + (camUp * sp.y) + (camDir * focalLen));
}

float GetDepth(float3 pos)
{
    float4 vpPos = mul(UNITY_MATRIX_VP, float4(pos, 1.0));
#if defined(SHADER_TARGET_GLSL)
    return (vpPos.z / vpPos.w) * 0.5 + 0.5;
#else 
    return vpPos.z / vpPos.w;
#endif 
}

float3 GetNormalOfDistanceFunction(float3 pos)
{
    float d = 0.001;
    return 0.5 + 0.5 * normalize(float3(
        DistanceFunction(pos + float3(  d, 0.0, 0.0)) - DistanceFunction(pos + float3( -d, 0.0, 0.0)),
        DistanceFunction(pos + float3(0.0,   d, 0.0)) - DistanceFunction(pos + float3(0.0,  -d, 0.0)),
        DistanceFunction(pos + float3(0.0, 0.0,   d)) - DistanceFunction(pos + float3(0.0, 0.0,  -d))));
}

inline float ObjectSpaceDistanceFunction(float3 pos)
{
	return DistanceFunction(ToLocal(pos));
}

float3 GetNormalOfObjectSpaceDistanceFunction(float3 pos)
{
    float d = 0.001;
    return 0.5 + 0.5 * normalize(float3(
        ObjectSpaceDistanceFunction(pos + float3(  d, 0.0, 0.0)) - ObjectSpaceDistanceFunction(pos + float3( -d, 0.0, 0.0)),
        ObjectSpaceDistanceFunction(pos + float3(0.0,   d, 0.0)) - ObjectSpaceDistanceFunction(pos + float3(0.0,  -d, 0.0)),
        ObjectSpaceDistanceFunction(pos + float3(0.0, 0.0,   d)) - ObjectSpaceDistanceFunction(pos + float3(0.0, 0.0,  -d))));
}


void Raymarch(inout float3 pos, out float distance, float3 rayDir, float minDistance, int loop)
{
	float len = 0.0;

	for (int n = 0; n < loop; ++n) {
		distance = ObjectSpaceDistanceFunction(pos);
		len += distance;
		pos += rayDir * distance;
		float3 lPos = mul(unity_ObjectToWorld, float4(pos, 1.0)).xyz;
		
		if(_Cull == 0){
			if (!IsInnerBox(lPos, float3(4.0,8.0,2.0)) || distance < minDistance) break;
		}
		else{
			if (distance < minDistance) break;
		}
	}

	if (distance > minDistance) discard;
}


#endif // RAYMARCHING_INCLUDED