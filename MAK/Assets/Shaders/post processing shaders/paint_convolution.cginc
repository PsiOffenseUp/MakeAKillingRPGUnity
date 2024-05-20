#ifndef PAINT_INCLUDED
#define PAINT_INCLUDED

#include "UnityCG.cginc"

//sampler2D _MainTex;
//float2 _MainTex_TexelSize;
float4 _MainTex_ST;
float _KernelSize;

struct region
{
	float3 mean;
	float variance;
};

region calcRegion(int2 lower, int2 upper, int samples, float2 uv)
{
	region r;
   
	float3 sum = 0.0;
	float3 squareSum = 0.0;

	int x, y;
	float2 offset;
	float3 tex;
	for (x = lower.x; x <= upper.x; ++x)
	{
		for (y = lower.y; y <= upper.y; ++y)
		{
			offset = float2(_MainTex_TexelSize.x * x, _MainTex_TexelSize.y * y);
			tex = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uv + offset);

			sum += tex;
			squareSum += tex * tex;
		}
	}

	r.mean = sum / samples;

	float3 variance = abs((squareSum / samples) - (r.mean * r.mean));
	r.variance = length(variance);

	return r;
}

void kuwahara_convolution_float(float2 uv, out float3 color, out float alpha)
{
	int upper = (_KernelSize - 1) / 2;
	int lower = -upper;

	int samples = (upper + 1) * (upper + 1);

	region regionA = calcRegion(texture, int2(lower, lower), int2(0, 0), samples, uv);
	region regionB = calcRegion(texture, int2(0, lower), int2(upper, 0), samples, uv);
	region regionC = calcRegion(texture, int2(lower, 0), int2(0, upper), samples, uv);
	region regionD = calcRegion(texture, int2(0, 0), int2(upper, upper), samples, uv);

	color = regionA.mean;
	float minVar = regionA.variance;

	float testVal;

	// Test region B.
	testVal = step(regionB.variance, minVar);
	color = lerp(color, regionB.mean, testVal);
	minVar = lerp(minVar, regionB.variance, testVal);

	// Test region C.
	testVal = step(regionC.variance, minVar);
	color = lerp(color, regionC.mean, testVal);
	minVar = lerp(minVar, regionC.variance, testVal);

	// Text region D.
	testVal = step(regionD.variance, minVar);
	color = lerp(color, regionD.mean, testVal);

	alpha = 1.0;
	
}

#endif