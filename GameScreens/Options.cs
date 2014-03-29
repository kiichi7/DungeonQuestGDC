// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DungeonQuest.GameLogic;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.Properties;
using DungeonQuest.Sounds;
using DungeonQuest.Game;
using DungeonQuest.GameScreens;
using DungeonQuest;
#endregion

namespace DungeonQuest.GameScreens
{
	/// <summary>
	/// Options
	/// </summary>
	/// <returns>IGame screen</returns>
	class Options : IGameScreen
	{
		#region Constants
		readonly Rectangle
			Line1ArrowGfxRect = new Rectangle(173, 38, 62, 39),
			Line2ArrowGfxRect = new Rectangle(76, 108, 62, 39),
			Line3ArrowGfxRect = new Rectangle(124, 181, 62, 39+44),
			Line4ArrowGfxRect = new Rectangle(154, 284, 62, 39),
			Line5ArrowGfxRect = new Rectangle(160, 354, 62, 39),
			Line6ArrowGfxRect = new Rectangle(72, 437, 62, 39),
			Resolution640x480GfxRect = new Rectangle(339, 112, 98, 32),
			Resolution800x600GfxRect = new Rectangle(454, 112, 98, 32),
			Resolution1024x768GfxRect = new Rectangle(575, 112, 108, 32),
			Resolution1280x1024GfxRect = new Rectangle(704, 112, 116, 32),
			ResolutionAutoGfxRect = new Rectangle(838, 112, 69, 32),
			Pixelshader11GfxRect = new Rectangle(339, 182, 168, 36),
			Pixelshader20GfxRect = new Rectangle(539, 182, 168, 36),
			Pixelshader30GfxRect = new Rectangle(742, 182, 168, 36),
			PostScreenEffectsGfxRect = new Rectangle(339, 226, 206, 36),
			ShadowsGfxRect = new Rectangle(616, 226, 90, 36),
			HighDetailGfxRect = new Rectangle(784, 226, 120, 36),
			SoundGfxRect = new Rectangle(384, 281, 448, 39),
			MusicGfxRect = new Rectangle(384, 354, 448, 39),
			SensibilityGfxRect = new Rectangle(384, 428, 448, 39);
		#endregion

		#region Variables
		/// <summary>
		/// Current player name, copied from the settings file.
		/// Can be changed in this screen and will be saved to the settings file.
		/// Just a variable here to make it easier to change the name and
		/// because of performance (reading Settings every frame is not good).
		/// </summary>
		string currentPlayerName = GameSettings.Default.PlayerName;
		/*unused, got new values below
		/// <summary>
		/// Resolution width
		/// </summary>
		int resolutionWidth = GameSettings.Default.ResolutionWidth;
		/// <summary>
		/// Resolution height
		/// </summary>
		int resolutionHeight = GameSettings.Default.ResolutionHeight;
		/// <summary>
		/// Performance settings
		/// </summary>
		private int performanceSettings = GameSettings.Default.PerformanceSettings;
		/// <summary>
		/// Music volume
		/// </summary>
		int musicVolume = (int)(GameSettings.Default.MusicVolume * 10.0f);
		/// <summary>
		/// Sound volume
		/// </summary>
		int soundVolume = (int)(GameSettings.Default.SoundVolume * 10.0f);
		/// <summary>
		/// Controller sensibility
		/// </summary>
		float controllerSensibility = GameSettings.Default.ControllerSensibility;
		 */
    
    /// <summary>
    /// Name of this game screen
    /// </summary>
    /// <returns>String</returns>
    public string Name
    {
      get
      {
        return "Highscores";
      } // get
    } // Name

    private bool quit = false;
    /// <summary>
    /// Returns true if we want to quit this screen and return to the
    /// previous screen. If no more screens are left the game is exited.
    /// </summary>
    /// <returns>Bool</returns>
    public bool Quit
    {
      get
      {
        return quit;
      } // get
    } // Quit
		#endregion

		#region Static constructor
		/// <summary>
		/// Create options class, will just load the player name.
		/// </summary>
		static Options()
		{
			// Get player name from windows computer/user name if not set yet.
			if (String.IsNullOrEmpty(GameSettings.Default.PlayerName))
			{
				// And save it back
				GameSettings.Default.PlayerName = WindowsHelper.GetDefaultPlayerName();
			} // if (String.IsNullOrEmpty)
		} // Options()
		#endregion

