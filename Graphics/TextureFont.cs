// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Graphics
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:41

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using DungeonQuest.Game;
using DungeonQuest.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
#endregion

namespace DungeonQuest.Graphics
{
	/// <summary>
	/// Texture font for our game, uses GameFont.png.
	/// If you want to know more details about creating bitmap fonts in XNA,
	/// how to generate the bitmaps and more details about using it, please
	/// check out the following links:
	/// http://blogs.msdn.com/garykac/archive/2006/08/30/728521.aspx
	/// http://blogs.msdn.com/garykac/articles/732007.aspx
	/// http://www.angelcode.com/products/bmfont/
	/// </summary>
	public class TextureFont
	{
		#region Constants
		#region Font xml info
		const string FontXmlInfo =
@"<Font Height=""51"">
    <Char c=""a"" u=""2"" v=""10"" width=""22""/>
    <Char c=""b"" u=""2"" v=""41"" width=""21""/>
    <Char c=""c"" u=""2"" v=""71"" width=""21""/>
    <Char c=""d"" u=""2"" v=""101"" width=""21""/>
    <Char c=""e"" u=""2"" v=""131"" width=""21""/>
    <Char c=""f"" u=""2"" v=""161"" width=""16""/>
    <Char c=""g"" u=""2"" v=""186"" width=""22""/>
    <Char c=""h"" u=""2"" v=""217"" width=""22""/>
    <Char c=""i"" u=""2"" v=""248"" width=""10""/>
    <Char c=""j"" u=""2"" v=""267"" width=""10""/>
    <Char c=""k"" u=""2"" v=""286"" width=""20""/>
    <Char c=""l"" u=""2"" v=""315"" width=""10""/>
    <Char c=""m"" u=""2"" v=""334"" width=""34""/>
    <Char c=""n"" u=""2"" v=""377"" width=""22""/>
    <Char c=""o"" u=""2"" v=""408"" width=""20""/>
    <Char c=""p"" u=""2"" v=""437"" width=""21""/>
    <Char c=""q"" u=""2"" v=""467"" width=""21""/>
    <Char c=""r"" u=""54"" v=""10"" width=""21""/>
    <Char c=""s"" u=""54"" v=""40"" width=""22""/>
    <Char c=""t"" u=""54"" v=""71"" width=""18""/>
    <Char c=""u"" u=""54"" v=""98"" width=""22""/>
    <Char c=""v"" u=""54"" v=""129"" width=""17""/>
    <Char c=""w"" u=""54"" v=""155"" width=""29""/>
    <Char c=""x"" u=""54"" v=""193"" width=""20""/>
    <Char c=""y"" u=""54"" v=""222"" width=""22""/>
    <Char c=""z"" u=""54"" v=""253"" width=""22""/>
    <Char c=""A"" u=""54"" v=""284"" width=""17""/>
    <Char c=""B"" u=""54"" v=""310"" width=""21""/>
    <Char c=""C"" u=""54"" v=""340"" width=""21""/>
    <Char c=""D"" u=""54"" v=""370"" width=""21""/>
    <Char c=""E"" u=""54"" v=""400"" width=""21""/>
    <Char c=""F"" u=""54"" v=""430"" width=""19""/>
    <Char c=""G"" u=""54"" v=""458"" width=""20""/>
    <Char c=""H"" u=""54"" v=""487"" width=""22""/>
    <Char c=""I"" u=""106"" v=""10"" width=""10""/>
    <Char c=""J"" u=""106"" v=""29"" width=""22""/>
    <Char c=""K"" u=""106"" v=""60"" width=""20""/>
    <Char c=""L"" u=""106"" v=""89"" width=""18""/>
    <Char c=""M"" u=""106"" v=""116"" width=""29""/>
    <Char c=""N"" u=""106"" v=""154"" width=""22""/>
    <Char c=""O"" u=""106"" v=""185"" width=""20""/>
    <Char c=""P"" u=""106"" v=""214"" width=""18""/>
    <Char c=""Q"" u=""106"" v=""241"" width=""20""/>
    <Char c=""R"" u=""106"" v=""270"" width=""22""/>
    <Char c=""S"" u=""106"" v=""301"" width=""22""/>
    <Char c=""T"" u=""106"" v=""332"" width=""18""/>
    <Char c=""U"" u=""106"" v=""359"" width=""22""/>
    <Char c=""V"" u=""106"" v=""390"" width=""17""/>
    <Char c=""W"" u=""106"" v=""416"" width=""29""/>
    <Char c=""X"" u=""106"" v=""454"" width=""20""/>
    <Char c=""Y"" u=""106"" v=""483"" width=""17""/>
    <Char c=""Z"" u=""158"" v=""10"" width=""22""/>
    <Char c=""1"" u=""158"" v=""41"" width=""10""/>
    <Char c=""2"" u=""158"" v=""60"" width=""21""/>
    <Char c=""3"" u=""158"" v=""90"" width=""21""/>
    <Char c=""4"" u=""158"" v=""120"" width=""22""/>
    <Char c=""5"" u=""158"" v=""151"" width=""21""/>
    <Char c=""6"" u=""158"" v=""181"" width=""21""/>
    <Char c=""7"" u=""158"" v=""211"" width=""22""/>
    <Char c=""8"" u=""158"" v=""242"" width=""20""/>
    <Char c=""9"" u=""158"" v=""271"" width=""21""/>
    <Char c=""0"" u=""158"" v=""301"" width=""20""/>
    <Char c=""-"" u=""158"" v=""330"" width=""19""/>
    <Char c=""="" u=""158"" v=""358"" width=""32""/>
    <Char c=""!"" u=""158"" v=""399"" width=""10""/>
    <Char c=""@"" u=""158"" v=""418"" width=""54""/>
    <Char c=""#"" u=""158"" v=""481"" width=""30""/>
    <Char c=""$"" u=""210"" v=""10"" width=""22""/>
    <Char c=""%"" u=""210"" v=""41"" width=""35""/>
    <Char c=""^"" u=""210"" v=""85"" width=""26""/>
    <Char c=""&amp;"" u=""210"" v=""120"" width=""21""/>
    <Char c=""*"" u=""210"" v=""150"" width=""15""/>
    <Char c=""("" u=""210"" v=""174"" width=""10""/>
    <Char c="")"" u=""210"" v=""193"" width=""10""/>
    <Char c=""_"" u=""210"" v=""212"" width=""30""/>
    <Char c=""+"" u=""210"" v=""251"" width=""32""/>
    <Char c=""["" u=""210"" v=""292"" width=""9""/>
    <Char c=""]"" u=""210"" v=""310"" width=""9""/>
    <Char c=""{"" u=""210"" v=""328"" width=""9""/>
    <Char c=""}"" u=""210"" v=""346"" width=""9""/>
    <Char c="";"" u=""210"" v=""364"" width=""10""/>
    <Char c=""'"" u=""210"" v=""383"" width=""10""/>
    <Char c="":"" u=""210"" v=""402"" width=""10""/>
    <Char c=""&amp;qwo;"" u=""210"" v=""421"" width=""18""/>
    <Char c="","" u=""210"" v=""448"" width=""10""/>
    <Char c=""."" u=""210"" v=""467"" width=""10""/>
    <Char c=""&lt;"" u=""262"" v=""10"" width=""32""/>
    <Char c=""&gt;"" u=""262"" v=""51"" width=""32""/>
    <Char c=""/"" u=""262"" v=""92"" width=""16""/>
    <Char c=""?"" u=""262"" v=""117"" width=""21""/>
    <Char c=""\"" u=""262"" v=""147"" width=""16""/>
    <Char c=""|"" u=""262"" v=""172"" width=""10""/>
    <Char c="" "" u=""262"" v=""191"" width=""15""/>
</Font>";
		#endregion

