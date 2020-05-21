float ambient;
float4 ambientColor;
float lightAmbient;

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
    output.position = mul(input.Position, Projection);
	output.Color = input.color;

	return output;
}

Texture ColorMap;
sampler ColorMapSampler = sampler_state 
{
	texture = <ColorMap>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture ShadingMap;
sampler ShadingMapSampler = sampler_state 
{
	texture = <ShadingMap>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture NormalMap;
sampler NormalMapSampler = sampler_state 
{
	texture = <NormalMap>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};


float2 iResolution = float2(1920, 1080);
float3 halfVec = float3(0, 0, 1);
float specVal = 0.008;

float4 CombinedPixelShader(VertexShaderOutput input) : COLOR0
{	
	float onex = 0.00052083333;
	float oney = 0.00092592592;

	//if (texCoords.x <= onex || texCoords.y <= oney)
	//{
	//	return float4(1,0,0,1);
	//}

	float2 texCoords = input.texCoord;

	float4 color2 = tex2D(ColorMapSampler, texCoords);
	float4 shading = tex2D(ShadingMapSampler, texCoords + float2(onex, oney));
	float normal = tex2D(NormalMapSampler, texCoords).rgb;
	
	//Ambient Occlusion
	float4 normalCol = tex2D(NormalMapSampler, texCoords);	
	float2 p = (texCoords.xy) * iResolution.xy;
	float3 normal1 = (2.0f * (tex2D(NormalMapSampler, texCoords))) - 1.0f;
	normal1 *= float3(1, -1, 1);

	float4 col = float4(0.025, 0.025, 0.025, 255);

	//Get the direction of the current pixel from the center of the light
	float3 lightDirNorm = normalize(float3(0.25, 0, 0) - float3(p.x, p.y, -25));	
	float amount = max(dot(normal1, lightDirNorm), 0);		
	float3 reflect = normalize(2.0 * amount * normal1 - lightDirNorm);
	float specular = min(pow(saturate(dot(reflect, halfVec)), specVal * 255), amount); 

	col += specular;

	if (normal > 0.0f)
	{
		//float4 finalColor = color2 * ambientColor;
		//finalColor += shading;
		//return finalColor;

		//Darker
		float4 finalColor = color2 * ambientColor * ambient;
		finalColor += (shading * color2) * lightAmbient;
		
		return finalColor + (col * 0.25f);
	}
	else
	{
		return color2 * ambientColor;
	}
}

technique DeferredCombined2
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 CombinedPixelShader();
		VertexShader = compile vs_3_0 SpriteVertexShader();
    }
}