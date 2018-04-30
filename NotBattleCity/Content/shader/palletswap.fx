float4x4 WorldViewProj	: WORLDVIEWPROJ;

Texture ColorTexture;

sampler ColorTextureSampler = sampler_state { texture = <ColorTexture> ; magfilter = NONE; minfilter = NONE; mipfilter = NONE; AddressU = wrap; AddressV = wrap; };

Texture PaletteTexture;

sampler PaletteTextureSampler = sampler_state { texture = <PaletteTexture> ; magfilter = NONE; minfilter = NONE; mipfilter = NONE; AddressU = wrap; AddressV = wrap; };

struct a2v
{
	float4 Position   	: POSITION0;
	float2 TexCoords  	: TEXCOORD0;
};

struct v2p
{
	float4 Position		: POSITION0;
	float2 TexCoords   	: TEXCOORD0;
};

struct p2f
{
	float4 Color 		: COLOR0;
};

void vs( in a2v IN, out v2p OUT )
{
	OUT.Position = mul( IN.Position, WorldViewProj );
	OUT.TexCoords = IN.TexCoords;
}

void ps( in v2p IN, out p2f OUT )
{
	float4 TextureColor = tex2D( ColorTextureSampler, IN.TexCoords );
	OUT.Color = tex1D( PaletteTextureSampler, TextureColor.r );
	OUT.Color.a = OUT.Color.r;
	OUT.Color.r = OUT.Color.b;
	OUT.Color.b = OUT.Color.a;
}

technique PaletteTechnique
{
    pass p0
    {
        vertexshader = compile vs_1_1 vs();
		pixelshader = compile ps_2_0 ps();
    }
}