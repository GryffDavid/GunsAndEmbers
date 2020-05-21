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

float4 Color = float4(1,1,1,1);

float4 StartColor, EndColor;
float LerpPerc;
float Trans = 1;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float2 uv = input.texCoord;

	//float onex = 0.00052083333;
	//float oney = 0.00092592592;

	//if (input.texCoord.x <= onex || input.texCoord.y <= oney)
	//{
	//	return float4(1,0,0,1);
	//}

	float4 color = tex2D(TextureSampler, uv);
	color = color * lerp(EndColor, StartColor, LerpPerc) * Trans;
	return color;
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
