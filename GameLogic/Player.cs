// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using DungeonQuest.GameLogic;
using DungeonQuest.GameScreens;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.Properties;
using DungeonQuest.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Player helper class, holds all the current game properties:
	/// Health, Weapon and Score.
	/// Note: This is a static class and holds always all player entries
	/// for the current game. If we would have more than 1 player (e.g.
	/// in multiplayer mode) this should not be a static class!
	/// </summary>
	static class Player
	{
		#region Variables
		/// <summary>
		/// Current game time in ms. Used for time display in game. Also used to
		/// update the level position.
		/// </summary>
		private static float gameTimeMs = 0, lastGameTimeMs = 0;

		/// <summary>
		/// Stop time, sets gameTime to -1 and stops all animations, etc.
		/// </summary>
		public static void StopTime()
		{
			gameTimeMs = lastGameTimeMs = -1;
		} // StopTime()

		/// <summary>
		/// Set game time ms
		/// </summary>
		/// <param name="newMs">New ms</param>
		public static void SetGameTimeMs(float newMs)
		{
			lastGameTimeMs = gameTimeMs;
			gameTimeMs = newMs;
		} // SetGameTimeMs(newMs)

		/// <summary>
		/// Game time
		/// </summary>
		/// <returns>Float</returns>
		public static float GameTime
		{
			get
			{
				return gameTimeMs / 1000.0f;
			} // get
		} // GameTime

		/// <summary>
		/// Last game time, useful to check if we reached a new animation step.
		/// </summary>
		/// <returns>Float</returns>
		public static float LastGameTime
		{
			get
			{
				return lastGameTimeMs / 1000.0f;
			} // get
		} // LastGameTime

		/// <summary>
		/// Won or lost?
		/// </summary>
		public static bool victory = false;

		/// <summary>
		/// Game over?
		/// </summary>
		public static bool gameOver = false;

		/// <summary>
		/// Is game over?
		/// </summary>
		/// <returns>Bool</returns>
		public static bool GameOver
		{
			get
			{
				return gameOver;
			} // get
		} // GameOver

		/// <summary>
		/// Remember if we already uploaded our highscore for this game.
		/// Don't do this twice (e.g. when pressing esc).
		/// </summary>
		static bool alreadyUploadedHighscore = false;

		/// <summary>
		/// Set game over and upload highscore
		/// </summary>
		public static void SetGameOverAndUploadHighscore()
		{
			// Set lifes to 0 and set gameOver to true to mark this game as ended.
			gameOver = true;

			// Upload highscore
			if (alreadyUploadedHighscore == false)
			{
				alreadyUploadedHighscore = true;
				Highscores.SubmitHighscore(score, Highscores.DefaultLevelName);
			} // if (alreadyUploadedHighscore)
		} // SetGameOverAndUploadHighscore()
		#endregion

		#region Current player values (health, weapon, etc.)
		/// <summary>
		/// Max health the player 
		/// </summary>
		public static float MaxHealth = 100.0f;

		/// <summary>
		/// Health, 100 means we have 100% health, everything below means we
		/// are damaged. If we reach 0, we die!
		/// </summary>
		public static float health = 100.0f;

		/// <summary>
		/// Weapon types we can carry with our ship
		/// </summary>
		public enum WeaponTypes
		{
			Club,
			Sword,
			BigSword,
		} // enum WeaponTypes

		/// <summary>
		/// Weapon damage
		/// </summary>
		public static readonly float[] WeaponDamage = new float[]
		{
			5,//4, // Club
			7,//6, // Sword
			9,//8,//9, //15, // Big Sword
		}; // WeaponDamage

		/// <summary>
		/// Role playing stuff
		/// </summary>
		public static int levelPointsToSpend = 0;
		public static int damageIncrease = 0,
			defenseIncrease = 0,
			speedIncrease = 0;

		/// <summary>
		/// Current weapon damage
		/// </summary>
		/// <returns>Float</returns>
		public static float CurrentWeaponDamage
		{
			get
			{
				return WeaponDamage[(int)currentWeapon] + damageIncrease;// *2;
			} // get
		} // CurrentWeaponDamage

		/// <summary>
		/// Current defense
		/// </summary>
		/// <returns>Float</returns>
		public static float CurrentDefense
		{
			get
			{
				return defenseIncrease;
			} // get
		} // CurrentDefense

		/// <summary>
		/// Extra speed
		/// </summary>
		/// <returns>Float</returns>
		public static float ExtraSpeed
		{
			get
			{
				return speedIncrease;
			} // get
		} // ExtraSpeed

		/// <summary>
		/// Weapon we currently have, each weapon is replaced by the
		/// last collected one. No ammunition is used.
		/// </summary>
		public static WeaponTypes currentWeapon = WeaponTypes.Club;

    /// <summary>
    /// Got Club, Sword or Big Sword weapons? If false, we can't
    /// switch to that weapon.
    /// </summary>
    public static bool[] gotWeapons = new bool[3] { true, false, false };

    /// <summary>
    /// Choose next weapon if possible.
    /// </summary>
    public static void NextWeapon()
    {
			WeaponTypes nextWeapon = (WeaponTypes)(((int)currentWeapon+1)%3);
			if (gotWeapons[(int)nextWeapon] == true)
			{
				currentWeapon = nextWeapon;
				return;
			} // if (gotWeapons)
			// Try the other direction
			nextWeapon = (WeaponTypes)(((int)currentWeapon + 2) % 3);
			if (gotWeapons[(int)nextWeapon] == true)
				currentWeapon = nextWeapon;
    } // NextWeapon()

    /// <summary>
    /// Choose previous weapon if possible.
    /// </summary>
    public static void PreviousWeapon()
    {
			WeaponTypes previousWeapon = (WeaponTypes)(((int)currentWeapon + 2) % 3);
			if (gotWeapons[(int)previousWeapon] == true)
			{
				currentWeapon = previousWeapon;
				return;
			} // if (gotWeapons)

			// Try the other direction
			previousWeapon = (WeaponTypes)(((int)currentWeapon + 1) % 3);
			if (gotWeapons[(int)previousWeapon] == true)
				currentWeapon = previousWeapon;
    } // PreviousWeapon()

		/// <summary>
		/// Current score. Used as highscore if game is over.
		/// Start with 25 points to make it easier to get next level!
		/// </summary>
		public static int score = 25; 

		/// <summary>
		/// Current level, start with 1, end with whatever.
		/// </summary>
		public static int level = 1;

    /// <summary>
    /// Did we get the key? We just have to go close to it to collect.
    /// When we got it we can proceed through the door to the second
    /// level.
    /// </summary>
    public static bool gotKey = false;
		#endregion

		#region Reset everything for starting a new game
		/// <summary>
		/// Reset all player entries for restarting a game.
		/// </summary>
		public static void Reset()
		{
			gameOver = false;
			alreadyUploadedHighscore = false;
			gameTimeMs = 0;
			lastGameTimeMs = 0;
			health = 100.0f;
			MaxHealth = 100.0f;
			score = 25;
			level = 1;
			levelPointsToSpend = 0;
			damageIncrease = 0;
			defenseIncrease = 0;
			speedIncrease = 0;
			currentWeapon = WeaponTypes.Club;
			gotWeapons = new bool[3] { true, false, false };
			gotKey = false;
		} // Reset(setLevelName)
		#endregion

		#region Reached next level
		/// <summary>
		/// Need points for level
		/// </summary>
		/// <param name="checkLevel">New level</param>
		/// <returns>Float</returns>
		public static float NeedPointsForLevel(int checkLevel)
		{
			float points = 0;
			float pointsAdd = 50;
			for (int i = 0; i < checkLevel; i++)
			{
				points += pointsAdd;
				pointsAdd += 10;
			} // for (int)
			return points;
		} // NeedPointsForLevel(checkLevel)

		/// <summary>
		/// Reached next level
		/// </summary>
		/// <returns>Bool</returns>
		public static bool ReachedNextLevel()
		{
			return (Player.score > NeedPointsForLevel(level));
		} // ReachedNextLevel()
		#endregion

		#region Handle game logic
		/// <summary>
		/// RX
		/// </summary>
		/// <param name="x">X</param>
		/// <returns>Int</returns>
		static int RX(int x)
		{
			return (int)(0.5f + x * BaseGame.Width / 1024.0f);
		} // RX()

		/// <summary>
		/// RY
		/// </summary>
		/// <param name="y">Y</param>
		/// <returns>Int</returns>
		static int RY(int y)
		{
			return (int)(0.5f + y * BaseGame.Height / 640.0f);
		} // RY()

		/// <summary>
		/// Handle game logic
		/// </summary>
		public static void HandleGameLogic()
		{
			// Don't handle any more game logic if game is over.
			if (Player.GameOver)
			{
				if (victory)
				{
					TextureFont.WriteTextCentered(
						BaseGame.Width / 2, BaseGame.Height / 8,
						"You reached the Treasure. Good work!",
						Color.Yellow, 1.0f);

					// Display Victory message
					TextureFont.WriteTextCentered(
							BaseGame.Width / 2, BaseGame.Height / 4,
							"Victory! You won.");
				} // if (victory)
				else
				{
					// Display game over message
					TextureFont.WriteTextCentered(
							BaseGame.Width / 2, BaseGame.Height / 4,
							"Game Over! You lost.");
				} // else

				TextureFont.WriteTextCentered(
					BaseGame.Width / 2, BaseGame.Height / 4 + RY(40),
					"Your Highscore: " + Player.score +
					" (#" + (Highscores.GetRankFromCurrentScore(Player.score) + 1) + ")",
					ColorHelper.FromArgb(200, 200, 200), 1.0f);
				TextureFont.WriteTextCentered(
					BaseGame.Width / 2, BaseGame.Height / 4 + RY(80),
					"Press Start or Enter to restart",
					ColorHelper.FromArgb(200, 200, 200), 1.0f);

				// Show highscores!
				int xPos = RX(300);
				int yPos = BaseGame.Height / 4 + RY(80) + RY(65);
				TextureFont.WriteText(xPos, yPos,
					"Highscores:");
				yPos += RY(40);
				Highscores.Highscore[] selectedHighscores = Highscores.AllHighscores;
				for (int num = 0; num < Highscores.NumOfHighscores; num++)
				{
					Color col = Input.MouseInBox(new Rectangle(
						xPos, yPos + num * RY(24), RX(400), RY(28))) ?
						Color.White : ColorHelper.FromArgb(200, 200, 200);
					TextureFont.WriteText(xPos, yPos + num * RY(24),
						(1 + num) + ".", col);
					TextureFont.WriteText(xPos + RX(50), yPos + num * RY(24),
						selectedHighscores[num].name, col);
					TextureFont.WriteText(xPos + RX(340), yPos + num * RY(24),
						selectedHighscores[num].points.ToString(), col);
				} // for (num)

				// Restart with start
				if (Input.GamePad.Buttons.Start == ButtonState.Pressed ||
					Input.KeyboardEnterJustPressed ||
					Input.MouseLeftButtonJustPressed)
				{
					// Reset everything
					Player.Reset();
					BaseGame.camera.Reset();
					GameManager.Reset();
				} // if (Input.GamePad.Buttons.Start)
				return;
			} // if (Player.GameOver)

			// Increase game time
			lastGameTimeMs = gameTimeMs;
			gameTimeMs += BaseGame.ElapsedTimeThisFrameInMs;// / 10.0f; //tst!

			// Change weapon with left/right shoulder weapons
			if (Input.GamePadLeftShoulderJustPressed ||
				Input.MouseWheelDelta < 0 ||
				Input.KeyboardKeyJustPressed(GameSettings.Default.RollRightKey))
			{
				PreviousWeapon();
			} // if (Input.GamePadLeftShoulderJustPressed)
			else if (Input.GamePadRightShoulderJustPressed ||
				Input.MouseWheelDelta > 0 ||
				Input.KeyboardKeyJustPressed(GameSettings.Default.RollLeftKey))
			{
				NextWeapon();
			} // else if

			//TextureFont.WriteText(100, 100, "Playerpos="+BaseGame.camera.PlayerPos);

			// Player has fallen out? Then restart!
			if (BaseGame.camera.PlayerPos.Z < -85.0f)
			{
				// Reset everything
				Player.Reset();
				BaseGame.camera.Reset();
				GameManager.Reset();
			} // if (Input.GamePad.Buttons.Start)

			// Increase level?
			if (ReachedNextLevel())
			{
				// Inc level
				Player.level++;
				// Give skill point to spend
				Player.levelPointsToSpend++;
				// Give player more total health
				Player.MaxHealth += 10.0f;
				// Refresh health
				Player.health = Player.MaxHealth;

				// Just give the player a sword at level 5 and
				// the big sword at level 10.
				if (Player.level == 5)
					currentWeapon = WeaponTypes.Sword;
				else if (Player.level == 10)
					currentWeapon = WeaponTypes.BigSword;

				// Show screen border effect
				GameManager.bloodBorderColor = Color.Green;
				GameManager.bloodBorderEffect = 1.0f;// 0.75f;
				// Also show glow and some effects
				EffectManager.AddRocketOrShipFlareAndSmoke(
					BaseGame.camera.PlayerPos + new Vector3(0, 0, 1.5f), 5.0f, 0.1f);
				EffectManager.AddEffect(
					BaseGame.camera.PlayerPos + new Vector3(0, 0, 1.5f),
					EffectManager.EffectType.LightShort, 2.5f, 0);
				// Show text message
				GameManager.AddTextMessage("You reached Level " + Player.level,
					GameManager.TextTypes.LevelUp);
				GameManager.AddTextMessage(
					"Improve your skills with X, Y, or B.",
					GameManager.TextTypes.Normal);
				// And don't forget the sound effect
				Sound.Play(Sound.Sounds.LevelUp);
			} // if (Player.score)

			// If we run out of hitpoints, we die
			if (Player.health <= 0.0f)
			{
				Player.gameOver = true;
				Player.victory = false;
				Sound.Play(Sound.Sounds.PlayerWasHit);
				Sound.Play(Sound.Sounds.Defeat);

				Highscores.SubmitHighscore(Player.score, Highscores.DefaultLevelName);

				GameManager.AddTextMessage("You died!",
					GameManager.TextTypes.Died);
			} // if (Player.health)

			// If we reach the treasure we win (only check x/y because
			// the helper is too low in the ground).
			if (Vector2.Distance(new Vector2(
				BaseGame.camera.PlayerPos.X, BaseGame.camera.PlayerPos.Y),
				new Vector2(
				GameManager.treasurePosition.X, GameManager.treasurePosition.Y)) <
				4.0f)
			{
				Player.gameOver = true;
				Player.victory = true;
				Sound.Play(Sound.Sounds.Victory);

				Highscores.SubmitHighscore(Player.score, Highscores.DefaultLevelName);

				GameManager.AddTextMessage("You won!",
					GameManager.TextTypes.LevelUp);
			} // if (Vector3.Distance)

			// Handle skill points, if we have any
			if (levelPointsToSpend > 0)
			{
				if (Input.GamePadXJustPressed ||
					Input.KeyboardKeyJustPressed(Keys.X))
				{
					defenseIncrease++;
					levelPointsToSpend--;
					Sound.Play(Sound.Sounds.Click);

					GameManager.AddTextMessage("You improved your defense.",
						GameManager.TextTypes.LevelUp);
				} // if (Input.GamePadXJustPressed)
				else if (Input.GamePadYJustPressed ||
					Input.KeyboardKeyJustPressed(Keys.Y))
				{
					speedIncrease++;
					levelPointsToSpend--;
					Sound.Play(Sound.Sounds.Click);

					GameManager.AddTextMessage("You can run faster now.",
						GameManager.TextTypes.LevelUp);
				} // else if
				else if (Input.GamePadBJustPressed ||
					Input.KeyboardKeyJustPressed(Keys.B))
				{
					damageIncrease++;
					levelPointsToSpend--;
					Sound.Play(Sound.Sounds.Click);

					GameManager.AddTextMessage("Your weapon skills were improved.",
						GameManager.TextTypes.LevelUp);
				} // else if
			} // if (levelPointsToSpend)

			// Was F2 or Alt+O just pressed?
			if (Input.KeyboardF2JustPressed ||
				((Input.Keyboard.IsKeyDown(Keys.LeftAlt) ||
				Input.Keyboard.IsKeyDown(Keys.RightAlt)) &&
				Input.KeyboardKeyJustPressed(Keys.O)))
			{
				// Then open up the options screen!
				DungeonQuestGame.AddGameScreen(new Options());
			} // if
		} // HandleGameLogic()
		#endregion
	} // class Player
} // namespace DungeonQuest
