// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Graphics
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:41

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DungeonQuest;
using DungeonQuest.Game;
using DungeonQuest.Helpers;
using System;
using System.IO;
#endregion

namespace DungeonQuest.Graphics
{
	/// <summary>
	/// Texture class helping you with using Xna Textures and handling
	/// possible errors that can happen while loading (stupid Xna
	/// error messages telling you absolutly nothing, so a lot of pre-checks
	/// help you determinate the error before even calling Xna methods).
	/// </summary>
	public class Texture : IDisposable
	{
		#region Variables
		/// <summary>
		/// If you want to add a default extension if none is given, set it here.
		/// If this is empty, no default extension will be applied.
		/// </summary>
		protected const string AddExtensionIfNoneGiven = "dds";

		/// <summary>
		/// Texture filename
		/// </summary>
		protected string texFilename = "";
		/// <summary>
		/// Get filename of texture.
		/// </summary>
		public string Filename
		{
			get
			{
				return texFilename;
			} // get
		} // Filename

		/// <summary>
		/// Size of texture
		/// </summary>
		protected int texWidth, texHeight;

		/// <summary>
		/// Width of texture
		/// </summary>
		public int Width
		{
			get
			{
				return texWidth;
			} // get
		} // Width
		/// <summary>
		/// Height of texture
		/// </summary>
		public int Height
		{
			get
			{
				return texHeight;
			} // get
		} // Height

		/// <summary>
		/// Gfx rectangle
		/// </summary>
		/// <returns>Rectangle</returns>
		public Rectangle GfxRectangle
		{
			get
			{
				return new Rectangle(0, 0, texWidth, texHeight);
			} // get
		} // GfxRectangle

		/// <summary>
		/// Size of half a pixel, will be calculated when size is set.
		/// </summary>
		private Vector2 precaledHalfPixelSize = Vector2.Zero;//.Empty;
		/// <summary>
		/// Get the size of half a pixel, used to correct texture
		/// coordinates when rendering on screen, see Texture.RenderOnScreen.
		/// </summary>
		public Vector2 HalfPixelSize
		{
			get
			{
				return precaledHalfPixelSize;
			} // get
		} // HalfPixelSize

		/// <summary>
		/// Calc half pixel size
		/// </summary>
		protected void CalcHalfPixelSize()
		{
			precaledHalfPixelSize = new Vector2(
				(1.0f / (float)texWidth) / 2.0f,
				(1.0f / (float)texHeight) / 2.0f);
		} // CalcHalfPixelSize()

		/// <summary>
		/// Graphic Texture
		/// </summary>
		protected Microsoft.Xna.Framework.Graphics.Texture2D internalXnaTexture;
		/// <summary>
		/// Xna texture
		/// </summary>
		public virtual Microsoft.Xna.Framework.Graphics.Texture2D XnaTexture
		{
			get
			{
				return internalXnaTexture;
			} // get
		} // XnaTexture

		/// <summary>
		/// Loading succeeded?
		/// </summary>
		protected bool loaded = true;
		/// <summary>
		/// Error?
		/// </summary>
		protected string error = "";

		/// <summary>
		/// Is texture valid? Will be false if loading failed.
		/// </summary>
		public virtual bool Valid
		{
			get
			{
				return loaded &&
					internalXnaTexture != null;
			} // get
		} // Valid

		/// <summary>
		/// Has alpha?
		/// </summary>
		protected bool hasAlpha = false;
		/// <summary>
		/// Has texture alpha information?
		/// </summary>
		public bool HasAlphaPixels
		{
			get
			{
				return hasAlpha;
			} // get
		} // HasAlphaPixels
		#endregion