		#region Constructor
		int currentOptionsNumber = 0;
		int currentResolution = 4;
		int currentPixelshader = 2;
		bool usePostScreenShaders = true;
		bool useShadowMapping = true;
		bool useHighDetail = true;
		float currentMusicVolume = 1.0f;
		float currentSoundVolume = 1.0f;
		float currentSensitivity = 1.0f;
		/// <summary>
		/// Create options
		/// </summary>
		public Options()
		{
			// Current resolution:
			// 0=640x480, 1=800x600, 2=1024x768, 3=1280x1024, 4=auto (default)
			if (BaseGame.Width == 640 && BaseGame.Height == 480)
				currentResolution = 0;
			if (BaseGame.Width == 800 && BaseGame.Height == 600)
				currentResolution = 1;
			if (BaseGame.Width == 1024 && BaseGame.Height == 768)
				currentResolution = 2;
			if (BaseGame.Width == 1280 && BaseGame.Height == 1024)
				currentResolution = 3;

			// Get pixelshader
			currentPixelshader = GameSettings.Default.PerformanceSettings;
			if (currentPixelshader < 0 || currentPixelshader > 2)
				currentPixelshader = 2;
			// Check if ps20 is possible
			if (currentPixelshader > 0 && BaseGame.CanUsePS20 == false)
				currentPixelshader = 0;
			if (currentPixelshader > 1 && BaseGame.CanUsePS30 == false)
				currentPixelshader = 1;

			usePostScreenShaders = BaseGame.UsePostScreenShaders;
			useShadowMapping = BaseGame.AllowShadowMapping;
			useHighDetail = BaseGame.HighDetail;

			// Get music and sound volume
			currentMusicVolume = GameSettings.Default.MusicVolume;
			currentSoundVolume = GameSettings.Default.SoundVolume;

			// Get sensitivity
			currentSensitivity = GameSettings.Default.ControllerSensibility;
		} // Options()
		#endregion

		#region Run
		/// <summary>
		/// Render game screen. Called each frame.
		/// </summary>
    public void Run(DungeonQuestGame game)
    {
      // This starts both menu and in game post screen shader!
      BaseGame.UI.PostScreenMenuShader.Start();

      // Render background
      game.RenderMenuBackground();

      // Options background
      BaseGame.UI.OptionsScreen.RenderOnScreenRelative4To3(
          0, 125, BaseGame.UI.OptionsScreen.GfxRectangle);

			#region Edit player name
			// Edit player name
			int xPos = BaseGame.XToRes(352);
            int yPos = BaseGame.YToRes(125 + 65 - 20);//18);
			TextureFont.WriteText(xPos, yPos,
				currentPlayerName +
				// Add blinking |
				((int)(BaseGame.TotalTime / 0.35f) % 2 == 0 ? "|" : ""));
			Input.HandleKeyboardInput(ref currentPlayerName);
			#endregion

			#region Select resolution
			// Select resolution
			// Use inverted color for selection (see below for sprite blend mode)
			Color selColor =
				//inverse doesn't work anymore: new Color(255-255, 255-156, 255-0);
				//new Color(255, 156, 0);
				//new Color(160, 96, 0);
				new Color(255, 156, 0, 160);//128);//48);//32);//64);

			Rectangle res0Rect = BaseGame.CalcRectangleKeep4To3(
				Resolution640x480GfxRect);
			res0Rect.Y += BaseGame.YToRes(125);
			bool inRes0Rect = Input.MouseInBox(res0Rect);
			if (inRes0Rect || currentResolution == 0)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					res0Rect, Resolution640x480GfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inRes0Rect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				currentResolution = 0;
			} // if (inRes0Rect)

			Rectangle res1Rect = BaseGame.CalcRectangleKeep4To3(
				Resolution800x600GfxRect);
			res1Rect.Y += BaseGame.YToRes(125);
			bool inRes1Rect = Input.MouseInBox(res1Rect);
			if (inRes1Rect || currentResolution == 1)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					res1Rect, Resolution800x600GfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inRes1Rect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				currentResolution = 1;
			} // if (inRes0Rect)

