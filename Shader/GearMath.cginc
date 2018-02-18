#ifndef GEAR_MATH_INCLUDED
#define GEAR_MATH_INCLUDED

#include "CommonConstant.cginc"


// By Cross  return [ 0:out, 1:in, ?:line ]
float CheckTriangleHit(float2 p, float2 a, float2 b, float2 c){
	float2 ab = b - a;
	float2 bc = c - b;
	float2 ca = a - c;
	float2 ap = p - a;
	float2 bp = p - b;
	float2 cp = p - c;
	float az = ca.x * ap.y - ca.y * ap.x;
	float bz = ab.x * bp.y - ab.y * bp.x;
	float cz = bc.x * cp.y - bc.y * cp.x;
	if(az > 0.0 && bz > 0.0 && cz > 0.0) return 1.0;
	if(az < 0.0 && bz < 0.0 && cz < 0.0) return 1.0;
	return 0.0;
}

float GetRadian(float2 center, float2 target){
	float2 posDiff = target - center;
	float rad = atan2(posDiff.y, posDiff.x);
	if(rad < 0.0) rad += PI2;
	return rad;
}

// reflected offset and time, get rad in circularPitch
float GetPitchRad(float rad, float pitch, float offset, float num, float rpm){
	float pitchPeriod = abs(60.0 / rpm) / num;
	float pitchPeriodRate = (_Time.y % pitchPeriod) / pitchPeriod;
	if(sign(rpm) < 0.0) pitchPeriodRate = 1.0 - pitchPeriodRate;
	float radOffset = pitch * (pitchPeriodRate + offset);
	return (rad + radOffset) % pitch;
}


// float4 gear4 = (topRadius, gearHight, gearNum, spaceWidthRate)
// pitchOffset = 0.0 ~ 1.0 [rate]
// rpm>0 : cw, rpm<0 : ccw

float GetGearGrid(
	float2 pos, float2 center, float4 gear4, float rpm, float pitchOffset){
	
	// check outer gear
	float radius = distance(center, pos);
	if(radius > gear4.x) return 0.0;
	
	// check gear axis
	if(radius < gear4.x * 0.1) return 0.0;
	
	// pitch [rad]
	float rad = GetRadian(center, pos);
	float circularPitch = PI2 / gear4.z;
	float gearPitch = circularPitch * (1.0 - gear4.w);
	//float spacePitch = circularPitch * gear4.w;
	
	// reflected offset and time
	float newRad = GetPitchRad(rad, circularPitch, pitchOffset, gear4.z, rpm);
	float2 newPos = radius * float2(cos(newRad), sin(newRad));
	float2 newCenter = float2(0.0, 0.0);
	
	// gear bottom point
	float bottomR = gear4.x - gear4.y; // gear bottom radius
	float2 b0 = float2(bottomR, 0.0);
	float2 b1 = bottomR * float2(cos(gearPitch), sin(gearPitch));
	float2 b2 = bottomR * float2(cos(circularPitch), sin(circularPitch));
	
	// check gear root
	if(CheckTriangleHit(newPos, newCenter, b0, b1) == 1.0) return 1.0;
	if(CheckTriangleHit(newPos, newCenter, b1, b2) == 1.0) return 1.0;
	
	// gear top point
	float pa = PI * 10.0 / 180.0;
	float2 t0 = b0 + gear4.y * float2(cos(pa), sin(pa));
	float2 t1 = b1 + gear4.y * float2(cos(gearPitch-pa), sin(gearPitch-pa));

	// check gear
	if(CheckTriangleHit(newPos, b0, t0, t1) == 1.0) return 1.0;
	if(CheckTriangleHit(newPos, b1, b0, t1) == 1.0) return 1.0;
	
	return 0.0;
}

