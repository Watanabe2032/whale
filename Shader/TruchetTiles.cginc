#ifndef TRUCHET_TILES_INCLUDED
#define TRUCHET_TILES_INCLUDED

#include "UnityCG.cginc"
#include "CommonConstant.cginc"

float _Interval;
float _RotationTime;

float Random(float3 f3){
	return frac(sin(dot(f3, float3(12.9898, 78.233, 56.787))) * 43758.5453);
}

float GetRightAngle(float interval, float rotationTime){
	float time = mod(_Time.y, interval + rotationTime);
	return PI * 0.5 * clamp(time-interval, 0.0, rotationTime) / rotationTime;
}

float GetStage(float interval, float rotationTime, float cycleCount){
	float time = mod(_Time.y, (interval + rotationTime) * cycleCount);
	return floor(time / (interval + rotationTime));
}

float GetIndex(float normal){
	if(normal <= 0.25) return 0.0;
	else if(normal <= 0.5) return 1.0;
	else if(normal <= 0.75) return 2.0;
	return 3.0;
}

float2 Rotate2D(float2 pos, float angle){
	float s = sin(angle);
	float c = cos(angle);
	float2 _pos = pos - 0.5;
	return float2(_pos.x*c+_pos.y*s, _pos.x*-s+_pos.y*c) + 0.5;
}

float2 Split(inout float2 nPos, float split){
	float2 index = trunc(nPos * split);
	nPos = frac(nPos * split);
	return index;
}

// for GetTileTypeB
float GetCellAngle(float index, float stage, float angle, float maxI){
	float setCount = floor(stage / maxI);
	float setIndex = stage - maxI * setCount;
	float _angle = PI * 0.5 * setCount;
	if(index < setIndex) _angle += PI * 0.5;
	if(index == setIndex) _angle += angle;
	return _angle;
}

float GetTileTypeB(float2 nPos, float split){
	
	float2 posL1 = nPos;
	float2 integer = Split(posL1, split);
	float rand = Random(float3(integer,0));	
	float cellIndex = GetIndex(rand);
	float sign = 1.0;
	if(frac(rand*100.0) > 0.5) sign = -1.0;
	float stage = GetStage(1.0, 0.5, 16.0);
	float angle = GetCellAngle(cellIndex, stage, GetRightAngle(1.0, 0.5), 4.0);
	angle += PI * 0.5 * cellIndex;
		
	posL1 = Rotate2D(posL1, sign * angle);
	return step(posL1.x,posL1.y);
}

float GetWindmill(float2 pos, float2 integer){
	float2 isOddNum;
	isOddNum.x = step(1.0, mod(integer.x, 2.0));
	isOddNum.y = step(1.0, mod(integer.y, 2.0));
	if(isOddNum.x - isOddNum.y != 0.0) pos.x = 1.0 - pos.x;
	if(isOddNum.x == 0.0) return step(pos.y, pos.x);
	return step(pos.x, pos.y);
}

float IsEvenNumber(float2 integer){
	float temp = step(1.0, mod(integer.x, 2.0))-step(1.0, mod(integer.y, 2.0));
	if(temp != 0.0) return 0.0;
	return 1.0;
}

float2 RotateTileLayerTypeD(inout float2 pos,float2 time, float layer){
	float2 integer = Split(pos, 2.0);
	float stage = GetStage(time.x, time.y, 8.0);
	float index = IsEvenNumber(integer);
	float angle = GetRightAngle(time.x, time.y);
	angle = GetCellAngle(index, stage, angle, 2.0);
	pos = Rotate2D(pos, angle);
	return integer;
}

float GetTileTypeD(float2 nPos, float split){
	float2 pos = frac(nPos * split * 0.125);
	float2 time = float2(1.0, 0.5);
	float2 integer;
	for (int n = 0; n < 3; ++n) {
		integer = RotateTileLayerTypeD(pos, time, n - 3.0);
	}
	return GetWindmill(pos, integer);
}

float2 RotateTileLayerTypeE(inout float2 pos, float2 time, float layer){
	float2 integer = Split(pos, 2.0);
	float index = IsEvenNumber(integer) * layer + layer - 1.0;
	float stage = GetStage(time.x, time.y, 8.0 * layer);
	float angle = GetRightAngle(time.x, time.y);
	angle = GetCellAngle(index, stage, angle, 2.0 * layer);
	pos = Rotate2D(pos, angle);
	return integer;
}

float GetTileTypeE(float2 nPos, float split){
	float2 pos = frac(nPos * split * 0.0625);
	float2 time = float2(1.0, 0.5);
	float2 integer;
	for (int n = 0; n < 5; ++n) {
		integer = RotateTileLayerTypeE(pos, time, n+1.0);
	}
	return GetWindmill(pos, integer);
}

float GetTile(float2 nPos, float split, int mode){
	if(mode == 0) return GetTileTypeB(nPos, split);
	if(mode == 1) return GetTileTypeD(nPos, split);
	if(mode == 2) return GetTileTypeE(nPos, split);
	return 0.0;
}



// ----------- Quarter Circles -------------

float GetCircle(float2 pos, float width){
	float len = length(pos);
	if(abs(len - 0.5) < width) return 1.0;
	len = distance(pos, float2(1,1));
	if(abs(len - 0.5) < width) return 1.0;
	return 0.0;
}

float GetHalfCicleAngle(float interval, float rotationTime){
	float time = fmod(_Time.y, (interval + rotationTime)*2.0);
	float angle = 0.0;
	if(time > interval + rotationTime){
		angle += PI * 0.5;
		time -= interval + rotationTime;
	}
	angle += PI * 0.5 * clamp(time-interval, 0.0, rotationTime) / rotationTime;
	return angle;
}

float GetGridQC(float2 nPos, float split){
	float2 pos = frac(nPos * split);
	float2 integer = trunc(nPos * split);
	float rand = Random(float3(integer,0));
	if(rand > 0.5) pos.x = 1.0 - pos.x;
	
	float index = GetIndex(rand);
	if(index > 1.0) index -= 1.0;
	else index -= 2.0;
	
	float angle = GetHalfCicleAngle(5.0, 1.0);
	
	pos = Rotate2D(pos, angle*index);
	return GetCircle(pos, 0.1);
}


#endif // TRUCHET_TILES_INCLUDED

