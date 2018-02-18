#ifndef COMMON_CONSTANT_INCLUDED
#define COMMON_CONSTANT_INCLUDED

#define PI 3.14159265358979
#define PI2 6.28318530717959


float3 mod(float3 a, float3 b)
{
	return frac(abs(a / b)) * abs(b);
}


#endif // COMMON_CONSTANT_INCLUDED