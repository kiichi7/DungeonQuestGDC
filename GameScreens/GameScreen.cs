// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameScreens
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:40

#region Using directives
using DungeonQuest.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DungeonQuest.GameScreens
{
	/// <summary>
	/// Game screen
	/// </summary>
	class GameScreen : IGameScreen
	{
		#region Properties
		/// <summary>
		/// Name of this game screen
		/// </summary>
		/// <returns>String</returns>
		public string Name
		{
			get
			{
				return "GameScreen";
			} // get
		} // Name

		/// <summary>
		/// Quit
		/// </summary>
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

		#region Constructor
		/// <summary>
		/// Create game screen
		/// </summary>
		public GameScreen()
		{
			/*TODO: load cave, start level, etc.
			// Load level
			RacingGameManager.LoadLevel(TrackSelection.SelectedTrack);

			// Reset player variables (start new game, reset time and position)
			RacingGameManager.Player.Reset();

			// Fix light direction (was changed by CarSelection screen!)
			// LightDirection will normalize
			BaseGame.LightDirection = LensFlare.DefaultLightPos;

			// Start gear sound
			Sound.StartGearSound();

			// Play game music
			Sound.Play(Sound.Sounds.GameMusic);

			// Start sign sound
			Sound.Play(Sound.Sounds.Startsign);
			 */
		} // GameScreen()
		#endregion

		#region Render
		/// <summary>
		/// Render game screen. Called each frame.
		/// </summary>
		public void Run(DungeonQuestGame game)
		{
			// Let everything be handled by the game manager!
			game.gameManager.Run(true);

			if (Input.KeyboardEscapeJustPressed ||
				Input.GamePadBackJustPressed)
				// Just exit this screen and show end screen!
				quit = true;
		} // Run(game)
		#endregion
	} // class GameScreen
} // namespace DungeonQuest.GameScreens