		/// <summary>
		/// Game font filename for our bitmap.
		/// </summary>
		const string GameFontFilename = "Gothican";

		/// <summary>
		/// TextureFont height
		/// </summary>
		const int FontHeight = 51;//first try texture: 36;
		
		/// <summary>
		/// Substract this value from the y postion when rendering.
		/// Most letters start below the CharRects, this fixes that issue.
		/// </summary>
		const int SubRenderHeight = 0;//7;//5;//7;//6;'
		/// <summary>
		/// Substract this value from the pixel location we assign for each
		/// letter for rendering.
		/// </summary>
		const int SubRenderWidth = -3;//-6;//5;

		/// <summary>
		/// Char rectangles, goes from space (32) to ~ (126).
		/// Height is not used (always the same), instead we save the actual
		/// used width for rendering in the height value!
		/// This are the characters:
		///  !"#$%&'()*+,-./0123456789:;<=>?@
		/// ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`
		/// abcdefghijklmnopqrstuvwxyz{|}~
		/// 
		/// Note: Was done before in RocketCommanderXna and this way it
		/// is easier to reuse the code below!
		/// </summary>
		static Rectangle[] CharRects = new Rectangle[126 - 32 + 1];

		#region obs
		/*obs
		/// <summary>
		/// Char rectangles, goes from space (32) to ~ (126).
		/// Height is not used (always the same), instead we save the actual
		/// used width for rendering in the height value!
		/// This are the characters:
		///  !"#$%&'()*+,-./0123456789:;<=>?@
		/// ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`
		/// abcdefghijklmnopqrstuvwxyz{|}~
		/// Then we also got 4 extra rects for the XBox Buttons: A, B, X, Y
		/// </summary>
		static Rectangle[] CharRects = new Rectangle[126 - 32 + 1]
		{
			new Rectangle(0, 0, 1, 8), // space
			new Rectangle(1, 0, 11, 10),
			new Rectangle(12, 0, 14, 13),
			new Rectangle(26, 0, 20, 18),
			new Rectangle(46, 0, 20, 18),
			new Rectangle(66, 0, 24, 22),
			new Rectangle(90, 0, 25, 23),
			new Rectangle(115, 0, 8, 7),
			new Rectangle(124, 0, 10, 9),
			new Rectangle(136, 0, 10, 9),
			new Rectangle(146, 0, 20, 18),
			new Rectangle(166, 0, 20, 18),
			new Rectangle(186, 0, 10, 8),
			new Rectangle(196, 0, 10, 9),
			new Rectangle(207, 0, 10, 8),
			new Rectangle(217, 0, 18, 16),
			new Rectangle(235, 0, 20, 19),

			new Rectangle(0, 36, 20, 18), // 1
			new Rectangle(20, 36, 20, 18),
			new Rectangle(40, 36, 20, 18),
			new Rectangle(60, 36, 21, 19),
			new Rectangle(81, 36, 20, 18),
			new Rectangle(101, 36, 20, 18),
			new Rectangle(121, 36, 20, 18),
			new Rectangle(141, 36, 20, 18),
			new Rectangle(161, 36, 20, 18), // 9
			new Rectangle(181, 36, 10, 8),
			new Rectangle(191, 36, 10, 8),
			new Rectangle(201, 36, 20, 18),
			new Rectangle(221, 36, 20, 18),

			new Rectangle(0, 72, 20, 18), // >
			new Rectangle(20, 72, 19, 17),
			new Rectangle(39, 72, 26, 24),
			new Rectangle(65, 72, 22, 20),
			new Rectangle(87, 72, 22, 20),
			new Rectangle(109, 72, 22, 20),
			new Rectangle(131, 72, 23, 21),
			new Rectangle(154, 72, 20, 18),
			new Rectangle(174, 72, 19, 17),
			new Rectangle(193, 72, 23, 21),
			new Rectangle(216, 72, 23, 21),
			new Rectangle(239, 72, 11, 10),

			new Rectangle(0, 108, 15, 13), // J
			new Rectangle(15, 108, 22, 20),
			new Rectangle(37, 108, 19, 17),
			new Rectangle(56, 108, 29, 26),
			new Rectangle(85, 108, 23, 21),
			new Rectangle(108, 108, 24, 22), // O
			new Rectangle(132, 108, 22, 20),
			new Rectangle(154, 108, 24, 22),
			new Rectangle(178, 108, 24, 22),
			new Rectangle(202, 108, 21, 19),
			new Rectangle(223, 108, 17, 15), // T

			new Rectangle(0, 144, 22, 20), // U
			new Rectangle(22, 144, 22, 20),
			new Rectangle(44, 144, 30, 28),
			new Rectangle(74, 144, 22, 20),
			new Rectangle(96, 144, 20, 18),
			new Rectangle(116, 144, 20, 18),
			new Rectangle(136, 144, 10, 9),
			new Rectangle(146, 144, 18, 16),
			new Rectangle(167, 144, 10, 9),
			new Rectangle(177, 144, 17, 16),
			new Rectangle(194, 144, 17, 16),
			new Rectangle(211, 144, 17, 16),
			new Rectangle(228, 144, 20, 18),

			new Rectangle(0, 180, 20, 18), // b
			new Rectangle(20, 180, 18, 16),
			new Rectangle(38, 180, 20, 18),
			new Rectangle(58, 180, 20, 18), // e
			new Rectangle(79, 180, 14, 12), // f
			new Rectangle(93, 180, 20, 18), // g
			new Rectangle(114, 180, 19, 18), // h
			new Rectangle(133, 180, 11, 10),
			new Rectangle(145, 180, 11, 10), // j
			new Rectangle(156, 180, 20, 18),
			new Rectangle(176, 180, 11, 9),
			new Rectangle(187, 180, 29, 27),
			new Rectangle(216, 180, 20, 18),
			new Rectangle(236, 180, 20, 19),

			new Rectangle(0, 216, 20, 18), // p
			new Rectangle(20, 216, 20, 18),
			new Rectangle(40, 216, 13, 12), // r
			new Rectangle(53, 216, 17, 16),
			new Rectangle(70, 216, 14, 11), // t
			new Rectangle(84, 216, 19, 18),
			new Rectangle(104, 216, 17, 16),
			new Rectangle(122, 216, 25, 23),
			new Rectangle(148, 216, 19, 17),
			new Rectangle(168, 216, 18, 16),
			new Rectangle(186, 216, 16, 15),
			new Rectangle(203, 216, 10, 9),
			new Rectangle(214, 216, 12, 11), // |
			new Rectangle(227, 216, 10, 9),
			new Rectangle(237, 216, 18, 17),
		};
		/*obs, first try
		/// <summary>
		/// XBox 360 Button rects, can just be added to your text :)
		/// Check out the unit test below.
		/// </summary>
		public const char XBoxAButton = (char)(126 + 1),
			XBoxBButton = (char)(126 + 2),
			XBoxXButton = (char)(126 + 3),
			XBoxYButton = (char)(126 + 4);

		/// <summary>
		/// Char rectangles, goes from space (32) to ~ (126).
		/// Height is not used (always the same), instead we save the actual
		/// used width for rendering in the height value!
		/// This are the characters:
		///  !"#$%&'()*+,-./0123456789:;<=>?@
		/// ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`
		/// abcdefghijklmnopqrstuvwxyz{|}~
		/// Then we also got 4 extra rects for the XBox Buttons: A, B, X, Y
		/// </summary>
		static Rectangle[] CharRects = new Rectangle[126 - 32 + 4 + 1]
		{
			new Rectangle(0, 0, 1, 6), // space
			new Rectangle(1, 0, 6, 5),
			new Rectangle(7, 0, 8, 6),
			new Rectangle(15, 0, 10, 9),
			new Rectangle(25, 0, 11, 10),
			new Rectangle(36, 0, 16, 14),
			new Rectangle(52, 0, 14, 12),
			new Rectangle(66, 0, 5, 3),
			new Rectangle(71, 0, 9, 7),
			new Rectangle(80, 0, 9, 7),
			new Rectangle(89, 0, 11, 9),
			new Rectangle(100, 0, 10, 9),
			new Rectangle(110, 0, 6, 5),
			new Rectangle(116, 0, 7, 5),
			new Rectangle(123, 0, 6, 5),
			new Rectangle(129, 0, 5, 3),
			new Rectangle(134, 0, 13, 11),
			new Rectangle(147, 0, 13, 11),
			new Rectangle(160, 0, 13, 11),
			new Rectangle(173, 0, 13, 11),
			new Rectangle(186, 0, 13, 11),
			new Rectangle(199, 0, 13, 11),
			new Rectangle(212, 0, 13, 11),
			new Rectangle(225, 0, 13, 11),
			new Rectangle(238, 0, 13, 11),
			new Rectangle(0, 25, 13, 11), // 9
			new Rectangle(13, 25, 6, 5),
			new Rectangle(19, 25, 6, 5),
			new Rectangle(25, 25, 10, 9),
			new Rectangle(35, 25, 10, 9),
			new Rectangle(45, 25, 10, 9),
			new Rectangle(55, 25, 9, 7),
			new Rectangle(64, 25, 16, 14),
			new Rectangle(80, 25, 15, 13),
			new Rectangle(95, 25, 13, 11),
			new Rectangle(108, 25, 12, 10),
			new Rectangle(120, 25, 13, 12),
			new Rectangle(133, 25, 11, 9),
			new Rectangle(144, 25, 11, 9),
			new Rectangle(155, 25, 13, 11),
			new Rectangle(168, 25, 14, 13),
			new Rectangle(182, 25, 8, 6),
			new Rectangle(190, 25, 9, 7),
			new Rectangle(199, 25, 14, 12),
			new Rectangle(213, 25, 11, 9),
			new Rectangle(224, 25, 16, 14),
			new Rectangle(240, 25, 13, 12),
			new Rectangle(0, 50, 13, 11), // O
			new Rectangle(13, 50, 12, 11),
			new Rectangle(25, 50, 13, 11),
			new Rectangle(38, 50, 13, 12),
			new Rectangle(51, 50, 11, 9),
			new Rectangle(62, 50, 11, 9),
			new Rectangle(73, 50, 13, 11),
			new Rectangle(86, 50, 13, 11),
			new Rectangle(99, 50, 18, 16),
			new Rectangle(117, 50, 14, 13),
			new Rectangle(131, 50, 14, 12),
			new Rectangle(145, 50, 12, 11),
			new Rectangle(157, 50, 8, 7),
			new Rectangle(165, 50, 5, 3),
			new Rectangle(170, 50, 8, 7),
			new Rectangle(178, 50, 10, 8),
			new Rectangle(188, 50, 8, 6),
			new Rectangle(196, 50, 5, 4),
			new Rectangle(201, 50, 11, 10),
			new Rectangle(212, 50, 12, 10),
			new Rectangle(224, 50, 10, 9),
			new Rectangle(234, 50, 12, 10),
			new Rectangle(0, 75, 11, 9), // e
			new Rectangle(11, 75, 9, 7),
			new Rectangle(20, 75, 11, 10),
			new Rectangle(31, 75, 12, 10),
			new Rectangle(43, 75, 7, 5),
			new Rectangle(50, 75, 8, 6),
			new Rectangle(58, 75, 12, 10),
			new Rectangle(70, 75, 7, 5),
			new Rectangle(77, 75, 18, 16),
			new Rectangle(95, 75, 12, 10),
			new Rectangle(107, 75, 11, 9),
			new Rectangle(118, 75, 12, 10),
			new Rectangle(130, 75, 12, 10),
			new Rectangle(142, 75, 10, 9),
			new Rectangle(152, 75, 9, 8),
			new Rectangle(161, 75, 9, 7),
			new Rectangle(170, 75, 12, 10),
			new Rectangle(182, 75, 11, 9),
			new Rectangle(193, 75, 15, 13),
			new Rectangle(208, 75, 13, 11),
			new Rectangle(221, 75, 11, 10),
			new Rectangle(232, 75, 10, 8),
			new Rectangle(242, 75, 6, 5),
			new Rectangle(0, 100, 10, 9), // |
			new Rectangle(10, 100, 6, 5),
			new Rectangle(16, 100, 10, 8),
			// 4 extra chars for the xbox controller buttons
			new Rectangle(29, 100, 20, 21),
			new Rectangle(49, 100, 20, 21),
			new Rectangle(69, 100, 20, 21),
			new Rectangle(89, 100, 20, 21),
		};
		 */
		#endregion
		#endregion

