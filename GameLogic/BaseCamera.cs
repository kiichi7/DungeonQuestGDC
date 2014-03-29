// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 31.07.2007 05:45
// Last modified: 31.07.2007 05:47

#region Using directives
using DungeonQuest.Game;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using DungeonQuest.GameLogic;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Base camera for all camera classes here.
	/// See BaseGame.camera for more details!
	/// </summary>
	public class BaseCamera : GameComponent
	{
		#region Constructor
		/// <summary>
		/// Create base camera
		/// </summary>
		/// <param name="game">Game</param>
		public BaseCamera(BaseGame game)
			: base(game)
		{
		} // BaseCamera(game)
    #endregion

		#region Player dummies
		/// <summary>
		/// Player position dummy just to support switching cameras.
		/// </summary>
		/// <returns>Vector 3</returns>
		public virtual Vector3 PlayerPos
		{
			get
			{
				return Vector3.Zero;
			} // get
		} // PlayerPos

		/// <summary>
		/// Player rotation
		/// </summary>
		/// <returns>Float</returns>
		public virtual float PlayerRotation
		{
			get
			{
				return 0;
			} // get
		} // PlayerRotation

		/// <summary>
		/// Player blend states
		/// </summary>
		public virtual float[] PlayerBlendStates
		{
			get
			{
				return new float[] { 1, 0, 0, 0, 0 };
			} // get
		} // PlayerBlendStates

		/// <summary>
		/// Dummy
		/// </summary>
		public AnimatedGameObject playerObject = new AnimatedGameObject(
			GameManager.AnimatedTypes.Hero, Matrix.Identity);

		/// <summary>
		/// Reset
		/// </summary>
		public virtual void Reset()
		{
			//Dummy
		} // Reset()
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		/// <param name="gameTime">Game time</param>
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
		} // Update(gameTime)
		#endregion
	} // class BaseCamera
}
