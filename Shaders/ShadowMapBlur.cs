// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:47

#region Using directives
using Microsoft.Xna.Framework.Graphics;
using System;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.Game;
#endregion

namespace DungeonQuest.Shaders
{
	/// <summary>
	/// ShadowMapBlur based on PostScreenBlur to blur the final shadow map
	/// output for a more smooth view on the screen.
	/// </summary>
	public class ShadowMapBlur : ShaderEffect
	{
		#region Variables
		/// <summary>
		/// The shader xnaEffect filename for this shader.
		/// </summary>
		private const string Filename = "PostScreenShadowBlur";

		/// <summary>
		/// Effect handles for window size and scene map.
		/// </summary>
		private EffectParameter windowSize,
			sceneMap;//,
			//blurMap;

		/// <summary>
		/// Links to the render to texture instances.
		/// </summary>
		private RenderToTexture sceneMapTexture;//,
			//blurMapTexture;

		/// <summary>
		/// Scene map texture
		/// </summary>
		/// <returns>Render to texture</returns>
		public RenderToTexture SceneMapTexture
		{
			get
			{
				return sceneMapTexture;
			} // get
		} // SceneMapTexture

		/*obs
		/// <summary>
		/// Blur map texture
		/// </summary>
		/// <returns>Render to texture</returns>
		public RenderToTexture BlurMapTexture
		{
			get
			{
				return blurMapTexture;
			} // get
		} // BlurMapTexture
		 */
		#endregion

		#region Create
		/// <summary>
		/// Create shadow map screen blur shader.
		/// obs, using full size again: But only use 1/4 of the screen!
		/// </summary>
		public ShadowMapBlur()
			: base(Filename)
		{
			// Scene map texture
			sceneMapTexture = new RenderToTexture(
				//RenderToTexture.SizeType.FullScreen);
				//RenderToTexture.SizeType.FullScreenWithZBuffer);
				//improve performance:
				//RenderToTexture.SizeType.HalfScreen);
//TODO if problems:				
				RenderToTexture.SizeType.HalfScreenWithZBuffer);
			/*obs
			blurMapTexture = new RenderToTexture(
				RenderToTexture.SizeType.FullScreen);
				//RenderToTexture.SizeType.FullScreenWithZBuffer);
				//improve performance:
				//RenderToTexture.SizeType.HalfScreen);
			 */
		} // ShadowMapBlur()
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
			//blurMap = xnaEffect.Parameters["blurMap"];

			// We need both windowSize and sceneMap.
			if (windowSize == null ||
				sceneMap == null)
				throw new NotSupportedException("windowSize and sceneMap must be " +
					"valid in PostScreenShader=" + Filename);
		} // GetParameters()
		#endregion

		#region RenderShadows
		/// <summary>
		/// Render shadows
		/// </summary>
		/// <param name="renderCode">Render code</param>
		public void RenderShadows(BaseGame.RenderDelegate renderCode)
		{
			// Render into our scene map texture
			sceneMapTexture.SetRenderTarget();
//*TODO
			DepthStencilBuffer remBackBufferSurface = null;
			if (sceneMapTexture.ZBufferSurface != null)
			{
				remBackBufferSurface = BaseGame.Device.DepthStencilBuffer;
				BaseGame.Device.DepthStencilBuffer =
					sceneMapTexture.ZBufferSurface;
			} // if (sceneMapTexture.ZBufferSurface)
//*/

			// Clear render target
			sceneMapTexture.Clear(Color.White);

			// Render everything
			renderCode();

			// Resolve render target
			sceneMapTexture.Resolve();

			// Restore back buffer as render target
			BaseGame.ResetRenderTarget(false);
//*TODO
			if (sceneMapTexture.ZBufferSurface != null)
				BaseGame.Device.DepthStencilBuffer = remBackBufferSurface;
//*/
		} // RenderShadows(renderCode)
		#endregion

