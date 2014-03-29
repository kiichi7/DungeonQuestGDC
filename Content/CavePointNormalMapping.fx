// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 05.03.2007 20:26
// Last modified: 31.07.2007 04:43

string description = "Normal mapping shaders for six dynamic point light";

// Variables that are provided by the application.
// Support for UIWidget is also added for FXComposer and 3DS Max :)

// Default variables, supported by the engine (may not be used here)
// If you don't need any global variable, just comment it out, this way
// the game engine does not have to set it!
float4x4 viewProj              : ViewProjection;
float3   cameraPos             : CameraPosition;

#define NUMBER_OF_LIGHTS 6
#define NUMBER_OF_LIGHTS_PS20 3
float3 lightPositions[NUMBER_OF_LIGHTS] : POSITION
<
	string UIName = "Light Positions";
	string Object = "PointLight";
	string Space = "World";
> =
{
	{-1.0f, +1.0f, -1.0f},
	{-1.0f, -1.0f, -1.0f},
	{-1.0f, +1.0f, +1.0f},
	{-1.0f, -1.0f, +1.0f},
	{+1.0f, +1.0f, -1.0f},
	{+1.0f, -1.0f, -1.0f},
};

float detailFactor
<
	string UIName = "Detail Factor";
	string UIWidget = "slider";
	float UIMin = 2.0;
	float UIMax = 28.0;
	float UIStep = 0.1;
> = 60.0;

float fresnelBias
<
	string UIName = "Fresnel Bias";
	string UIWidget = "slider";
	float UIMin = 0.01;
	float UIMax = 0.5;
	float UIStep = 0.01;
> = 0.2;

float fresnelPower
<
	string UIName = "Fresnel Power";
	string UIWidget = "slider";
	float UIMin = 0.01;
	float UIMax = 10.0;
	float UIStep = 0.01;
> = 4.0;

float parallaxAmount
<
	string UIName = "Parallax Amount";
	string UIWidget = "slider";
	float UIMin = 0.0;
	float UIMax = 0.5;
	float UIStep = 0.001;
> = 0.0005f;//0.004;

// The ambient, diffuse and specular colors are pre-multiplied with the light color!
float4 ambientColor : Ambient
<
	string UIName = "Ambient Color";
	string Space = "material";
> = {0.0f, 0.0f, 0.0f, 1.0f};

float4 diffuseColor : Diffuse
<
	string UIName = "Diffuse Color";
	string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float4 specularColor : Specular
<
	string UIName = "Specular Color";
	string Space = "material";
> = {1.0f, 1.0f, 1.0f, 1.0f};

float specularPower : SpecularPower
<
	string UIName = "Specular Power";
	string UIWidget = "slider";
	float UIMin = 1.0;
	float UIMax = 128.0;
	float UIStep = 1.0;
> = 18.0;

float fogStart = 15.0f;
float fogEnd = 60.0f;
float4 fogColor = {0.34f, 0.34f, 0.24f, 1.0f};

// Texture and samplers
texture diffuseTexture : Diffuse
<
	string UIName = "Diffuse Texture";
	string ResourceName = "Cave.dds";
