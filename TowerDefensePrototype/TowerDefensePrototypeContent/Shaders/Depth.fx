float4x4 World;
float4x4 Projection;

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

float depth;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	//float onex = 0.00052083333;
	//float oney = 0.00092592592;

	//if (input.texCoord.x >= 0.5 || input.texCoord.y >= 0.5)
	//{
	//	return float4(0,0,1,1);
	//}

	float4 tex;
	
	tex = tex2D(TextureSampler, input.texCoord);
	
	if (tex.a == 0)
		clip(tex.a - 1);

	return float4(depth, depth, depth, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
