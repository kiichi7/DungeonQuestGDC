// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 05.03.2007 20:26
// Last modified: 31.07.2007 04:43

// Simple shader, based on Rocket Commander, Tutorial 06
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
	string ResourceName = "marble.jpg";
>;
sampler diffuseTextureSampler = sampler_state
{
	Texture = <diffuseTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	AddressW  = Wrap;//Clamp;
	MinFilter=linear;
	MagFilter=linear;
	MipFilter=linear;
};

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
	float3 pos      : POSITION;
	float2 texCoord : TEXCOORD0;
	float3 normal   : NORMAL;
	float3 tangent	: TANGENT;
};

// Vertex output structure
struct VertexOutput_SpecularPerPixel
{
	float4 pos      : POSITION;
	float2 texCoord	: TEXCOORD0;
	float3 normal   : TEXCOORD1;
	float3 halfVec	: TEXCOORD2;
};

// Common functions
float4 TransformPosition(float3 pos)//float4 pos)
{
	return mul(float4(pos.xyz, 1), worldViewProj);
} // TransformPosition(.)

float3 GetWorldPos(float3 pos)
{
	return mul(float4(pos, 1), world).xyz;
} // GetWorldPos(.)

float3 GetCameraPos()
{
	return viewInverse[3].xyz;
} // GetCameraPos()

float3 CalcNormalVector(float3 nor)
{
	//return normalize(mul(nor, world));//worldInverseTranspose));
	return normalize(mul(nor, (float3x3)world));
} // CalcNormalVector(.)

// Vertex output structure
struct VertexOutput_Diffuse
{
	float4 pos      : POSITION;
	float2 texCoord	: TEXCOORD0;
	float3 normal   : TEXCOORD1;
};

// Very simple diffuse mapping shader
VertexOutput_Diffuse VS_Diffuse(VertexInput In)
{
	VertexOutput_Diffuse Out = (VertexOutput_Diffuse)0;      
	Out.pos = TransformPosition(In.pos);
	Out.texCoord = In.texCoord;

	// Calc normal vector
	Out.normal = 0.5 + 0.5 * CalcNormalVector(In.normal);
	
	// Rest of the calculation is done in pixel shader
	return Out;
} // VS_Diffuse

// Pixel shader
float4 PS_Diffuse(VertexOutput_Diffuse In) : COLOR
{
	float4 diffuseTexture = tex2D(diffuseTextureSampler, In.texCoord);
	// Convert colors back to vectors. Without normalization it is
	// a bit faster (2 instructions less), but not as correct!
	float3 normal = 2.0 * (In.normal-0.5);

	// Diffuse factor
	float diff = saturate(dot(normal, lightDir));//normalize(-lightDir.xyz)));

	// Output the color
	float4 diffAmbColor = ambientColor + diff * diffuseColor;
	return diffuseTexture * diffAmbColor;
} // PS_Diffuse

technique Diffuse
{
	pass P0
	{
		VertexShader = compile vs_1_1 VS_Diffuse();
		PixelShader  = compile ps_1_1 PS_Diffuse();
	} // pass P0
} // technique Diffuse

// No need to write new shader for ps20
technique Diffuse20
{
	pass P0
	{
		VertexShader = compile vs_2_0 VS_Diffuse();
		PixelShader  = compile ps_2_0 PS_Diffuse();
	} // pass P0
} // Diffuse20

// -----------------------------------------------------

// Vertex shader for ps_1_1 (specular is just not as strong)
VertexOutput_SpecularPerPixel VS_SpecularPerPixel(VertexInput In)
{
	VertexOutput_SpecularPerPixel Out = (VertexOutput_SpecularPerPixel)0;      
	Out.pos = TransformPosition(In.pos);
	Out.texCoord = In.texCoord;

	// Determine the eye vector
	float3 worldEyePos = GetCameraPos();
	float3 worldVertPos = GetWorldPos(In.pos);

	// Calc normal vector
	Out.normal = 0.5 + 0.5 * CalcNormalVector(In.normal);
	// Eye vector
	float3 eyeVec = normalize(worldEyePos - worldVertPos);
	// Half angle vector
	Out.halfVec = 0.5 + 0.5 *
		normalize(eyeVec + lightDir);//normalize(-lightDir.xyz));

	// Rest of the calculation is done in pixel shader
	return Out;
} // VS_SpecularPerPixel

