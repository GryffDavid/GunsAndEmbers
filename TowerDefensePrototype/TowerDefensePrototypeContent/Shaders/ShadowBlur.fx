float4x4 Projection;
float4x4 World;

struct VertexShaderInput
{
    float4 Position : SV_Position;
	float2 texCoord : TEXCOORD0;
	float4 color : COLOR0;
};

struct VertexShaderOutput
{
	float2 texCoord : TEXCOORD0;
    float4 position : POSITION0;
	float4 Color : COLOR0;
};

VertexShaderOutput SpriteVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.texCoord = input.texCoord;

	float4x4 preWorldProjection = mul(World, Projection);

    output.position = mul(input.Position, preWorldProjection);

    //output.position = mul(input.Position, Projection);
	output.Color = input.color;

	return output;
}

Texture Texture;
sampler TextureSampler = sampler_state 
{
	texture = <Texture>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = clamp;
	AddressV = clamp;
	MaxAnisotropy = 16;
};

float Gaussian (float sigma, float x)
{
    return exp(-(x*x) / (2.0 * sigma*sigma));
}

float2 texSize;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 uv = input.texCoord;
	//float2 HalfPixel = float2(0.5, 0.5)/texSize;
	float2 OnePixel = float2(1,1)/texSize;

	float4 color = tex2D(TextureSampler, uv);
	float4 c = float4(0, 0, 0, 0);

	int offset = 6;

		for (int x = -offset; x < offset; x++)
		{
			float fx = Gaussian(1.5f, x);

			for (int y = -offset; y < offset; y++)
			{
				float fy = Gaussian(1.5f, y);
				c += tex2D(TextureSampler, uv + float2(OnePixel.x * x, OnePixel.y * y)).xyzw * fx * fy;
			}
		}
	
	float4 newCol = float4(c/((offset*2)*(offset*2)));
	return newCol * input.Color;
	//return tex2D(TextureSampler, uv) * input.Color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