		#region Constructor
		/// <summary>
		/// Create texture from given filename.
		/// </summary>
		/// <param name="setFilename">Set filename, must be relative and be a
		/// valid file in the textures directory.</param>
		public Texture(string setFilename)
		{
			if (String.IsNullOrEmpty(setFilename))
				throw new ArgumentNullException("setFilename",
					"Unable to create texture without valid filename.");

			// Set content name (cut off extension!)
			texFilename = StringHelper.ExtractFilename(setFilename, true);
			string fullFilename =
				Path.Combine(Directories.ContentDirectory, texFilename);

			// Try to load texture
			try
			{
				// Check if filename does not exist, then try to add ~0
				// Required for collada texture loading!
				if (File.Exists(fullFilename + ".xnb") == false)
				{
	#if !XBOX360
					// If this is not the Xbox 360, then try loading with dds too.
					if (File.Exists(fullFilename + ".dds"))
					{
						// Finally load texture
						internalXnaTexture = Texture2D.FromFile(
							BaseGame.Device, fullFilename + ".dds");

						// Get info from the texture directly.
						texWidth = internalXnaTexture.Width;
						texHeight = internalXnaTexture.Height;

						// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
						// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
						hasAlpha =
							internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
							internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
							internalXnaTexture.Format.ToString().StartsWith("A");

						loaded = true;

						CalcHalfPixelSize();
						return;
					} // if (File.Exists)
	#endif

					fullFilename = StringHelper.CutExtension(fullFilename) + "~0";
				} // if (File.Exists)

				// Try loading as 2d texture
				internalXnaTexture = BaseGame.ContentMan.Load<Texture2D>(fullFilename);

				// Get info from the texture directly.
				texWidth = internalXnaTexture.Width;
				texHeight = internalXnaTexture.Height;

				// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
				// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
				hasAlpha =
					internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
					internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
					internalXnaTexture.Format.ToString().StartsWith("A");

				loaded = true;

				CalcHalfPixelSize();
			} // try
			catch (Exception ex)
			{
#if XBOX360
				// Failed to load
				loaded = false;
				// Create dummy empty texture (to prevent any errors in the game)
				internalXnaTexture = new Texture2D(BaseGame.Device,
					16, 16, 0, TextureUsage.None, SurfaceFormat.Dxt1);
				Log.Write("Failed to load texture " + fullFilename +
					", will use empty texture! Error: " + ex.ToString());
#else
				// Try again with filename
				// Add default extension if none is given
				if (AddExtensionIfNoneGiven.Length > 0 &&
					StringHelper.GetExtension(texFilename).Length == 0)
					texFilename = texFilename + "." + AddExtensionIfNoneGiven;
				// Build full filename
				fullFilename =
					Directories.ContentDirectory + "\\" + texFilename;

				// Check if file exists, else we can't continue loading!
				if (File.Exists(fullFilename))
				{
					// Finally load texture
					internalXnaTexture = Texture2D.FromFile(
						BaseGame.Device, fullFilename);
					
					// Get info from the texture directly.
					texWidth = internalXnaTexture.Width;
					texHeight = internalXnaTexture.Height;

					// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
					// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
					hasAlpha =
						internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
						internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
						internalXnaTexture.Format.ToString().StartsWith("A");

					loaded = true;

					CalcHalfPixelSize();
				} // if (File.Exists)
				else
				{
					// Failed to load
					loaded = false;
					// Create dummy empty texture (to prevent any errors in the game)
					internalXnaTexture = new Texture2D(BaseGame.Device,
						16, 16, 0, TextureUsage.None,
						SurfaceFormat.Dxt1);
					Log.Write("Failed to load texture " + fullFilename +
						", will use empty texture! Error: " + ex.ToString());
				} // else
#endif
			} // catch (ex)
		} // Texture(setFilename)

		/// <summary>
		/// Create texture, protected version for derived classes.
		/// </summary>
		protected Texture()
		{
		} // Texture()

		/// <summary>
		/// Create texture by just assigning a Texture2D.
		/// </summary>
		/// <param name="tex">Tex</param>
		public Texture(Texture2D tex)
		{
			internalXnaTexture = tex;
			
			// Get info from the texture directly.
			texWidth = internalXnaTexture.Width;
			texHeight = internalXnaTexture.Height;

			// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
			// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
			hasAlpha =
				internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
				internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
				internalXnaTexture.Format.ToString().StartsWith("A");

			loaded = true;

			CalcHalfPixelSize();
		} // Texture(tex)
		#endregion

		#region Disposing
		/// <summary>
		/// Dispose
		/// </summary>
		public virtual void Dispose()
		{
			if (internalXnaTexture != null)
				internalXnaTexture.Dispose();
			internalXnaTexture = null;
			loaded = false;
		} // Dispose()
		#endregion

		#region Render on screen
		/// <summary>
		/// Render texture at rect directly on screen using pixelRect.
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreen(Rectangle rect, Rectangle pixelRect)
		{
			SpriteHelper.AddSpriteToRender(this, rect, pixelRect);
		} // RenderOnScreen(rect, pixelRect)

