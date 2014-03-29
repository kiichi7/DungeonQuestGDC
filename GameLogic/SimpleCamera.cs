// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using DungeonQuest.Helpers;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Simple camera class just to move around a little.
	/// Always focuses on the center and uses the same height.
	/// </summary>
	public class SimpleCamera : BaseCamera
	{
		#region Variables
		Vector3 camPos = new Vector3(0, 0, 50);
		#endregion

		#region Constructor
		public SimpleCamera(BaseGame game)
			: base(game)
		{
		} // SimpleCamera(game)
		#endregion

		#region Initialize
		public override void Initialize()
		{
			base.Initialize();
		} // Initialize
		#endregion

		#region Update
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// Update camera position (allow mouse and gamepad)
			camPos.X += Input.MouseXMovement / 10;
			camPos.Y += Input.MouseYMovement / 10;
			camPos.X += Input.GamePad.ThumbSticks.Right.X;
			camPos.Y += Input.GamePad.ThumbSticks.Right.Y;
			camPos.Z += Input.GamePad.ThumbSticks.Left.Y;
			camPos.Z += Input.MouseWheelDelta / 20;
			BaseGame.ViewMatrix = Matrix.CreateLookAt(
				camPos, Vector3.Zero, new Vector3(0, 0, 1));
		} // Update(gameTime)
		#endregion

		#region Set position
		public void SetPosition(Vector3 newPos)
		{
			camPos = newPos;
		} // SetPosition(newPos)
		#endregion
	} // class SimpleCamera
} // namespace DungeonQuest.Game