		#region ShowShadows
		/// <summary>
		/// Show shadows with help of our blur map shader.
		/// We got 2 parts to fix the problem on the Xbox360 that we can't have
		/// more than one rendertarget in parallel.
		/// </summary>
		public void ShowShadowsPart1()
		{
			return;
			/*obs

			// Only apply post screen blur if texture is valid and xnaEffect are valid
			if (sceneMapTexture == null ||
				Valid == false ||
				// If the shadow scene map is not yet filled, there is no point
				// continuing here ...
				sceneMapTexture.XnaTexture == null)
				return;

			try
			{
				// Don't use or write to the z buffer
				BaseGame.Device.RenderState.DepthBufferEnable = false;
				BaseGame.Device.RenderState.DepthBufferWriteEnable = false;
				// Disable alpha for the first pass
				BaseGame.Device.RenderState.AlphaBlendEnable = false;
/*obs
				// Make sure we use the default add mode for alpha blend operations,
				// don't ask me why, but for some reason this is NOT the default
				// on the Xbox360 and it cost me a freaking day to figure this out :(
				BaseGame.Device.RenderState.AlphaBlendOperation = BlendFunction.Add;

				// Make sure we clamp everything to 0-1
				BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
				BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
				// Restore back buffer as render target
				//not required: BaseGame.ResetRenderTarget(false);
*
				if (windowSize != null)
					windowSize.SetValue(
						new float[] { sceneMapTexture.Width, sceneMapTexture.Height });
				if (sceneMap != null)
					sceneMap.SetValue(sceneMapTexture.XnaTexture);

				xnaEffect.CurrentTechnique = xnaEffect.Techniques[
					BaseGame.CanUsePS20 ? "ScreenAdvancedBlur20" : "ScreenAdvancedBlur"];
				
				// We must have exactly 2 passes!
				if (xnaEffect.CurrentTechnique.Passes.Count != 2)
					throw new Exception("This shader should have exactly 2 passes!");

				// Just start pass 0
				blurMapTexture.SetRenderTarget();

				RenderSinglePassShader(VBScreenHelper.Render);
				/*obs
				effect.Begin(SaveStateMode.None);

				EffectPass effectPass = effect.CurrentTechnique.Passes[0];
				effectPass.Begin();
				VBScreenHelper.Render();
				effectPass.End();
				 *

				blurMapTexture.Resolve();

				BaseGame.ResetRenderTarget(false);

				/*obs
				DepthStencilBuffer remBackBufferSurface = null;
				xnaEffect.Begin(SaveStateMode.None);
				for (int pass = 0; pass < xnaEffect.CurrentTechnique.Passes.Count; pass++)
				{
/*#if XBOX360
					// Just use 1 pass and render directly!
					// Else Xbox360 will render black rectangles on top of our
					// background, I really can't kill those (strange bug).

					// Use ZeroSourceBlend alpha mode for the final result
					BaseGame.Device.RenderState.AlphaBlendEnable = true;
					BaseGame.Device.RenderState.AlphaBlendOperation = BlendFunction.Add;
					BaseGame.Device.RenderState.SourceBlend = Blend.Zero;
					BaseGame.Device.RenderState.DestinationBlend = Blend.SourceColor;
#else
*
					if (pass == 0)
					{
						blurMapTexture.SetRenderTarget();
						if (blurMapTexture.ZBufferSurface != null)
						{
							remBackBufferSurface = BaseGame.Device.DepthStencilBuffer;
							BaseGame.Device.DepthStencilBuffer =
								blurMapTexture.ZBufferSurface;
						} // if (blurMapTexture.ZBufferSurface)
					} // if
					else
					{
						BaseGame.ResetRenderTarget(false);
						if (blurMapTexture.ZBufferSurface != null)
							BaseGame.Device.DepthStencilBuffer = remBackBufferSurface;

						// Use ZeroSourceBlend alpha mode for the final result
						BaseGame.Device.RenderState.AlphaBlendEnable = true;
						BaseGame.Device.RenderState.AlphaBlendOperation = BlendFunction.Add;
						BaseGame.Device.RenderState.SourceBlend = Blend.Zero;
						BaseGame.Device.RenderState.DestinationBlend = Blend.SourceColor;
					} // else
//#endif

					EffectPass effectPass = xnaEffect.CurrentTechnique.Passes[pass];
					effectPass.Begin();
					VBScreenHelper.Render();
					effectPass.End();

/*#if XBOX360
					break;
#else
*
					if (pass == 0)
					{
						blurMapTexture.Resolve();
						if (blurMap != null)
							blurMap.SetValue(blurMapTexture.XnaTexture);
						xnaEffect.CommitChanges();
					} // if
//#endif
				} // for (pass, <, ++)
				 *
			} // try
			finally
			{
				//xnaEffect.End();

				// Restore z buffer state
				BaseGame.Device.RenderState.DepthBufferEnable = true;
				BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
				// Set u/v addressing back to wrap
				BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
				BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
				// Restore normal alpha blending
				BaseGame.Device.RenderState.BlendFunction = BlendFunction.Add;
				//BaseGame.AlphaMode = BaseGame.AlphaModes.Default;
			} // finally
			 */
		} // ShowShadowsPart1()

