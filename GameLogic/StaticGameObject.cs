// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using DungeonQuest.GameLogic;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Static game model
	/// </summary>
	public class StaticGameObject
	{
		#region Variables
		/// <summary>
		/// Type for this model.
		/// </summary>
		public GameManager.StaticTypes type;

		/// <summary>
		/// Is collectable? The player can collect the key and the 3 weapons:
		/// Club, Sword and BigSword.
		/// </summary>
		/// <returns>Bool</returns>
		public bool IsCollectable
		{
			get
			{
				return type == GameManager.StaticTypes.Key ||
					type == GameManager.StaticTypes.Club ||
					type == GameManager.StaticTypes.Sword ||
					type == GameManager.StaticTypes.BigSword;
			} // get
		} // IsCollectable

		/// <summary>
		/// Position matrix for positioning this entry, scale is unused
		/// and rotation is kinda important too. Can be updated in derived classes.
		/// Will always stay the same for static meshes.
		/// </summary>
		public Matrix positionMatrix;
		#endregion

		#region Constructor
		/// <summary>
		/// Create static game model
		/// </summary>
		/// <param name="setType">Set type</param>
		/// <param name="setPositionMatrix">Set position matrix</param>
		public StaticGameObject(GameManager.StaticTypes setType,
			Matrix setPositionMatrix)
		{
			type = setType;
			positionMatrix = setPositionMatrix;
		} // StaticGameModel(setType, setPositionMatrix)
		#endregion
	} // class StaticGameModel
} // namespace DungeonQuest.Game
