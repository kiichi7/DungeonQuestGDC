// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 05:50

#region Using directives
using DungeonQuest.Collision;
using DungeonQuest.GameLogic;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.Properties;
using DungeonQuest.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Third person camera camera class for Dungeon Quest.
	/// Always focuses on the head of the player and uses the same direction
	/// vector, which is only rotate around the z axis.
	/// </summary>
	public class ThirdPersonCamera : BaseCamera
	{
		#region Variables
		/// <summary>
		/// Initial player position.
		/// </summary>
		static readonly Vector3 InitialPlayerPosition = new Vector3(0, 0, 0);

		/// <summary>
		/// The player is about 2 meters in height and we always look on his head
		/// (around 1.9m).
		/// </summary>
		const float DefaultPlayerHeight = 1.5f;//1.9f;

		/// <summary>
		/// Player position
		/// </summary>
		Vector3 playerPos = InitialPlayerPosition,
			wannaHavePlayerPos = InitialPlayerPosition;

		/// <summary>
		/// X rotation is fixed, but the player can change +/- 15 degrees.
		/// The Z rotation is freely chooseable (just rotate around),
		/// Y (roll) is never changed.
		/// </summary>
		const float InitialXRotation = MathHelper.PiOver2 - MathHelper.Pi / 12.0f,
			MaxXRotationOffset = (float)Math.PI * 25.0f / 180.0f,
			InitialZRotation = MathHelper.Pi,
			InitialPlayerRotation = 0;//MathHelper.Pi;

		/// <summary>
		/// X and z rotation for the camera behind the player.
		/// The player itself has its own rotation and moves in that direction.
		/// The rotation will be calculated from the movement the user
		/// wants, but is has to be interpolated to look smooth.
		/// The wanna have variables help us interpolating.
		/// </summary>
		float xRotation = InitialXRotation,
			zRotation = InitialZRotation,
			playerRotation = InitialPlayerRotation,
			wannaHaveXRotation = InitialXRotation,
			wannaHaveZRotation = InitialZRotation,
			wannaHavePlayerRotation = InitialPlayerRotation;

		/// <summary>
		/// The camera distance is always the same.
		/// </summary>
		public const float CameraDistance = 3.0f;//2.25f;//2.75f;//1.75f;
		#endregion

    #region Reset
		/// <summary>
		/// Reset
		/// </summary>
		public override void Reset()
		{
			playerPos = InitialPlayerPosition;
			wannaHavePlayerPos = InitialPlayerPosition;
			xRotation = InitialXRotation;
			zRotation = InitialZRotation;
			playerRotation = InitialPlayerRotation;
			wannaHaveXRotation = InitialXRotation;
			wannaHaveZRotation = InitialZRotation;
			wannaHavePlayerRotation = InitialPlayerRotation;
			playerObject = new AnimatedGameObject(
				GameManager.AnimatedTypes.Hero, Matrix.Identity);
		} // Reset()
    #endregion

    #region Properties
    /// <summary>
		/// Player position
		/// </summary>
		/// <returns>Vector 3</returns>
		public override Vector3 PlayerPos
		{
			get
			{
				return playerPos;
			} // get
		} // PlayerPos

		/// <summary>
		/// Player rotation
		/// </summary>
		/// <returns>Float</returns>
		public override float PlayerRotation
		{
			get
			{
				return playerRotation;
			} // get
		} // PlayerRotation

		/// <summary>
		/// Player blend states
		/// </summary>
		public override float[] PlayerBlendStates
		{
			get
			{
				return playerObject.blendedStates;
			} // get
		} // PlayerBlendStates
		#endregion

		#region Constructor
		/// <summary>
		/// Keys for moving around. Assigned from settings!
		/// </summary>
		static Keys moveLeftKey,
			moveRightKey,
			moveUpKey,
			moveDownKey;

		public ThirdPersonCamera(BaseGame game)
			: base(game)
		{
			// Assign keys. Warning: This is VERY slow, never use it
			// inside any render loop (getting Settings, etc.)!
			//Note: Not true anymore for the new GameSettings class, it is faster!
			moveLeftKey = GameSettings.Default.MoveLeftKey;
			moveRightKey = GameSettings.Default.MoveRightKey;
			moveUpKey = GameSettings.Default.MoveForwardKey;
			moveDownKey = GameSettings.Default.MoveBackwardKey;
		} // ThridPersonCamera(game)
		#endregion

		#region Initialize
		/// <summary>
		/// Initialize
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
		} // Initialize()
		#endregion

		#region Update
		/// <summary>
		/// Update
		/// </summary>
		/// <param name="gameTime">Game time</param>
		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			// Player movement!
			Vector2 playerMove = Vector2.Zero;
			float playerSpeed = 5.0f + Player.speedIncrease;
			playerMove.X +=
				Input.GamePad.ThumbSticks.Left.X * BaseGame.MoveFactorPerSecond *
				playerSpeed;
			playerMove.Y +=
				Input.GamePad.ThumbSticks.Left.Y * BaseGame.MoveFactorPerSecond *
				playerSpeed;
			if (Input.Keyboard.IsKeyDown(moveLeftKey) ||
				Input.Keyboard.IsKeyDown(Keys.Left) ||
				Input.GamePad.DPad.Left == ButtonState.Pressed)
				playerMove.X -= BaseGame.MoveFactorPerSecond * playerSpeed;
			if (Input.Keyboard.IsKeyDown(moveRightKey) ||
				Input.Keyboard.IsKeyDown(Keys.Right) ||
				Input.GamePad.DPad.Right == ButtonState.Pressed)
				playerMove.X += BaseGame.MoveFactorPerSecond * playerSpeed;
			if (Input.Keyboard.IsKeyDown(moveUpKey) ||
				Input.Keyboard.IsKeyDown(Keys.Up) ||
				Input.GamePad.DPad.Up == ButtonState.Pressed)
				playerMove.Y += BaseGame.MoveFactorPerSecond * playerSpeed;
			if (Input.Keyboard.IsKeyDown(moveDownKey) ||
				Input.Keyboard.IsKeyDown(Keys.Down) ||
				Input.GamePad.DPad.Down == ButtonState.Pressed)
				playerMove.Y -= BaseGame.MoveFactorPerSecond * playerSpeed;

			// Update wannaHavePlayerPos, which will change the playerPos below,
			// but use the rotated playerMove for that (else movement is not
			// relative to the camera)
			Vector3 playerMovement = Vector3.Transform(
				//normal:new Vector3(playerMove, 0),
				new Vector3(playerMove*2, 0),
				Matrix.CreateRotationZ(zRotation));

			// If the movement is big enough, update the rotation!
			if (playerMovement.Length() > 0.0001f)
			{
				wannaHavePlayerRotation =
					MathHelper.PiOver2 +
					(float)Math.Atan2(playerMovement.Y, playerMovement.X);
				Sound.StartSteps();
			} // if (playerMovement.Length)
			else
				Sound.StopSteps();

			if (Player.GameOver ||
				//always: Player.victory == false ||
				Player.GameTime < 0)
			{
				if (Player.GameOver &&
					Player.victory == false)
					playerObject.state = AnimatedGameObject.States.Die;
				wannaHaveZRotation = zRotation = BaseGame.TotalTime / 4.509f;
			} // if (Player.GameOver)
			else if (Input.Keyboard.IsKeyDown(Keys.LeftControl) ||
				Input.GamePadAPressed ||
				Input.MouseLeftButtonPressed)
			{
				// Don't change hit mode until we got through with the animation.
				if (playerObject.state != AnimatedGameObject.States.Hit1 &&
					playerObject.state != AnimatedGameObject.States.Hit2)
					playerObject.state =
						RandomHelper.GetRandomInt(2) == 0 ?
						AnimatedGameObject.States.Hit1 : AnimatedGameObject.States.Hit2;
				playerMovement = Vector3.Zero;
			} // else if
			else if (playerMovement.Length() > BaseGame.MoveFactorPerSecond * 5)
				playerObject.state = AnimatedGameObject.States.Run;
			else
				playerObject.state = AnimatedGameObject.States.Idle;
			// Update and blend states.
			playerObject.UpdateState();

			//tst: Thread.Sleep(100);

			// Use collision system now!
			CaveCollision.UpdatePlayerPosition(
				ref wannaHavePlayerPos, playerMovement, false, playerObject);

			// Always hit at frame 11
			if (playerObject.state == AnimatedGameObject.States.Hit1 ||
				playerObject.state == AnimatedGameObject.States.Hit2)
			{
				// Use 49 for animation length for Hit1 and 29 for Hit2,
				// see AnimatedColladaModel.cs:218 for details.
				//int animationLength =
				//	playerObject.state == AnimatedGameObject.States.Hit1 ? 45 : 29;

				AnimatedGameObject nearestEnemy = null;
				foreach (AnimatedGameObject model in GameManager.animatedObjects)
					if ((nearestEnemy == null ||
						(BaseGame.camera.PlayerPos - model.positionMatrix.Translation).
						Length() <
						(BaseGame.camera.PlayerPos - nearestEnemy.positionMatrix.Translation).
						Length()) &&
						// Must not be dead
						model.state != AnimatedGameObject.States.Die)
						nearestEnemy = model;

				if (nearestEnemy != null &&
					(nearestEnemy.positionMatrix.Translation -
					BaseGame.camera.PlayerPos).Length() < 2.25f)
				{
					// Rotate player towards this enemy if not moving right now
					if (playerMovement.Length() <= 0.0001f)
					{
						Vector3 helperVector =
							nearestEnemy.positionMatrix.Translation -
							BaseGame.camera.PlayerPos;
						wannaHavePlayerRotation =
							MathHelper.PiOver2 +
							(float)Math.Atan2(helperVector.Y, helperVector.X);
					} // if (playerMovement.Length)

					if (playerObject.GetAnimationElapsedTimeFrame(16))//22))
					{
						// Hurt monster if its close.
						Vector3 effectPos = Vector3.Transform(
							new Vector3(0.2f, -0.82f, 1.2f) +
							RandomHelper.GetRandomVector3(-0.3f, 0.3f),
							Matrix.CreateRotationZ(wannaHavePlayerRotation));

						float damage = Player.CurrentWeaponDamage;

						EffectManager.AddBlood(wannaHavePlayerPos + effectPos, 0.125f);
						//Sound.Play(Sound.Sounds.HitFlesh);

						// Hit1 does 33% more damage (animation takes longer)!
						damage = (playerObject.state == AnimatedGameObject.States.Hit1 ?
							1.33f : 1.0f) * damage;

						// More speed gives also more attack!
						damage *= 1.0f + Player.speedIncrease / 6.0f;

						// Increase armor for higher levels
						damage -= Player.level / 2;

						if (damage > 0)
						{
							nearestEnemy.hitpoints -= damage;

							Player.score++;
							// Give a little energy to player if monster died
							if (nearestEnemy.hitpoints <= 0)
							{
								Player.health += (3 + (int)nearestEnemy.type);
								if (Player.health > 100)
									Player.health = 100;
							} // if (nearestEnemy.hitpoints)
						} // if (damage)

						if (damage > 0)
						{
							// Do not always play a player cry
							if (RandomHelper.GetRandomInt(3) == 0)
								Sound.Play(
									nearestEnemy.type == GameManager.AnimatedTypes.Ogre ||
									nearestEnemy.type == GameManager.AnimatedTypes.BigOgre ?
									Sound.Sounds.OgreWasHit : Sound.Sounds.GoblinWasHit);
						} // if (damage)
						// No damage? Then don't cry

						// But always play hit sound, this makes the sound make much realer
						Sound.Play(Player.currentWeapon == Player.WeaponTypes.Club ?
							Sound.Sounds.HitClub : Sound.Sounds.HitFlesh);
					} // if (playerObject.GetAnimationElapsedTimeFrame)
					//else if ((int)(Player.GameTime * 30) % animationLength == 11 ||
					//	(int)(Player.GameTime * 30) % animationLength == 12)
					else if (playerObject.GetAnimationElapsedTimeFrame(11) ||
						playerObject.GetAnimationElapsedTimeFrame(25))
					{
						// Hurt player if he is close.
						Vector3 effectPos = Vector3.Transform(
							new Vector3(0.2f, -0.82f, 1.2f) +
							RandomHelper.GetRandomVector3(-0.3f, 0.3f),
							Matrix.CreateRotationZ(wannaHavePlayerRotation));
						EffectManager.AddBlood(wannaHavePlayerPos + effectPos, 0.1f);
					} // else if
				} // if (nearestEnemy)

				//if ((int)(Player.GameTime * 30) % animationLength == 2 ||
					// 30 is only used for Hit1, Hit2 has only 29 ani steps
				//	(int)(Player.GameTime * 30) % animationLength == 30)
				if (playerObject.GetAnimationElapsedTimeFrame(1) ||
					playerObject.GetAnimationElapsedTimeFrame(10) ||
					playerObject.GetAnimationElapsedTimeFrame(11))
				{
					Sound.Play(Sound.Sounds.Whosh);
				} // if (playerObject.GetAnimationElapsedTimeFrame)

				// At end of animation?
				//if ((int)(Player.GameTime*30) % animationLength == animationLength-1)
				if (playerObject.GetAnimationElapsedTimeFrame(0))
				{
					// Set state back to idle, next frame it will be replaced
					// with a randomly choosen new hit animation!
					playerObject.state =
						RandomHelper.GetRandomInt(2) == 0 ?
						AnimatedGameObject.States.Hit1 : AnimatedGameObject.States.Hit2;
				} // if (playerObject.GetAnimationElapsedTimeFrame)
			} // if (playerObject.state)
			
			//TextureFont.WriteText(2, 60, "Blended states: " +
			//	StringHelper.WriteArrayData(playerObject.blendedStates));

			//obs: wannaHavePlayerPos += playerMovement;

			/*tst:
			if (Input.Keyboard.IsKeyDown(Keys.U))
				wannaHavePlayerPos.Z += BaseGame.MoveFactorPerSecond * 5.0f;
			if (Input.Keyboard.IsKeyDown(Keys.V))
				wannaHavePlayerPos.Z -= BaseGame.MoveFactorPerSecond * 5.0f;

			// Update player rotation which just results from the direction
			// we are moving to. If the player is not moving, there will be no change.
			/*TODO
			rotation += BaseGame.MoveFactorPerSecond / 1.75f;
			cameraPos = Vector3.Transform(initialPos*distance,
				Matrix.CreateRotationZ(rotation));
			 */
			//TODO: resulting player rotation

			// Allow rotating with gamepad, mouse and keyboard are not supported yet.
			wannaHaveXRotation -=
				Input.GamePad.ThumbSticks.Right.Y * BaseGame.MoveFactorPerSecond * 1.6f;
			/*obs, cursor keys allow movement now!
			if (Input.Keyboard.IsKeyDown(Keys.Up))
				wannaHaveXRotation -= BaseGame.MoveFactorPerSecond * 1.0f;
			if (Input.Keyboard.IsKeyDown(Keys.Down))
				wannaHaveXRotation += BaseGame.MoveFactorPerSecond * 1.0f;
			 */
			if (wannaHaveXRotation < InitialXRotation - MaxXRotationOffset)
				wannaHaveXRotation = InitialXRotation - MaxXRotationOffset;
			if (wannaHaveXRotation > InitialXRotation + MaxXRotationOffset)
				wannaHaveXRotation = InitialXRotation + MaxXRotationOffset;

			wannaHaveZRotation -=
				Input.GamePad.ThumbSticks.Right.X * BaseGame.MoveFactorPerSecond * 3.5f;
			/*obs, cursor keys allow movement now!
			if (Input.Keyboard.IsKeyDown(Keys.Left))
				wannaHaveZRotation += BaseGame.MoveFactorPerSecond * 2.5f;
			if (Input.Keyboard.IsKeyDown(Keys.Right))
				wannaHaveZRotation -= BaseGame.MoveFactorPerSecond * 2.5f;
			 */

			// Allow control with mouse instead!
			// Ignore first 100ms, else we rotate around like crazy!
			if (Player.GameTime > 0.1f)
			{
				wannaHaveXRotation -=
					Input.MouseYMovement * BaseGame.MoveFactorPerSecond * 0.5f / 3.0f;
				wannaHaveZRotation -=
					Input.MouseXMovement * BaseGame.MoveFactorPerSecond * 1.0f / 3.5f;
			} // if (Player.GameTime)

			// Do the interpolation (25% each frame)
			// Do not allow any movement anymore if game is over
			if (Player.GameOver == false &&
				Player.GameTime >= 0)
			{
				xRotation = xRotation * 0.75f + wannaHaveXRotation * 0.25f;
					//TODO: InterpolateRotation(xRotation, wannaHaveXRotation, 0.75f);
				zRotation =
					//Problematic because 0-360, just assign.
					wannaHaveZRotation;
					//TODO: zRotation * 0.75f + wannaHaveZRotation * 0.25f;

				// Quick fix!
				if (playerRotation - wannaHavePlayerRotation > 3.1415f)
					wannaHavePlayerRotation += MathHelper.Pi * 2.0f;
				else if (playerRotation - wannaHavePlayerRotation < -3.1415f)
					wannaHavePlayerRotation -= MathHelper.Pi * 2.0f;

				playerRotation =
					playerRotation * 0.925f + wannaHavePlayerRotation * 0.075f;
				playerPos = playerPos * 0.75f + wannaHavePlayerPos * 0.25f;
			} // if (Player.GameOver)

			// Look at the player pos (at his head) and build the view matrix
			// from all the rotation values we got.
			Vector3 lookTarget = playerPos + new Vector3(0, 0, DefaultPlayerHeight);
			BaseGame.ViewMatrix = Matrix.CreateLookAt(
				lookTarget + Vector3.Transform(
				new Vector3(0, 0, CameraDistance),
				Matrix.CreateRotationX(xRotation) *
				Matrix.CreateRotationZ(zRotation)),
				lookTarget, new Vector3(0, 0, 1));
		} // Update(gameTime)
		#endregion
	} // class ThirdPersonCamera
} // namespace DungeonQuest.Game
