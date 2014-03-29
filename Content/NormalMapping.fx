// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 09.03.2007 20:26
// Last modified: 31.07.2007 04:43

// Note: For testing FX Composer from NVIDIA can be used
string description = "Normal mapping shaders for XnaGraphicEngine";

// Default variables, supported by the engine (may not be used here)
// If you don't need any global variable, just comment it out, this way
// the game engine does not have to set it!
float4x4 viewProj         : ViewProjection;
float4x4 world            : World;
float3   cameraPos        : CameraPosition;

#define NUMBER_OF_LIGHTS 3 // Faster with 3 lights and looks pretty much the same
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
	//{-1.0f, -1.0f, +1.0f},
	//{+1.0f, +1.0f, -1.0f},
	//{+1.0f, -1.0f, -1.0f},
};

// The ambient, diffuse and specular colors are pre-multiplied with the light color!
float4 ambientColor : Ambient
<
	string UIName = "Ambient Color";
	string Space = "material";
> = {0.1f, 0.1f, 0.1f, 1.0f};
//> = {0.25f, 0.25f, 0.25f, 1.0f};

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
> = 16.0;

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

float fogStart = 15.0f;
float fogEnd = 60.0f;
float4 fogColor = {0.34f, 0.34f, 0.24f, 1.0f};

// Texture and samplers
texture diffuseTexture : Diffuse
<
	string UIName = "Diffuse Texture";
	string ResourceName = "asteroid4.dds";
