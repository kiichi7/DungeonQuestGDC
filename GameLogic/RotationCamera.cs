// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using DungeonQuest.GameLogic;
using DungeonQuest.Helpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Simple rotation camera class just to rotate around a little.
	/// Always focuses on the center and uses the same direction vector,
	/// which is only rotate around the z axis.
	/// </summary>
	public class RotationCamera : BaseCamera
	{
		#region Variables
		Vector3 initialPos = new Vector3(0, -18, 14)*2,
			cameraPos;
		float rotation = 0;
		float distance = 0.1f;//1.0f;
		#endregion

		#region Constructor
		public RotationCamera(BaseGame game)
			: base(game)
		{
		} // RotationCamera(game)
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
			rotation += BaseGame.MoveFactorPerSecond / 1.75f;
			cameraPos = Vector3.Transform(initialPos*distance,
				Matrix.CreateRotationZ(rotation));

			// Allow zooming in/out with gamepad, mouse wheel and cursor keys.
			distance += Input.GamePad.ThumbSticks.Left.Y;
			distance += Input.MouseWheelDelta / 5000.0f;
			if (Input.KeyboardDownPressed)
				distance += BaseGame.MoveFactorPerSecond / 2.5f;
			if (Input.KeyboardUpPressed)
				distance -= BaseGame.MoveFactorPerSecond / 2.5f;
			if (distance < 0.01f)
				distance = 0.01f;
			if (distance > 10)
				distance = 10;

			BaseGame.ViewMatrix = Matrix.CreateLookAt(
				cameraPos, new Vector3(0, 0, 0), new Vector3(0, 0, 1));
		} // Update(gameTime)
		#endregion
	} // class SimpleCamera
} // namespace DungeonQuest.Game