		#region Variables
		/// <summary>
		/// TextureFont texture
		/// </summary>
		Texture fontTexture;
		/// <summary>
		/// TextureFont sprite
		/// </summary>
		SpriteBatch fontSprite; 
		#endregion

		#region Properties
		/// <summary>
		/// Height
		/// </summary>
		/// <returns>Int</returns>
		public static int Height
		{
			get
			{
				return (FontHeight - SubRenderHeight) * BaseGame.Height / 1200;
			} // get
		} // Height
		#endregion

		#region Constructor
		/// <summary>
		/// Create texture font
		/// </summary>
		public TextureFont()
		{
			fontTexture = new Texture(GameFontFilename);
			fontSprite = new SpriteBatch(BaseGame.Device);

			// Load all char rects from the xml data above
			XmlNode xmlFont = XmlHelper.LoadXmlFromText(FontXmlInfo);
			for (int i = 0; i < CharRects.Length; i++)
			{
				// Use default values for letters that are not supported
				// They will not be displayed at all
				Rectangle rect = new Rectangle(0, 0, 0, 0);

				// Convert to char, start with '!'
				char ch = (char)((int)' ' + i);
				// Find out if it is available in the list above
				foreach (XmlNode node in xmlFont)
					if (XmlHelper.GetXmlAttribute(node, "c").Length > 0 &&
						XmlHelper.GetXmlAttribute(node, "c")[0] == ch)
					{
						// Found it! Assign values
						rect.X = Convert.ToInt32(XmlHelper.GetXmlAttribute(node, "v"))-4;
						rect.Y = Convert.ToInt32(XmlHelper.GetXmlAttribute(node, "u"))-2;
						rect.Width =
							Convert.ToInt32(XmlHelper.GetXmlAttribute(node, "width"))+6;
						// Use the default sub value for the actual render width
						rect.Height = rect.Width - SubRenderWidth;
						//tst:
						//Log.Write("Found ch=" + ch + " at " + i + " with rect: " + rect);
						break;
					} // foreach if (XmlHelper.GetXmlAttribute)

				// Assign the result
				CharRects[i] = rect;
			} // for (int)
		} // TextureFont()
    #endregion

