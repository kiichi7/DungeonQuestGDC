// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:47

#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using DungeonQuest.Game;
using DungeonQuest.Helpers;
using DungeonQuest.GameScreens;
using DungeonQuest.Sounds;
using DungeonQuest.Graphics;
using DungeonQuest.Properties;
using Texture = DungeonQuest.Graphics.Texture;
using DungeonQuest.GameLogic;
#endregion

namespace DungeonQuest
{
	/// <summary>
	/// DungeonQuestGame main class
	/// </summary>
	public class DungeonQuestGame : BaseGame
	{
		#region Variables
		/// <summary>
		/// Game screens stack. We can easily add and remove game screens
		/// and they follow the game logic automatically. Very cool.
		/// </summary>
		private static Stack<IGameScreen> gameScreens = new Stack<IGameScreen>();

		/// <summary>
		/// Background landscape models and ship models.
		/// </summary>
		public GameManager gameManager = null;

		/*obs
		/// <summary>
		/// Explosion texture, displayed when any ship is exploding.
		/// </summary>
		public AnimatedTexture explosionTexture = null;
		 */

        /// <summary>
        /// ShowMouseCursor
        /// </summary>
        /// <returns>Bool</returns>
        public static bool ShowMouseCursor
        {
            get
            {
                // Only if not in Game, not in logo or splash screen!
                return gameScreens.Count > 0 &&
                    gameScreens.Peek().GetType() != typeof(GameScreen);
            } // get
        } // ShowMouseCursor
		#endregion

		#region Constructor
		/// <summary>
		/// Create your game
		/// </summary>
		public DungeonQuestGame()
		{
			// Disable mouse, we use our own mouse texture in the menu
			// and don't use any mouse cursor in the game anyway.
			this.IsMouseVisible = false;
		} // DungeonQuestGame()
		#endregion

		#region Initialize
		/// <summary>
		/// Allows the game to perform any initialization it needs.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();

			// Make sure mouse is centered
			Input.Update();
			Input.MousePos = new Point(
				Window.ClientBounds.X + width / 2,
				Window.ClientBounds.Y + height / 2);
			Input.Update();

			// Load explosion effect
			//explosionTexture = new AnimatedTexture("Destroy");
			gameManager = new GameManager();

			// Create main menu screen
			//gameScreens.Push(new MainMenu());
			// Start game
			//gameScreens.Push(new Mission());
			//inGame = true;

			// Show end screen after quitting the game
			gameScreens.Push(new EndScreen());
			gameScreens.Push(new GameScreen());
			
			// Start music
			if (GameSettings.Default.MusicOn)
				Sound.StartMusic();

			// Start background athmosphere sound (loops)
			Sound.Play(Sound.Sounds.Athmosphere);
		} // Initialize()
		#endregion

