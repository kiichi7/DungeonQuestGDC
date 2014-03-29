// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 24.02.2007 20:26
// Last modified: 31.07.2007 04:43

// Skinning shader for the test project to load skinned collada files
// Based on RocketCommanderXNA shaders.

// Default variables, supported by the engine (may not be used here)
// If you don't need any global variable, just comment it out, this way
// the game engine does not have to set it!
float4x4 viewProj         : ViewProjection;
float4x4 world            : World;
float3 cameraPos          : CameraPosition;

// Use 3x4 matrices to save some variables, this way we can have 80 instead
// of 60 bone matrices if we would use 4x4 matrices. Shader model 2.0
// can only guarantee 256 constants and 80 3x4 will fill up 240 of them.
// Note: Storing these as a big float4 makes both setting and accessing
// the data a little bit easier. Indices are always pre-multiplied by 3!
#define SKINNED_MATRICES_SIZE_VS20 40//old:80
float4 skinnedMatricesVS20[SKINNED_MATRICES_SIZE_VS20*3];

// Only use 3 lights for non-level shaders, faster and easier to work with.
#define NUMBER_OF_LIGHTS 3
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

// Color values for our material, all colors are pre-multiplied with the light color!
float4 ambientColor : Ambient
<
	string UIName = "Ambient Color";
	string Space = "material";
> = {0.1f, 0.1f, 0.1f, 1.0f};

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
> = 12.0;

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

// Material textures and samplers
texture diffuseTexture : Diffuse
<
	string UIName = "Diffuse Texture";
	string ResourceName = "Goblin.dds";
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
	float3 blendWeights : BLENDWEIGHT;
	float3 blendIndices : BLENDINDICES;
	float2 texCoord : TEXCOORD0;
	float3 normal   : NORMAL;
	float3 tangent	: TANGENT;
};

//----------------------------------------------------

// Common functions
float3x3 ComputeTangentMatrix(float3 tangent, float3 normal, float4x4 world)
{
	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace;
	float3 binormal = cross(normal, tangent);
	// Please not the reversed order we use the world matrix here.
	// The reason for this is the rebuild skin matrix, which is transposed!
	worldToTangentSpace[0] = normalize(mul((float3x3)world, (float3)tangent));
	worldToTangentSpace[1] = normalize(mul((float3x3)world, (float3)binormal));
	worldToTangentSpace[2] = normalize(mul((float3x3)world, (float3)normal));
	return worldToTangentSpace;
} // ComputeTangentMatrix(..)

// Note: This returns a transposed matrix, use it in reversed order.
// First tests used a 3x3 matrix +3 w values for the transpose values, but
// reconstructing this in the shader costs 20+ extra instructions and after
// some testing I found out this is finally the best way to use 4x3 matrices
// for skinning :)
float4x4 RebuildSkinMatrix(float index)
{
	return float4x4(
		skinnedMatricesVS20[index+0],
		skinnedMatricesVS20[index+1],
		skinnedMatricesVS20[index+2],
		float4(0, 0, 0, 1));
} // RebuildSkinMatrix(.)

float CalcLightAttenuation(float dist)
{
/*old, complicated, hard to test
	// Compute distance based attenuation with the following formular:
	// Attenuation = 1 / (attn1 + attn2*dist + attn3*dist*dist)
	return saturate(1 / (lightAttenuation.x +
		lightAttenuation.y * dist + lightAttenuation.z * dist * dist));
		//old: clamp(0, 1, ...
*/

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
	return 1.25f * //1.65f *
		(distance1 + distance2 * distance2);
		//distance1;
		//(distance2*distance2);
} // CalcLightAttenuation(.)

float CalcFogFactor(float distanceToCamera)
{
	// Find out how much fog we got here (0-1)
	float zfactor = saturate((distanceToCamera - fogStart) / (fogEnd - fogStart));
	if (zfactor < 0.0)
		zfactor = 0.0;
	return zfactor;
} // CalcFogFactor(float distanceToCamera)

// -----------------------------------------------------

// Vertex output structure
struct VertexOutput_DiffuseSpecular20
{
	float4 pos      : POSITION;
	float2 texCoord	: TEXCOORD0;
	float3 viewVec  : TEXCOORD1;
	// Light directions + attentuation in w
	float4 lightVecs[NUMBER_OF_LIGHTS] : TEXCOORD2;
	float fogFactor     : COLOR0;
};
struct VertexOutput_DiffuseSpecular30
{
	//float4 pos      : POSITION;
	float2 texCoord	: TEXCOORD0;
	float3 viewVec  : TEXCOORD1;
	// Light directions + attentuation in w
	float4 lightVecs[NUMBER_OF_LIGHTS] : TEXCOORD2;
	float fogFactor     : COLOR0;
};

