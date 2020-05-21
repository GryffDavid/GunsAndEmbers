Texture2D NormalMap;
sampler NormalMapSampler = sampler_state 
{
	texture = <NormalMap>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture2D ColorMap;
sampler ColorMapSampler = sampler_state 
{
	texture = <ColorMap>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture2D ShadowMap;
sampler ShadowMapSampler = sampler_state 
{
	texture = <ShadowMap>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

Texture2D DepthMap;
sampler DepthMapSampler = sampler_state 
{
	texture = <DepthMap>;
	magfilter = POINT;
	minfilter = POINT;
	mipfilter = POINT;
	AddressU = Clamp;
	AddressV = Clamp;
};

struct VertexToPixel
{
	float4 Position : POSITION;
	float2 TexCoord : TEXCOORD0;
	float4 Color : COLOR0;
};

struct PixelToFrame
{
	float4 Color : COLOR0;
};

VertexToPixel MyVertexShader(float4 inPos: POSITION0, float2 texCoord: TEXCOORD0, float4 color: COLOR0)
{
	VertexToPixel Output = (VertexToPixel)0;
	
	Output.Position = inPos;
	Output.TexCoord = texCoord;
	Output.Color = color;
	
	return Output;
}



////////////////////
//Variables to use//
///////////////////

float2 iResolution = float2(1920, 1080);
float3 LightPosition;
float4 LightColor, AmbientColor;
float LightPower, LightSize;

//////////////////
//Dist functions//
//////////////////

float circleDist(float2 p, float radius)
{
	return length(p) - radius;
}

//////////////////////////////
//Light and Shadow Functions//
//////////////////////////////

float4 drawLight(float2 p, float2 pos, float4 color, float dist, float range, float radius)
{
	// distance to light
	float ld = length(p - pos);
	
	// out of range
	if (ld > range) 
		return float4(0, 0, 0, 0);
	
	// shadow and falloff
	float fall = (range - ld)/range;
	fall *= fall;
    
	float source = clamp(-circleDist(p - pos, radius), 0.0, 1.0);
	return (fall + source) * color;
}

float luminance(float4 col)
{
	return 0.2126 * col.r + 0.7152 * col.g + 0.0722 * col.b;
}

void setLuminance(inout float4 col, float lum)
{
	lum /= luminance(col);
	col *= lum;
}


//MAIN SCENE
float3 halfVec = float3(0, 0, 1);
float specVal = 0.005;
float lightDepth;

PixelToFrame PointLightShader(VertexToPixel PSIn)
{	
	PixelToFrame Output = (PixelToFrame)0;
	
	float4 colorMap = tex2D(ColorMapSampler, PSIn.TexCoord);	
		
	float2 p = (PSIn.TexCoord.xy) * iResolution.xy;


	float4 depthMap = tex2D(DepthMapSampler, PSIn.TexCoord);
	normalize(depthMap);

	//The angle of the pixel based on the normal map interpretation
	float3 normal;

	if (depthMap.r > lightDepth && depthMap.a == 1)
		normal = float4(0, 0, 0, 1);
	else
		normal = (2.0f * (tex2D(NormalMapSampler, PSIn.TexCoord))) - 1.0f;


	//The angle of the pixel based on the normal map interpretation
	//float3 normal = (2.0f * (tex2D(NormalMapSampler, PSIn.TexCoord))) - 1.0f;
	normal *= float3(1, -1, 1);

	
	float dist = 1.0;
	float4 col = AmbientColor;


	float2 light1Pos = float2(LightPosition.x, LightPosition.y);
	float4 light1Col = LightColor;
	
	//Get the direction of the current pixel from the center of the light
	float3 lightDirNorm = normalize(float3(light1Pos, 0) - float3(p.x, p.y, -25));	
	float amount = max(dot(normal, lightDirNorm), 0);		
	float3 reflect = normalize(2.0 * amount * normal - lightDirNorm);
	float specular = min(pow(saturate(dot(reflect, halfVec)), specVal * 255), amount); 
	setLuminance(light1Col, LightPower);

	col += drawLight(p, light1Pos, light1Col, dist, LightSize, 1.0) * specular;
	
	col = lerp(col, col, clamp(-dist, 0.0, 1.0));
	
	//NEED TO CHECK DEPTH MAP HERE BEFORE APPLYING SHADOWS SO THAT SHADOW AREN'T CAST ON OBJECTS THAT CAST THE SHADOW
	if (depthMap.r >= 0.01)
	{
		Output.Color = clamp(col, 0.0, 1.0);			
	}
	else
	{
		Output.Color = clamp(col, 0.0, 1.0) * tex2D(ShadowMapSampler, PSIn.TexCoord);	
	}

	return Output;
}

technique DeferredPointLight
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 MyVertexShader();
        PixelShader = compile ps_3_0 PointLightShader();
    }
}