>;
sampler diffuseTextureSampler = sampler_state
{
	Texture = <diffuseTexture>;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture detailGroundDiffuseTexture : Diffuse
<
	string UIName = "Detail Ground Diffuse Texture";
	string ResourceName = "CaveDetailGround.dds";
>;
sampler detailGroundDiffuseTextureSampler = sampler_state
{
	Texture = <detailGroundDiffuseTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
};

texture detailGroundNormalTexture : Diffuse
<
	string UIName = "Detail Ground Normal Texture";
	string ResourceName = "CaveDetailGroundNormal.dds";
>;
sampler detailGroundNormalTextureSampler = sampler_state
{
	Texture = <detailGroundNormalTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
};

texture detailGroundHeightTexture : Diffuse
<
	string UIName = "Detail Ground Height Texture";
	string ResourceName = "CaveDetailGroundHeight.dds";
>;
sampler detailGroundHeightTextureSampler = sampler_state
{
	Texture = <detailGroundHeightTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture detailWallDiffuseTexture : Diffuse
<
	string UIName = "Detail Wall Diffuse Texture";
	string ResourceName = "CaveDetailWall.dds";
>;
sampler detailWallDiffuseTextureSampler = sampler_state
{
	Texture = <detailWallDiffuseTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
};

texture detailWallNormalTexture : Diffuse
<
	string UIName = "Detail Wall Normal Texture";
	string ResourceName = "CaveDetailWallNormal.dds";
>;
sampler detailWallNormalTextureSampler = sampler_state
{
	Texture = <detailWallNormalTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	MinFilter = Anisotropic;
	MagFilter = Anisotropic;
	MipFilter = Linear;
};

texture detailWallHeightTexture : Diffuse
<
	string UIName = "Detail Wall Height Texture";
	string ResourceName = "CaveDetailWallHeight.dds";
>;
sampler detailWallHeightTextureSampler = sampler_state
{
	Texture = <detailWallHeightTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
	float3 pos      : POSITION;
	float3 normal   : NORMAL;
	float2 texCoord : TEXCOORD0;
	float3 tangent	: TANGENT;
};

// vertex shader output structure
struct VertexOutput
{
	float4 pos          : POSITION;
	float2 texCoord     : TEXCOORD0;
	float3 viewVec      : TEXCOORD1;
	// Light directions + attentuation in w
	float4 lightVecs[NUMBER_OF_LIGHTS] : TEXCOORD2;
	float fogFactor     : COLOR0;
};

struct VertexOutput20
{
	float4 pos          : POSITION;
	float2 texCoord     : TEXCOORD0;
	float3 viewVec      : TEXCOORD1;
	// Light directions + attentuation in w
	float4 lightVecs[NUMBER_OF_LIGHTS_PS20] : TEXCOORD2;
	float fogFactor     : COLOR0;
};

struct VertexOutputPS20
{
	//float4 pos          : POSITION;
	float2 texCoord     : TEXCOORD0;
	float3 viewVec      : TEXCOORD1;
	// Light directions + attentuation in w
	float4 lightVecs[NUMBER_OF_LIGHTS_PS20] : TEXCOORD2;
	float fogFactor     : COLOR0;
};

struct VertexOutputPS30
{
	//float4 pos          : POSITION;
	float2 texCoord     : TEXCOORD0;
	float3 viewVec      : TEXCOORD1;
	// Light directions + attentuation in w
	float4 lightVecs[NUMBER_OF_LIGHTS] : TEXCOORD2;
	float fogFactor     : COLOR0;
};

//----------------------------------------------------

// Common functions
float4 TransformPosition(float3 pos)
{
	return mul(float4(pos.xyz, 1), viewProj);
} // TransformPosition(.)

float CalcLightAttenuation(float dist)
{
	// Use HDR lights with 1.65
	//return 1.65f * saturate(1 / (lightAttenuation.x +
	//	lightAttenuation.y * dist + lightAttenuation.z * dist * dist));
		
	// Just use distance based.
	//return 1.5*saturate(1.33f/((dist/2)*(dist/2)));
	
	// Get distance between 0-1
	float distance1 = dist/7;
	if (distance1 > 1)
		distance1 = 1;
		
	// Max 1-distance
	distance1 = 1-distance1;
	
	// Get distance between 0-1
	float distance2 = dist/18;//14;
	if (distance2 > 1)
		distance2 = 1;
		
	// Max 1-distance
	distance2 = 1-distance2;

	// Use linear and exponential formular
	return 1.25f *
		(distance1 + distance2 * distance2);
} // CalcLightAttenuation(.)

float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace;
	//Note: We don't need world matrices here because the level is always Identity
	worldToTangentSpace[0] = tangent;//cross(normal, tangent);//tangent;
	worldToTangentSpace[1] = cross(tangent, normal);//cross(normal, tangent);
	worldToTangentSpace[2] = normal;
	return worldToTangentSpace;
} // ComputeTangentMatrix(..)

float3 CalcFogFactor(float distanceToCamera)
{
	// Find out how much fog we got here (0-1)
	float zfactor = saturate((distanceToCamera - fogStart) / (fogEnd - fogStart));
	if (zfactor < 0.0)
		zfactor = 0.0;
	return zfactor;
} // CalcFogFactor(float distanceToCamera)

//----------------------------------------------------

// Vertex shader function
VertexOutput20 VS_DiffuseSpecular20(VertexInput In)
{
	VertexOutput20 Out = (VertexOutput20) 0; 
	Out.pos = TransformPosition(In.pos);
	// Duplicate texture coordinates for diffuse and normal maps
	Out.texCoord = In.texCoord;

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal);

	float3 worldEyePos = cameraPos;
    // Since we have no world matrix, this code is a little simpler
	float3 worldVertPos = In.pos;

	// Determine the distance from the light to the vertex and the direction
	for (int num=0; num<NUMBER_OF_LIGHTS_PS20; num++)
	{
		float3 lightDir =
			//worldVertPos - lightPositions[num];
			lightPositions[num] - worldVertPos;
			//lightPositions - worldVertPos;
		float dist = length(lightDir);
		// Normalize, but store light attenuation too (in w)
		lightDir = lightDir / dist;
		Out.lightVecs[num] = float4(
			//worldToTangentSpace[0],
			//lightDir,
			//float3(1, 1, 1),
			normalize(mul(worldToTangentSpace, lightDir)),
			CalcLightAttenuation(dist));
			//dist);
	} // for
		
	Out.viewVec =
		normalize(mul(worldToTangentSpace, worldEyePos - worldVertPos));
	
	Out.fogFactor = CalcFogFactor(length(worldEyePos-worldVertPos));

	// And pass everything to the pixel shader
	return Out;
} // VS_DiffuseSpecular20(.)

