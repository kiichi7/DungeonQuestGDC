// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:43

#region Using directives
using DungeonQuest.Game;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Texture = DungeonQuest.Graphics.Texture;
using XnaModel = Microsoft.Xna.Framework.Graphics.Model;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture;
#endregion

namespace DungeonQuest.Shaders
{
	/// <summary>
	/// Shader effect class. You can either directly use this class by
	/// providing a fx filename in the constructor or derive from this class
	/// for special shader functionality (see post screen shaders for a more
	/// complex example).
	/// </summary>
	public class CavePointNormalMapping : ShaderEffect
	{
		#region Variables
		/// <summary>
		/// Extra effect handles for this shader.
		/// Most effect parameters are just editable in the shader file itself.
		/// This way this code is simpler and easier to test by just changing
		/// the shader because we use this shader just for the cave itself.
		/// </summary>
		protected EffectParameter detailGroundDiffuseTexture,
			detailGroundNormalTexture,
			detailGroundHeightTexture,
			detailWallDiffuseTexture,
			detailWallNormalTexture,
			detailWallHeightTexture;

		/// <summary>
		/// Extra textures we need for this shader.
		/// </summary>
		Texture groundDiffuseTexture,
			groundNormalTexture,
			groundHeightTexture,
			wallDiffuseTexture,
			wallNormalTexture,
			wallHeightTexture;
		#endregion

		#region Constructor
		/// <summary>
		/// Create cave point normal mapping
		/// </summary>
		/// <param name="shaderName">Shader name</param>
		public CavePointNormalMapping(string shaderName)
			: base(shaderName)
		{
			// Getting the effect parameters happens in GetParameters.

			// But we can load all the required textures here.
			groundDiffuseTexture = new Texture("CaveDetailGround");
			groundNormalTexture = new Texture("CaveDetailGroundNormal");
			groundHeightTexture = new Texture("CaveDetailGroundHeight");
			wallDiffuseTexture = new Texture("CaveDetailWall");
			wallNormalTexture = new Texture("CaveDetailWallNormal");
			wallHeightTexture = new Texture("CaveDetailWallHeight");
		} // CavePointNormalMapping(shaderName)
		#endregion

		#region Get parameters
		/// <summary>
		/// Get parameters, override to support more
		/// </summary>
		protected override void GetParameters()
		{
			base.GetParameters();

			// Get extra parameters
			detailGroundDiffuseTexture =
				xnaEffect.Parameters["detailGroundDiffuseTexture"];
			detailGroundNormalTexture =
				xnaEffect.Parameters["detailGroundNormalTexture"];
			detailGroundHeightTexture =
				xnaEffect.Parameters["detailGroundHeightTexture"];
			detailWallDiffuseTexture =
				xnaEffect.Parameters["detailWallDiffuseTexture"];
			detailWallNormalTexture =
				xnaEffect.Parameters["detailWallNormalTexture"];
			detailWallHeightTexture =
				xnaEffect.Parameters["detailWallHeightTexture"];

			// We always use the same technique!
			if (BaseGame.Device.GraphicsDeviceCapabilities.PixelShaderVersion.
				Major >= 3)
				xnaEffect.CurrentTechnique = xnaEffect.Techniques["DiffuseSpecular30"];
			else
				xnaEffect.CurrentTechnique = xnaEffect.Techniques["DiffuseSpecular20"];
		} // GetParameters()
		#endregion

		#region Render
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="setMat">Set matrix</param>
		/// <param name="techniqueName">Technique name</param>
		/// <param name="renderDelegate">Render delegate</param>
		public void RenderCave(Material setMat,
			//auto: List<Vector3> setLightPositions,
			BaseGame.RenderDelegate renderDelegate)
		{
			// And set all the other parameters
			detailGroundDiffuseTexture.SetValue(groundDiffuseTexture.XnaTexture);
			detailGroundNormalTexture.SetValue(groundNormalTexture.XnaTexture);
			detailGroundHeightTexture.SetValue(groundHeightTexture.XnaTexture);
			detailWallDiffuseTexture.SetValue(wallDiffuseTexture.XnaTexture);
			detailWallNormalTexture.SetValue(wallNormalTexture.XnaTexture);
			detailWallHeightTexture.SetValue(wallHeightTexture.XnaTexture);

			// Set all the basic parameters.
			SetParameters(setMat);

			// Set all light positions that are close to us.
			if (LightManager.closestLightsForRendering.Count != 6)
				throw new InvalidOperationException(
					"You must always set exactly " + LightManager.NumberOfLights +
					" lights for the shader!");
			lightPositions.SetValue(LightManager.closestLightsForRendering.ToArray());

			// Start effect (current technique should be set)
			xnaEffect.Begin(SaveStateMode.None);
			// Start first pass
			xnaEffect.CurrentTechnique.Passes[0].Begin();

			// Render
			renderDelegate();

			// End pass and shader
			xnaEffect.CurrentTechnique.Passes[0].End();
			xnaEffect.End();
		} // RenderCave(passName, renderDelegate)
		#endregion
	} // class ShaderEffect
} // namespace DungeonQuest.Shaders
