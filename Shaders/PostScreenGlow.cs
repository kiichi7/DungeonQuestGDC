// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:46

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using System;
using System.Collections;
using System.Text;
using System.IO;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Texture = DungeonQuest.Graphics.Texture;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using DungeonQuest.Game;
#endregion

namespace DungeonQuest.Shaders
{
	/// <summary>
	/// Post screen glow shader based on PostScreenGlow.fx.
	/// Derive from PostScreenMenu, this way we can save duplicating
	/// the xnaEffect parameters and use the same RenderToTextures.
	/// </summary>
	public class PostScreenGlow : ShaderEffect
	{
		#region Variables
		/// <summary>
		/// The shader xnaEffect filename for this shader.
		/// </summary>
		private const string Filename = "PostScreenGlow";

		/// <summary>
		/// Effect handles for window size and scene map.
		/// </summary>
		protected EffectParameter windowSize,
			sceneMap,
			downsampleMap,
			blurMap1,
			blurMap2,
			noiseMap;
		/// <summary>
		/// Effect handles for window size and scene map.
		/// </summary>
		private EffectParameter radialSceneMap,
			radialBlurScaleFactor,
			screenBorderFadeoutMap;

		/// <summary>
		/// Links to the passTextures, easier to write code this way.
		/// This are just reference copies. Static to load them only once
		/// (used for both PostScreenMenu and PostScreenGlow).
		/// </summary>
		internal static RenderToTexture sceneMapTexture,
			downsampleMapTexture,
			blurMap1Texture,
			blurMap2Texture;
		/// <summary>
		/// Links to the passTextures, easier to write code this way.
		/// This are just reference copies.
		/// </summary>
		private RenderToTexture radialSceneMapTexture;

		/// <summary>
		/// Helper texture for the noise and film effects.
		/// </summary>
		private Texture noiseMapTexture = null;

		/// <summary>
		/// Is this post screen shader started?
		/// Else don't execute Show if it is called.
		/// </summary>
		protected static bool startedPostScreen = false;

		/// <summary>
		/// Started
		/// </summary>
		/// <returns>Bool</returns>
		public bool Started
		{
			get
			{
				return startedPostScreen;
			} // get
		} // Started

		/// <summary>
		/// Helper texture for the screen border (darken the borders).
		/// </summary>
		private Texture screenBorderFadeoutMapTexture = null;
		#endregion
		
		#region Properties
		/// <summary>
		/// Last used radial blur scale factor
		/// </summary>
		private float lastUsedRadialBlurScaleFactor = -0.0001f;
		/// <summary>
		/// Radial blur scale factor
		/// </summary>
		public float RadialBlurScaleFactor
		{
			get
			{
				return lastUsedRadialBlurScaleFactor;
			} // get
			set
			{
				if (radialBlurScaleFactor != null &&
					lastUsedRadialBlurScaleFactor != value)
				{
					lastUsedRadialBlurScaleFactor = value;
					radialBlurScaleFactor.SetValue(value);
				} // if (xnaEffect.D3DEffect)
			} // set
		} // RadialBlurScaleFactor
		#endregion

		#region Constructor
		/// <summary>
		/// Create post screen glow
		/// </summary>
		public PostScreenGlow()
			: base(Filename)
		{
			// Scene map texture
			if (sceneMapTexture == null)
				sceneMapTexture = new RenderToTexture(
					RenderToTexture.SizeType.FullScreenWithZBuffer);//.FullScreen);
			// Downsample map texture (to 1/4 of the screen)
			if (downsampleMapTexture == null)
				downsampleMapTexture = new RenderToTexture(
					RenderToTexture.SizeType.QuarterScreen);

			// Blur map texture
			if (blurMap1Texture == null)
				blurMap1Texture = new RenderToTexture(
					RenderToTexture.SizeType.QuarterScreen);
			// Blur map texture
			if (blurMap2Texture == null)
				blurMap2Texture = new RenderToTexture(
					RenderToTexture.SizeType.QuarterScreen);

			// Final map for glow, used to perform radial blur next step
			radialSceneMapTexture = new RenderToTexture(
				RenderToTexture.SizeType.FullScreen);
		} // PostScreenGlow()
		#endregion

