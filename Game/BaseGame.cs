// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Game
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:38

#region Using directives
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.Properties;
using DungeonQuest.Shaders;
using DungeonQuest.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Texture = DungeonQuest.Graphics.Texture;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Base game class for all the basic game support.
	/// Connects all our helper classes together and makes our live easier!
	/// </summary>
	public class BaseGame : Microsoft.Xna.Framework.Game
	{
		#region Constants
		/// <summary>
		/// Background color
		/// </summary>
		public static Color BackgroundColor = Color.Black;

		/// <summary>
		/// Field of view and near and far plane distances for the
		/// ProjectionMatrix creation.
		/// Use a standard field of view of 90 degrees, and use 1 for near plane
		/// and 1000 for far plane. Field of view might be changed for speed items.
		/// </summary>
		public const float FieldOfView = (float)Math.PI / 3,//1.8f,//1.75f,//2.0f,
			NearPlane = 0.1f,
			FarPlane = 175;//100;//50;//250;//150;
		#endregion

		#region Variables
		/// <summary>
		/// Graphics
		/// </summary>
		protected GraphicsDeviceManager graphics;
		/// <summary>
		/// Content
		/// </summary>
		protected static ContentManager content;

		/// <summary>
		/// Remember graphics device and allow the useage wherever needed via
		/// the Device property.
		/// </summary>
		private static GraphicsDevice device;

		/// <summary>
		/// BaseGame of our game.
		/// </summary>
		protected static int width, height;

		/// <summary>
		/// Back buffer depth format
		/// </summary>
		static DepthFormat backBufferDepthFormat = DepthFormat.Depth32;

		/// <summary>
		/// Remember multi sample type in Initialize for later use
		/// in RenderToTexture!
		/// </summary>
		static MultiSampleType remMultiSampleType = MultiSampleType.None;
		/// <summary>
		/// Remember multi sample quality.
		/// </summary>
		static int remMultiSampleQuality = 0;

        /// <summary>
        /// UI Renderer helper class for all 2d rendering.
        /// </summary>
        protected static UIRenderer ui = null;

		/// <summary>
		/// Quit game, used to check if other threads have to be quitted too.
		/// </summary>
		private static bool quit = false;

		/// <summary>
		/// Aspect ratio of our current resolution
		/// </summary>
		public static float aspectRatio = 1.0f;

		/// <summary>
		/// Render delegate for rendering methods, also used for many other
		/// methods.
		/// </summary>
		public delegate void RenderDelegate();

		/// <summary>
		/// Matrices for shaders. Used in a similar way than in Rocket Commander,
		/// but since we don't have a fixed function pipeline here we just use
		/// these values in the shader. Make sure to set all matrices before
		/// calling a shader. Inside a shader you have to update the shader
		/// parameter too, just setting the WorldMatrix alone does not work.
		/// </summary>
		private static Matrix worldMatrix,
			viewMatrix,
			projectionMatrix;

		/// <summary>
		/// Precalculated view projection matrix for the Convert3DPointTo2D,
		/// IsInFrontOfCamera and IsVisible methods. Calculated in ViewMatrix.
		/// </summary>
		private static Matrix viewProjMatrix;

		
		/// <summary>
		/// Line manager 2D
		/// </summary>
		private static LineManager2D lineManager2D = null;

        /*unused
        /// <summary>
        /// Line manager 3D
        /// </summary>
        private static LineManager3D lineManager3D = null;

        /// <summary>
        /// Mesh render manager to render meshes of models in a highly
        /// optimized manner. We don't really have anything stored in here
        /// except for a sorted list on how to render everything based on the
        /// techniques and the materials and links to the renderable meshes.
        /// </summary>
        private static MeshRenderManager meshRenderManager =
            new MeshRenderManager();
         */

		/// <summary>
		/// Texture font to render text on the screen.
		/// </summary>
		private static TextureFont font;

		/// <summary>
		/// Camera for everything, used also for the rocket input.
		/// </summary>
		public static BaseCamera camera = null;
		//obs: public static SimpleCamera camera = null;
    //public static RotationCamera camera = null;
		//public static ThirdPersonCamera camera = null;

		/// <summary>
		/// Post screen effect: Bloom and motion blur effect, every cool :)
		/// </summary>
		protected static PostScreenGlow glowShader = null;

		/// <summary>
		/// Glow shader
		/// </summary>
		public static PostScreenGlow GlowShader
		{
			get
			{
				return glowShader;
			} // get
		} // GlowShader

		/// <summary>
		/// Effect manager for all the cool effects in this game (mostly billboard effects).
		/// </summary>
		public static EffectManager effectManager = null;
		#endregion

		#region Properties
		#region Calc rectangle helpers
		/// <summary>
		/// XToRes helper method to convert 1024x640 to the current
		/// screen resolution. Used to position UI elements.
		/// </summary>
		/// <param name="xIn1024px">X in 1024px width resolution</param>
		/// <returns>Int</returns>
		public static int XToRes(int xIn1024px)
		{
			return (int)Math.Round(xIn1024px * BaseGame.Width / 1024.0f);
		} // XToRes(xIn1024px)

		/// <summary>
		/// YToRes helper method to convert 1024x768 to the current
		/// screen resolution. Used to position UI elements.
		/// </summary>
		/// <param name="yIn768px">Y in 768px</param>
		/// <returns>Int</returns>
		public static int YToRes(int yIn768px)
		{
			return (int)Math.Round(yIn768px * BaseGame.Height / 768.0f);
		} // YToRes(yIn768px)

		/// <summary>
		/// XTo res 1600
		/// </summary>
		/// <param name="xIn1600px">X in 1600px</param>
		/// <returns>Int</returns>
		public static int XToRes1600(int xIn1600px)
		{
			return (int)Math.Round(xIn1600px * BaseGame.Width / 1600.0f);
		} // XToRes1600(xIn1600px)

		/// <summary>
		/// YTo res 1200
		/// </summary>
		/// <param name="yIn768px">Y in 1200px</param>
		/// <returns>Int</returns>
		public static int YToRes1200(int yIn1200px)
		{
			return (int)Math.Round(yIn1200px * BaseGame.Height / 1200.0f);
		} // YToRes1200(yIn1200px)

		/// <summary>
		/// XTo res 1400
		/// </summary>
		/// <param name="xIn1400px">X in 1400px</param>
		/// <returns>Int</returns>
		public static int XToRes1400(int xIn1400px)
		{
			return (int)Math.Round(xIn1400px * BaseGame.Width / 1400.0f);
		} // XToRes1400(xIn1400px)

		/// <summary>
		/// YTo res 1200
		/// </summary>
		/// <param name="yIn1050px">Y in 1050px</param>
		/// <returns>Int</returns>
		public static int YToRes1050(int yIn1050px)
		{
			return (int)Math.Round(yIn1050px * BaseGame.Height / 1050.0f);
		} // YToRes1050(yIn1050px)

		/// <summary>
		/// Calc rectangle, helper method to convert from our images (1024)
		/// to the current resolution. Everything will stay in the 16/9
		/// format of the textures.
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="width">Width</param>
		/// <param name="height">Height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangle(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 640.0f;
			return new Rectangle(
				(int)Math.Round(relX * widthFactor),
				(int)Math.Round(relY * heightFactor),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor));
		} // CalcRectangle(x, y, width)

		/// <summary>
		/// Calc rectangle with bounce effect, same as CalcRectangle, but sizes
		/// the resulting rect up and down depending on the bounceEffect value.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <param name="bounceEffect">Bounce effect</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleWithBounce(
			int relX, int relY, int relWidth, int relHeight, float bounceEffect)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 640.0f;
			float middleX = (relX + relWidth / 2) * widthFactor;
			float middleY = (relY + relHeight / 2) * heightFactor;
			float retWidth = relWidth * widthFactor * bounceEffect;
			float retHeight = relHeight * heightFactor * bounceEffect;
			return new Rectangle(
				(int)Math.Round(middleX - retWidth / 2),
				(int)Math.Round(middleY - retHeight / 2),
				(int)Math.Round(retWidth),
				(int)Math.Round(retHeight));
		} // CalcRectangleWithBounce(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle, same method as CalcRectangle, but keep the 4 to 3
		/// ratio for the image. The Rect will take same screen space in
		/// 16:9 and 4:3 modes. E.g. Buttons should be displayed this way.
		/// Should be used for 1024px width graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleKeep4To3(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 768.0f;
			return new Rectangle(
				(int)Math.Round(relX * widthFactor),
				(int)Math.Round(relY * heightFactor),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor));
		} // CalcRectangleKeep4To3(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle, same method as CalcRectangle, but keep the 4 to 3
		/// ratio for the image. The Rect will take same screen space in
		/// 16:9 and 4:3 modes. E.g. Buttons should be displayed this way.
		/// Should be used for 1024px width graphics.
		/// </summary>
		/// <param name="gfxRect">Gfx rectangle</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleKeep4To3(
			Rectangle gfxRect)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 768.0f;
			return new Rectangle(
				(int)Math.Round(gfxRect.X * widthFactor),
				(int)Math.Round(gfxRect.Y * heightFactor),
				(int)Math.Round(gfxRect.Width * widthFactor),
				(int)Math.Round(gfxRect.Height * heightFactor));
		} // CalcRectangleKeep4To3(gfxRect)

		/// <summary>
		/// Calc rectangle for 1600px width graphics.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangle1600(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1600.0f;
			float heightFactor = (height / 1200.0f);
			return new Rectangle(
				(int)Math.Round(relX * widthFactor),
				(int)Math.Round(relY * heightFactor),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor));
		} // CalcRectangle1600(relX, relY, relWidth)
		
		/// <summary>
		/// Calc rectangle 2000px, just a helper to scale stuff down
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangle2000(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 2000.0f;
			float heightFactor = (height / 1500.0f);
			return new Rectangle(
				(int)Math.Round(relX * widthFactor),
				(int)Math.Round(relY * heightFactor),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor));
		} // CalcRectangle2000(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle keep 4 to 3 align bottom
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleKeep4To3AlignBottom(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor16To9 = height / 640.0f;
			float heightFactor4To3 = height / 768.0f;
			return new Rectangle(
				(int)(relX * widthFactor),
				(int)(relY * heightFactor16To9) -
				(int)Math.Round(relHeight * heightFactor4To3),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor4To3));
		} // CalcRectangleKeep4To3AlignBottom(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle keep 4 to 3 align bottom right
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relWidth">Rel width</param>
		/// <param name="relHeight">Rel height</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleKeep4To3AlignBottomRight(
			int relX, int relY, int relWidth, int relHeight)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor16To9 = height / 640.0f;
			float heightFactor4To3 = height / 768.0f;
			return new Rectangle(
				(int)(relX * widthFactor) -
				(int)Math.Round(relWidth * widthFactor),
				(int)(relY * heightFactor16To9) -
				(int)Math.Round(relHeight * heightFactor4To3),
				(int)Math.Round(relWidth * widthFactor),
				(int)Math.Round(relHeight * heightFactor4To3));
		} // CalcRectangleKeep4To3AlignBottomRight(relX, relY, relWidth)

		/// <summary>
		/// Calc rectangle centered with given height.
		/// This one uses relX and relY points as the center for our rect.
		/// The relHeight is then calculated and we align everything
		/// with help of gfxRect (determinating the width).
		/// Very useful for buttons, logos and other centered UI textures.
		/// </summary>
		/// <param name="relX">Rel x</param>
		/// <param name="relY">Rel y</param>
		/// <param name="relHeight">Rel height</param>
		/// <param name="gfxRect">Gfx rectangle</param>
		/// <returns>Rectangle</returns>
		public static Rectangle CalcRectangleCenteredWithGivenHeight(
			int relX, int relY, int relHeight, Rectangle gfxRect)
		{
			float widthFactor = width / 1024.0f;
			float heightFactor = height / 640.0f;
			int rectHeight = (int)Math.Round(relHeight * heightFactor);
			// Keep aspect ratio
			int rectWidth = (int)Math.Round(
				gfxRect.Width * rectHeight / (float)gfxRect.Height);
			return new Rectangle(
				Math.Max(0, (int)Math.Round(relX * widthFactor) - rectWidth / 2),
				Math.Max(0, (int)Math.Round(relY * heightFactor) - rectHeight / 2),
				rectWidth, rectHeight);
		} // CalcRectangleCenteredWithGivenHeight(relX, relY, relHeight)
		#endregion

		#region Other properties
		/// <summary>
		/// Graphics device access for the whole the engine.
		/// </summary>
		/// <returns>Graphics device</returns>
		public static GraphicsDevice Device
		{
			get
			{
				return device;
			} // get
		} // Device

		/// <summary>
		/// Quit game, used to check if other threads have to be quitted too.
		/// </summary>
		public static bool Quit
		{
			get
			{
				return quit;
			} // get
		} // Quit

		/// <summary>
		/// Content manager access for the whole the engine.
		/// </summary>
		/// <returns>Content manager</returns>
		public static ContentManager ContentMan
		{
			get
			{
				return content;
			} // get
		} // Content

		/// <summary>
		/// Width of our backbuffer we render into.
		/// </summary>
		/// <returns>Int</returns>
		public static int Width
		{
			get
			{
				return width;
			} // get
		} // Width

		/// <summary>
		/// Height of our backbuffer we render into.
		/// </summary>
		/// <returns>Int</returns>
		public static int Height
		{
			get
			{
				return height;
			} // get
		} // Height

		/// <summary>
		/// BaseGame rectangle
		/// </summary>
		/// <returns>Rectangle</returns>
		public static Rectangle ResolutionRect
		{
			get
			{
				return new Rectangle(0, 0, width, height);
			} // get
		} // ResolutionRect

		/// <summary>
		/// Alpha blending
		/// </summary>
		/// <returns>Bool</returns>
		public static bool AlphaBlending
		{
			set
			{
				if (value)
				{
					device.RenderState.AlphaBlendEnable = true;
					device.RenderState.SourceBlend = Blend.SourceAlpha;
					device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
				} // if (value)
				else
					device.RenderState.AlphaBlendEnable = false;
			} // set
		} // AlphaBlending

		/// <summary>
		/// Light direction
		/// </summary>
		private static Vector3 lightDir =
			new Vector3(0, 0, 1);

		/// <summary>
		/// Light direction, mainly used for lighting and shadow mapping.
		/// </summary>
		public static Vector3 LightDirection
		{
			get
			{
				// Note: Not really used here yet, only for the Model.Render method!
				// Note2: Because we have no valid tangent data, the model will
				// not look right yet, read Chapter 7 on how to fix that.
				//return new Vector3(0, 0, 1);
				//if (Input.KeyboardSpaceJustPressed)
					//lightDir = new Vector3(1, 0, 0);

				return lightDir;
			} // get
			set
			{
				lightDir = value;
				lightDir.Normalize();
			} // set
		} // LightDirection
		#endregion

		#region Matrices
		/// <summary>
		/// World matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix WorldMatrix
		{
			get
			{
				return worldMatrix;
			} // get
			set
			{
				worldMatrix = value;
				// Update worldViewProj here?
			} // set
		} // WorldMatrix

		/// <summary>
		/// View matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix ViewMatrix
		{
			get
			{
				return viewMatrix;
			} // get
			set
			{
				// Set view matrix, usually only done in ChaseCamera.Update!
				viewMatrix = value;

				// Update camera pos and rotation, used all over the game!
				invViewMatrix = Matrix.Invert(viewMatrix);
				CameraPos = invViewMatrix.Translation;
				cameraRotation = Vector3.TransformNormal( 
					new Vector3(0, 0, 1), invViewMatrix);

				// Precalculate viewProjMatrix for 3d helper methods below.
				viewProjMatrix = viewMatrix * projectionMatrix;
			} // set
		} // ViewMatrix

		/// <summary>
		/// Projection matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix ProjectionMatrix
		{
			get
			{
				return projectionMatrix;
			} // get
			set
			{
				projectionMatrix = value;
				// Update worldViewProj here?
			} // set
		} // ProjectionMatrix

		/// <summary>
		/// Camera pos, updated each loop in Update()!
		/// Public to allow easy access from everywhere, will be called a lot each
		/// frame, for example Model.Render uses this for distance checks.
		/// </summary>
		public static Vector3 CameraPos;

		/// <summary>
		/// Camera rotation, used to compare objects for visibility.
		/// </summary>
		private static Vector3 cameraRotation = new Vector3(0, 0, 1);

		/// <summary>
		/// Camera rotation
		/// </summary>
		/// <returns>Vector 3</returns>
		public static Vector3 CameraRotation
		{
			get
			{
				return cameraRotation;
			} // get
		} // CameraRotation

		/// <summary>
		/// Remember inverse view matrix.
		/// </summary>
		private static Matrix invViewMatrix;

		/// <summary>
		/// Inverse view matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix InverseViewMatrix
		{
			get
			{
				return invViewMatrix;//Matrix.Invert(ViewMatrix);
			} // get
		} // InverseViewMatrix

		/// <summary>
		/// View projection matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix ViewProjectionMatrix
		{
			get
			{
				return viewProjMatrix;// ViewMatrix* ProjectionMatrix;
			} // get
		} // ViewProjectionMatrix

		/// <summary>
		/// World view projection matrix
		/// </summary>
		/// <returns>Matrix</returns>
		public static Matrix WorldViewProjectionMatrix
		{
			get
			{
				return WorldMatrix * viewProjMatrix;// ViewMatrix* ProjectionMatrix;
			} // get
		} // WorldViewProjectionMatrix
		#endregion

		#region Frames per second
		/// <summary>
		/// Elapsed time this frame in ms. Always have something valid here
		/// in case we devide through this values!
		/// </summary>
		private static float elapsedTimeThisFrameInMs = 0.001f, totalTimeMs = 0;//,
			//lastFrameTotalTimeMs = 0;

		/// <summary>
		/// Helper for calculating frames per second. 
		/// </summary>
		private static float startTimeThisSecond = 0;

		/// <summary>
		/// For more accurate frames per second calculations,
		/// just count for one second, then fpsLastSecond is updated.
		/// Btw: Start with 1 to help some tests avoid the devide through zero
		/// problem.
		/// </summary>
		private static int
			frameCountThisSecond = 0,
			totalFrameCount = 0,
			fpsLastSecond = 60;

		/// <summary>
		/// Fps
		/// </summary>
		/// <returns>Int</returns>
		public static int Fps
		{
			get
			{
				return fpsLastSecond;
			} // get
		} // Fps

		/*suxx, just use fpsInterpolated, it more accurate than overall checks
		/// <summary>
		/// Fps after 10 seconds
		/// </summary>
		/// <returns>Int</returns>
		public static int FpsAfter10Seconds
		{
			get
			{
				// Return 100 if 10 seconds have not passed yet.
				return TotalTime < 10 ? 100 :
					// Else return total frames devided by the game time in sec.
					(int)(totalFrameCount / TotalTime);
			} // get
		} // FpsAfter10Seconds
		 */

		/// <summary>
		/// Interpolated fps over the last 10 seconds.
		/// Obviously goes down if our framerate is low.
		/// </summary>
		private static float fpsInterpolated = 100.0f;

		/// <summary>
		/// Total frames
		/// </summary>
		/// <returns>Int</returns>
		public static int TotalFrames
		{
			get
			{
				return totalFrameCount;
			} // get
		} // TotalFrames
		#endregion

		#region Timing stuff
		/// <summary>
		/// Elapsed time this frame in ms
		/// </summary>
		/// <returns>Int</returns>
		public static float ElapsedTimeThisFrameInMs
		{
			get
			{
				return elapsedTimeThisFrameInMs;
			} // get
		} // ElapsedTimeThisFrameInMs

		/// <summary>
		/// Total time in seconds
		/// </summary>
		/// <returns>Int</returns>
		public static float TotalTime
		{
			get
			{
				return totalTimeMs / 1000.0f;
			} // get
		} // TotalTime

		/// <summary>
		/// Total time ms
		/// </summary>
		/// <returns>Float</returns>
		public static float TotalTimeMs
		{
			get
			{
				return totalTimeMs;
			} // get
		} // TotalTimeMs

		/// <summary>
		/// Move factor per second, when we got 1 fps, this will be 1.0f,
		/// when we got 100 fps, this will be 0.01f.
		/// </summary>
		public static float MoveFactorPerSecond
		{
			get
			{
				return elapsedTimeThisFrameInMs / 1000.0f;
			} // get
		} // MoveFactorPerSecond

		/// <summary>
		/// Every ms. Returns true if numberOfMsToCheck has passed.
		/// E.g. checking every 100ms works this way:
		/// return (oldElapsedTimeMs/100 != elapsedTimeMs/100);
		/// </summary>
		/// <param name="numberOfMsToCheck">Number of ms to check</param>
		/// <returns>Bool</returns>
		public static bool EveryMs(int numberOfMsToCheck)
		{
			return (int)(totalTimeMs - elapsedTimeThisFrameInMs) / numberOfMsToCheck !=
				(int)totalTimeMs / numberOfMsToCheck;
		} // EveryMs(numberOfMsToCheck)

		/// <summary>
		/// Every ms
		/// </summary>
		/// <param name="numberOfMsToCheck">Number of ms to check</param>
		/// <param name="offsetMs">Offset ms</param>
		/// <returns>Bool</returns>
		public static bool EveryMs(int numberOfMsToCheck, int offsetMs)
		{
			return (int)(totalTimeMs - (offsetMs + elapsedTimeThisFrameInMs)) /
				numberOfMsToCheck !=
				(int)(totalTimeMs - offsetMs) / numberOfMsToCheck;
		} // EveryMs(numberOfMsToCheck, offsetMs)
		#endregion

		#region Settings
		/// <summary>
		/// Performance setting
		/// </summary>
		public enum PerformanceSetting
		{
			Fastest,
			Medium,
			Quality,
		} // enum PerformanceSetting

		public static PerformanceSetting GraphicsPerformanceSetting
		{
			get
			{
				return GameSettings.Default.PerformanceSettings == 0 ?
					PerformanceSetting.Fastest :
					GameSettings.Default.PerformanceSettings == 1 ?
					PerformanceSetting.Medium :
					PerformanceSetting.Quality;
			} // get
			set
			{
				int newSetting = value == PerformanceSetting.Fastest ? 0 :
					value == PerformanceSetting.Medium ? 1 : 2;
				GameSettings.Default.PerformanceSettings = newSetting;
			} // set
		} // GraphicsPerformanceSetting
		#endregion

		#region Enable or disable alpha blending
		/// <summary>
		/// Enable alpha blending with default SourceAlpha - InvSourceAlpha
		/// </summary>
		public static void EnableAlphaBlending()
		{
			device.RenderState.AlphaBlendEnable = true;
			device.RenderState.SourceBlend = Blend.SourceAlpha;
			device.RenderState.DestinationBlend = Blend.InverseSourceAlpha;
		} // EnableAlphaBlending()

		/// <summary>
		/// Disable alpha blending
		/// </summary>
		public static void DisableAlphaBlending()
		{
			device.RenderState.AlphaBlendEnable = false;
		} // DisableAlphaBlending()
		#endregion

		#region Enable or disable linear texture filtering
		/// <summary>
		/// Enable linear texture filtering
		/// </summary>
		public static void EnableLinearTextureFiltering()
		{
			device.SamplerStates[0].MinFilter = TextureFilter.Linear;
			device.SamplerStates[0].MagFilter = TextureFilter.Linear;
			device.SamplerStates[0].MipFilter = TextureFilter.Linear;
		} // EnableLinearTextureFiltering()

		/// <summary>
		/// Disable linear texture filtering
		/// </summary>
		public static void DisableLinearTextureFiltering()
		{
			device.SamplerStates[0].MinFilter = TextureFilter.Point;
			device.SamplerStates[0].MagFilter = TextureFilter.Point;
			device.SamplerStates[0].MipFilter = TextureFilter.Point;
		} // DisableLinearTextureFiltering()
		#endregion

		#region MeshRenderManager
		/*unused
		/// <summary>
		/// Mesh render manager to render meshes of models in a highly
		/// optimized manner.
		/// </summary>
		/// <returns>Mesh render manager</returns>
		public static MeshRenderManager MeshRenderManager
		{
			get
			{
				return meshRenderManager;
			} // get
		} // MeshRenderManager
		 */
		#endregion

		#region Other properties
		/// <summary>
		/// Back buffer depth format
		/// </summary>
		/// <returns>Surface format</returns>
		public static DepthFormat BackBufferDepthFormat
		{
			get
			{
				return backBufferDepthFormat;
			} // get
		} // BackBufferDepthFormat

		/// <summary>
		/// MultiSampleType
		/// </summary>
		public static MultiSampleType MultiSampleType
		{
			get
			{
				return remMultiSampleType;
			} // get
		} // MultiSampleType

		/// <summary>
		/// Multi sample quality
		/// </summary>
		public static int MultiSampleQuality
		{
			get
			{
				return remMultiSampleQuality;
			} // get
		} // MultiSampleQuality

		/// <summary>
		/// Game font
		/// </summary>
		/// <returns>Texture font</returns>
		public static TextureFont GameFont
		{
			get
			{
				return font;
			} // get
		} // GameFont

		/// <summary>
		/// Use PS
		/// </summary>
		/// <returns>Bool</returns>
		public static bool UsePS
		{
			get
			{
				return GameSettings.Default.PerformanceSettings > 0;
			} // get
		} // UsePS

		/// <summary>
		/// Use PS 2.0
		/// </summary>
		/// <returns>Bool</returns>
		public static bool UsePS20
		{
			get
			{
				return GameSettings.Default.PerformanceSettings > 1;
			} // get
		} // UsePS20

        private static bool alreadyCheckedOptionsAndPSVersion = false,
            canUsePS20 = false,
            canUsePS30 = false;

        /// <summary>
        /// Can use PS20
        /// </summary>
        /// <returns>Bool</returns>
        public static bool CanUsePS20
        {
            get
            {
                if (alreadyCheckedOptionsAndPSVersion == false)
                    CheckOptionsAndPSVersion();

                return canUsePS20;
            } // get
        } // CanUsePS20

        /// <summary>
        /// Can use PS30
        /// </summary>
        /// <returns>Bool</returns>
        public static bool CanUsePS30
        {
            get
            {
                if (alreadyCheckedOptionsAndPSVersion == false)
                    CheckOptionsAndPSVersion();

                return canUsePS30;
            } // get
        } // CanUsePS30

        private static bool highDetail = true;
        /// <summary>
        /// High detail
        /// </summary>
        /// <returns>Bool</returns>
        public static bool HighDetail
        {
            get
            {
                if (alreadyCheckedOptionsAndPSVersion == false)
                    CheckOptionsAndPSVersion();

                return highDetail;
            } // get
        } // HighDetail

        private static bool allowShadowMapping = true;
        /// <summary>
        /// Allow shadow mapping
        /// </summary>
        /// <returns>Bool</returns>
        public static bool AllowShadowMapping
        {
            get
            {
                if (alreadyCheckedOptionsAndPSVersion == false)
                    CheckOptionsAndPSVersion();

                return allowShadowMapping;
            } // get
        } // AllowShadowMapping

        private static bool usePostScreenShaders = true;
        /// <summary>
        /// Use post screen shaders
        /// </summary>
        /// <returns>Bool</returns>
        public static bool UsePostScreenShaders
        {
            get
            {
                if (alreadyCheckedOptionsAndPSVersion == false)
                    CheckOptionsAndPSVersion();

                return usePostScreenShaders;
            } // get
        } // UsePostScreenShaders

        /// <summary>
        /// Check options and PS version
        /// </summary>
        internal static void CheckOptionsAndPSVersion()
        {
            if (device == null)
                throw new InvalidOperationException("Device is not created yet!");

            alreadyCheckedOptionsAndPSVersion = true;

            usePostScreenShaders = GameSettings.Default.PostScreenEffects;
            allowShadowMapping = GameSettings.Default.ShadowMapping;
            highDetail = GameSettings.Default.HighDetail;

            canUsePS20 =
                device.GraphicsDeviceCapabilities.PixelShaderVersion.Major >= 2 &&
                GameSettings.Default.PerformanceSettings != 0;
            canUsePS30 =
                device.GraphicsDeviceCapabilities.PixelShaderVersion.Major >= 3 &&
                (GameSettings.Default.PerformanceSettings == -1 ||
                GameSettings.Default.PerformanceSettings == 2);
        } // CheckOptionsAndPSVersion()

		#endregion

		#region Is active?
		private static bool isWindowActive = false;
		/// <summary>
		/// Is active?
		/// </summary>
		public static bool IsWindowActive
		{
			get
			{
				return isWindowActive;
			} // get
		} // IsActive

		/// <summary>
		/// On activated event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnActivated(object sender, EventArgs args)
		{
			base.OnActivated(sender, args);
			isWindowActive = true;
		} // OnActivated

		/// <summary>
		/// On deactivated event
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void OnDeactivated(object sender, EventArgs args)
		{
			base.OnDeactivated(sender, args);
			isWindowActive = false;
		} // OnDeactivated
		#endregion

        #region UI
        /// <summary>
        /// User interface renderer helper ^^
        /// </summary>
        /// <returns>UIRenderer</returns>
        public static UIRenderer UI
        {
            get
            {
                return ui;
            } // get
        } // UI
        #endregion

		#endregion

		#region Helper methods for 3d-calculations
		/// <summary>
		/// Epsilon (1/1000000) for comparing stuff which is nearly equal.
		/// </summary>
		public const float Epsilon = 0.000001f;

		/// <summary>
		/// Convert 3D vector to 2D vector, this is kinda the oposite of
		/// GetScreenPlaneVector (not shown here). This can be useful for user
		/// input/output, because we will often need the actual position on screen
		/// of an object in 3D space from the users view to handle it the right
		/// way. Used for lens flare and asteroid optimizations.
		/// </summary>
		/// <param name="point">3D world position</param>
		/// <return>Resulting 2D screen position</return>
		public static Point Convert3DPointTo2D(Vector3 point)
		{
			/*doesn't work, Vector3.Transform does not behave like
			 * MDX Vector3.TransformCoordinate!
			Log.Write("point=" + point);
			Log.Write("ViewMatrix=" + ViewMatrix);
			Log.Write("ProjectionMatrix=" + ProjectionMatrix);
			Log.Write("ViewProjectionMatrix=" + ViewProjectionMatrix);
			Vector3 result = Vector3.Transform(point, ViewProjectionMatrix);
			Log.Write("result=" + result);
			 */

			// Fix:
			Vector4 result4 = Vector4.Transform(point,
				ViewProjectionMatrix);

			//Log.Write("result4=" + result4);
			if (result4.W == 0)
				result4.W = BaseGame.Epsilon;
			Vector3 result = new Vector3(
				result4.X / result4.W,
				result4.Y / result4.W,
				result4.Z / result4.W);
			//Log.Write("result=" + result);

			// Output result from 3D to 2D
			return new Point(
				(int)Math.Round(+result.X * (width / 2)) + (width / 2),
				(int)Math.Round(-result.Y * (height / 2)) + (height / 2));
		} // Convert3DPointTo2D(point)

		/// <summary>
		/// Is point in front of camera?
		/// </summary>
		/// <param name="point">Position to check.</param>
		/// <returns>Bool</returns>
		public static bool IsInFrontOfCamera(Vector3 point)
		{
			Vector4 result = Vector4.Transform(
				new Vector4(point.X, point.Y, point.Z, 1),
				viewProjMatrix);

			// Is result in front?
			return result.Z > result.W - NearPlane;
		} // IsInFrontOfCamera(point)

		/// <summary>
		/// Helper to check if a 3d-point is visible on the screen.
		/// Will basically do the same as IsInFrontOfCamera and Convert3DPointTo2D,
		/// but requires less code and is faster. Also returns just an bool.
		/// Will return true if point is visble on screen, false otherwise.
		/// Use the offset parameter to include points into the screen that are
		/// only a couple of pixel outside of it.
		/// </summary>
		/// <param name="point">Point</param>
		/// <param name="checkOffset">Check offset in percent of total
		/// screen</param>
		/// <returns>Bool</returns>
		public static bool IsVisible(Vector3 point, float checkOffset)
		{
			Vector4 result = Vector4.Transform(
				new Vector4(point.X, point.Y, point.Z, 1),
				viewProjMatrix);

			// Point must be in front of camera, else just skip everything.
			if (result.Z > result.W - NearPlane)
			{
				Vector2 screenPoint = new Vector2(
					result.X / result.W, result.Y / result.W);

				// Change checkOffset depending on how depth we are into the scene
				// for very near objects (z < 5) pass all tests!
				// for very far objects (z >> 5) only pass if near to +- 1.0f
				float zDist = Math.Abs(result.Z);
				if (zDist < 5.0f)
					return true;
				checkOffset = 1.0f + (checkOffset / zDist);

				return
					screenPoint.X >= -checkOffset && screenPoint.X <= +checkOffset &&
					screenPoint.Y >= -checkOffset && screenPoint.Y <= +checkOffset;
			} // if (result.z)

			// Point is not in front of camera, return false.
			return false;
		} // IsVisible(point)
		#endregion

		#region Occlusion testing (not supported in XNA)
		/// <summary>
		/// Occlusion intensity
		/// </summary>
		/// <param name="tex">Tex</param>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		/// <returns>Float</returns>
		public static float OcclusionIntensity(Texture tex, Point pos, int size)
		{
			return 0.0f;
		} // OcclusionIntensity(tex, pos, size)
		#endregion

		#region Constructor
		/// <summary>
		/// Create base game
		/// </summary>
		public BaseGame()
		{
			graphics = new GraphicsDeviceManager(this);
            content = this.Content; // new ContentManager(Services);
            content.RootDirectory = "Content";
            

			int resolutionWidth = GameSettings.Default.ResolutionWidth;
			int resolutionHeight = GameSettings.Default.ResolutionHeight;
			// Use current desktop resolution if autodetect is selected.
			if (resolutionWidth <= 0 ||
				resolutionHeight <= 0)
			{
				resolutionWidth =
					GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
				resolutionHeight =
					GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
			} // if (resolutionWidth)

#if XBOX360
			// Telling the Xbox 360 which resolution we want does not matter
			graphics.IsFullScreen = true;

			// Tell the Xbox 360 the resolution anyways, the viewport may just be
			// 800x600 else!
			graphics.PreferredBackBufferWidth =
				GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			graphics.PreferredBackBufferHeight =
				GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
#else
			graphics.PreferredBackBufferWidth = resolutionWidth;
			graphics.PreferredBackBufferHeight = resolutionHeight;
			graphics.IsFullScreen = GameSettings.Default.Fullscreen;
#endif

#if DEBUG
			// Don't limit the framerate to the vertical retrace in debug mode!
//tst without:			
			graphics.SynchronizeWithVerticalRetrace = false;
#endif

			// Always turn on multisampling, looks much nicer, especially on the Xbox360
			this.graphics.PreferMultiSampling = true;

			// We do not use a fixed time step!
			this.IsFixedTimeStep = false;

			//tst: graphics.IsFullScreen = true;

			// Init the space camera
			camera = new ThirdPersonCamera(this);
			//camera = new RotationCamera(this);
			this.Components.Add(camera);
		} // BaseGame()

		/// <summary>
		/// Remember depth stencil buffer in case we have to reset it!
		/// </summary>
		static DepthStencilBuffer remDepthBuffer = null;

		/// <summary>
		/// Initialize
		/// </summary>
		protected override void Initialize()
		{
#if !XBOX360
			// Add screenshot capturer. Works only on windows platform.
			// Note: Don't do this in constructor,
			// we need the correct window name for screenshots!
			this.Components.Add(new ScreenshotCapturer(this));
#endif

			// Remember device
			device = graphics.GraphicsDevice;

			// Remember resolution
			width = graphics.GraphicsDevice.Viewport.Width;
			height = graphics.GraphicsDevice.Viewport.Height;
			backBufferDepthFormat = graphics.PreferredDepthStencilFormat;
			remMultiSampleType = device.PresentationParameters.MultiSampleType;
			
            //if (remMultiSampleType == MultiSampleType.NonMaskable)
			//	remMultiSampleType = MultiSampleType.None;
			
            remMultiSampleQuality = device.PresentationParameters.MultiSampleQuality;
			remDepthBuffer = device.DepthStencilBuffer;

			// Update resolution if it changes and restore device after it was lost
			Window.ClientSizeChanged += new EventHandler(Window_ClientSizeChanged);
			graphics.DeviceReset += new EventHandler(graphics_DeviceReset);
			graphics_DeviceReset(null, EventArgs.Empty);
			
			// Create matrices for our shaders, this makes it much easier to manage
			// all the required matrices and we have to do this ourselfs since there
			// is no fixed function support and theirfore no Device.Transform class.
			WorldMatrix = Matrix.Identity;
			aspectRatio = (float)width / (float)height;
			ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				FieldOfView, aspectRatio, NearPlane, FarPlane);

			// ViewMatrix is updated in camera class
			ViewMatrix = Matrix.CreateLookAt(
				new Vector3(0, 0, 15), Vector3.Zero, Vector3.Up);

            // Init global manager classes, which will be used all over the place ^^
            lineManager2D = new LineManager2D();
            //lineManager3D = new LineManager3D();
            ui = new UIRenderer();
            // Create font and numbers font
			font = new TextureFont();

			// Make sure we can use PS1 or PS2, see UsePS and UsePS20
			if (device.GraphicsDeviceCapabilities.PixelShaderVersion.Major >= 2 &&
				GameSettings.Default.PerformanceSettings < 2)
				GameSettings.Default.PerformanceSettings = 2;
			else if (
				device.GraphicsDeviceCapabilities.PixelShaderVersion.Major >= 1 &&
				GameSettings.Default.PerformanceSettings < 1)
				GameSettings.Default.PerformanceSettings = 1;

			// Init post screen glow
			glowShader =
				//new PostScreenMenu();
				new PostScreenGlow();

			// Init effect manager
			effectManager = new EffectManager();

			// Init light position
			LightDirection = new Vector3(2, -7, 5);

			base.Initialize();

			device.RenderState.DepthBufferEnable = true;

			// Always create depth buffer from now on
			RenderToTexture.alwaysCreateRenderTargetDepthBuffer = true;
		} // Initialize()

		/// <summary>
		/// Graphics device reset
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">E</param>
		void graphics_DeviceReset(object sender, EventArgs e)
		{
			// Reset device
			device = graphics.GraphicsDevice;

			// Restore z buffer state
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
			// Set u/v addressing back to wrap
			BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
			BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
			// Restore normal alpha blending
			BaseGame.EnableAlphaBlending();

			// Recreate all render-targets
			foreach (RenderToTexture renderToTexture in remRenderToTextures)
				renderToTexture.HandleDeviceReset();
		} // graphics_DeviceReset(sender, e)

		/// <summary>
		/// Window client size changed
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">E</param>
		void Window_ClientSizeChanged(object sender, EventArgs e)
		{
			// Update width and height
			width = device.Viewport.Width;//Window.ClientBounds.Width;
			height = device.Viewport.Height;//Window.ClientBounds.Height;
			aspectRatio = (float)width / (float)height;
			ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				FieldOfView, aspectRatio, NearPlane, FarPlane);
		} // Window_ClientSizeChanged(sender, e)
		#endregion

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			// We quit, make sure everyone knows about it.
			quit = true;
			base.Dispose(disposing);
		} // Dispose(disposing)
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		/// <param name="gameTime">Game time</param>
		protected override void Update(GameTime gameTime)
		{
			Sound.Update();

			Input.Update();
			
			// Only quit this way when using unit tests
//#if DEBUG
			if (this.GetType() == typeof(TestGame) &&
				(Input.KeyboardEscapeJustPressed ||
				Input.GamePadBackJustPressed))
				this.Exit();
//#endif

			//lastFrameTotalTimeMs = totalTimeMs;
			elapsedTimeThisFrameInMs =
				(float)gameTime.ElapsedRealTime.TotalMilliseconds;
			totalTimeMs += elapsedTimeThisFrameInMs;

			// Make sure elapsedTimeThisFrameInMs is never 0
			if (elapsedTimeThisFrameInMs <= 0)
				elapsedTimeThisFrameInMs = 0.001f;

			// Increase frame counter for FramesPerSecond
			frameCountThisSecond++;
			totalFrameCount++;

			// One second elapsed?
			if (totalTimeMs - startTimeThisSecond > 1000.0f)
			{
				// Calc fps
				fpsLastSecond = (int)(frameCountThisSecond * 1000.0f /
					(totalTimeMs - startTimeThisSecond));
				// Reset startSecondTick and repaintCountSecond
				startTimeThisSecond = totalTimeMs;
				frameCountThisSecond = 0;

				fpsInterpolated =
					MathHelper.Lerp(fpsInterpolated, fpsLastSecond, 0.1f);
			} // if (Math.Abs)

			base.Update(gameTime);
		} // Update(gameTime)
		#endregion

		#region Draw
		bool showFps =
