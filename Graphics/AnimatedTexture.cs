/*unused
// Project: DungeonQuest, File: AnimatedTexture.cs
// Namespace: DungeonQuest.Graphics, Class: AnimatedTexture
// Path: C:\code\DungeonQuest\Graphics, Author: GDC
// Code lines: 194, Size of file: 5.68 KB
// Creation date: 3/5/2007 11:53 AM
// Last modified: 3/5/2007 5:23 PM
// Generated with Commenter by abi.exDream.com

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DungeonQuest.Helpers;
using DungeonQuest.Game;
using Microsoft.Xna.Framework.Graphics;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
using DungeonQuest.Sounds;
using Microsoft.Xna.Framework;
#endregion

namespace DungeonQuest.Graphics
{
	/// <summary>
	/// Animated texture
	/// </summary>
	public class AnimatedTexture : Texture
	{
		#region Variables
		/// <summary>
		/// Textures for the animation.
		/// You can still use d3dTexture, it will just hold current animation
		/// texture! See Select(.).
		/// </summary>
		private XnaTexture[] xnaTextures = null;

		/// <summary>
		/// Get animation length (how many textures are used inside of this
		/// animated texture). Can be used for the Select(.) method.
		/// </summary>
		/// <returns>Int</returns>
		public int AnimationLength
		{
			get
			{
				return xnaTextures != null ?
					xnaTextures.Length : 1;
			} // get
		} // AnimationLength

		/// <summary>
		/// Get animated texture
		/// </summary>
		/// <param name="animationNumber">Animation number</param>
		/// <returns>XnaTexture</returns>
		public XnaTexture GetAnimatedTexture(int animationNumber)
		{
			return xnaTextures[animationNumber % xnaTextures.Length];
		} // GetAnimatedTexture(aniNumber)
		#endregion

		#region Constructor
		/// <summary>
		/// Create animated texture, don't allow empty constructor from outside.
		/// </summary>
		protected AnimatedTexture()
		{
		} // AnimatedTexture()

		/// <summary>
		/// Create animated texture from given filename.
		/// Will go through all filenames with textureFilename + "0001", "0002",
		/// etc. until the filename is not longer found.
		/// </summary>
		/// <param name="setFilename">Set filename, must be relative and
		/// without the 0001.dds, 0002.dds, etc. extension.</param>
		public AnimatedTexture(string setFilename)
		{
			if (String.IsNullOrEmpty(setFilename))
				throw new ArgumentNullException("setFilename",
					"Unable to create texture without valid filename.");

			// Set filename
			texFilename = StringHelper.ExtractFilename(setFilename, true);

			// Try to load texture
			try
			{
				string filenameFirstPart =
					Path.Combine(Directories.ContentDirectory, texFilename);

				// Build full filename
				string fullFilename = filenameFirstPart + "0001";

				// Check if file exists, else we can't continue loading!
				if (File.Exists(fullFilename+".xnb"))
				{
					// Try loading as 2d texture
					internalXnaTexture = BaseGame.Content.Load<Texture2D>(fullFilename);

					// Get info from the texture directly.
					texWidth = internalXnaTexture.Width;
					texHeight = internalXnaTexture.Height;

					// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
					// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
					hasAlpha =
						internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
						internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
						internalXnaTexture.Format.ToString().StartsWith("A");

					loaded = true;

					CalcHalfPixelSize();

					// Ok, now load all other animated textures
					List<XnaTexture> animatedTextures =
						new List<XnaTexture>();
					animatedTextures.Add(internalXnaTexture);
					int texNumber = 2;
					while (File.Exists(filenameFirstPart +
						texNumber.ToString("0000") + ".xnb"))
					{
						animatedTextures.Add(BaseGame.Content.Load<Texture2D>(
							filenameFirstPart + texNumber.ToString("0000")));
						texNumber++;
					} // while (File.Exists)
					xnaTextures = animatedTextures.ToArray();
				} // if (File.Exists)
				else if (File.Exists(fullFilename + ".dds"))
				{
					// Try loading as 2d texture
					internalXnaTexture = Texture2D.FromFile(
						BaseGame.Device,
						fullFilename + ".dds");

					// Get info from the texture directly.
					texWidth = internalXnaTexture.Width;
					texHeight = internalXnaTexture.Height;

					// We will use alpha for Dxt3, Dxt5 or any format starting with "A",
					// there are a lot of those (A8R8G8B8, A4R4G4B4, A8B8G8R8, etc.)
					hasAlpha =
						internalXnaTexture.Format == SurfaceFormat.Dxt5 ||
						internalXnaTexture.Format == SurfaceFormat.Dxt3 ||
						internalXnaTexture.Format.ToString().StartsWith("A");

					loaded = true;

					CalcHalfPixelSize();

					// Ok, now load all other animated textures
					List<XnaTexture> animatedTextures =
						new List<XnaTexture>();
					animatedTextures.Add(internalXnaTexture);
					int texNumber = 2;
					while (File.Exists(filenameFirstPart +
						texNumber.ToString("0000") + ".dds"))
					{
						animatedTextures.Add(Texture2D.FromFile(
							BaseGame.Device,
							filenameFirstPart + texNumber.ToString("0000") + ".dds"));
						texNumber++;
					} // while (File.Exists)
					xnaTextures = animatedTextures.ToArray();
				} // else if
				else
					throw new FileNotFoundException(fullFilename + ".xnb or .dds");
			} // try
			catch (Exception ex)
			{
				// Failed to load
				loaded = false;
				Log.Write("Failed to load animated texture " + texFilename +
					", will use empty texture! Error: " + ex.ToString());
			} // catch (ex)
		} // AnimatedTexture(name)
		#endregion

		#region Select
		/// <summary>
		/// Select this texture as texture stage 0
		/// </summary>
		/// <param name="animationNumber">Number</param>
		public void Select(int animationNumber)
		{
			if (xnaTextures != null &&
				xnaTextures.Length > 0)
			{
				// Select new animation number
				internalXnaTexture = xnaTextures[animationNumber % xnaTextures.Length];
			} // if
		} // Select(num)
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test textures
		/// </summary>
		public static void TestAnimatedTexture()
		{
			AnimatedTexture explosionTexture = null;
			TestGame.Start("TestAnimatedTexture",
				delegate
				{
					explosionTexture = new AnimatedTexture("destroy");
				},
				delegate
				{
					explosionTexture.Select((int)BaseGame.TotalTimeMs / 33);
					int screenSize =
						Math.Max(BaseGame.Width * 2 / 19, BaseGame.Height * 2 / 19);
					explosionTexture.RenderOnScreen(new Rectangle(
						BaseGame.Width / 2 - screenSize / 2,
						BaseGame.Height / 2 - screenSize / 2,
						screenSize, screenSize));

					if (Input.MouseLeftButtonJustPressed)
						Sound.Play(Sound.Sounds.Click);
				});
		} // TestAnimatedTexture()
#endif
		#endregion
	} // class AnimatedTexture
} // namespace DungeonQuest.Graphics
*/