		/// <summary>
		/// Show shadows with help of our blur map shader
		/// </summary>
		public void ShowShadowsPart2()
		{
			// Don't use or write to the z buffer
			BaseGame.Device.RenderState.DepthBufferEnable = false;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = false;

			// Make sure we clamp everything to 0-1
			BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
			BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

			if (windowSize != null)
				windowSize.SetValue(
					new float[] { sceneMapTexture.Width, sceneMapTexture.Height });
			if (sceneMap != null)
				sceneMap.SetValue(sceneMapTexture.XnaTexture);

			// Just use the simple blur, the advanced blur will cause too much throuble!
			xnaEffect.CurrentTechnique = xnaEffect.Techniques["ScreenBlur"];

			// Use ZeroSourceBlend alpha mode for the final result
			BaseGame.Device.RenderState.AlphaBlendEnable = true;
			BaseGame.Device.RenderState.AlphaBlendOperation = BlendFunction.Add;
			BaseGame.Device.RenderState.SourceBlend = Blend.Zero;
			BaseGame.Device.RenderState.DestinationBlend = Blend.SourceColor;

			RenderSinglePassShader(VBScreenHelper.Render);

			// Restore z buffer state
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
			// Set u/v addressing back to wrap
			BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
			BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
			// Restore normal alpha blending
			BaseGame.Device.RenderState.BlendFunction = BlendFunction.Add;
			
			// Done!
			return;
			/*obs
			// Only apply post screen blur if texture is valid and effect are valid
			if (blurMapTexture == null ||
				Valid == false ||
				// If the shadow scene map is not yet filled, there is no point
				// continuing here ...
				blurMapTexture.XnaTexture == null)
				return;

			try
			{
				// Don't use or write to the z buffer
				BaseGame.Device.RenderState.DepthBufferEnable = false;
				BaseGame.Device.RenderState.DepthBufferWriteEnable = false;

				// Make sure we clamp everything to 0-1
				BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
				BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;
				// Restore back buffer as render target
				//not required: BaseGame.ResetRenderTarget(false);

				if (blurMap != null)
					blurMap.SetValue(blurMapTexture.XnaTexture);

				xnaEffect.CurrentTechnique = xnaEffect.Techniques[
					BaseGame.CanUsePS20 ? "ScreenAdvancedBlur20" : "ScreenAdvancedBlur"];

				// We must have exactly 2 passes!
				if (xnaEffect.CurrentTechnique.Passes.Count != 2)
					throw new Exception("This shader should have exactly 2 passes!");

				// Render second pass
				xnaEffect.Begin(SaveStateMode.None);

				// Use ZeroSourceBlend alpha mode for the final result
				BaseGame.Device.RenderState.AlphaBlendEnable = true;
				BaseGame.Device.RenderState.AlphaBlendOperation = BlendFunction.Add;
				BaseGame.Device.RenderState.SourceBlend = Blend.Zero;
				BaseGame.Device.RenderState.DestinationBlend = Blend.SourceColor;

				EffectPass effectPass = xnaEffect.CurrentTechnique.Passes[1];
				effectPass.Begin();
				VBScreenHelper.Render();
				effectPass.End();
			} // try
			finally
			{
				xnaEffect.End();

				// Restore z buffer state
				BaseGame.Device.RenderState.DepthBufferEnable = true;
				BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
				// Set u/v addressing back to wrap
				BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
				BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
				// Restore normal alpha blending
				//BaseGame.Device.RenderState.BlendFunction = BlendFunction.Add;
				//BaseGame.AlphaMode = BaseGame.AlphaModes.Default;
			} // finally
			 */
		} // ShowShadows()
		#endregion
	} // class ShadowMapBlur
} // namespace DungeonQuest.Shaders
