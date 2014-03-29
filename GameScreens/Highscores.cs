// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameScreens
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:40

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using System.Runtime.Serialization;
using DungeonQuest.Helpers;
using DungeonQuest.Graphics;
using DungeonQuest.Properties;
using DungeonQuest.Game;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace DungeonQuest.GameScreens
{
	/// <summary>
	/// Highscores
	/// </summary>
	class Highscores : IGameScreen
	{
		#region Variables
		/// <summary>
		/// Highscore modes
		/// </summary>
		private enum HighscoreModes
		{
			Local,
			//obs: OnlineThisHour,
			//OnlineTotal,
		} // enum HighscoreModes

		/// <summary>
		/// Current highscore mode, initially set to local.
		/// </summary>
		private HighscoreModes highscoreMode = HighscoreModes.Local;
		#endregion

		#region Properties
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

		#region Highscore helper class
		/// <summary>
		/// Highscore helper class
		/// </summary>
		public struct Highscore
		{
			/// <summary>
			/// Player name
			/// </summary>
			public string name;
			/// <summary>
			/// Level name
			/// </summary>
			public string level;
			/// <summary>
			/// Highscore points 
			/// </summary>
			public int points;

			/// <summary>
			/// Create highscore
			/// </summary>
			/// <param name="setName">Set name</param>
			/// <param name="setLevelName">Set level name</param>
			/// <param name="setPoints">Set points</param>
			public Highscore(string setName, string setLevelName, int setPoints)
			{
				name = setName;
				level = setLevelName;
				points = setPoints;
			} // Highscore(setName, setLevelName, setPoints)

			/// <summary>
			/// To string
			/// </summary>
			/// <returns>String</returns>
			public override string ToString()
			{
				return name + ":" + level + ":" + points;
			} // ToString()
		} // struct Highscore

		/// <summary>
		/// Number of highscores displayed in this screen.
		/// </summary>
		public const int NumOfHighscores = 10;
		/// <summary>
		/// List of remembered highscores.
		/// </summary>
		private static Highscore[] highscores = new Highscore[NumOfHighscores];

		/// <summary>
		/// Get top highscore, displayed in the upper right when playing.
		/// </summary>
		public static int TopHighscore
		{
			get
			{
				return highscores[0].points;
			} // get
		} // TopHighscore

		/// <summary>
		/// Get all highscores, displayed on the game over screen.
		/// </summary>
		/// <returns>Int</returns>
		public static Highscore[] AllHighscores
		{
			get
			{
				return highscores;
			} // get
		} // AllHighscores

		/// <summary>
		/// Write highscores to string. Used to save to settings.
		/// </summary>
		private static void WriteHighscoresToSettings()
		{
			GameSettings.Default.Highscores = StringHelper.WriteArrayData(highscores);
		} // WriteHighscoresToSettings()

		/// <summary>
		/// Read highscores from settings
		/// </summary>
		/// <returns>True if reading succeeded, false otherwise.</returns>
		private static bool ReadHighscoresFromSettings()
		{
			if (String.IsNullOrEmpty(GameSettings.Default.Highscores))
				return false;

			try
			{
				string[] allHighscores = GameSettings.Default.Highscores.Split(
					new char[] { ',' });

				for (int num = 0; num < allHighscores.Length &&
					num < highscores.Length; num++)
				{
					string[] oneHighscore =
						StringHelper.SplitAndTrim(allHighscores[num], ':');
					highscores[num] = new Highscore(
						oneHighscore[0], oneHighscore[1],
						Convert.ToInt32(oneHighscore[2]));
				} // for (num)

				return true;
			} // try
			catch (Exception ex)
			{
				Log.Write("Failed to load highscores: " + ex.ToString());
				return false;
			} // catch
		} // ReadHighscoresFromSettings()
		#endregion

		#region Static constructor
		/// <summary>
		/// Default level name
		/// </summary>
		public static string DefaultLevelName = "Apocalypse";

		/// <summary>
		/// Create Highscores class, will basically try to load highscore list,
		/// if that fails we generate a standard highscore list!
		/// </summary>
		static Highscores()
		{
			//obs: gameFileHash = GenerateHashFromFile("Rocket Commander.exe");

			if (ReadHighscoresFromSettings() == false)
			{
				// Generate default list
				highscores[9] = new Highscore("Newbie", DefaultLevelName, 0);
				highscores[8] = new Highscore("Master_L", DefaultLevelName, 50);
				highscores[7] = new Highscore("Freshman", DefaultLevelName, 100);
				highscores[6] = new Highscore("Desert-Fox", DefaultLevelName, 150);
				highscores[5] = new Highscore("Judge", DefaultLevelName, 200);
				highscores[4] = new Highscore("ViperDK", DefaultLevelName, 250);
				highscores[3] = new Highscore("Netfreak", DefaultLevelName, 300);
				highscores[2] = new Highscore("exDreamBoy", DefaultLevelName, 350);
				highscores[1] = new Highscore("Waii", DefaultLevelName, 400);
				highscores[0] = new Highscore("abi", DefaultLevelName, 450);

				WriteHighscoresToSettings();
			} // if (ReadHighscoresFromSettings)
		} // Highscores()
		#endregion

		#region Constructor
		/// <summary>
		/// Create highscores
		/// </summary>
		public Highscores()
		{
		} // Highscores()
		#endregion

		#region Get rank from current score
		/// <summary>
		/// Get rank from current score.
		/// Used in game to determinate rank while flying around ^^
		/// </summary>
		/// <param name="score">Score</param>
		/// <returns>Int</returns>
		public static int GetRankFromCurrentScore(int score)
		{
			// Just compare with all highscores and return the rank we have reached.
			for (int num = 0; num < highscores.Length; num++)
			{
				if (score >= highscores[num].points)
					return num;
			} // for (num)

			// No Rank found, use rank 11
			return highscores.Length;
		} // GetRankFromCurrentScore(score)
		#endregion

		#region Submit highscore after game
		//obs: static bool onlineUploadHighscoreFailedAlreadyLogged = false;
		/// <summary>
		/// Submit highscore. Done after each game is over (won or lost).
		/// New highscore will be added to the highscore screen and send
		/// to the online server.
		/// </summary>
		/// <param name="score">Score</param>
		/// <param name="levelName">Level name</param>
		public static void SubmitHighscore(int score, string levelName)
		{
			// Search which highscore rank we can replace
			for (int num = 0; num < highscores.Length; num++)
			{
				if (score >= highscores[num].points)
				{
					// Move all highscores up
					for (int moveUpNum = highscores.Length - 1; moveUpNum > num;
						moveUpNum--)
					{
						highscores[moveUpNum] = highscores[moveUpNum - 1];
					} // for (moveUpNum)

					// Add this highscore into the local highscore table
					highscores[num].name = GameSettings.Default.PlayerName;
					highscores[num].level = levelName;
					highscores[num].points = score;

					// And save that
					Highscores.WriteHighscoresToSettings();

					break;
				} // if
			} // for (num)

			// Else no highscore was reached, we can't replace any rank.
			/*can't do on the Xbox 360!
			// Upload highscore to server with help of the webservice :)
			// Do this asyncronly, it could take a while and we don't want to wait
			// for it to complete (there is no return value anyway).
			new Thread(new ThreadStart(
			 * etc.
			 */
		} // SubmitHighscore(score)
		#endregion

		#region Get online highscores
		Highscore[] onlineHighscores = new Highscore[10];
		//obs: Thread onlineGetHighscoreThread = null;

		/// <summary>
		/// Get online highscores
		/// </summary>
		/// <param name="onlyThisHour">Only this hour</param>
		private void GetOnlineHighscores(bool onlyThisHour)
		{
			// Clear all online highscores and wait for a new update!
			for (int num = 0; num < onlineHighscores.Length; num++)
			{
				onlineHighscores[num].name = "-";
				onlineHighscores[num].level = "";
				onlineHighscores[num].points = 0;
			} // for (num)

			/*obs
			// Stop any old threads
			if (onlineGetHighscoreThread != null)
				onlineGetHighscoreThread.Abort();

			// Ask web service for highscore list! Do this asyncronly,
			// it could take a while and we don't want to wait for it to complete.
			onlineGetHighscoreThread = new Thread(new ThreadStart(
				// Anoymous delegates, isn't .NET 2.0 great? ^^
			 * etc.
			 */
		} // GetOnlineHighscores(onlyThisHour)
		#endregion

		#region Run
		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		public void Run(DungeonQuestGame game)
		{
			// Render background
			game.RenderMenuBackground();

			// Show highscores, allow to select between local highscores,
			// online highscores this hour and online highscores best of all time. 
			int xPos = 100 * BaseGame.Width / 1024;
			int yPos = 260 * BaseGame.Height / 768;
			TextureFont.WriteText(xPos, yPos,
				"Highscores:");

			// Local Highscores
			Rectangle rect = new Rectangle(
				xPos + 210 * BaseGame.Width / 1024,
				yPos + 0 * BaseGame.Height / 768, 130, 28);
			bool highlighted = Input.MouseInBox(rect);
			TextureFont.WriteText(rect.X, rect.Y, "Local",
				highscoreMode == HighscoreModes.Local ? Color.Red :
				highlighted ? Color.LightSalmon : Color.White);
			if (highlighted &&
				Input.MouseLeftButtonJustPressed)
				highscoreMode = HighscoreModes.Local;

			yPos = 310 * BaseGame.Height / 768;
			Highscore[] selectedHighscores =
				highscoreMode == HighscoreModes.Local ?
				highscores : onlineHighscores;

			for (int num = 0; num < NumOfHighscores; num++)
			{
				Color col = Input.MouseInBox(new Rectangle(
					xPos, yPos + num * 30, 600 + 200, 28)) ?
					Color.White : ColorHelper.FromArgb(200, 200, 200);
				TextureFont.WriteText(xPos, yPos + num * 29,
					(1 + num) + ".", col);
				TextureFont.WriteText(xPos + 50, yPos + num * 30,
					selectedHighscores[num].name, col);
				TextureFont.WriteText(xPos + 340, yPos + num * 30,
					"Score: " + selectedHighscores[num].points, col);
				TextureFont.WriteText(xPos + 610, yPos + num * 30,
					"Mission: " + selectedHighscores[num].level, col);
			} // for (num)
			/*TODO
			if (game.RenderMenuButton(MenuButton.Back,
				new Point(1024 - 230, 768 - 150)))
			{
				quit = true;
			} // if
			 */
		} // Run(game)
		#endregion
	} // class Highscores
} // namespace DungeonQuest.GameScreens
