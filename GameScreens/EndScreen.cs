// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameScreens
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using DungeonQuest.Game;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.Properties;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Texture = DungeonQuest.Graphics.Texture;
#endregion

namespace DungeonQuest.GameScreens
{
	/// <summary>
	/// Credits
	/// </summary>
	/// <returns>IGame screen</returns>
	class EndScreen : IGameScreen
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
				return "Credits";
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

		/// <summary>
		/// Book texture for end screen ad ^^
		/// </summary>
		private Texture bookTexture = null;

		/// <summary>
		/// Book gfx rect
		/// </summary>
		static readonly Rectangle BookGfxRect = new Rectangle(1, 2, 254, 316);
		#endregion

		#region Constructor
		/// <summary>
		/// Create credits
		/// </summary>
		public EndScreen()
		{
			bookTexture = new Texture("XnaBook");
		} // EndScreen()
		#endregion

		#region Write credits
		/// <summary>
		/// Write credits
		/// </summary>
		/// <param name="xPos">X coordinate</param>
		/// <param name="yPos">Y coordinate</param>
		/// <param name="leftText">Left text</param>
		/// <param name="rightText">Right text</param>
		private void WriteCredits(int xPos, int yPos,
			string leftText, string rightText)
		{
			TextureFont.WriteText(xPos, yPos, leftText, Color.Gray);
			TextureFont.WriteText(xPos + RX(480), yPos/* + 8*/, rightText);
		} // WriteCredits(xPos, yPos, leftText)

		/// <summary>
		/// Write link
		/// </summary>
		/// <param name="xPos">X position</param>
		/// <param name="yPos">Y position</param>
		/// <param name="text">Text</param>
		/// <param name="link">Link</param>
		private void WriteLink(int xPos, int yPos,
			string text, string link)
		{
			TextureFont.WriteText(xPos, yPos, text, Color.Gray);
			TextureFont.WriteText(xPos, yPos+RY(36), link, Color.Yellow);
		} // WriteLink(xPos, yPos, text)

		/*obs
		/// <summary>
		/// Write credits with link
		/// </summary>
		/// <param name="xPos">X coordinate</param>
		/// <param name="yPos">Y coordinate</param>
		/// <param name="leftText">Left text</param>
		/// <param name="rightText">Right text</param>
		/// <param name="linkText">Link text</param>
		private void WriteCreditsWithLink(int xPos, int yPos, string leftText,
			string rightText, string linkText, DungeonQuestGame game)
		{
			WriteCredits(xPos, yPos, leftText, rightText);

			// Process link (put below rightText)
			bool overLink = Input.MouseInBox(new Rectangle(
				xPos + 440, yPos + 8 + TextureFont.Height, 200, TextureFont.Height));
			TextureFont.WriteText(xPos + 440, yPos /*+ 8* + TextureFont.Height, linkText,
				overLink ? Color.Red : Color.White);
			if (overLink &&
				Input.MouseLeftButtonJustPressed)
			{
#if !XBOX360
				Process.Start(linkText);
				Thread.Sleep(100);
#endif
			} // if (overLink)
		} // WriteCreditsWithLink(xPos, yPos, leftText)
		 */
		#endregion

		#region Run
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
		/// Run game screen. Called each frame.
		/// </summary>
		/// <param name="game">Form for access to asteroid manager and co</param>
		public void Run(DungeonQuestGame game)
		{
			// Render background
			game.RenderMenuBackground();

			// Fill complete background to black (75%)
			bookTexture.RenderOnScreen(
				BaseGame.ResolutionRect,
				new Rectangle(128, 380, 1, 1),
				ColorHelper.ApplyAlphaToColor(Color.Black, 0.75f));

			//TODO: at the beginning?
			//Better at end when quitting, also show link to website and book and stuff ..

			// Credits
			TextureFont.WriteTextCentered(BaseGame.Width/2, RY(20), "Credits");

			int xPos = RX(60);
			int yPos = RY(111);
			WriteCredits(xPos, yPos- RY(40),
				"Dungeon Quest was created in 4 days on the GDC 2007.",
				"");
			WriteCredits(xPos, yPos, "Idea, Design, Programming",
				"Benjamin Nitschke (abi)");
			WriteCredits(xPos, yPos + RY(40), "3D Modeling, Design",
				"Christoph Rienaecker (WAII)");
			WriteCredits(xPos, yPos + RY(80),
				"Thanks to Kai Walter for the sound effects and music!", "");

			TextureFont.WriteText(xPos, yPos + RY(140),
				"Want to learn XNA?", Color.Gray);
			TextureFont.WriteText(xPos, yPos + RY(170),
				"Get my book:", Color.Gray);
			bookTexture.RenderOnScreen(new Rectangle(
				xPos, yPos + RY(200),
				RX((int)(BookGfxRect.Width*0.8f)),
				RY((int)(BookGfxRect.Height*0.8f))),
				BookGfxRect);

			xPos = RX(400);
			yPos += RY(130);
			WriteLink(xPos, yPos,
				"My blog: Many XNA Games and my book",
				"http://abi.exdream.com");
			WriteLink(xPos, yPos + RY(90),
				"XNA Creators Club Website",
				"http://creators.xna.com");
			WriteLink(xPos, yPos + RY(180),
				"XNA Team Blog",
				"http://blogs.msdn.com/xna");
			WriteLink(xPos, yPos + RY(270),
				"Official XNA Website on MSDN",
				"http://msdn.microsoft.com/xna");

			TextureFont.WriteTextCentered(BaseGame.Width/2, RY(600),
				"Dedicated to the great XNA Framework.");

			if (Input.KeyboardEscapeJustPressed ||
				Input.GamePadBackJustPressed ||
				Input.KeyboardSpaceJustPressed ||
				Input.GamePadAJustPressed ||
				Input.MouseLeftButtonJustPressed)
				// Just exit this screen and show end screen!
				quit = true;
		} // Run(game)
		#endregion
	} // class EndScreen
} // namespace DungeonQuest.GameScreens