#if DEBUG
			false;//true;
#else
			false;
#endif
		/// <summary>
		/// Draw
		/// </summary>
		/// <param name="gameTime">Game time</param>
		protected override void Draw(GameTime gameTime)
		{
      #region PS 2.0 Required
			if (device.GraphicsDeviceCapabilities.PixelShaderVersion.Major < 2)
			{
				// Clear fonts
				font.WriteAll();

				// Display message that PS 2.0 is required
				BaseGame.device.Clear(Color.Black);
				TextureFont.WriteTextCentered(
					BaseGame.width / 2, BaseGame.height / 2,
					"Sorry, at least Pixel Shader 2.0 is required for this game!");
				TextureFont.WriteTextCentered(
					BaseGame.width / 2, BaseGame.height / 2 + 40,
					"Unable to continue. Press any key to exit.");

				// Show fonts
				font.WriteAll();

				base.Draw(gameTime);

				if (Input.KeyboardEscapeJustPressed ||
					Input.KeyboardDownPressed ||
					Input.KeyboardSpaceJustPressed ||
					Input.Keyboard.IsKeyDown(Keys.LeftAlt) ||
					Input.Keyboard.IsKeyDown(Keys.LeftControl) ||
					Input.MouseLeftButtonJustPressed ||
					Input.MouseRightButtonJustPressed ||
					Input.GamePadAPressed ||
					Input.GamePadBPressed)
				{
					quit = true;
					Exit();
				} // if (Input.KeyboardEscapeJustPressed)
				return;
			} // if (device.GraphicsDeviceCapabilities.PixelShaderVersion.Major)
			#endregion

#if DEBUG
      // Only do this for unit tests, game handles this itself better!
			if (this.GetType() == typeof(TestGame))
			{
				// Use alpha blending for blending out asteroids
				BaseGame.EnableAlphaBlending();
				//BaseGame.Device.RenderState.FillMode = FillMode.WireFrame;

				// Render all meshes!
				//unused: BaseGame.MeshRenderManager.Render();

				// Execute post screen shader
				glowShader.Show();
			} // if
#endif

			// Draw all sprites
      UIRenderer.Render(lineManager2D);  //SpriteHelper.DrawAllSprites();

      ui.RenderTextsAndMouseCursor();

			// Show fonts
			font.WriteAll();

			//unused: lineManager2D.Render();
			//lineManager3D.Render();

			base.Draw(gameTime);            

			// Allow clearing background next frame
			clearedYet = false;
		} // Draw(gameTime)
		#endregion

		#region ClearBackground
		private bool clearedYet = false;
		/// <summary>
		/// Clear background, will only be executed once per frame, so
		/// it is save to call it multiple times.
		/// </summary>
		public void ClearBackground()
		{
			if (clearedYet == false)
				graphics.GraphicsDevice.Clear(BackgroundColor);
			clearedYet = true;

			// Disable alpha blending and alpha testing, we won't need that at
			// the beginning of the rendering (first all solid stuff is going
			// to be rendered)
			device.RenderState.AlphaBlendEnable = false;
			device.RenderState.AlphaTestEnable = false;
		} // ClearBackground()
		#endregion

		#region Line helper methods
		/*unused
		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPoint">Start point</param>
		/// <param name="endPoint">End point</param>
		/// <param name="color">Color</param>
		public static void DrawLine(Point startPoint, Point endPoint, Color color)
		{
			lineManager2D.AddLine(startPoint, endPoint, color);
		} // DrawLine(startPoint, endPoint, color)

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPoint">Start point</param>
		/// <param name="endPoint">End point</param>
		public static void DrawLine(Point startPoint, Point endPoint)
		{
			lineManager2D.AddLine(startPoint, endPoint, Color.White);
		} // DrawLine(startPoint, endPoint)

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		/// <param name="color">Color</param>
		public static void DrawLine(Vector3 startPos, Vector3 endPos, Color color)
		{
			lineManager3D.AddLine(startPos, endPos, color);
		} // DrawLine(startPos, endPos, color)

		/// <summary>
		/// Draw line with shadow
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		/// <param name="color">Color</param>
		public static void DrawLineWithShadow(Point startPoint, Point endPoint,
			Color color)
		{
			lineManager2D.AddLine(
				new Point(startPoint.X + 1, startPoint.Y + 1),
				new Point(endPoint.X + 1, endPoint.Y + 1), Color.Black);
			lineManager2D.AddLine(startPoint, endPoint, color);
		} // DrawLineWithShadow(startPos, endPos, color)

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		/// <param name="startColor">Start color</param>
		/// <param name="endColor">End color</param>
		public static void DrawLine(Vector3 startPos, Vector3 endPos,
			Color startColor, Color endColor)
		{
			lineManager3D.AddLine(startPos, startColor, endPos, endColor);
		} // DrawLine(startPos, endPos, startColor)

		/// <summary>
		/// Draw line
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		public static void DrawLine(Vector3 startPos, Vector3 endPos)
		{
			lineManager3D.AddLine(startPos, endPos, Color.White);
		} // DrawLine(startPos, endPos)

		/// <summary>
		/// Flush line manager 2D. Renders all lines and allows more lines
		/// to be rendered. Used to render lines into textures and stuff.
		/// </summary>
		public static void FlushLineManager2D()
		{
			lineManager2D.Render();
		} // FlushLineManager2D()

		/// <summary>
		/// Flush line manager 3D. Renders all lines and allows more lines
		/// to be rendered.
		/// </summary>
		public static void FlushLineManager3D()
		{
			lineManager3D.Render();
		} // FlushLineManager3D()
		 */
		#endregion

		#region Set and reset render targets
		/// <summary>
		/// Remember scene render target. This is very important because
		/// for our post screen shaders we have to render our whole scene
		/// to this render target. But in the process we will use many other
		/// shaders and they might set their own render targets and then
		/// reset it, but we need to have this scene still to be set.
		/// Don't reset to the back buffer (with SetRenderTarget(0, null), this
		/// would stop rendering to our scene render target and the post screen
		/// shader will not be able to process our screen.
		/// The whole reason for this is that we can't use StrechRectangle
		/// like in Rocket Commander because XNA does not provide that function
		/// (the reason for that is cross platform compatibility with the XBox360).
		/// Instead we could use ResolveBackBuffer, but that method is VERY SLOW.
		/// Our framerate would drop from 600 fps down to 20, not good.
		/// However, multisampling will not work, so we will disable it anyway!
		/// </summary>
		static RenderTarget2D remSceneRenderTarget = null;
		/// <summary>
		/// Remember the last render target we set, this way we can check
		/// if the rendertarget was set before calling resolve!
		/// </summary>
		static RenderTarget2D lastSetRenderTarget = null;

		/// <summary>
		/// Remember render to texture instances to allow recreating them all
		/// when DeviceReset is called.
		/// </summary>
		static List<RenderToTexture> remRenderToTextures =
			new List<RenderToTexture>();

		/// <summary>
		/// Add render to texture instance to allow recreating them all
		/// when DeviceReset is called with help of the remRenderToTextures list. 
		/// </summary>
		public static void AddRemRenderToTexture(RenderToTexture renderToTexture)
		{
			remRenderToTextures.Add(renderToTexture);
		} // AddRemToTexture(renderToTexture)

		/// <summary>
		/// Current render target we have set, null if it is just the back buffer.
		/// </summary>
		public static RenderTarget2D CurrentRenderTarget
		{
			get
			{
				return lastSetRenderTarget;
			} // get
		} // CurrentRenderTarget

		/// <summary>
		/// Set render target
		/// </summary>
		/// <param name="isSceneRenderTarget">Is scene render target</param>
		internal static void SetRenderTarget(RenderTarget2D renderTarget,
			bool isSceneRenderTarget)
		{
			//Log.Write("SetRenderTarget: "+renderTarget.Name+
			//	", isSceneRenderTarget="+isSceneRenderTarget);
			Device.SetRenderTarget(0, renderTarget);
			if (isSceneRenderTarget)
				remSceneRenderTarget = renderTarget;
			lastSetRenderTarget = renderTarget;
		} // SetRenderTarget(isSceneRenderTarget)

		/// <summary>
		/// Reset render target
		/// </summary>
		/// <param name="fullResetToBackBuffer">Full reset to back buffer</param>
		internal static void ResetRenderTarget(bool fullResetToBackBuffer)
		{
			//Log.Write("ResetRenderTarget: "+fullResetToBackBuffer);
			if (remSceneRenderTarget == null ||
				fullResetToBackBuffer)
			{
				remSceneRenderTarget = null;
				lastSetRenderTarget = null;
				Device.SetRenderTarget(0, null);
				//Device.DepthStencilBuffer = remDepthBuffer;
			} // if (remSceneRenderTarget)
			else
			{
				Device.SetRenderTarget(0, remSceneRenderTarget);
				lastSetRenderTarget = remSceneRenderTarget;
			} // else
		} // ResetRenderTarget(fullResetToBackBuffer)
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test vector 3 transform
		/// </summary>
		public static void TestVector3Transform()
		{
			// Put some point on the y axis
			Vector3 point = new Vector3(+2500, +22500, +15000);
			// Build a view matrix looking at 0, 0, 0 from above
			// and a default projection matrix in 800x600.
			Matrix matrix = new Matrix(
				0.75f, 0, 0, 0,
				0, 1, 0, 0,
				0, 0, -1, -1,
				0, 0, 49, 50);
			//Matrix.CreateLookAt(new Vector3(0, 0, 50), Vector3.Zero, Vector3.Up) *
			//Matrix.CreatePerspectiveFieldOfView(
			// FieldOfView, 800.0f/600.0f, 1, 10000);
			Log.Write("point=" + point);
			Log.Write("matrix=" + matrix);

			// Transform point
			Vector3 resultXNA = Vector3.Transform(point, matrix);

			// Should return {X:-0.125 Y:-1.505 Z:1.0} (ruffly)
			// Does return {X:1875 Y:22500 Z:-14951}
			Log.Write("resultXNA=" + resultXNA);
			//Assert.IsTrue(Vector3.Distance(
			//  new Vector3(0.125f, -1.505f, 1.0f), resultXNA) < 0.001f);

			// This returns the correct value
			Vector4 result4 = Vector4.Transform(point, matrix);
			if (result4.W == 0)
				result4.W = BaseGame.Epsilon;
			Vector3 result = new Vector3(
				result4.X / result4.W,
				result4.Y / result4.W,
				result4.Z / result4.W);
			Log.Write("result4=" + result4);
			Log.Write("result=" + result);
			Log.Write("Is result correct: " + (Vector3.Distance(
				new Vector3(-0.125f, -1.505f, 1.0f), result) < 0.001f));
		} // TestVector3Transform()
#endif
		#endregion
	} // class BaseGame
} // namespace DungeonQuest.Game