// Vertex shader for ps_2_0 (need more constants than ps_1_1)
VertexOutput_DiffuseSpecular20 VS_Specular20(VertexInput In)
{
	VertexOutput_DiffuseSpecular20 Out = (VertexOutput_DiffuseSpecular20)0;      

	// First transform position with bones that affect this vertex
	// Use the 3 indices and blend weights we have precalculated.
	float4x4 skinMatrix =
		RebuildSkinMatrix(In.blendIndices.x) * In.blendWeights.x +
		RebuildSkinMatrix(In.blendIndices.y) * In.blendWeights.y +
		RebuildSkinMatrix(In.blendIndices.z) * In.blendWeights.z;
	
	// Calculate local world matrix with help of the skinning matrix
	float4x4 localWorld =
		//skinMatrix;//world;//
		mul(world, skinMatrix);
	
	// Now calculate final screen position with world and viewProj matrices.
	float4 worldPos = mul(localWorld, float4(In.pos, 1));
	Out.pos = mul(worldPos, viewProj);
	Out.texCoord = In.texCoord;

	float3 worldEyePos = cameraPos;
	float3 worldVertPos = worldPos.xyz;
	
	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal, localWorld);

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

	// Rest of the calculation is done in pixel shader
	return Out;
} // VS_Specular20

// Pixel shader
float4 PS_Specular20(VertexOutput_DiffuseSpecular20 In) : COLOR
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
		totalBump += bump; // devide by 2 to make effect darker (to bright)
	
		// Specular factor
		float3 reflect = normalize(2 * bump * normalVector - lightVector);
		float spec = pow(saturate(dot(reflect, viewVector)), specularPower);
		totalSpec += spec;
	} // for (int num=0; num<NUMBER_OF_LIGHTS; num++)

	// Reflection
	half3 R = reflect(-viewVector, normalVector);
	//R = float3(R.x, R.z, R.y);
	
	// Fresnel
	float3 E = -viewVector;
	float facing = 1.0 - max(dot(E, -normalVector), 0);
	float fresnel = fresnelBias + (1.0-fresnelBias)*pow(facing, fresnelPower);
	totalSpec += fresnel;
	
	float4 ambDiffColor = ambientColor + totalBump * diffuseColor;
	float4 ret = diffuseTexture * ambDiffColor +
		totalBump * totalSpec * specularColor * specularTexture;
	
	// Also add the fog, which was already calculated in the vertex shader
	// (which is obviously faster and we already have a long slow pixel shader)
	// Use full color if there is no fog, and the fog color for fog
	return lerp(ret, fogColor, In.fogFactor);		
} // PS_Specular20

technique Specular20
{
	pass P0
	{
		VertexShader = compile vs_2_0 VS_Specular20();
		PixelShader  = compile ps_2_a PS_Specular20();
	} // pass P0
} // technique Specular20

/*not required anymore because we just use 3 lights!
// Vertex shader for ps_2_0 (need more constants than ps_1_1)
VertexOutput_DiffuseSpecular20 VS_Specular30(VertexInput In)
{
	VertexOutput_DiffuseSpecular20 Out = (VertexOutput_DiffuseSpecular20)0;      

	// First transform position with bones that affect this vertex
	// Use the 3 indices and blend weights we have precalculated.
	float4x4 skinMatrix =
		RebuildSkinMatrix(In.blendIndices.x) * In.blendWeights.x +
		RebuildSkinMatrix(In.blendIndices.y) * In.blendWeights.y +
		RebuildSkinMatrix(In.blendIndices.z) * In.blendWeights.z;
	
	// Calculate local world matrix with help of the skinning matrix
	float4x4 localWorld =
		//skinMatrix;//world;//
		mul(world, skinMatrix);
	
	// Now calculate final screen position with world and viewProj matrices.
	float4 worldPos = mul(localWorld, float4(In.pos, 1));
	Out.pos = mul(worldPos, viewProj);
	Out.texCoord = In.texCoord;

	float3 worldEyePos = cameraPos;
	float3 worldVertPos = worldPos.xyz;
	
	// Compute the 3x3 tranform from tangent space to object space
	float3x3 worldToTangentSpace =
		ComputeTangentMatrix(In.tangent, In.normal, localWorld);

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

	// Rest of the calculation is done in pixel shader
	return Out;
} // VS_Specular30

// Pixel shader
float4 PS_Specular30(VertexOutput_DiffuseSpecular30 In) : COLOR
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
		totalBump += bump; // devide by 2 to make effect darker (to bright)
	
		// Specular factor
		float3 reflect = normalize(2 * bump * normalVector - lightVector);
		float spec = pow(saturate(dot(reflect, viewVector)), specularPower);
		totalSpec += spec;
	} // for (int num=0; num<NUMBER_OF_LIGHTS; num++)

	// Reflection
	half3 R = reflect(-viewVector, normalVector);
	//R = float3(R.x, R.z, R.y);
	
	// Fresnel
	float3 E = -viewVector;
	float facing = 1.0 - max(dot(E, -normalVector), 0);
	float fresnel = fresnelBias + (1.0-fresnelBias)*pow(facing, fresnelPower);
	totalSpec += fresnel;
	
	float4 ambDiffColor = ambientColor + totalBump * diffuseColor;
	float4 ret = diffuseTexture * ambDiffColor +
		totalBump * totalSpec * specularColor * specularTexture;
	
	// Also add the fog, which was already calculated in the vertex shader
	// (which is obviously faster and we already have a long slow pixel shader)
	// Use full color if there is no fog, and the fog color for fog
	return lerp(ret, fogColor, In.fogFactor);		
} // PS_Specular30

technique Specular30
{
	pass P0
	{
		VertexShader = compile vs_3_0 VS_Specular30();
		PixelShader  = compile ps_3_0 PS_Specular30();
	} // pass P0
} // technique Specular30
*/