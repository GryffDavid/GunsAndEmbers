float4x4 MatrixTransform;

void SpriteVertexShader(inout float2 texCoord : TEXCOORD0,
                        inout float4 position : SV_Position)
{
    position = mul(position, MatrixTransform);	
}

sampler meterSample : register(s0);
float meterValue = 1;

float4 PixelShaderFunction(float2 texCoord: TEXCOORD0) : COLOR0
{    
    float4 tex = tex2D(meterSample, texCoord);  
    float2 direction = texCoord - float2(0.5, 0.5);
	float angle = atan2(-direction.y, -direction.x);
  
    float alpha = saturate(angle / (3.141592654 * 2) - 0.5 + meterValue) * 100;  

    //if (alpha > 0)
	//	alpha = 1;
	
    tex.a = min(tex.a, alpha);  
  
    return tex;  
}

technique Technique1
{
    pass Pass1
    {       
		VertexShader = compile vs_3_0 SpriteVertexShader();
        PixelShader = compile ps_3_0 PixelShaderFunction(); 
    }
}