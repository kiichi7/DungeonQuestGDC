// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 13.03.2007 22:57
// Last modified: 31.07.2007 04:43

// Skinning shader for the test project to load skinned collada files
// Based on RocketCommanderXNA shaders.

string description = "Generate and use a shadow map with a directional light";

// Default variables, supported by the engine (may not be used here)
// If you don't need any global variable, just comment it out, this way
// the game engine does not have to set it!
float4x4 worldViewProj         : WorldViewProjection;

// Use 3x4 matrices to save some variables, this way we can have 80 instead
// of 60 bone matrices if we would use 4x4 matrices. Shader model 2.0
// can only guarantee 256 constants and 80 3x4 will fill up 240 of them.
// Note: Storing these as a big float4 makes both setting and accessing
// the data a little bit easier. Indices are always pre-multiplied by 3!
#define SKINNED_MATRICES_SIZE_VS20 40//old: 80
float4 skinnedMatricesVS20[SKINNED_MATRICES_SIZE_VS20*3];

// Extra values for this shader
// Transformation matrix for converting world pos
// to texture coordinates of the shadow map.
float4x4 shadowTexTransform;
// worldViewProj of the light projection
float4x4 worldViewProjLight : WorldViewProjection;

// Hand adjusted near and far plane for better percision.
float nearPlane = 2.0f;
float farPlane = 8.0f;
// Depth bias, controls how much we remove from the depth
// to fix depth checking artifacts. For ps_1_1 this should
// be a very high value (0.01f), for ps_2_0 it can be very low.
float depthBias = 0.0025f;
// Substract a very low value from shadow map depth to
// move everything a little closer to the camera.
// This is done when the shadow map is rendered before any
// of the depth checking happens, should be a very small value.
float shadowMapDepthBias = -0.0005f;

// Color for shadowed areas, should be black too, but need
// some alpha value (e.g. 0.5) for blending the color to black.
float4 ShadowColor =
	//{0.35f, 0.36f, 0.37f, 1.0f};
	//{0.30f, 0.31f, 0.32f, 1.0f};
	//{0.25f, 0.26f, 0.27f, 1.0f};
	//{0.15f, 0.16f, 0.17f, 1.0f};
	{0.01f, 0.05f, 0.1f, 1.0f};
	//{0, 0, 0, 1.0f};

float3 lightDir : Direction
<
	string UIName = "Light Direction";
	string Object = "DirectionalLight";
	string Space = "World";
> = {1.0f, -1.0f, 1.0f};

/*unused
texture shadowDistanceFadeoutTexture : Diffuse
<
	string UIName = "Shadow distance fadeout texture";
	string ResourceName = "ShadowDistanceFadeoutMap.dds";
>;
sampler shadowDistanceFadeoutTextureSampler = sampler_state
{
	Texture = <shadowDistanceFadeoutTexture>;
	AddressU  = Wrap;//Clamp;
	AddressV  = Wrap;//Clamp;
	AddressW  = Wrap;//Clamp;
	MinFilter = Linear;
	MagFilter = Linear;
	MipFilter = Linear;
};
*/
// Vertex input structure (used for ALL techniques here!)
struct VertexInput
{
	// We just use the position here, nothing else is required.
	float3 pos      : POSITION;
	// Note: If blendWeights.x is 0 no skinning is used!
	float3 blendWeights : BLENDWEIGHT;
	float3 blendIndices : BLENDINDICES;
};

// Struct used for passing data from VS_GenerateShadowMap to ps
struct VB_GenerateShadowMap
{
	float4 pos      : POSITION;
	// Ps 1.1 will use color, ps 2.0 will use TexCoord.
	// This way we get the most percision in each ps model.
	float4 depth    : COLOR0;
};

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

// Helper functions
float4 TransformPosition(float3 pos)
{
	return mul(float4(pos.xyz, 1), worldViewProj);
} // TransformPosition(.)

//-------------------------------------------------------------------

// Struct used for passing data from VS_GenerateShadowMap to ps
struct VB_GenerateShadowMap20
{
	float4 pos      : POSITION;
	float2 depth    : TEXCOORD0;
};