// Pixel shader
float4 PS_SpecularPerPixel(VertexOutput_SpecularPerPixel In) : COLOR
{
	// This pixel shader requires ps_2_0, it is possible with a lot of work
	// to emulate specular lighting per pixel, but its hard and complicated:
	// http://www.gamasutra.com/features/20020801/beaudoin_04.htm
	// I don't think it looks any good anyway, so why bother?

	float4 diffuseTexture = tex2D(diffuseTextureSampler, In.texCoord);
	// Convert colors back to vectors. Without normalization it is
	// a bit faster (2 instructions less), but not as correct!
	float3 normal = 2.0 * (In.normal-0.5);
	float3 halfVec = 2.0 * (In.halfVec-0.5);
		//(2.0f * texCUBE(NormalizeCubeTextureSampler, In.halfVec))-1.0f;

	// Diffuse factor
	float diff = saturate(dot(normal, lightDir));//normalize(-lightDir.xyz)));
	// Specular factor
	float spec = saturate(dot(normal, halfVec));//, 32);//fixed, can't do more! shininess);
	//max. possible pow fake with mults here: spec = pow(spec, 8);
	//same as: spec = spec*spec*spec*spec*spec*spec*spec*spec;

	// (saturate(4*(dot(N,H)^2-0.75))^2*2 is a close approximation to pow(dot(N,H), 16).
	// see: http://personal.telefonica.terra.es/web/codegarrofi/perPixelLighting/perPixelLighting.htm
	// I use something like (saturate(4*(dot(N,H)^4-0.75))^2*2 for approx. pow(dot(N,H), 32)
	//spec = pow(spec, 16);
	spec = pow(saturate(4*(pow(spec, 2)-0.75)), 2);//*2;

	// Output the color
	float4 diffAmbColor = ambientColor + diff * diffuseColor;
	return diffuseTexture *
		diffAmbColor +
		spec * specularColor * diffuseTexture.a;
} // PS_SpecularPerPixel

technique SpecularPerPixel
{
	pass P0
	{
		VertexShader = compile vs_1_1 VS_SpecularPerPixel();
		PixelShader  = compile ps_1_1 PS_SpecularPerPixel();
	} // pass P0
} // technique SpecularPerPixel

//-------------------------------------

// Vertex shader
VertexOutput_SpecularPerPixel VS_SpecularPerPixel20(VertexInput In)
{
	VertexOutput_SpecularPerPixel Out = (VertexOutput_SpecularPerPixel)0;
	float4 pos = float4(In.pos, 1); 
	Out.pos = mul(pos, worldViewProj);
	Out.texCoord = In.texCoord;
	Out.normal = mul(In.normal, world);
	// Eye pos
	float3 eyePos = viewInverse[3];
	// World pos
	float3 worldPos = mul(pos, world);
	// Eye vector
	float3 eyeVector = normalize(eyePos-worldPos);
	// Half vector
	Out.halfVec = normalize(eyeVector+lightDir);
	
	return Out;
} // VS_SpecularPerPixel20(In)

// Pixel shader
float4 PS_SpecularPerPixel20(VertexOutput_SpecularPerPixel In) : COLOR
{
	float4 textureColor = tex2D(diffuseTextureSampler, In.texCoord);
	float3 normal = normalize(In.normal);
	float brightness = dot(normal, lightDir);
	float specular = pow(dot(normal, In.halfVec), specularPower);
	return textureColor *
		(ambientColor +
		brightness * diffuseColor) +
		specular * specularColor;
} // PS_SpecularPerPixel20(In)

technique SpecularPerPixel20
{
	pass P0
	{
		VertexShader = compile vs_2_0 VS_SpecularPerPixel20();
		PixelShader = compile ps_2_0 PS_SpecularPerPixel20();
	} // pass P0
} // SpecularPerPixel

//---------------------------------------------------

// vertex shader output structure
struct VertexOutput_ShadowCar20
{
	float4 pos          : POSITION;
	float2 texCoord     : TEXCOORD0;
};

// Special shader for car rendering, which allows to change the car color!
float4 shadowCarColor
<
	string UIName = "Shadow Car Color";
	string Space = "material";
> = {1.0f, 1.0f, 1.0f, 0.125f};//0.09f};//0.06f};

// Vertex shader function
VertexOutput_ShadowCar20
	VS_ShadowCar20(VertexInput In)
{
	VertexOutput_ShadowCar20 Out =
		(VertexOutput_ShadowCar20) 0;
	
	Out.pos = TransformPosition(In.pos);
	
	// Copy texture coordinates for diffuse and normal maps
	Out.texCoord = In.texCoord;

	// And pass everything to the pixel shader
	return Out;
} // VS_ShadowCar20(.)

// Pixel shader function
float4 PS_ShadowCar20(
  VertexOutput_ShadowCar20 In) : COLOR
{
	return shadowCarColor;
	// Grab texture data
	//float4 diffuseTexture = tex2D(diffuseTextureSampler, In.texCoord);
	// Add a little color from the car
	//ret.rgb += diffuseTexture / 10;
	//return ret;
} // PS_ShadowCar20(.)

technique ShadowCar
{
	pass P0
	{
		//ZEnable = true;
		ZWriteEnable = false;//true;
		//CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = One;
		VertexShader = compile vs_1_1 VS_ShadowCar20();
		PixelShader  = compile ps_1_1 PS_ShadowCar20();
	} // pass P0
} // ShadowCar20

technique ShadowCar20
{
	pass P0
	{
		//ZEnable = true;
		ZWriteEnable = false;//true;
		//CullMode = None;
		AlphaBlendEnable = true;
		SrcBlend = SrcAlpha;
		DestBlend = One;
		VertexShader = compile vs_1_1 VS_ShadowCar20();
		PixelShader  = compile ps_2_0 PS_ShadowCar20();
	} // pass P0
} // ShadowCar20