// Pixel shader function
float4 PS_DiffuseSpecular20(VertexOutputPS20 In) : COLOR
{
	// Normalize viewVec
	float3 viewVector = normalize(In.viewVec);
	
	// Grab the detail blend factor (we have to do that before the parallax calculation
	// because for that 
	float detailBlendFactor = 1-tex2D(diffuseTextureSampler, In.texCoord).a;
	detailBlendFactor = (detailBlendFactor-0.5f)*2.0f+0.5f;
	
	// Calculate parallax into texCoord, use both detail height textures!
	float heightTexture = lerp(
		tex2D(detailGroundHeightTextureSampler, In.texCoord*detailFactor).r,
		tex2D(detailWallHeightTextureSampler, In.texCoord*detailFactor).r,
		detailBlendFactor);
	
	// Use parallax offset for all other texture calls
	float2 texCoord = In.texCoord +
		(heightTexture*parallaxAmount - parallaxAmount*0.5f)*viewVector;

	// Grab texture data (just the ambient occlusion and some color data)
	float3 diffuseTexture = tex2D(diffuseTextureSampler, texCoord).rgb;
	
	// Use the detailFactor for all the detail samplers from here on
	texCoord = texCoord*detailFactor;
	float3 detailTexture = lerp(
		tex2D(detailGroundDiffuseTextureSampler, texCoord).rgb,
		tex2D(detailWallDiffuseTextureSampler, texCoord).rgb,
		detailBlendFactor);
	float3 normalVector = normalize(lerp(
		tex2D(detailGroundNormalTextureSampler, texCoord).agb * 2.0f - 1.0f,
		tex2D(detailWallNormalTextureSampler, texCoord).agb * 2.0f - 1.0f,
		detailBlendFactor));
		
	diffuseTexture = (diffuseTexture-0.5f)*2.0f+0.5f;

	// Use all lights for the light calculation
	float totalBump = 0;
	float totalSpec = 0;
	for (int num=0; num<NUMBER_OF_LIGHTS_PS20; num++)
	{
		float3 lightVector = normalize(In.lightVecs[num].xyz);
		// Compute the angle to the light
		float attentuation = In.lightVecs[num].w;
		// If this is the first light (player flare), make it much weaker
		//obs: if (num == 0)
		//	attentuation /= 2.0f;
		float bump = saturate(dot(normalVector, lightVector));// * attentuation;
		totalBump += bump * attentuation;
	
		// Specular factor
		float3 reflect = normalize(2 * bump * normalVector - lightVector);
		float spec = pow(saturate(dot(reflect, viewVector)), specularPower);
		totalSpec += spec * attentuation;
	} // for (int num=0; num<NUMBER_OF_LIGHTS; num++)

	// Mix the detail texture with the diffuse texture here.
	diffuseTexture = detailTexture * diffuseTexture;

//return float4(totalBump, 0, 0, 1);
	// Just use the rgb color, make sure alpha is always 1
	float4 ret = float4(diffuseTexture * (ambientColor +
		totalBump * (diffuseColor + 2 * totalSpec * specularColor)), 1);

	// Also add the fog, which was already calculated in the vertex shader
	// (which is obviously faster and we already have a long slow pixel shader)
	// Use full color if there is no fog, and the fog color for fog
	return lerp(ret, fogColor, In.fogFactor);
} // PS_DiffuseSpecular20(.)

// Techniques
technique DiffuseSpecular20
{
	pass P0
	{
		VertexShader = compile vs_2_0 VS_DiffuseSpecular20();
		PixelShader  = compile ps_2_a PS_DiffuseSpecular20();
	} // pass P0
} // DiffuseSpecular20

//----------------------------------------------------