float GetInternalGearGrid(
	float2 pos, float2 center, float4 gear4, float rpm, float pitchOffset){
	
	// check outer gear
	float radius = distance(center, pos);
	float gearBttomR = gear4.x + gear4.y;
	float outerRingR = gearBttomR + gear4.y * 1.5;
	if(radius > outerRingR) return 0.0;
	
	// pitch [rad]
	float rad = GetRadian(center, pos);
	float circularPitch = PI2 / gear4.z;
	float gearPitch = circularPitch * (1.0 - gear4.w);
	
	// reflected offset and time
	float newRad = GetPitchRad(rad, circularPitch, pitchOffset, gear4.z, rpm);
	
	// check gear bottom rad
	if(newRad > gearPitch){
		if(radius < gearBttomR) return 0.0;
		return 1.0;
	}
	
	// check gear rad part
	float halfRad = newRad;
	if(halfRad > gearPitch * 0.5) halfRad = gearPitch - halfRad;
	float2 newPos = radius * float2(cos(halfRad), sin(halfRad));
	
	// gear top center position
	float2 gtcPos;
	gtcPos.x = gear4.x * cos(gearPitch);
	gtcPos.x += (gear4.x - gtcPos.x) * 0.5;
	gtcPos.y = gear4.x * sin(gearPitch) * 0.5;
	float2 b0 = float2(gear4.x, 0.0);
	float2 b1 = float2(gearBttomR, 0.0);
	float pa = PI * 10.0 / 180.0;
	float2 gt0Pos = b1 + float2(-cos(pa), sin(pa));
	if(CheckTriangleHit(newPos, float2(0.0,0.0), b0, gtcPos) == 1.0) return 0.0;
	if(CheckTriangleHit(newPos, b0, b1, gt0Pos) == 1.0) return 0.0;
	
	return 1.0;
}

inline float GetStandardGearGridTypeA(
	float2 pos, float2 center, float rpm, float pitchOffset){
	
	float circularPitch = 0.2617994;
	float2 b0 = float2(0.324999, 0.0);
	float2 b1 = float2(0.322219, 0.042421);
	float2 b2 = float2(0.313925, 0.084116);
	float2 t0 = float2(0.379164, 0.009550);
	float2 t1 = float2(0.377167, 0.040021);
	
	float radius = distance(center, pos);
	if(radius > 0.38) return 0.0;
	if(radius < 0.038) return 0.0;
	float rad = GetRadian(center, pos);
	float newRad = GetPitchRad(rad, circularPitch, pitchOffset, 24.0, rpm);
	float2 newPos = radius * float2(cos(newRad), sin(newRad));
	float2 newCenter = float2(0.0, 0.0);
	
	// check gear root
	if(CheckTriangleHit(newPos, newCenter, b0, b1) == 1.0) return 1.0;
	if(CheckTriangleHit(newPos, newCenter, b1, b2) == 1.0) return 1.0;
	
	// check gear
	if(CheckTriangleHit(newPos, b0, t0, t1) == 1.0) return 1.0;
	if(CheckTriangleHit(newPos, b1, b0, t1) == 1.0) return 1.0;
	
	return 0.0;
}


// Lightening Hole
// float4 hole4 = (gearBottomRadius, pillarWidth, holeNum, rpm) 

float GetLighteningHole(
	float2 pos, float2 center, float4 hole4){
	
	float radius = distance(center, pos);
	if(radius > hole4.x) return 0.0;
	if(radius < hole4.x * 0.1) return 1.0;
	
	float rad = GetRadian(center, pos);
	float holePitch = PI2 / hole4.z;
	float halfPitch = holePitch * 0.5;
	float halfRad = GetPitchRad(rad, holePitch, 0.0, hole4.z, hole4.w);
	if(halfRad > halfPitch) halfRad = holePitch - halfRad;
	float2 newPos = radius * float2(cos(halfRad), sin(halfRad));
	
	// chamfer holes
	float2 ch0 = hole4.x * 0.24 * float2(cos(halfPitch), sin(halfPitch));
	float chamferR = ch0.y - hole4.y * 0.5;
	float ch1Y = hole4.y * 0.5 + chamferR;
	float ch1R = hole4.x * 0.8;
	float ch1A = asin(ch1Y / ch1R);
	float2 ch1 = float2(ch1R * cos(ch1A), ch1Y);
	if(distance(newPos, ch0) < chamferR) return 1.0;
	if(distance(newPos, ch1) < chamferR) return 1.0;
	
	// check hole
	float2 ch0b = ch0 - float2(0.0, chamferR);
	float2 ch1b = ch1 - float2(0.0, chamferR);
	if(halfRad > ch1A && radius < hole4.x * 0.8 + chamferR &&
		newPos.x > ch0b.x && newPos.y > hole4.y * 0.5) return 1.0;
	if(CheckTriangleHit(newPos, ch0b, ch1b, ch1) == 1.0) return 1.0;
	
	return 0.0;
}



#endif // GEAR_MATH_INCLUDED