		#region Dispose
		/*obs, XNA handles this fine
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			DisposeHelper.Dispose(ref fontTexture);
			DisposeHelper.Dispose(ref fontSprite);
		} // Dispose()
		 */
		#endregion

		#region Get text width
		/// <summary>
		/// Get the text width of a given text.
		/// </summary>
		/// <param name="text">Text</param>
		/// <returns>Width (in pixels) of the text</returns>
		public int GetTextWidth(string text)
		{
			int width = 0;
			//foreach (char c in text)
			char[] chars = text.ToCharArray();
			for (int num = 0; num < chars.Length; num++)
			{
				int charNum = (int)chars[num];
				if (charNum >= 32 &&
					charNum - 32 < CharRects.Length)
					width += CharRects[charNum - 32].Height + SubRenderWidth;
			} // foreach
			return width * //BaseGame.Height / 1050;
				BaseGame.Height / (1700 * 3 / 4);
		} // GetTextWidth(text)
		#endregion

		#region Write all
		/// <summary>
		/// TextureFont to render
		/// </summary>
		internal class FontToRender
		{
			#region Variables
			/// <summary>
			/// X and y position
			/// </summary>
			public int x, y;
			/// <summary>
			/// Text
			/// </summary>
			public string text;
			/// <summary>
			/// Color
			/// </summary>
			public Color color;
			#endregion

