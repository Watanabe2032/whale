#ifndef GEAR_GRID_INCLUDED
#define GEAR_GRID_INCLUDED

#include "CommonConstant.cginc"
#include "GearMath.cginc"


float GetGearTileTypeA(float2 pos){
	
	int quadrant = 0;
	if(pos.x > 0.5) quadrant += 1;
	if(pos.y > 0.5) quadrant += 2;
	
	float4 gear;
	float4 hole;
	float2 _pos;
	float2 offset;
	
	// cross gear
	gear = float4(0.087, 0.02, 12.0, 0.5);
	_pos = pos;
	if(pos.y > 0.25 && pos.y < 0.75) _pos.x = frac(pos.x + 0.5);
	else _pos.y = frac(pos.y + 0.5);
	if(GetGearGrid(_pos, float2(0.5, 0.5), gear, 6.0, 0.0) == 1.0) return 1.0;
	
	
	// quad gear
	gear = float4(0.08, 0.02, 12.0, 0.5);
	float2 qSign = sign(pos - float2(0.5, 0.5));
	float qOffset = 0.35;

	_pos = float2(pos.x, frac(pos.y - qSign.y * qOffset));
	if(GetGearGrid(_pos, float2(0.5, 0.5), gear, -6.0, 0.0) == 1.0) return 1.0;
	_pos = float2(frac(pos.x - qSign.x * 0.356), pos.y + qSign.x * 0.042);
	if(GetGearGrid(_pos, float2(0.5, 0.5), gear, -6.0, 0.95) == 1.0) return 1.0;
	
	// small gear
	gear = float4(0.06, 0.02, 9.0, 0.5);
	float sRate = 0.92;
	if(pos.x < 0.5) sRate = abs(sRate - 0.5);
	_pos = float2(frac(pos.x - qSign.x * 0.388), pos.y + qSign.x * 0.1626);
	if(GetGearGrid(_pos, float2(0.5, 0.5), gear, 8.0, sRate) == 1.0) return 1.0;
	
	// triple gear
	gear = float4(0.12, 0.02, 18.0, 0.5);
	offset = float2(0.3, -0.3);
	_pos = pos;
	if(quadrant == 1 && pos.y < 0.5 - gear.x) _pos -= offset;
	else if(quadrant == 2 && pos.y > 0.5 + gear.x) _pos += offset;
	else _pos = float2(0.0, 0.0); // to erase center
	if(GetGearGrid(_pos, float2(0.5, 0.5), gear, -4.0, 0.35) == 1.0) return 1.0;
	
	// double gear
	gear = float4(0.12, 0.02, 18.0, 0.5);
	offset = float2(0.1, -0.2);
	_pos = pos;
	if(pos.y < 0.5) _pos -= offset;
	else _pos += offset;
	if(GetGearGrid(_pos, float2(0.5, 0.5), gear, 4.0, 0.35) == 1.0) return 1.0;
	
	// big gear
	gear = float4(0.20, 0.02, 30.0, 0.5);
	offset = float2(0.237, 0.237);
	_pos = pos;
	if(pos.y < 0.5) _pos += offset;
	else _pos -= offset;
	
	hole = float4(gear.x - 0.02, 0.02, 5.0, 2.4);
	if(GetLighteningHole(_pos, float2(0.5, 0.5), hole) == 0.0 &&
	GetGearGrid(_pos, float2(0.5, 0.5), gear, 2.4, 0.0) == 1.0) return 1.0;
	
	return 0.0;
}

float GetGearTileTypeB(float2 pos){
	
	float4 cGear = float4(0.38, 0.055, 24.0, 0.5);
	float2 cPos = pos + float2(0.5, 0.5);
	if(pos.x > 0.5) cPos.x -= 1.0;
	if(pos.y > 0.5) cPos.y -= 1.0;
	if(GetStandardGearGridTypeA(cPos, float2(0.5, 0.5), 1.0, 0.0) == 1.0) return 1.0;
	if(GetStandardGearGridTypeA(pos, float2(0.5, 0.5), -1.0, 0.0) == 1.0) return 1.0;
	
	return 0.0;
}