// Vertex shader function
VB_GenerateShadowMap20 VS_GenerateShadowMap20(VertexInput In)
{
	VB_GenerateShadowMap20 Out = (VB_GenerateShadowMap20) 0;
	// First transform position with bones that affect this vertex
	// Use the 3 indices and blend weights we have precalculated.
	float4 pos = float4(In.pos, 1);
	//if (In.blendWeights.x > 0)
		pos =
			mul(RebuildSkinMatrix(In.blendIndices.x), pos) * In.blendWeights.x +
			mul(RebuildSkinMatrix(In.blendIndices.y), pos) * In.blendWeights.y +
			mul(RebuildSkinMatrix(In.blendIndices.z), pos) * In.blendWeights.z;
	
	// Calculate final position with help of worldViewProj
	Out.pos = mul(pos, worldViewProj);

	// Use farPlane/10 for the internal near plane, we don't have any
	// objects near the light, use this to get much better percision!
	//obs: float internalNearPlane = farPlane / 10;

	// Linear depth calculation instead of normal depth calculation.
	Out.depth = float2(
		(Out.pos.z - nearPlane),
		(farPlane - nearPlane));//+ shadowMapDepthBias;

	return Out;
} // VS_GenerateShadowMap20(.)

// Vertex shader function
VB_GenerateShadowMap20 VS_GenerateShadowMapStatic20(VertexInput In)
{
	VB_GenerateShadowMap20 Out = (VB_GenerateShadowMap20) 0;
	Out.pos = TransformPosition(In.pos);

	// Use farPlane/10 for the internal near plane, we don't have any
	// objects near the light, use this to get much better percision!
	//obs: float internalNearPlane = farPlane / 10;

	// Linear depth calculation instead of normal depth calculation.
	Out.depth = float2(
		(Out.pos.z - nearPlane),
		(farPlane - nearPlane));//+ shadowMapDepthBias;

	return Out;
} // VS_GenerateShadowMapStatic20(.)

// Pixel shader function
float4 PS_GenerateShadowMap20(VB_GenerateShadowMap20 In) : COLOR
{
//return float4(0, 0, 0, 1);
//return In.depth.x/In.depth.y;
	// Just set the interpolated depth value.
	float ret = (In.depth.x/In.depth.y) + shadowMapDepthBias;
	//if (ret < 0.98)
	//	return ret;
	//else
	//	return float4(0.35f, 0, 0, 1);
	return ret;
} // PS_GenerateShadowMap20(.)

technique GenerateShadowMap20
{
	pass AnimatedPass
	{
		VertexShader = compile vs_2_0 VS_GenerateShadowMap20();
		PixelShader  = compile ps_2_0 PS_GenerateShadowMap20();
	} // pass P0
	
	pass StaticPass
	{
		VertexShader = compile vs_2_0 VS_GenerateShadowMapStatic20();
		PixelShader  = compile ps_2_0 PS_GenerateShadowMap20();
	} // pass P0
} // GenerateShadowMap20

//-------------------------------------------------------------------

texture shadowMap : Diffuse;
// Sampler for ps_2_0, use point filtering to do bilinear filtering ourself!
sampler ShadowMapSampler20 = sampler_state
{
	Texture = <shadowMap>;
	AddressU  = Border;//CLAMP;
	AddressV  = Border;//CLAMP;
	MinFilter = Point;//Linear;
	MagFilter = Point;//Linear;
	MipFilter = None;//Linear;
	BorderColor = 0xFFFFFFFF;
};

// Vertex shader output structure for using the shadow map
struct VB_UseShadowMap20
{
	float4 pos            : POSITION;
	float4 shadowTexCoord : TEXCOORD0;
	float2 depth          : TEXCOORD1;
};

VB_UseShadowMap20 VS_UseShadowMap20(VertexInput In)
{
	VB_UseShadowMap20 Out = (VB_UseShadowMap20)0;
	// First transform position with bones that affect this vertex
	// Use the 3 indices and blend weights we have precalculated.
	float4 pos = float4(In.pos, 1);
	//if (In.blendWeights.x > 0)
		pos =
			mul(RebuildSkinMatrix(In.blendIndices.x), pos) * In.blendWeights.x +
			mul(RebuildSkinMatrix(In.blendIndices.y), pos) * In.blendWeights.y +
			mul(RebuildSkinMatrix(In.blendIndices.z), pos) * In.blendWeights.z;
	
	// Calculate final position with help of worldViewProj
	Out.pos = mul(pos, worldViewProj);

	// Transform model-space vertex position to light-space:
	float4 shadowTexPos =
		mul(pos, shadowTexTransform);
	// Set first texture coordinates
	Out.shadowTexCoord = float4(//float2(
		shadowTexPos.x,//shadowTexPos.w,
		shadowTexPos.y,//shadowTexPos.w);//already done in matrix: - texelSize / 2.0f;
		0.0f,
		shadowTexPos.w);

	// Get depth of this point relative to the light position
	float4 depthPos = mul(pos, worldViewProjLight);
	
	// Use farPlane/10 for the internal near plane, we don't have any
	// objects near the light, use this to get much better percision!
	//float internalNearPlane = farPlane / 10;
	
	// Same linear depth calculation as above.
	// Also substract depthBias to fix shadow mapping artifacts.
	Out.depth = float2(
		(depthPos.z - nearPlane),
		(farPlane - nearPlane));

	return Out;
} // VS_UseShadowMap20(VertexInput In)