			Rectangle res2Rect = BaseGame.CalcRectangleKeep4To3(
				Resolution1024x768GfxRect);
			res2Rect.Y += BaseGame.YToRes(125);
			bool inRes2Rect = Input.MouseInBox(res2Rect);
			if (inRes2Rect || currentResolution == 2)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					res2Rect, Resolution1024x768GfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inRes2Rect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				currentResolution = 2;
			} // if (inRes0Rect)

			Rectangle res3Rect = BaseGame.CalcRectangleKeep4To3(
				Resolution1280x1024GfxRect);
			res3Rect.Y += BaseGame.YToRes(125);
			bool inRes3Rect = Input.MouseInBox(res3Rect);
			if (inRes3Rect || currentResolution == 3)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					res3Rect, Resolution1280x1024GfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inRes3Rect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				currentResolution = 3;
			} // if (inRes0Rect)

			Rectangle res4Rect = BaseGame.CalcRectangleKeep4To3(
				ResolutionAutoGfxRect);
			res4Rect.Y += BaseGame.YToRes(125);
			bool inRes4Rect = Input.MouseInBox(res4Rect);
			if (inRes4Rect || currentResolution == 4)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					res4Rect, ResolutionAutoGfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inRes4Rect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				currentResolution = 4;
			} // if (inRes0Rect)
			#endregion

			#region Select pixelshader
			Rectangle ps11Rect = BaseGame.CalcRectangleKeep4To3(
				Pixelshader11GfxRect);
			ps11Rect.Y += BaseGame.YToRes(125);
			bool inPs11Rect = Input.MouseInBox(ps11Rect);
			if (inPs11Rect || currentPixelshader == 0)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					ps11Rect, Pixelshader11GfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inPs11Rect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				currentPixelshader = 0;
			} // if (inPs11Rect)

			Rectangle ps20Rect = BaseGame.CalcRectangleKeep4To3(
				Pixelshader20GfxRect);
			ps20Rect.Y += BaseGame.YToRes(125);
			bool inPs20Rect = Input.MouseInBox(ps20Rect);
			if (inPs20Rect || currentPixelshader == 1)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					ps20Rect, Pixelshader20GfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inPs20Rect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				currentPixelshader = 1;
			} // if (inPs11Rect)

			Rectangle ps30Rect = BaseGame.CalcRectangleKeep4To3(
				Pixelshader30GfxRect);
			ps30Rect.Y += BaseGame.YToRes(125);
			bool inPs30Rect = Input.MouseInBox(ps30Rect);
			if (inPs30Rect || currentPixelshader == 2)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					ps30Rect, Pixelshader30GfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inPs30Rect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				currentPixelshader = 2;
			} // if (inPs30Rect)
			#endregion

			#region Graphics options
			Rectangle pseRect = BaseGame.CalcRectangleKeep4To3(
				PostScreenEffectsGfxRect);
			pseRect.Y += BaseGame.YToRes(125);
			bool inPseRect = Input.MouseInBox(pseRect);
			if (inPseRect || usePostScreenShaders)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					pseRect, PostScreenEffectsGfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inPseRect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				usePostScreenShaders = !usePostScreenShaders;
			} // if (inPseRect)

			Rectangle smRect = BaseGame.CalcRectangleKeep4To3(
				ShadowsGfxRect);
			smRect.Y += BaseGame.YToRes(125);
			bool inSmRect = Input.MouseInBox(smRect);
			if (inSmRect || useShadowMapping)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					smRect, ShadowsGfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inSmRect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				useShadowMapping = !useShadowMapping;
			} // if (inSmRect)

			Rectangle hdRect = BaseGame.CalcRectangleKeep4To3(
				HighDetailGfxRect);
			hdRect.Y += BaseGame.YToRes(125);
			bool inHdRect = Input.MouseInBox(hdRect);
			if (inHdRect || useHighDetail)
				BaseGame.UI.OptionsScreen.RenderOnScreen(
					hdRect, HighDetailGfxRect,
					selColor, SpriteBlendMode.AlphaBlend);
			if (inHdRect && Input.MouseLeftButtonJustPressed)
			{
				Sound.Play(Sound.Sounds.ButtonClick);
				useHighDetail = !useHighDetail;
			} // if (inHdRect)
			#endregion

			#region Sound volume
			Rectangle soundRect = BaseGame.CalcRectangleKeep4To3(
				SoundGfxRect);
			soundRect.Y += BaseGame.YToRes(125);
			if (Input.MouseInBox(soundRect))
			{
				if (Input.MouseLeftButtonJustPressed)
				{
					currentSoundVolume =
						(Input.MousePos.X - soundRect.X) / (float)soundRect.Width;
					Sound.Play(Sound.Sounds.Highlight);
				} // if (Input.MouseLeftButtonJustPressed)
			} // if (Input.MouseInBox)

			// Handel controller input
			if (currentOptionsNumber == 3)
			{
				if (Input.GamePadLeftJustPressed ||
					Input.KeyboardLeftJustPressed)
				{
					currentSoundVolume -= 0.1f;
					Sound.Play(Sound.Sounds.Highlight);
				} // if
				if (Input.GamePadRightJustPressed ||
					Input.KeyboardRightJustPressed)
				{
					currentSoundVolume += 0.1f;
					Sound.Play(Sound.Sounds.Highlight);
				} // if
				if (currentSoundVolume < 0)
					currentSoundVolume = 0;
				if (currentSoundVolume > 1)
					currentSoundVolume = 1;
			} // if (currentOptionsNumber)

			// Render radio button
			Rectangle gfxRect = UIRenderer.SelectionRadioButtonGfxRect;
			BaseGame.UI.Buttons.RenderOnScreen(new Rectangle(
				soundRect.X + (int)(soundRect.Width * currentSoundVolume) -
				BaseGame.XToRes(gfxRect.Width) / 2,
				soundRect.Y,
				BaseGame.XToRes(gfxRect.Width), BaseGame.YToRes(gfxRect.Height)),
				gfxRect);

            Sound.SetSoundVolume(currentSoundVolume);
			#endregion

			#region Music volume
			Rectangle musicRect = BaseGame.CalcRectangleKeep4To3(
				MusicGfxRect);
			musicRect.Y += BaseGame.YToRes(125);
			if (Input.MouseInBox(musicRect))
			{
				if (Input.MouseLeftButtonJustPressed)
				{
					currentMusicVolume =
						(Input.MousePos.X - musicRect.X) / (float)musicRect.Width;
					Sound.Play(Sound.Sounds.Highlight);
				} // if (Input.MouseLeftButtonJustPressed)
			} // if (Input.MouseInBox)

			// Handel controller input
			if (currentOptionsNumber == 4)
			{
				if (Input.GamePadLeftJustPressed ||
					Input.KeyboardLeftJustPressed)
				{
					currentMusicVolume -= 0.1f;
					Sound.Play(Sound.Sounds.Highlight);
				} // if
				if (Input.GamePadRightJustPressed ||
					Input.KeyboardRightJustPressed)
				{
					currentMusicVolume += 0.1f;
					Sound.Play(Sound.Sounds.Highlight);
				} // if
				if (currentMusicVolume < 0)
					currentMusicVolume = 0;
				if (currentMusicVolume > 1)
					currentMusicVolume = 1;
			} // if (currentOptionsNumber)

			// Render radio button
			BaseGame.UI.Buttons.RenderOnScreen(new Rectangle(
				musicRect.X + (int)(musicRect.Width * currentMusicVolume) -
				BaseGame.XToRes(gfxRect.Width) / 2,
				musicRect.Y,
				BaseGame.XToRes(gfxRect.Width), BaseGame.YToRes(gfxRect.Height)),
				gfxRect);

            Sound.SetMusicVolume(currentMusicVolume);
			#endregion

			#region Controller sensitivity
			Rectangle sensitivityRect = BaseGame.CalcRectangleKeep4To3(
				SensibilityGfxRect);
			sensitivityRect.Y += BaseGame.YToRes(125);
			if (Input.MouseInBox(sensitivityRect))
			{
				if (Input.MouseLeftButtonJustPressed)
				{
					currentSensitivity =
						2* (Input.MousePos.X - sensitivityRect.X) /
						(float)sensitivityRect.Width;
					Sound.Play(Sound.Sounds.Highlight);
				} // if (Input.MouseLeftButtonJustPressed)
			} // if (Input.MouseInBox)

			// Handel controller input
			if (currentOptionsNumber == 5)
			{
				if (Input.GamePadLeftJustPressed ||
					Input.KeyboardLeftJustPressed)
				{
					currentSensitivity -= 0.2f;
					Sound.Play(Sound.Sounds.Highlight);
				} // if
				if (Input.GamePadRightJustPressed ||
					Input.KeyboardRightJustPressed)
				{
					currentSensitivity += 0.2f;
					Sound.Play(Sound.Sounds.Highlight);
				} // if
				if (currentSoundVolume < 0)
					currentSoundVolume = 0;
				if (currentSoundVolume > 2)
					currentSoundVolume = 2;
			} // if (currentOptionsNumber)

			// Render radio button
			BaseGame.UI.Buttons.RenderOnScreen(new Rectangle(
				sensitivityRect.X +
				(int)(sensitivityRect.Width * currentSensitivity/2) -
				BaseGame.XToRes(gfxRect.Width) / 2,
				sensitivityRect.Y,
				BaseGame.XToRes(gfxRect.Width), BaseGame.YToRes(gfxRect.Height)),
				gfxRect);
			#endregion

			#region Show selected line
			Rectangle[] lineArrowGfxRects = new Rectangle[]
			{
				Line1ArrowGfxRect,
				Line2ArrowGfxRect,
				Line3ArrowGfxRect,
				Line4ArrowGfxRect,
				Line5ArrowGfxRect,
				Line6ArrowGfxRect,
			};
			for (int num = 0; num < 6; num++)
			{
				Rectangle lineRect = BaseGame.CalcRectangleKeep4To3(
					lineArrowGfxRects[num]);
				lineRect.Y += BaseGame.YToRes(125);
				lineRect.X -= BaseGame.XToRes(8 + (int)Math.Round(8 *
					Math.Sin(BaseGame.TotalTime / 0.21212f)));
				bool inLineRect = Input.MouseInBox(lineRect);
				if (Input.MousePos.Y >= lineRect.Y &&
					Input.MousePos.Y <= lineRect.Y + lineRect.Height &&
					currentOptionsNumber != num)
				{
					Sound.Play(Sound.Sounds.Highlight);
					currentOptionsNumber = num;
				} // if (Input.MousePos.Y)

				// Fix height (we changed it for line 3)
				lineRect.Height = BaseGame.CalcRectangleKeep4To3(
					lineArrowGfxRects[0]).Height;
				if (currentOptionsNumber == num)
					BaseGame.UI.Buttons.RenderOnScreen(
						lineRect, UIRenderer.SelectionArrowGfxRect, Color.White);
			} // for (num)

			// Xbox selection (just the last 3 are useable)
			// Left/Right input is handled above
			if (Input.GamePadUpJustPressed ||
				Input.KeyboardUpJustPressed)
			{
				Sound.Play(Sound.Sounds.Highlight);
				currentOptionsNumber = (6 + currentOptionsNumber - 1) % 6;
			} // if (Input.GamePadLeftJustPressed)
			else if (Input.GamePadDownJustPressed ||
				Input.KeyboardDownJustPressed)
			{
				Sound.Play(Sound.Sounds.Highlight);
				currentOptionsNumber = (currentOptionsNumber + 1) % 6;
			} // else if
			#endregion

			#region Bottom buttons
			//BaseGame.UI.RenderBottomButtons(true);
			#endregion

			#region Apply settings when quitting
			if (Input.KeyboardF2JustPressed ||
				Input.KeyboardEscapeJustPressed ||
				Input.KeyboardEnterJustPressed ||
				Input.GamePadBJustPressed ||
        Input.GamePadBackJustPressed) //BaseGame.UI.backButtonPressed
			{
				// Apply settings, for xbox only set music/sound and sensitivity!
				GameSettings.Default.PlayerName = currentPlayerName;
				switch (currentResolution)
				{
					case 0:
						GameSettings.Default.ResolutionWidth = 640;
						GameSettings.Default.ResolutionHeight = 480;
						break;
					case 1:
						GameSettings.Default.ResolutionWidth = 800;
						GameSettings.Default.ResolutionHeight = 600;
						break;
					case 2:
						GameSettings.Default.ResolutionWidth = 1024;
						GameSettings.Default.ResolutionHeight = 768;
						break;
					case 3:
						GameSettings.Default.ResolutionWidth = 1280;
						GameSettings.Default.ResolutionHeight = 1024;
						break;
					case 4:
						// Try to use best resolution available
						GameSettings.Default.ResolutionWidth = 0;
						GameSettings.Default.ResolutionHeight = 0;
						break;
				} // switch
				GameSettings.Default.PerformanceSettings = currentPixelshader;
				GameSettings.Default.PostScreenEffects = usePostScreenShaders;
				GameSettings.Default.ShadowMapping = useShadowMapping;
				GameSettings.Default.HighDetail = useHighDetail;
				GameSettings.Default.MusicVolume = currentMusicVolume;
				GameSettings.Default.SoundVolume = currentSoundVolume;
				GameSettings.Default.ControllerSensibility = currentSensitivity;

				// Save all
				GameSettings.Save();
				// Update game settings
				BaseGame.CheckOptionsAndPSVersion();

        quit = true;                
			} // if (Input.KeyboardEscapeJustPressed)
			#endregion
		} // Render()
		#endregion
	} // class Options
} // namespace DungeonQuest.GameScreens