>;
sampler diffuseTextureSampler = sampler_state
{
	Texture = <diffuseTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	AddressW  = Wrap;//Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture normalTexture : Diffuse
<
	string UIName = "Normal Texture";
	string ResourceName = "asteroid4Normal.dds";
>;
sampler normalTextureSampler = sampler_state
{
	Texture = <normalTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	AddressW  = Wrap;//Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

texture specularTexture : Diffuse
<
	string UIName = "Specular Texture";
	string ResourceName = "asteroid4Normal.dds";
>;
sampler specularTextureSampler = sampler_state
{
	Texture = <specularTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	AddressW  = Wrap;//Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};

//----------------------------------------------------

// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
	float3 pos      : POSITION;
	float2 texCoord : TEXCOORD0;
	float3 normal   : NORMAL;
	float3 tangent	: TANGENT;
};

//----------------------------------------------------

// Common functions
float4 TransformPosition(float3 pos)//float4 pos)
{
	return mul(mul(float4(pos.xyz, 1), world), viewProj);
} // TransformPosition(.)

float3 GetWorldPos(float3 pos)
{
	return mul(float4(pos, 1), world).xyz;
} // GetWorldPos(.)

float3 CalcNormalVector(float3 nor)
{
	return normalize(mul(nor, (float3x3)world));//worldInverseTranspose));
} // CalcNormalVector(.)
	
float3x3 ComputeTangentMatrix(float3 tangent, float3 normal)
{
	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace;
	worldToTangentSpace[0] = mul(tangent, world);
	worldToTangentSpace[1] = mul(cross(tangent, normal), world);
	worldToTangentSpace[2] = mul(normal, world);
	return worldToTangentSpace;
} // ComputeTangentMatrix(..)

float CalcLightAttenuation(float dist)
{
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
	return 1.25f * //1.65f *
		(distance1 + distance2 * distance2);
} // CalcLightAttenuation(.)

float CalcFogFactor(float distanceToCamera)
{
	// Find out how much fog we got here (0-1)
	float zfactor = saturate((distanceToCamera - fogStart) / (fogEnd - fogStart));
	if (zfactor < 0.0)
		zfactor = 0.0;
	return zfactor;
} // CalcFogFactor(float distanceToCamera)

//----------------------------------------------------

// vertex shader output structure
struct VertexOutput_Specular20
{
	float4 pos          : POSITION;
	float2 texCoord     : TEXCOORD0;
	float3 viewVec      : TEXCOORD1;
	// Light directions + attentuation in w
	float4 lightVecs[NUMBER_OF_LIGHTS] : TEXCOORD2;
	float fogFactor     : COLOR0;
};

struct VertexOutput_Specular30
{
	//float4 pos          : POSITION;
	float2 texCoord     : TEXCOORD0;
	float3 viewVec      : TEXCOORD1;
	// Light directions + attentuation in w
	float4 lightVecs[NUMBER_OF_LIGHTS] : TEXCOORD2;
	float fogFactor     : COLOR0;
};


// Vertex shader function
VertexOutput_Specular20 VS_Specular20(VertexInput In)
{
	VertexOutput_Specular20 Out = (VertexOutput_Specular20) 0; 
	Out.pos = TransformPosition(In.pos);
	// We can duplicate texture coordinates for diffuse and normal map
	// in the pixel shader 2.0. For Pixel shader 1.1 we have to do that
	// in the vertex shader instead and pass it over.
	Out.texCoord = In.texCoord;// + float2(0, -time/1.5f);

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal);

	float3 worldEyePos = cameraPos;
	float3 worldVertPos = GetWorldPos(In.pos);

	// Determine the distance from the light to the vertex and the direction
	for (int num=0; num<NUMBER_OF_LIGHTS; num++)
	{
		float3 lightDir = lightPositions[num] - worldVertPos;
		float dist = length(lightDir);
		// Normalize, but store light attenuation too (in w)
		lightDir = lightDir / dist;
		Out.lightVecs[num] = float4(
			normalize(mul(worldToTangentSpace, lightDir)),
			CalcLightAttenuation(dist));
	} // for
		
	Out.viewVec =
		normalize(mul(worldToTangentSpace, worldEyePos - worldVertPos));
	
	Out.fogFactor = CalcFogFactor(length(worldEyePos-worldVertPos));

	// And pass everything to the pixel shader
	return Out;
} // VS_Specular20(.)

// Pixel shader function
float4 PS_Specular20(VertexOutput_Specular20 In) : COLOR
{
	// Grab texture data
	float4 diffuseTexture = tex2D(diffuseTextureSampler, In.texCoord);
	float3 normalVector = (2.0 * tex2D(normalTextureSampler, In.texCoord).agb) - 1.0;
	// Normalize normal to fix blocky errors
	normalVector = normalize(normalVector);
	float specularTexture = tex2D(specularTextureSampler, In.texCoord).r;

	// Use all lights for the light calculation
	float totalBump = 0;
	float totalSpec = 0;
	float3 viewVector = normalize(In.viewVec);
	//for (int num=0; num<1; num++)
	for (int num=0; num<NUMBER_OF_LIGHTS; num++)
	{
		float3 lightVector = normalize(In.lightVecs[num].xyz);
		// Compute the angle to the light
		float attentuation = In.lightVecs[num].w;
		// If this is the first light (player flare), make it much weaker
		//obs: if (num == 0)
		//	attentuation /= 2.0f;
		float bump = saturate(dot(normalVector, lightVector)) * attentuation;
		totalBump += bump;
	
		// Specular factor
		float3 reflect = normalize(2 * bump * normalVector - lightVector);
		float spec = pow(saturate(dot(reflect, viewVector)), specularPower);
		totalSpec += spec;
	} // for (int num=0; num<NUMBER_OF_LIGHTS; num++)

	// Reflection
	half3 R = reflect(-viewVector, normalVector);
	R = float3(R.x, R.z, R.y);
	
	// Fresnel
	float3 E = -viewVector;
	float facing = 1.0 - max(dot(E, -normalVector), 0);
	float fresnel = fresnelBias + (1.0-fresnelBias)*pow(facing, fresnelPower);
	totalSpec *= 2*fresnel;
	
	float4 ambDiffColor = ambientColor + totalBump * diffuseColor;
	float4 ret = diffuseTexture * ambDiffColor +
		totalBump * totalSpec * specularColor * specularTexture;

	// Also add the fog, which was already calculated in the vertex shader
	// (which is obviously faster and we already have a long slow pixel shader)
	// Use full color if there is no fog, and the fog color for fog
	return lerp(ret, fogColor, In.fogFactor);		
} // PS_Specular20(.)

// Techniques
technique Specular20
{
	pass P0
	{
		VertexShader = compile vs_2_0 VS_Specular20();
		PixelShader  = compile ps_2_a PS_Specular20();
	} // pass P0
} // Specular20

/*not required anymore because we just use 3 lights!
// Vertex shader function
VertexOutput_Specular20 VS_Specular30(VertexInput In)
{
	VertexOutput_Specular20 Out = (VertexOutput_Specular20) 0; 
	Out.pos = TransformPosition(In.pos);
	// We can duplicate texture coordinates for diffuse and normal map
	// in the pixel shader 2.0. For Pixel shader 1.1 we have to do that
	// in the vertex shader instead and pass it over.
	Out.texCoord = In.texCoord;// + float2(0, -time/1.5f);

	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal);

	float3 worldEyePos = cameraPos;
	float3 worldVertPos = GetWorldPos(In.pos);

	// Determine the distance from the light to the vertex and the direction
	for (int num=0; num<NUMBER_OF_LIGHTS; num++)
	{
		float3 lightDir = lightPositions[num] - worldVertPos;
		float dist = length(lightDir);
		// Normalize, but store light attenuation too (in w)
		lightDir = lightDir / dist;
		Out.lightVecs[num] = float4(
			normalize(mul(worldToTangentSpace, lightDir)),
			CalcLightAttenuation(dist));
	} // for
		
	Out.viewVec =
		normalize(mul(worldToTangentSpace, worldEyePos - worldVertPos));
	
	Out.fogFactor = CalcFogFactor(length(worldEyePos-worldVertPos));

	// And pass everything to the pixel shader
	return Out;
} // VS_Specular30(.)

// Pixel shader function
float4 PS_Specular30(VertexOutput_Specular30 In) : COLOR
{
	// Grab texture data
	float4 diffuseTexture = tex2D(diffuseTextureSampler, In.texCoord);
	float3 normalVector = (2.0 * tex2D(normalTextureSampler, In.texCoord).agb) - 1.0;
	// Normalize normal to fix blocky errors
	normalVector = normalize(normalVector);
	float specularTexture = tex2D(specularTextureSampler, In.texCoord).r;

	// Use all lights for the light calculation
	float totalBump = 0;
	float totalSpec = 0;
	float3 viewVector = normalize(In.viewVec);
	//for (int num=0; num<1; num++)
	for (int num=0; num<NUMBER_OF_LIGHTS; num++)
	{
		float3 lightVector = normalize(In.lightVecs[num].xyz);
		// Compute the angle to the light
		float attentuation = In.lightVecs[num].w;
		// If this is the first light (player flare), make it much weaker
		//obs: if (num == 0)
		//	attentuation /= 2.0f;
		float bump = saturate(dot(normalVector, lightVector)) * attentuation;
		totalBump += bump;
	
		// Specular factor
		float3 reflect = normalize(2 * bump * normalVector - lightVector);
		float spec = pow(saturate(dot(reflect, viewVector)), specularPower);
		totalSpec += spec;
	} // for (int num=0; num<NUMBER_OF_LIGHTS; num++)

	// Reflection
	half3 R = reflect(-viewVector, normalVector);
	R = float3(R.x, R.z, R.y);
	
	// Fresnel
	float3 E = -viewVector;
	float facing = 1.0 - max(dot(E, -normalVector), 0);
	float fresnel = fresnelBias + (1.0-fresnelBias)*pow(facing, fresnelPower);
	totalSpec *= 2*fresnel;
	
	float4 ambDiffColor = ambientColor + totalBump * diffuseColor;
	float4 ret = diffuseTexture * ambDiffColor +
		totalBump * totalSpec * specularColor * specularTexture;

	// Also add the fog, which was already calculated in the vertex shader
	// (which is obviously faster and we already have a long slow pixel shader)
	// Use full color if there is no fog, and the fog color for fog
	return lerp(ret, fogColor, In.fogFactor);		
} // PS_Specular30(.)

// Techniques
technique Specular30
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS_Specular30();
		PixelShader  = compile ps_3_0 PS_Specular30();
	} // pass P0
} // Specular30
*/