// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 05.03.2007 17:34
// Last modified: 31.07.2007 04:43

// Simple shader for billboard effects
float4x4 worldViewProj : WorldViewProjection;
float4x4 world : World;
float4x4 viewInverse : ViewInverse;

float3 lightDir : Direction
<
	string Object = "DirectionalLight";
	string Space = "World";
> = { 1, 0, 0 };

float4 ambientColor : Ambient = { 0.2f, 0.2f, 0.2f, 1.0f };
float4 diffuseColor : Diffuse = { 0.5f, 0.5f, 0.5f, 1.0f };
float4 specularColor : Specular = { 1.0, 1.0, 1.0f, 1.0f };
float specularPower : SpecularPower = 24.0f;

texture diffuseTexture : Diffuse
<
	string ResourceName = "marble.dds";
>;
sampler DiffuseTextureSampler = sampler_state
{
	Texture = <diffuseTexture>;
	MinFilter=linear;
	MagFilter=linear;
	MipFilter=linear;
};

struct VertexInput
{
	float3 pos : POSITION;
	float4 color : COLOR0;
	float2 texCoord : TEXCOORD0;
};

struct VertexOutput
{
	float4 pos : POSITION;
	float4 color : COLOR0;
	float2 texCoord : TEXCOORD0;
};

// Vertex shader
VertexOutput VS_SimpleEffect(VertexInput In)
{
	VertexOutput Out = (VertexOutput)0;
	float4 pos = float4(In.pos, 1); 
	Out.pos = mul(pos, worldViewProj);
	Out.color = In.color;
	Out.texCoord = In.texCoord;
	
	return Out;
} // VS_SimpleEffect(In)

// Pixel shader
float4 PS_SimpleEffect(VertexOutput In) : COLOR
{
	float4 textureColor = tex2D(DiffuseTextureSampler, In.texCoord);
	return textureColor * In.color;
} // PS_SimpleEffect(In)

technique SimpleEffect
{
	pass P0
	{
		VertexShader = compile vs_2_0 VS_SimpleEffect();
		PixelShader = compile ps_2_0 PS_SimpleEffect();
	} // pass P0
} // SimpleEffect