		#region Get parameters
		/// <summary>
		/// Reload
		/// </summary>
		protected override void GetParameters()
		{
			// Can't get parameters if loading failed!
			if (xnaEffect == null)
				return;

			windowSize = xnaEffect.Parameters["windowSize"];
			sceneMap = xnaEffect.Parameters["sceneMap"];

			// We need both windowSize and sceneMap.
			if (windowSize == null ||
				sceneMap == null)
				throw new NotSupportedException("windowSize and sceneMap must be " +
					"valid in PostScreenShader=" + Filename);

			// Init additional stuff
			downsampleMap = xnaEffect.Parameters["downsampleMap"];
			blurMap1 = xnaEffect.Parameters["blurMap1"];
			blurMap2 = xnaEffect.Parameters["blurMap2"];
			radialSceneMap = xnaEffect.Parameters["radialSceneMap"];

			// Load screen border texture
			screenBorderFadeoutMap = xnaEffect.Parameters["screenBorderFadeoutMap"];
			screenBorderFadeoutMapTexture = new Texture("ScreenBorderFadeout.dds");
			// Set texture
			screenBorderFadeoutMap.SetValue(
				screenBorderFadeoutMapTexture.XnaTexture);

			radialBlurScaleFactor = xnaEffect.Parameters["radialBlurScaleFactor"];
			time = xnaEffect.Parameters["time"];

			// Load noise texture for stripes effect
			noiseMap = xnaEffect.Parameters["noiseMap"];
			noiseMapTexture = new Texture("Noise128x128.dds");
			// Set texture
			noiseMap.SetValue(noiseMapTexture.XnaTexture);
		} // GetParameters()
		#endregion
		
		#region Start
		DepthStencilBuffer remBackBufferSurface = null;
		/// <summary>
		/// Start this post screen shader, will just call SetRenderTarget.
		/// All render calls will now be drawn on the sceneMapTexture.
		/// Make sure you don't reset the RenderTarget until you call Show()!
		/// </summary>
		public void Start()
		{
			// Only apply post screen shader if texture is valid and effect is valid 
			if (sceneMapTexture == null ||
				xnaEffect == null ||
				startedPostScreen == true ||
				// Also skip if we don't use post screen shaders at all!
				BaseGame.UsePostScreenShaders == false)
				return;

			BaseGame.SetRenderTarget(sceneMapTexture.RenderTarget, true);
			startedPostScreen = true;

			remBackBufferSurface = null;
			if (sceneMapTexture.ZBufferSurface != null)
			{
				remBackBufferSurface = BaseGame.Device.DepthStencilBuffer;
				BaseGame.Device.DepthStencilBuffer =
					sceneMapTexture.ZBufferSurface;
			} // if (sceneMapTexture.ZBufferSurface)
		} // Start()
		#endregion

