// Project: DungeonQuest, File: PostScreenMenu.cs
// Namespace: DungeonQuest.Shaders, Class: PostScreenMenu
// Path: C:\code\DungeonQuest\Shaders, Author: Abi
// Code lines: 369, Size of file: 10,70 KB
// Creation date: 27.09.2006 03:46
// Last modified: 15.10.2006 19:59
// Generated with Commenter by abi.exDream.com

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.IO;
using System.Text;
using DungeonQuest.GameLogic;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using Texture = DungeonQuest.Graphics.Texture;
using DungeonQuest.Game;
#endregion

namespace DungeonQuest.Shaders
{
	/// <summary>
	/// Post screen glow shader based on PostScreenMenu.fx
	/// </summary>
	/// <returns>Shader effect</returns>
	public class PostScreenMenu : ShaderEffect
	{
		#region Variables
		/// <summary>
		/// The shader effect filename for this shader.
		/// </summary>
		private const string Filename = "PostScreenMenu";

		/// <summary>
		/// Effect handles for window size and scene map.
		/// </summary>
		protected EffectParameter windowSize,
			sceneMap,
			downsampleMap,
			blurMap1,
			blurMap2,
			noiseMap,
			timer;

		/// <summary>
		/// Links to the passTextures, easier to write code this way.
		/// This are just reference copies. Static to load them only once
		/// (used for both PostScreenMenu and PostScreenGlow).
		/// </summary>
		//dunno why, but RenderToTexture crashes if we do this:
		protected static RenderToTexture sceneMapTexture,
		// Instead load twice:
		//protected RenderToTexture sceneMapTexture = null,
			downsampleMapTexture,
			blurMap1Texture,
			blurMap2Texture;

		/// <summary>
		/// Helper texture for the noise and film effects.
		/// </summary>
		private Texture noiseMapTexture = null;

		/// <summary>
		/// Is this post screen shader started?
		/// Else don't execute Show if it is called.
		/// </summary>
		protected static bool started = false;

		/// <summary>
		/// Started
		/// </summary>
		/// <returns>Bool</returns>
		public static bool Started
		{
			get
			{
				return started;
			} // get
		} // Started
		#endregion

		#region Constructor
		/// <summary>
		/// Create post screen menu. Also used for the constructor of
		/// PostScreenGlow (same RenderToTextures used there).
		/// </summary>
		protected PostScreenMenu(string shaderFilename)
			: base(shaderFilename)
		{
			// Scene map texture
			if (sceneMapTexture == null)
				sceneMapTexture = new RenderToTexture(
					RenderToTexture.SizeType.FullScreen);
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
		} // PostScreenMenu()

		/// <summary>
		/// Create post screen menu
		/// </summary>
		public PostScreenMenu()
			: this(Filename)
		{
		} // PostScreenMenu()
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
			timer = xnaEffect.Parameters["Timer"];

			// Load noise texture for stripes xnaEffect
			noiseMap = xnaEffect.Parameters["noiseMap"];
			noiseMapTexture = new Texture("Noise128x128.dds");
			// Set texture
			noiseMap.SetValue(noiseMapTexture.XnaTexture);
		} // GetParameters()
		#endregion
		
		#region Start
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
				started == true ||
				// Also skip if we don't use post screen shaders at all!
				BaseGame.UsePostScreenShaders == false)
				return;

			BaseGame.SetRenderTarget(sceneMapTexture.RenderTarget, true);
			started = true;
		} // Start()
		#endregion

		#region Show
		/// <summary>
		/// Execute shaders and show result on screen, Start(..) must have been
		/// called before and the scene should be rendered to sceneMapTexture.
		/// </summary>
		public virtual void Show()
		{
			// Only apply post screen glow if texture is valid and effect is valid 
			if (sceneMapTexture == null ||
				Valid == false ||
				started == false)
				return;

			started = false;

			// Resolve sceneMapTexture render target for Xbox360 support
			sceneMapTexture.Resolve();

			// Don't use or write to the z buffer
			BaseGame.Device.RenderState.DepthBufferEnable = false;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = false;
			// Also don't use any kind of blending.
			BaseGame.Device.RenderState.AlphaBlendEnable = false;

			if (windowSize != null)
				windowSize.SetValue(new float[]
					{ sceneMapTexture.Width, sceneMapTexture.Height });
			if (sceneMap != null)
				sceneMap.SetValue(sceneMapTexture.XnaTexture);

			if (timer != null)
				// Add a little offset to prevent first effect.
				timer.SetValue(BaseGame.TotalTime+0.75f);

			xnaEffect.CurrentTechnique = xnaEffect.Techniques[
				BaseGame.CanUsePS20 ? "ScreenGlow20" : "ScreenGlow"];
			
			// We must have exactly 4 passes!
			if (xnaEffect.CurrentTechnique.Passes.Count != 4)
				throw new InvalidOperationException(
					"This shader should have exactly 4 passes!");

			try
			{
				xnaEffect.Begin();
				for (int pass = 0; pass < xnaEffect.CurrentTechnique.Passes.Count; pass++)
				{
					if (pass == 0)
						downsampleMapTexture.SetRenderTarget();
					else if (pass == 1)
						blurMap1Texture.SetRenderTarget();
					else if (pass == 2)
						blurMap2Texture.SetRenderTarget();
					else
						// Do a full reset back to the back buffer
						BaseGame.ResetRenderTarget(true);

					EffectPass effectPass = xnaEffect.CurrentTechnique.Passes[pass];
					effectPass.Begin();
					VBScreenHelper.Render();
					effectPass.End();

					if (pass == 0)
					{
						downsampleMapTexture.Resolve();
						if (downsampleMap != null)
							downsampleMap.SetValue(downsampleMapTexture.XnaTexture);
						xnaEffect.CommitChanges();
					} // if
					else if (pass == 1)
					{
						blurMap1Texture.Resolve();
						if (blurMap1 != null)
							blurMap1.SetValue(blurMap1Texture.XnaTexture);
						xnaEffect.CommitChanges();
					} // else if
					else if (pass == 2)
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

		
	} // class PostScreenMenu
} // namespace DungeonQuest.Shaders