		#region Dispose
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
				GameSettings.Save();
		} // Dispose(disposing)
		#endregion

		#region Toggle music on/off
		/// <summary>
		/// Toggle music on off
		/// </summary>
		public void ToggleMusicOnOff()
		{
			if (GameSettings.Default.MusicOn)
				Sound.StopMusic();
			else
				Sound.StartMusic();
		} // ToggleMusicOnOff()
		#endregion

		#region Add game screen
		//always! static bool inGame = false;
		/// <summary>
		/// In game
		/// </summary>
		/// <returns>Bool</returns>
		public static bool InGame
		{
			get
			{
				return true;// inGame;
			} // get
		} // InGame

		/// <summary>
		/// Add game screen, which will be used until we quit it or add
		/// another game screen on top of it.
		/// </summary>
		/// <param name="newGameScreen">New game screen</param>
		public static void AddGameScreen(IGameScreen newGameScreen)
		{
			gameScreens.Push(newGameScreen);

			//always! inGame = newGameScreen.GetType() == typeof(Mission);
		} // AddGameScreen(newGameScreen)
		#endregion

		#region Remove current game screen
		/// <summary>
		/// Remove current game screen
		/// </summary>
		public void RemoveCurrentGameScreen()
		{
			gameScreens.Pop();

			//always: inGame = gameScreens.Count > 0 &&
			//	gameScreens.Peek().GetType() == typeof(Mission);
		} // RemoveCurrentGameScreen()
		#endregion

		#region Render menu background
		/// <summary>
		/// Render menu background
		/// </summary>
		public void RenderMenuBackground()
		{
			gameManager.Run(false);
			/*unused
			// Make sure alpha blending is enabled.
			BaseGame.EnableAlphaBlending();
			mainMenuTexture.RenderOnScreen(
				BaseGame.ResolutionRect,
				new Rectangle(0, 0, 1024, 768));
			 */
		} // RenderMenuBackground()
		#endregion

		#region Render button
		/*TODO?
		/// <summary>
		/// Render button
		/// </summary>
		/// <param name="buttonType">Button type</param>
		/// <param name="rect">Rectangle</param>
		public bool RenderMenuButton(
			MenuButton buttonType, Point pos)
		{
			// Calc screen rect for rendering (recalculate relative screen position
			// from 1024x768 to actual screen resolution, just in case ^^).
			Rectangle rect = new Rectangle(
				pos.X * BaseGame.Width / 1024,
				pos.Y * BaseGame.Height / 768,
				200 * BaseGame.Width / 1024,
				77 * BaseGame.Height / 768);

			// Is button highlighted?
			Rectangle innerRect = new Rectangle(
				rect.X, rect.Y + rect.Height / 5,
				rect.Width, rect.Height * 3 / 5);
			bool highlight = Input.MouseInBox(
				//rect);
				// Just use inner rect
				innerRect);

			/*unused
			// Was not highlighted last frame?
			if (highlight &&
				Input.MouseWasNotInRectLastFrame(innerRect))
				Sound.Play(Sound.Sounds.Highlight);
			 *

			// See MainMenu.dds for pixel locations
			int buttonNum = (int)buttonType;

			// Correct last 2 button numbers (exit and back)
			//if (buttonNum >= (int)MenuButton.Exit)
			//	buttonNum -= 2;

			Rectangle pixelRect = new Rectangle(3 + 204 * buttonNum,
				840 + 80 * (highlight ? 1 : 0), 200, 77);

			// Render
			mainMenuTexture.RenderOnScreen(rect, pixelRect);

			// Play click sound if button was just clicked
			bool ret =
				(Input.MouseLeftButtonJustPressed ||
				Input.GamePadAJustPressed) &&
				this.IsActive &&
				highlight;

			if (buttonType == MenuButton.Back &&
				(Input.GamePadBackJustPressed ||
				Input.KeyboardEscapeJustPressed))
				ret = true;
			if (buttonType == MenuButton.Missions &&
				Input.GamePadStartPressed)
				ret = true;

			if (ret == true)
				Sound.Play(Sound.Sounds.Click);

			// Return true if button was pressed, false otherwise
			return ret;
		} // RenderButton(buttonType, rect)
*/
		#endregion

		#region Render mouse cursor
		/// <summary>
		/// Render mouse cursor
		/// </summary>
		public void RenderMouseCursor()
		{
			/*
#if !XBOX360
			// We got 4 animation steps, rotate them by the current time
			int mouseAnimationStep = (int)(BaseGame.TotalTimeMs / 100) % 4;

			// And render mouse on screen.
			mouseCursorTexture.RenderOnScreen(
				new Rectangle(Input.MousePos.X, Input.MousePos.Y, 60*2/3, 64*2/3),
				new Rectangle(64 * mouseAnimationStep, 0, 60, 64));

			// Draw all sprites (just the mouse cursor)
			SpriteHelper.DrawSprites(width, height);
#endif
			*/
		} // RenderMouseCursor()
		#endregion

		#region Update
		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// If that game screen should be quitted, remove it from stack!
			if (gameScreens.Count > 0 &&
				gameScreens.Peek().Quit)
				RemoveCurrentGameScreen();

			// If no more game screens are left, it is time to quit!
			if (gameScreens.Count == 0)
			{
#if DEBUG
				// Don't exit if this is just a unit test
				if (this.GetType() != typeof(TestGame))
#endif
					Exit();
			} // if

			base.Update(gameTime);
		} // Update(gameTime)
		#endregion

		#region Draw
		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			// Kill background (including z buffer, which is important for 3D)
			ClearBackground();

			/*
			// Start post screen glow shader, will be shown in BaseGame.Draw
			BaseGame.GlowShader.Start();
			 */

			// Disable z buffer, mostly now only 2d content is rendered now.
			//BaseGame.Device.RenderState.DepthBufferEnable = false;

			try
			{
				// Execute the game screen on top.
				if (gameScreens.Count > 0)
					gameScreens.Peek().Run(this);
                
			} // try
			catch (Exception ex)
			{
				Log.Write("Failed to execute " + gameScreens.Peek().Name +
					"\nError: " + ex.ToString());
			} // catch

			base.Draw(gameTime);

			// Show mouse cursor (in all modes except in the game)
			if (gameScreens.Peek().GetType() != typeof(GameScreen) &&
				gameScreens.Count > 0)
				RenderMouseCursor();
			else
			{                
				// In game always center mouse
				Input.CenterMouse();
			} // else

			// Add scene glow on top of everything (2d and 3d!) 
			glowShader.Show();
		} // Draw(gameTime)
		#endregion
	} // class DungeonQuestGame
} // namespace DungeonQuest
