// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Helpers
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:41

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework.Storage;
#endregion

namespace DungeonQuest.Helpers
{
	/// <summary>
	/// Helper class which stores all used directories.
	/// </summary>
	class Directories
	{
		#region Game base directory
		/// <summary>
		/// We can use this to relocate the whole game directory to another
		/// location. Used for testing (everything is stored on a network drive).
		/// </summary>
		public static readonly string GameBaseDirectory =
			// Update to support Xbox360:
			StorageContainer.TitleLocation;
			//"";
		#endregion

		#region Directories
		/// <summary>
		/// Content directory for all our textures, models and shaders.
		/// </summary>
		/// <returns>String</returns>
		public static string ContentDirectory
		{
			get
			{
				return Path.Combine(GameBaseDirectory, "Content");
			} // get
		} // ContentDirectory

		/// <summary>
		/// Sounds directory, for some reason XAct projects don't produce
		/// any content files (bug?). We just load them ourself!
		/// </summary>
		/// <returns>String</returns>
		public static string SoundsDirectory
		{
			get
			{
				return Path.Combine(GameBaseDirectory, "Content\\Sounds");
			} // get
		} // SoundsDirectory

		/// <summary>
		/// Default Screenshots directory.
		/// </summary>
		/// <returns>String</returns>
		public static string ScreenshotsDirectory
		{
			get
			{
				return Path.Combine(GameBaseDirectory, "Screenshots");
			} // get
		} // ScreenshotsDirectory
		#endregion
	} // class Directories
} // namespace DungeonQuest.Helpers