			#region Constructor
			/// <summary>
			/// Create font to render
			/// </summary>
			/// <param name="setX">Set x</param>
			/// <param name="setY">Set y</param>
			/// <param name="setText">Set text</param>
			/// <param name="setColor">Set color</param>
			public FontToRender(int setX, int setY, string setText, Color setColor)
			{
				x = setX;
				y = setY;
				text = setText;
				color = setColor;
			} // FontToRender(setX, setY, setText)
			#endregion
		} // class FontToRender

		/// <summary>
		/// Remember font texts to render to render them all at once
		/// in our Render method (beween rest of the ui and the mouse cursor).
		/// </summary>
		static List<FontToRender> remTexts = new List<FontToRender>();

		/// <summary>
		/// Write the given text at the specified position.
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="color">Color</param>
		public static void WriteText(int x, int y, string text, Color color)
		{
			remTexts.Add(new FontToRender(x, y, text, color));
		} // WriteText(x, y, text)

		/// <summary>
		/// Write
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		public static void WriteText(int x, int y, string text)
		{
			remTexts.Add(new FontToRender(x, y, text, Color.White));
		} // WriteText(x, y, text)

		/// <summary>
		/// Write
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		public static void WriteText(Point pos, string text)
		{
			remTexts.Add(new FontToRender(pos.X, pos.Y, text, Color.White));
		} // WriteText(pos, text)