float GetGearTileTypeC(float2 pos){

	// inner big gear
	float4 bGear = float4(0.084, 0.02, 12.0, 0.5);
	float2 bOffset = float2(0.237, 0.237);
	float2 bPos = pos;
	if(pos.y < 0.5) bPos += bOffset;
	else bPos -= bOffset;
	if(GetGearGrid(bPos, float2(0.5, 0.5), bGear, 2.4, 0.0) == 1.0) return 1.0;
	
	// internal gear
	float4 iGear = float4(0.4, 0.02, 60.0, 0.5);
	if(GetInternalGearGrid(pos, float2(0.5, 0.5), iGear, 0.48, 0.5) == 1.0) return 1.0;
	
	return 0.0;
}


float GetGearScreen(float2 pos){

	int quadrant = 0;
	if(pos.x > 0.5) quadrant += 1;
	if(pos.y > 0.5) quadrant += 2;
	
	// gear rate  b:m:c = 3:1:2 

	// big gear (center gear)
	float4 bGear = float4(0.33, 0.02, 60.0, 0.5);
	float4 bHole = float4(bGear.x - bGear.y, 0.032, 6.0, 2.0);
	if(GetLighteningHole(pos, float2(0.5, 0.5), bHole) == 0.0 &&
	GetGearGrid(pos, float2(0.5, 0.5), bGear, 2.0, 0.0) == 1.0) return 1.0;
	
	// medium gear
	if(quadrant == 1 || quadrant == 2){
		float2 mCenter = float2(0.725, 0.855);
		float2 mPos = pos;
		if(quadrant == 1) mPos.y = 1.0 - pos.y;
		else mPos.x = 1.0 - pos.x;
		float4 mGear = float4(0.11, 0.02, 20.0, 0.5);
		if(GetGearGrid(mPos, mCenter, mGear, 6.0, 0.7) == 1.0) return 1.0;
	}
	
	// corner gear
	float4 cGear = float4(0.22, 0.02, 40.0, 0.5);
	float4 cHole = float4(cGear.x - 0.03, 0.02, 3.0, 3.0);
	float2 cPos = pos + float2(0.5, 0.5);
	if(pos.x > 0.5) cPos.x -= 1.0;
	if(pos.y > 0.5) cPos.y -= 1.0;
	if(GetLighteningHole(cPos, float2(0.5, 0.5), cHole) == 0.0 &&
	GetGearGrid(cPos, float2(0.5, 0.5), cGear, 3.0, 0.85) == 1.0) return 1.0;
	
	// inner big gear
	float iRPM = 2.4691358;
	float4 iGear = float4(0.068, 0.008, 27.0, 0.5);
	if(GetGearGrid(pos, float2(0.5, 0.5), iGear, iRPM, 0.0) == 1.0) return 0.5;
	
	// relay gear  (f:front, b:back)
	float2 rOffset = float2(0.1032, -0.0639);
	float rRPM = -2.4 / 0.9;
	float2 rPos = pos + rOffset;
	float4 rfGear = float4(0.06, 0.008, 25.0, 0.5);
	float4 rbGear = float4(0.106, 0.0095, 27.0, 0.57);
	
	// relay gear (front)
	if(GetGearGrid(rPos, float2(0.5, 0.5), rfGear, rRPM, 0.1) == 1.0) return 0.1;
	
	// middle layer
	if(GetGearTileTypeA(frac(pos * 2.0)) == 1.0) return 0.5;
	
	// back layer
	//if(GetGearTileTypeB(frac(pos * 16.0)) == 1.0) return 0.1;
	
	// relay gear (back)
	if(GetGearGrid(rPos, float2(0.5, 0.5), rbGear, rRPM, 0.5) == 1.0) return 1.0;
	
	// internal gear
	if(quadrant == 0 || quadrant == 3){
		if(GetGearTileTypeC(frac(pos * 2.0)) == 1.0) return 1.0;
	}
	
	return 0.0;
}

float GetSimpleGearScreen(float2 pos){
	float4 gear = float4(0.33, 0.02, 60.0, 0.5);
	float4 hole = float4(gear.x - gear.y, 0.032, 6.0, 2.0);
	if(GetLighteningHole(pos, float2(0.5, 0.5), hole) == 0.0 &&
	GetGearGrid(pos, float2(0.5, 0.5), gear, 2.0, 0.0) == 1.0) return 1.0;
	
	gear.x = gear.x - gear.y + 0.01;
	if(GetInternalGearGrid(pos, float2(0.5, 0.5), gear, 2.0, 0.5) == 1.0) return 1.0;
	return 0.0;
}

#endif // GEAR_GRID_INCLUDED