// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameScreens
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:40

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DungeonQuest.GameScreens
{
	/// <summary>
	/// Game screen helper interface for all game screens of our game.
	/// Helps us to put them all into one list and manage them in our BaseGame.
	/// </summary>
	public interface IGameScreen
	{
		/// <summary>
		/// Name of this game screen, e.g. Main menu, Highscores
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Returns true if we want to quit this screen and return to the
		/// previous screen. If no more screens are left the game is exited.
		/// </summary>
		bool Quit { get; }

		/// <summary>
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		void Run(DungeonQuestGame game);
	} // IGameScreen
} // namespace DungeonQuest.GameScreens