		/// <summary>
		/// Write small text centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="col">Color</param>
		/// <param name="alpha">Alpha</param>
		public static void WriteSmallTextCentered(int x, int y, string text,
			Color col, float alpha)
		{
			WriteText(x - BaseGame.GameFont.GetTextWidth(text) / 2, y, text,
				ColorHelper.ApplyAlphaToColor(col, alpha));
		} // WriteSmallTextCentered(x, y, text)

		/// <summary>
		/// Write small text centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="col">Color</param>
		/// <param name="alpha">Alpha</param>
		public static void WriteSmallTextCentered(int x, int y, string text,
			float alpha)
		{
			WriteText(x - BaseGame.GameFont.GetTextWidth(text) / 2, y, text,
				ColorHelper.ApplyAlphaToColor(Color.White, alpha));
		} // WriteSmallTextCentered(x, y, text)

		/// <summary>
		/// Write text centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		public static void WriteTextCentered(int x, int y, string text)
		{
			WriteText(x - BaseGame.GameFont.GetTextWidth(text) / 2, y, text);
		} // WriteTextCentered(x, y, text)

		/// <summary>
		/// Write text centered
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <param name="text">Text</param>
		/// <param name="col">Color</param>
		/// <param name="alpha">Alpha</param>
		public static void WriteTextCentered(int x, int y, string text,
			Color col, float alpha)
		{
			WriteText(x - BaseGame.GameFont.GetTextWidth(text) / 2, y, text,
				ColorHelper.ApplyAlphaToColor(col, alpha));
		} // WriteTextCentered(x, y, text)