// Vertex shader function
VertexOutput VS_DiffuseSpecular30(VertexInput In)
{
	VertexOutput Out = (VertexOutput) 0; 
	Out.pos = TransformPosition(In.pos);
	// Duplicate texture coordinates for diffuse and normal maps
	Out.texCoord = In.texCoord;

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal);

	float3 worldEyePos = cameraPos;
    // Since we have no world matrix, this code is a little simpler
	float3 worldVertPos = In.pos;

	// Determine the distance from the light to the vertex and the direction
	for (int num=0; num<NUMBER_OF_LIGHTS; num++)
	{
		float3 lightDir =
			//worldVertPos - lightPositions[num];
			lightPositions[num] - worldVertPos;
			//lightPositions - worldVertPos;
		float dist = length(lightDir);
		// Normalize, but store light attenuation too (in w)
		lightDir = lightDir / dist;
		Out.lightVecs[num] = float4(
			//worldToTangentSpace[0],
			//lightDir,
			//float3(1, 1, 1),
			normalize(mul(worldToTangentSpace, lightDir)),
			CalcLightAttenuation(dist));
			//dist);
	} // for
		
	Out.viewVec =
		normalize(mul(worldToTangentSpace, worldEyePos - worldVertPos));
	
	Out.fogFactor = CalcFogFactor(length(worldEyePos-worldVertPos));

	// And pass everything to the pixel shader
	return Out;
} // VS_DiffuseSpecular30(.)

// Pixel shader function
float4 PS_DiffuseSpecular30(VertexOutputPS30 In) : COLOR
{
	// Normalize viewVec
	float3 viewVector = normalize(In.viewVec);
	
	// Grab the detail blend factor (we have to do that before the parallax calculation
	// because for that 
	float detailBlendFactor = 1-tex2D(diffuseTextureSampler, In.texCoord).a;
	detailBlendFactor = (detailBlendFactor-0.5f)*2.0f+0.5f;
	
	// Calculate parallax into texCoord, use both detail height textures!
	float heightTexture = lerp(
		tex2D(detailGroundHeightTextureSampler, In.texCoord*detailFactor).r,
		tex2D(detailWallHeightTextureSampler, In.texCoord*detailFactor).r,
		detailBlendFactor);
	
	// Use parallax offset for all other texture calls
	float2 texCoord = In.texCoord +
		(heightTexture*parallaxAmount - parallaxAmount*0.5f)*viewVector;

	// Grab texture data (just the ambient occlusion and some color data)
	float3 diffuseTexture = tex2D(diffuseTextureSampler, texCoord).rgb;
	
	// Use the detailFactor for all the detail samplers from here on
	texCoord = texCoord*detailFactor;
	float3 detailTexture = lerp(
		tex2D(detailGroundDiffuseTextureSampler, texCoord).rgb,
		tex2D(detailWallDiffuseTextureSampler, texCoord).rgb,
		detailBlendFactor);
	float3 normalVector = normalize(lerp(
		tex2D(detailGroundNormalTextureSampler, texCoord).agb * 2.0f - 1.0f,
		tex2D(detailWallNormalTextureSampler, texCoord).agb * 2.0f - 1.0f,
		detailBlendFactor));
		
	diffuseTexture = (diffuseTexture-0.5f)*2.0f+0.5f;

	// Use all lights for the light calculation
	float totalBump = 0;
	float totalSpec = 0;
	for (int num=0; num<NUMBER_OF_LIGHTS; num++)
	{
		float3 lightVector = normalize(In.lightVecs[num].xyz);
		// Compute the angle to the light
		float attentuation = In.lightVecs[num].w;
		// If this is the first light (player flare), make it much weaker
		//obs: if (num == 0)
		//	attentuation /= 2.0f;
		float bump = saturate(dot(normalVector, lightVector));// * attentuation;
		totalBump += bump * attentuation;
	
		// Specular factor
		float3 reflect = normalize(2 * bump * normalVector - lightVector);
		float spec = pow(saturate(dot(reflect, viewVector)), specularPower);
		totalSpec += spec * attentuation;
	} // for (int num=0; num<NUMBER_OF_LIGHTS; num++)

	// Mix the detail texture with the diffuse texture here.
	diffuseTexture = detailTexture * diffuseTexture;

//return float4(totalBump, 0, 0, 1);
	// Just use the rgb color, make sure alpha is always 1
	float4 ret = float4(diffuseTexture * (ambientColor +
		totalBump * (diffuseColor + 2 * totalSpec * specularColor)), 1);

	// Also add the fog, which was already calculated in the vertex shader
	// (which is obviously faster and we already have a long slow pixel shader)
	// Use full color if there is no fog, and the fog color for fog
	return lerp(ret, fogColor, In.fogFactor);
} // PS_DiffuseSpecular30(.)

// Techniques
technique DiffuseSpecular30
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS_DiffuseSpecular30();
		PixelShader  = compile ps_3_0 PS_DiffuseSpecular30();
	} // pass P0
} // DiffuseSpecular30