		/// <summary>
		/// Render on screen
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelX">Pixel x</param>
		/// <param name="pixelY">Pixel y</param>
		/// <param name="pixelWidth">Pixel width</param>
		/// <param name="pixelHeight">Pixel height</param>
		public void RenderOnScreen(Rectangle rect,
			int pixelX, int pixelY, int pixelWidth, int pixelHeight)
		{
			SpriteHelper.AddSpriteToRender(this, rect,
				new Rectangle(pixelX, pixelY, pixelWidth, pixelHeight));
		} // RenderOnScreen(rect, pixelX, pixelY)

		/// <summary>
		/// Render on screen
		/// </summary>
		/// <param name="pos">Position</param>
		public void RenderOnScreen(Point pos)
		{
			SpriteHelper.AddSpriteToRender(this,
				new Rectangle(pos.X, pos.Y, texWidth, texHeight),
				new Rectangle(0, 0, texWidth, texHeight));
		} // RenderOnScreen(pos)

		/// <summary>
		/// Render on screen
		/// </summary>
		/// <param name="renderRect">Render rectangle</param>
		public void RenderOnScreen(Rectangle renderRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				renderRect, GfxRectangle);
		} // RenderOnScreen(renderRect)

		/// <summary>
		/// Render on screen no alpha
		/// </summary>
		/// <param name="renderRect">Render rectangle</param>
		public void RenderOnScreenNoAlpha(Rectangle renderRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				renderRect, GfxRectangle, Color.White, SpriteBlendMode.None);
		} // RenderOnScreenNoAlpha(renderRect)

		/// <summary>
		/// Render on screen relative for 1024x640 (16:9) graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenRelative16To9(int relX, int relY,
			Rectangle pixelRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				BaseGame.CalcRectangle(
				relX, relY, pixelRect.Width, pixelRect.Height),
				pixelRect);
		} // RenderOnScreenRelative(relX, relY, pixelRect)

		/// <summary>
		/// Render on screen relative 1024x786 (4:3)
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenRelative4To3(int relX, int relY,
			Rectangle pixelRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				BaseGame.CalcRectangleKeep4To3(
				relX, relY, pixelRect.Width, pixelRect.Height),
				pixelRect);
		} // RenderOnScreenRelative4To3(relX, relY, pixelRect)

		/// <summary>
		/// Render on screen relative for 1600px width graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenRelative1600(
			int relX, int relY, Rectangle pixelRect)
		{
			SpriteHelper.AddSpriteToRender(this,
				BaseGame.CalcRectangle1600(
				relX, relY, pixelRect.Width, pixelRect.Height),
				pixelRect);
		} // RenderOnScreenRelative1600(relX, relY, pixelRect)

		/// <summary>
		/// Render texture at rect directly on screen using texture cordinates.
		/// This method allows to render with specific color and alpha values.
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		/// <param name="color">Color</param>
		public void RenderOnScreen(Rectangle rect, Rectangle pixelRect,
			Color color)
		{
			SpriteHelper.AddSpriteToRender(this, rect, pixelRect, color);
		} // RenderOnScreen(rect, pixelRect, color)

		/// <summary>
		/// Render texture at rect directly on screen using texture cordinates.
		/// This method allows to render with specific color and alpha values.
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		/// <param name="color">Color</param>
		public void RenderOnScreen(Rectangle rect, Rectangle pixelRect,
			Color color, float alphaValue)
		{
			SpriteHelper.AddSpriteToRender(this, rect, pixelRect,
				ColorHelper.ApplyAlphaToColor(color, alphaValue));
		} // RenderOnScreen(rect, pixelRect, color)

		/// <summary>
		/// Render on screen
		/// </summary>
		/// <param name="rect">Rectangle</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		/// <param name="color">Color</param>
		/// <param name="blendMode">Blend mode</param>
		public void RenderOnScreen(Rectangle rect, Rectangle pixelRect,
			Color color, SpriteBlendMode blendMode)
		{
			SpriteHelper.AddSpriteToRender(this, rect, pixelRect, color, blendMode);
		} // RenderOnScreen(rect, pixelRect, color)
		#endregion

		#region Rendering on screen with rotation
		/// <summary>
		/// Render on screen with rotation
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		/// <param name="rotation">Rotation</param>
		public void RenderOnScreenWithRotation(
			Rectangle rect, Rectangle pixelRect,
			float rotation, Vector2 rotationPoint)
		{
			//Rectangle rect =
			//	new Rectangle(pos.X, pos.Y, pixelRect.Width, pixelRect.Height);
			SpriteHelper.AddSpriteToRender(this,
				rect, pixelRect, rotation,
				//new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2));
				rotationPoint);
		} // RenderOnScreenWithRotation(pos, pixelRect, rotation)

		/// <summary>
		/// Render on screen with rotation
		/// </summary>
		/// <param name="rocketPos">Rocket position</param>
		/// <param name="widthAndHeightDiv2">Width and height div 2</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenWithRotation(
			Point pos, int widthAndHeightDiv2, float rotation,
			Rectangle pixelRect, Vector2 rotationPos)
		{
			RenderOnScreenWithRotation(
				new Rectangle(pos.X, pos.Y,// - widthAndHeightDiv2, pos.Y - widthAndHeightDiv2,
				widthAndHeightDiv2 * 2, widthAndHeightDiv2 * 2),
				pixelRect, rotation,
				rotationPos);
		} // RenderOnScreenWithRotation(rocketPos, widthAndHeightDiv2, rotation)

		/// <summary>
		/// Render on screen with rotation
		/// </summary>
		/// <param name="rocketPos">Rocket position</param>
		/// <param name="widthAndHeightDiv2">Width and height div 2</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="pixelRect">Pixel rectangle</param>
		public void RenderOnScreenWithRotation(
			Point pos, int widthAndHeightDiv2, float rotation,
			Rectangle pixelRect, Color col)
		{
			SpriteHelper.AddSpriteToRender(this,
				new Rectangle(pos.X - widthAndHeightDiv2, pos.Y - widthAndHeightDiv2,
				widthAndHeightDiv2 * 2, widthAndHeightDiv2 * 2),
				pixelRect, rotation,
				new Vector2(pos.X, pos.Y),
				col);
		} // RenderOnScreenWithRotation(rocketPos, widthAndHeightDiv2, rotation)
		#endregion

		#region To string
		/// <summary>
		/// To string
		/// </summary>
		public override string ToString()
		{
			return "Texture(filename=" + texFilename +
				", width=" + texWidth +
				", height=" + texHeight +
				", xnaTexture=" + (internalXnaTexture != null ? "valid" : "null") +
				", hasAlpha=" + hasAlpha + ")";
		} // ToString()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test textures
		/// </summary>
		public static void TestRenderTexture()
		{
			Texture testTexture = null;
			Texture testTexture2 = null;

			TestGame.Start("TestTextures",
				delegate
				{
					testTexture = new Texture("MainMenu");
					testTexture2 = new Texture("Hud");
				},
				delegate
				{
					/*tst1
					testTexture.RenderOnScreen(
						new Rectangle(100, 100, 256, 256),
						testTexture.GfxRectangle,
						Color.White, SpriteBlendMode.Additive);

					// Use alpha blending
					testTexture.RenderOnScreen(
						new Rectangle(Input.MousePos.X, Input.MousePos.Y, 512, 512),
						testTexture.GfxRectangle, Color.Red, SpriteBlendMode.Additive);
					 */

					Point rocketPos = new Point(100, 100);
					float rocketRotation = Input.MousePos.X / 500.0f;
					Rectangle MiniMapRocketRect = new Rectangle(143, 21, 24, 24);
					//*
					testTexture2.RenderOnScreenWithRotation(
						rocketPos, MiniMapRocketRect.Width / 3,
						rocketRotation,
						MiniMapRocketRect,
						new Vector2(MiniMapRocketRect.Width / 2,
						MiniMapRocketRect.Width / 2.0f));
					/*
					testTexture2.RenderOnScreen(
						new Rectangle(200, 100,
						MiniMapRocketRect.Width, MiniMapRocketRect.Height),
						MiniMapRocketRect);
					 *

					Vector2 rotationPoint = new Vector2(
						MiniMapRocketRect.Width / 2,
						MiniMapRocketRect.Height / 2);
					testTexture2.RenderOnScreenWithRotation(
						new Rectangle(rocketPos.X, rocketPos.Y,
						MiniMapRocketRect.Width, MiniMapRocketRect.Height),
						MiniMapRocketRect,
						rocketRotation, rotationPoint);
					 */
				});
		} // TestTextures()
#endif
		#endregion
	} // class Texture
} // namespace DungeonQuest.Graphics