		/// <summary>
		/// Write all
		/// </summary>
		/// <param name="texts">Texts</param>
		public void WriteAll()
		{
			if (remTexts.Count == 0)
				return;

			BaseGame.AlphaBlending = true;

			// Start rendering
			fontSprite.Begin(SpriteBlendMode.AlphaBlend);

			// Draw each character in the text
			//foreach (UIRenderer.FontToRender fontText in texts)
			for (int textNum = 0; textNum < remTexts.Count; textNum++)
			{
				FontToRender fontText = remTexts[textNum];

				int x = fontText.x;
				int y = fontText.y;
				Color color = fontText.color;
				//foreach (char c in fontText.text)
				char[] chars = fontText.text.ToCharArray();
				for (int num = 0; num < chars.Length; num++)
				{
					int charNum = (int)chars[num];
					if (charNum >= 32 &&
						charNum - 32 < CharRects.Length)
					{
						// Draw this char
						Rectangle rect = CharRects[charNum - 32];
						// Reduce height to prevent overlapping pixels
						rect.Y += 1;
						rect.Height = FontHeight;// Height;// - 1;
						// Add 2 pixel for g, j, y
						//if (c == 'g' || c == 'j' || c == 'y')
						//	rect.Height+=2;
						Rectangle destRect = new Rectangle(x,
							y - SubRenderHeight,
							rect.Width, rect.Height);

						// Scale destRect (1600x1200 is the base size)
						//destRect.Width = destRect.Width;
						//destRect.Height = destRect.Height;

						// If we want upscaling, just use destRect
						fontSprite.Draw(fontTexture.XnaTexture, //obs:destRect,
							// Rescale to fit resolution, but only size
							//destRect,
							new Rectangle(
							destRect.X,// * BaseGame.Width / 1024,
							destRect.Y,// * BaseGame.Height / 768,
							//first try:
							//destRect.Width * BaseGame.Width / 1400,
							//destRect.Height * BaseGame.Height / 1050),
							//better for Dungeon Quest:
							destRect.Width * BaseGame.Width / 1700,//1920,
							destRect.Height * BaseGame.Height / (1700 * 3 / 4)),//(1920 * 3 / 4)),
							rect, color);
						//,
						// Make sure fonts are always displayed at the front of everything
						//0, Vector2.Zero, SpriteEffects.None, 1.1f);

						// Increase x pos by width we use for this character
						int charWidth = CharRects[charNum - 32].Height * BaseGame.Height /
							//old:1050;
							//(1920 * 3 / 4);
							(1700 * 3 / 4);
						x += charWidth;
					} // if (charNum)
				} // foreach
			} // foreach (fontText)

			// End rendering
			fontSprite.End();

			remTexts.Clear();
		} // WriteAll(texts)    
    #endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test render font
		/// </summary>
		public static void TestRenderFont()
		{
			TestGame.Start("TestRenderFont",
				delegate
				{
					TestGame.BackgroundColor = Color.Black;
					TestGame.GameFont.fontTexture.RenderOnScreen(
						new Rectangle(200, 200, 512, 512));
					TextureFont.WriteText(30, 30,
						"Evil Monster in da house");

					TextureFont.WriteText(30, 90,
						"aaaaaaaaaaaaaaa");
					TextureFont.WriteText(30, 120,
						"ttttttttttttttttt");
				});
		} // TestRenderFont()
#endif
		#endregion
	} // class TextureFont
} // namespace DungeonQuest.Graphics
