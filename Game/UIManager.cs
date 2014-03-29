// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Game
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using Texture = DungeonQuest.Graphics.Texture;
using DungeonQuest.GameLogic;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Helper class to manage all the UI we see (some bars and the enemy
	/// information)
	/// </summary>
	class UIManager
	{
		#region Variables
		/// <summary>
		/// UI box for all UI elements
		/// </summary>
		static Texture uiBox;

		/// <summary>
		/// Gfx rectangles
		/// </summary>
		static readonly Rectangle BigBoxGfxRect = new Rectangle(0, 0, 161, 63),
			SmallBoxGfxRect = new Rectangle(100, 81, 154, 46),
			XButtonGfxRect = new Rectangle(2, 77, 27, 27),
			YButtonGfxRect = new Rectangle(31, 77, 27, 27),
			BButtonGfxRect = new Rectangle(60, 77, 27, 27),
			BarGfxRect = new Rectangle(0, 64, 197, 13),
			KeyGfxRect = new Rectangle(183, 3, 69, 31);
		#endregion

		#region Draw UI
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
		/// Draw user interface
		/// </summary>
		public static void DrawUI()
		{
			if (uiBox == null)
				uiBox = new Texture("UIBox");

#if XBOX360
			int xBorder = BaseGame.Width / 20;
			int yBorder = BaseGame.Height / 20;
#else
			// For the PC 1% (or 2.5%) is fine
			int xBorder = BaseGame.Width / 100;// 40;
			int yBorder = BaseGame.Height / 100;// 40;
#endif

			// Find nearest enemy and show effect around him.
			AnimatedGameObject nearestEnemy = null;
			foreach (AnimatedGameObject model in GameManager.animatedObjects)
				if ((nearestEnemy == null ||
					(BaseGame.camera.PlayerPos - model.positionMatrix.Translation).
					Length() <
					(BaseGame.camera.PlayerPos - nearestEnemy.positionMatrix.Translation).
					Length()) &&
					// Must not be dead already
					model.state != AnimatedGameObject.States.Die)
					nearestEnemy = model;

			// Show enemy on top, just show the nearest one
			if (nearestEnemy != null &&
				// Must be near (20 units or less)
				Vector3.Distance(nearestEnemy.positionMatrix.Translation,
				BaseGame.camera.PlayerPos) <= 20)
			{
				TextureFont.WriteTextCentered(BaseGame.Width / 2,// + RX(6),
					yBorder,
          StringHelper.SplitFunctionNameToWordString(
					nearestEnemy.type.ToString()));
				float hitpointsPercentage = nearestEnemy.hitpoints /
					GameManager.MonsterHitpoints[(int)nearestEnemy.type];
				uiBox.RenderOnScreen(
					new Rectangle(BaseGame.Width / 2 - RX(75), yBorder + RY(30),
					RX((int)(150 * hitpointsPercentage)), RY(13)),
					BarGfxRect,
					// Use red color
					new Color(255, 105, 105, 200));

				// Also show 3D effect around that enemy
				EffectManager.AddEnemyRing(
					nearestEnemy.positionMatrix.Translation + new Vector3(0, 0,
					0.5f + (float)Math.Sin(Player.GameTime / 0.256f) * 0.175f),
					0.7f);//0.75f);
				EffectManager.AddEnemyRing(
					nearestEnemy.positionMatrix.Translation + new Vector3(0, 0,
					0.5f + (float)Math.Sin((0.68f + Player.GameTime) / 0.256f) * 0.1625f),
					0.8f);//0.75f);
				EffectManager.AddEnemyRing(
					nearestEnemy.positionMatrix.Translation + new Vector3(0, 0,
					0.5f + (float)Math.Sin((1.36f + Player.GameTime) / 0.256f) * 0.15f),
					0.9f);//0.75f);
			} // if (nearestEnemy)

			// Show right box (skills and level ups)
			int rightBoxWidth =
				BigBoxGfxRect.Width * BaseGame.Width / 800;// 1024;
			int rightBoxHeight = 2 * // more place for the text on the right
				BigBoxGfxRect.Height * BaseGame.Height / 600;// 768;
			Rectangle rightBoxRect = new Rectangle(
				BaseGame.Width - (xBorder + rightBoxWidth),
				BaseGame.Height - (yBorder + rightBoxHeight),
				rightBoxWidth, rightBoxHeight);
			uiBox.RenderOnScreen(rightBoxRect, BigBoxGfxRect,
				new Color(255, 255, 255, 200));

			// Do we got the key?
			if (Player.gotKey)
			{
				// Show it (above the skills box)
				uiBox.RenderOnScreen(new Rectangle(
					rightBoxRect.Right-RX(KeyGfxRect.Width*2),
					rightBoxRect.Y-RY(KeyGfxRect.Height*2+5),
					RX(KeyGfxRect.Width*2), RY(KeyGfxRect.Height*2)),
					KeyGfxRect);
			} // if (Player.gotKey)

			TextureFont.WriteTextCentered(
				rightBoxRect.X + rightBoxRect.Width / 2 - RX(6),
				rightBoxRect.Y + RY(7),
				"Skills ("+Player.levelPointsToSpend+" left)");

			uiBox.RenderOnScreen(new Rectangle(
				rightBoxRect.X + RX(5), rightBoxRect.Y + RY(32 + 6), RX(20), RY(20)),
				XButtonGfxRect);
			TextureFont.WriteText(rightBoxRect.X + RX(36), rightBoxRect.Y + RY(32+5),
				"Defence ("+Player.defenseIncrease+")");
			uiBox.RenderOnScreen(new Rectangle(
				rightBoxRect.X + RX(5), rightBoxRect.Y + RY(58 + 10), RX(20), RY(20)),
				YButtonGfxRect);
			TextureFont.WriteText(rightBoxRect.X + RX(36), rightBoxRect.Y + RY(58+10),
				"Speed (" + Player.speedIncrease + ")");
			uiBox.RenderOnScreen(new Rectangle(
				rightBoxRect.X + RX(5), rightBoxRect.Y + RY(83 + 15), RX(20), RY(20)),
				BButtonGfxRect);
			TextureFont.WriteText(rightBoxRect.X + RX(36), rightBoxRect.Y + RY(83+15),
				"Attack (" + Player.damageIncrease + ")");

			// Left side shows health bar, next level bar and points
			int leftBoxWidth =
				BigBoxGfxRect.Width * BaseGame.Width / 800;// 1024;
			int leftBoxHeight =
				SmallBoxGfxRect.Height * BaseGame.Height / 600;// 768;
			Rectangle lightBoxRect3 = new Rectangle(
				xBorder, BaseGame.Height - (yBorder + leftBoxHeight + RY(10)),
				leftBoxWidth, leftBoxHeight + RY(10));
			Rectangle lightBoxRect2 = new Rectangle(
				xBorder, lightBoxRect3.Y - (leftBoxHeight +RY(4)),
				leftBoxWidth, leftBoxHeight);
			Rectangle lightBoxRect1 = new Rectangle(
				xBorder, lightBoxRect2.Y - (leftBoxHeight + RY(4)),
				leftBoxWidth, leftBoxHeight);
			uiBox.RenderOnScreen(lightBoxRect1, SmallBoxGfxRect,
				new Color(255, 255, 255, 200));
			uiBox.RenderOnScreen(lightBoxRect2, SmallBoxGfxRect,
				new Color(255, 255, 255, 200));
			uiBox.RenderOnScreen(lightBoxRect3, SmallBoxGfxRect,
				new Color(255, 255, 255, 200));

			TextureFont.WriteTextCentered(
				lightBoxRect1.X + lightBoxRect1.Width / 2,// + RX(4),
				lightBoxRect1.Y + RY(5),
				"Health");
			float healthPercentage = Player.health / Player.MaxHealth;
			uiBox.RenderOnScreen(
				new Rectangle(lightBoxRect1.X + RX(5), lightBoxRect1.Y+RY(30),
				(int)((lightBoxRect1.Width - RX(10))*healthPercentage), RY(13)),
				BarGfxRect,
				// Use green color
				new Color(105, 255, 105, 200));

			TextureFont.WriteTextCentered(
				lightBoxRect2.X + lightBoxRect2.Width / 2 - RX(4),
				lightBoxRect2.Y + RY(5),
				"Next Level "+Player.level);
			//obs: float scorePercentage = (float)(Player.score % 50)/50.0f;
			float thisLevelPoints = Player.NeedPointsForLevel(Player.level - 1);
			float nextLevelPoints = Player.NeedPointsForLevel(Player.level);
			float scorePercentage =
				(Player.score - thisLevelPoints) / (nextLevelPoints - thisLevelPoints);
			uiBox.RenderOnScreen(
				new Rectangle(lightBoxRect2.X + RX(5), lightBoxRect2.Y + RY(30),
				(int)((lightBoxRect2.Width - RX(10)) * scorePercentage), RY(13)),
				BarGfxRect,
				// Use yellow color
				new Color(255, 255, 105, 200));

			TextureFont.WriteTextCentered(
				lightBoxRect3.X + lightBoxRect3.Width / 2 - RX(4),
				lightBoxRect3.Y + RY(5),
				"Points: "+Player.score);
			TextureFont.WriteTextCentered(
				lightBoxRect3.X + lightBoxRect3.Width / 2 - RX(4),
				lightBoxRect3.Y + RY(5 + 28),
				"Time: " +
				(((int)Player.GameTime) / 60).ToString("00") + ":" +
				(((int)Player.GameTime) % 60).ToString("00"));

			/*tst
			uiBox.RenderOnScreen(
				//TODO: screen relative (use 1280 as default?)
				new Rectangle(100, 100, 200, 13),
				BarGfxRect,
				new Color(255, 255, 255, 200));
			/*tst boxes
			uiBox.RenderOnScreen(
				//TODO: screen relative (use 1280 as default?)
				new Rectangle(100, 100, 161*3/2, 63*3/2),
				BigBoxGfxRect);
			uiBox.RenderOnScreen(
				//TODO: screen relative
				new Rectangle(100, 300, 154*3/2, 46*3/2),
				SmallBoxGfxRect);

			/*tst buttons
			uiBox.RenderOnScreen(
				//TODO: screen relative
				new Rectangle(100, 500, 27, 27),
				XButtonGfxRect);
			uiBox.RenderOnScreen(
				//TODO: screen relative
				new Rectangle(150, 500, 27, 27),
				YButtonGfxRect);
			uiBox.RenderOnScreen(
				//TODO: screen relative
				new Rectangle(200, 500, 27, 27),
				BButtonGfxRect);
			 */
		} // DrawUI()
		#endregion

		#region Unit Testing
		/// <summary>
		/// Test user interface
		/// </summary>
		static public void TestUI()
		{
			Texture backgroundTexture = null;

			TestGame.Start("TestUI",
				delegate
				{
					backgroundTexture = new Texture("TestScreenshot");
				},
				delegate
				{
					backgroundTexture.RenderOnScreen(BaseGame.ResolutionRect);

					UIManager.DrawUI();
				});
		} // TestUI()
    #endregion
	} // class UIManager
} // namespace DungeonQuest.Game
