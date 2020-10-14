float4x4 MatrixTransform;

void SpriteVertexShader(inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	position = mul(position, MatrixTransform);
}

uniform extern texture ScreenTexture;
sampler ScreenS = sampler_state
{
	Texture = <ScreenTexture>;
};

uniform extern texture NormalTexture;
sampler NormalSample = sampler_state
{
	Texture = <NormalTexture>;
};

uniform extern texture DistortionTexture;
sampler DistortionSample = sampler_state
{
	Texture = <DistortionTexture>;
};

float CurrentTime;

float2 uv_polar(float2 uv, float2 center)
{
	float2 c = uv - center;
	float rad = length(c);
	float ang = atan2(c.x, c.y);
	return float2(ang, rad);
}

float2 uv_lens_half_sphere(float2 uv, float2 position, float radius, float refractivity)
{
	float2 polar = uv_polar(uv, position);
	float cone = clamp(1. - polar.y / radius, 0., 1.);
	float halfsphere = sqrt(1. - pow(cone - 1., 2.));
	float w = atan2(1. - cone, halfsphere);
	float refrac_w = w - asin(sin(w) / refractivity);
	float refrac_d = 1. - cone - sin(refrac_w)*halfsphere / cos(refrac_w);
	float2 refrac_uv = position + float2(sin(polar.x), cos(polar.x))*refrac_d*radius;
	return lerp(uv, refrac_uv, float(length(uv - position) < radius));
}

const float2 Resolution = float2(1920, 1080);

float4 PixelShaderFunction(float2 texCoord: TEXCOORD0) : COLOR0
{
	float2 HalfPixel = float2(0.5 / Resolution.x, 0.5 / Resolution.y);
	float4 CResult;
	float2 uv = texCoord.xy;
	float2 aspect = float2(1.,Resolution.y / Resolution.x);
	float2 pos = float2(0.08, 0.63);
	float2 uv_correct = pos.y + (uv - pos.y) * aspect;

	float4 disp = (tex2D(NormalSample, uv_lens_half_sphere(uv_correct, pos, 0.15, 3.0).xy / (0.03)) - float4(0.5, 0.5, 0.5, 0.0));
	float2 uv_lense_distorted = uv_lens_half_sphere(uv_correct, pos, 0.15, 1.1);
	float2 actualPos = float2(texCoord.x*Resolution.x, texCoord.y*Resolution.y);
	float dist2 = distance(actualPos, float2(Resolution.x*pos.x, Resolution.y*pos.y)) / Resolution;

	float offset;
	if (dist2 < 0.15)
		offset = ((0.05 * disp.xy * -disp.z) * 0.5) + (0.05 * disp.xy * -disp.z);
	else
		offset = 0;

	uv_lense_distorted = (pos.y + (uv_lense_distorted - pos.y) / aspect + offset);

	float4 lightblue = float4(0.01, 0.8, 0.9, 1) * 0.3;

	if (dist2 < 0.15 && texCoord.y < pos.y + 0.2 + (distance(texCoord.x, pos.x) * -abs(pos.x - texCoord.x) * 2))
	{
		if (disp.x <= 0.03 && disp.y <= 0.1)
		{
			uv_lense_distorted += HalfPixel;
			CResult = (tex2D(ScreenS, uv_lense_distorted)) + (float4(0.0125, 0.0125, 0.0125, 0.0125) + (lightblue * (dist2 * 2)));
		}
		else
		{
			uv_lense_distorted += HalfPixel;
			CResult = (tex2D(ScreenS, uv_lense_distorted)) + (lightblue * (dist2 * 2));
		}
	}
	else
	{
		uv += HalfPixel;
		CResult = tex2D(ScreenS, uv);
	}

	return CResult;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 PixelShaderFunction();
		VertexShader = compile vs_3_0 SpriteVertexShader();
	}
}