// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Collision
// Creation date: 23.07.2007 05:01
// Last modified: 31.07.2007 04:38

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DungeonQuest.Collision
{
	/// <summary>
	/// Ray helper class, used for collision checks.
	/// </summary>
	public class Ray
	{
		#region Variables
		/// <summary>
		/// Origin
		/// </summary>
		public Vector3 origin;
		/// <summary>
		/// Direction
		/// </summary>
		public Vector3 direction;
		#endregion

		#region Properties
		#region Length
		/// <summary>
		/// Length
		/// </summary>
		/// <returns>Float</returns>
		public float Length
		{
			get
			{
				return direction.Length();
			} // get
		} // Length
		#endregion

		#region EndPosition
		/// <summary>
		/// End position
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 EndPosition
		{
			get
			{
				return origin + direction;
			} // get
		} // EndPosition
		#endregion
		#endregion

		#region Constructor
		/// <summary>
		/// Create ray
		/// </summary>
		/// <param name="setOrigin">Set origin</param>
		/// <param name="setDirection">Set direction</param>
		public Ray(Vector3 setOrigin, Vector3 setDirection)
		{
			origin = setOrigin;
			direction = setDirection;
		} // Ray(setOrigin, setDirection)
		#endregion

		#region FromStartAndEnd
		/// <summary>
		/// From start and end
		/// </summary>
		/// <param name="rayStart">Ray start</param>
		/// <param name="rayEnd">Ray end</param>
		/// <returns>Ray</returns>
		public static Ray FromStartAndEnd(Vector3 rayStart, Vector3 rayEnd)
		{
			return new Ray(rayStart, rayEnd - rayStart);
		} // FromStartAndEnd(rayStart, rayEnd)
		#endregion
	} // class Ray
}