VB_UseShadowMap20 VS_UseShadowMapStatic20(VertexInput In)
{
	VB_UseShadowMap20 Out = (VB_UseShadowMap20)0;
	// Convert to float4 pos, used several times here.
	float4 pos = float4(In.pos, 1);
	Out.pos = mul(pos, worldViewProj);

	// Transform model-space vertex position to light-space:
	float4 shadowTexPos =
		mul(pos, shadowTexTransform);
	// Set first texture coordinates
	Out.shadowTexCoord = float4(//float2(
		shadowTexPos.x,//shadowTexPos.w,
		shadowTexPos.y,//shadowTexPos.w);//already done in matrix: - texelSize / 2.0f;
		0.0f,
		shadowTexPos.w);

	// Get depth of this point relative to the light position
	float4 depthPos = mul(pos, worldViewProjLight);
	
	// Use farPlane/10 for the internal near plane, we don't have any
	// objects near the light, use this to get much better percision!
	//float internalNearPlane = farPlane / 10;
	
	// Same linear depth calculation as above.
	// Also substract depthBias to fix shadow mapping artifacts.
	Out.depth = float2(
		(depthPos.z - nearPlane),
		(farPlane - nearPlane));

	return Out;
} // VS_UseShadowMapStatic20(VertexInput In)

//float2 shadowMapTexelSize = float2(1.0f/256.0f, 1.0f/256);
float2 shadowMapTexelSize = float2(1.0f/512.0f, 1.0f/512.0f);
//unused:
//float2 shadowMapTextureSize = float2(1024.0f, 1024.0f);

// Poison filter pseudo random filter positions for PCF with 10 samples
float2 FilterTaps[10] =
{
	// First test, still the best.
	{-0.84052f, -0.073954f},
	{-0.326235f, -0.40583f},
	{-0.698464f, 0.457259f},
	{-0.203356f, 0.6205847f},
	{0.96345f, -0.194353f},
	{0.473434f, -0.480026f},
	{0.519454f, 0.767034f},
	{0.185461f, -0.8945231f},
	{0.507351f, 0.064963f},
	{-0.321932f, 0.5954349f}
};
// Advanced pixel shader for shadow depth calculations in ps 2.0.
// However this shader looks blocky like PCF3x3 and should be smoothend
// out by a good post screen blur filter. This advanced shader does a good
// job faking the penumbra and can look very good when adjusted carefully.
float4 PS_UseShadowMap20(VB_UseShadowMap20 In) : COLOR
{
//return float4(0.98f, 0.1f, 0.0f, 0.25f);
	float depth = (In.depth.x/In.depth.y) - depthBias;
//return float4(1, depth, 0, 1);
	float2 shadowTex =
		(In.shadowTexCoord.xy / In.shadowTexCoord.w) -
		shadowMapTexelSize / 2.0f;

	float resultDepth = 0;
	for (int i=0; i<10; i++)
		resultDepth += depth > tex2D(ShadowMapSampler20,
			shadowTex+FilterTaps[i]*shadowMapTexelSize * 1.25f).r ? 1.0f/10.0f : 0.0f;
			
	// Multiply the result by the shadowDistanceFadeoutTexture, which
	// fades shadows in and out at the max. shadow distances
	//obs: resultDepth *= tex2D(shadowDistanceFadeoutTextureSampler, shadowTex).r;

	// We can skip this if its too far away anyway (else very far away landscape
	// parts will be darkenend)
	if (depth > 1)
		return 0;
	else
		// And apply
		return lerp(1, ShadowColor, resultDepth);
} // PS_UseShadowMap20(VB_UseShadowMap20 In)

technique UseShadowMap20
{  
	pass AnimatedPass
	{
		VertexShader = compile vs_2_0 VS_UseShadowMap20();
		PixelShader  = compile ps_2_0 PS_UseShadowMap20();
	} // AnimatedPass
	
	pass StaticPass
	{
		VertexShader = compile vs_2_0 VS_UseShadowMapStatic20();
		PixelShader  = compile ps_2_0 PS_UseShadowMap20();
	} // StaticPass
} // UseShadowMap20