		#region Show
		/// <summary>
		/// Execute shaders and show result on screen, Start(..) must have been
		/// called before and the scene should be rendered to sceneMapTexture.
		/// </summary>
		public void Show()
		{
			// Only apply post screen glow if texture is valid and xnaEffect is valid 
			if (sceneMapTexture == null ||
				xnaEffect == null ||
				startedPostScreen == false)
				return;

			startedPostScreen = false;

			// Resolve sceneMapTexture render target for Xbox360 support
			sceneMapTexture.Resolve();

			// Don't use or write to the z buffer
			BaseGame.Device.RenderState.DepthBufferEnable = false;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = false;
			// Also don't use any kind of blending.
			//Update: allow writing to alpha!
			BaseGame.Device.RenderState.AlphaBlendEnable = false;

			if (windowSize != null)
				windowSize.SetValue(
					new float[] { sceneMapTexture.Width, sceneMapTexture.Height });
			if (sceneMap != null)
				sceneMap.SetValue(sceneMapTexture.XnaTexture);

			RadialBlurScaleFactor = //-0.0025f //heavy: -0.0085f;//medium: -0.005f;
				// Warning: To big values will make the motion blur look to
				// stepy (we see each step and thats not good). -0.02 should be max.
				//-(0.0025f + RacingGameManager.Player.Speed * 0.005f /
				//Player.DefaultMaxSpeed);
				//try1: -0.005f;
				-0.0025f;

			if (time != null)
				time.SetValue(BaseGame.TotalTime);

			xnaEffect.CurrentTechnique = xnaEffect.Techniques[
				BaseGame.CanUsePS20 ? "ScreenGlow20" : "ScreenGlow"];
			
			// We must have exactly 5 passes!
			if (xnaEffect.CurrentTechnique.Passes.Count != 5)
				throw new InvalidOperationException(
					"This shader should have exactly 5 passes!");

			try
			{
				xnaEffect.Begin();
				for (int pass = 0; pass < xnaEffect.CurrentTechnique.Passes.Count;
					pass++)
				{
					if (pass == 0)
						radialSceneMapTexture.SetRenderTarget();
					else if (pass == 1)
						downsampleMapTexture.SetRenderTarget();
					else if (pass == 2)
						blurMap1Texture.SetRenderTarget();
					else if (pass == 3)
						blurMap2Texture.SetRenderTarget();
					else
						// Do a full reset back to the back buffer
						BaseGame.ResetRenderTarget(true);

					EffectPass effectPass = xnaEffect.CurrentTechnique.Passes[pass];
					effectPass.Begin();
					// For first xnaEffect we use radial blur, draw it with a grid
					// to get cooler results (more blur at borders than in middle).
					if (pass == 0)
						VBScreenHelper.Render10x10Grid();
					else
						VBScreenHelper.Render();
					effectPass.End();

					if (pass == 0)
					{
						radialSceneMapTexture.Resolve();
						if (radialSceneMap != null)
							radialSceneMap.SetValue(radialSceneMapTexture.XnaTexture);
						xnaEffect.CommitChanges();
					} // if
					else if (pass == 1)
					{
						downsampleMapTexture.Resolve();
						if (downsampleMap != null)
							downsampleMap.SetValue(downsampleMapTexture.XnaTexture);
						xnaEffect.CommitChanges();
					} // if
					else if (pass == 2)
					{
						blurMap1Texture.Resolve();
						if (blurMap1 != null)
							blurMap1.SetValue(blurMap1Texture.XnaTexture);
						xnaEffect.CommitChanges();
					} // else if
					else if (pass == 3)
					{
						blurMap2Texture.Resolve();
						if (blurMap2 != null)
							blurMap2.SetValue(blurMap2Texture.XnaTexture);
						xnaEffect.CommitChanges();
					} // else if
				} // for (pass, <, ++)
			} // try
			finally
			{
				xnaEffect.End();

				// Restore z buffer state
				BaseGame.Device.RenderState.DepthBufferEnable = true;
				BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
			} // finally
		} // Show()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test post screen glow
		/// </summary>
		public static void TestPostScreenGlow()
		{
			Texture backgroundTexture = null;

			TestGame.Start("TestPostScreenGlow",
				delegate
				{
					backgroundTexture = new Texture("XnaBook");
				},
				delegate
				{
					// Disable glow with ctrl or X
					if (Input.Keyboard.IsKeyDown(Keys.LeftControl) == false &&
						Input.GamePadXPressed == false)
						BaseGame.GlowShader.Start();

					backgroundTexture.RenderOnScreen(BaseGame.ResolutionRect,
						new Rectangle(0, 0, 256, 320));
					SpriteHelper.DrawSprites();
				});
		} // TestPostScreenGlow()
#endif
		#endregion
	} // class PostScreenGlow
} // namespace DungeonQuest.Shaders
