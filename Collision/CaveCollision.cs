// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Collision
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:37

#region Using directives
using DungeonQuest.Game;
using DungeonQuest.GameLogic;
using DungeonQuest.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using DungeonQuest.Helpers;
#endregion

namespace DungeonQuest.Collision
{
	/// <summary>
	/// Helper class to handle the collision with the cave.
	/// All stuff in here is static for easier access.
	/// </summary>
	class CaveCollision
	{
		#region Variables
		/// <summary>
		/// Collision mesh for the cave. This is very important ^^
		/// </summary>
		public static CollisionGeometry caveCollision = null;

		/// <summary>
		/// Max height for step player can climb without jumping.
		/// </summary>
		const float stepHeight = 0.2f;

		/// <summary>
		/// Collision box for the player. 2m height, 66cm width
		/// </summary>
    static BoxHelper box = new BoxHelper(
			new Vector3(-0.33f, -0.33f, stepHeight),
			new Vector3(+0.33f, +0.33f, 2));
		#endregion

		#region Load
		/// <summary>
		/// Load cave collision stuff
		/// </summary>
		static void Load()
		{
			// Load the geometry for the cave
			caveCollision = new CollisionGeometry("CaveCollision");
		} // CaveCollision()
		#endregion

		#region Update player position
		/// <summary>
		/// Update player position
		/// </summary>
		/// <param name="playerPos">Player position</param>
		/// <param name="movement">Movement</param>
		/// <param name="isMonster">Is monster</param>
		static public void UpdatePlayerPosition(ref Vector3 playerPos,
			Vector3 movement, bool isMonster, AnimatedGameObject obj)
		{
			if (caveCollision == null)
				Load();

			float movementAmount = movement.Length();

			// Do not do anything if we are not moving!
			if (obj.isFlyingCounter == 0 &&
				movementAmount < 0.001f)
				return;

			// Check the door, we can only move through it when we got the key!
			if (Player.gotKey == false)
			{
				// No key, then stop moving here!
				BounceOff(playerPos, GameManager.doorPosition, 3.5f, ref movement);
			} // if (Vector3.Distance)

			// Also bounce of every enemy and the player except ourself (see obj)
			if (obj != BaseGame.camera.playerObject)
				BounceOff(playerPos, BaseGame.camera.PlayerPos, 1.25f, ref movement);
			foreach (AnimatedGameObject model in GameManager.animatedObjects)
				if (model != obj &&
					// Should be alive
					model.state != AnimatedGameObject.States.Die)
					BounceOff(playerPos, model.positionMatrix.Translation,
						0.25f + model.sizeVariation, ref movement);		

			// Use 9.81m/s gravitation (use /2.75 because it is getting hard
			// getting up some tunnels and it stafes back all the time).
			movement.Z = -9.81f * BaseGame.MoveFactorPerSecond * 0.85f;// / 2.75f;
			// Monsters move slower and this will make it too hard to move up hills!
			//if (obj.wasAboveGroundLastFrame == false)
			//	movement.Z /= 2.0f;
			if (isMonster)
				movement.Z /= 2.5746523f;

			Vector3 newPosition = playerPos + movement;
			Vector3 newVelocity = Vector3.Zero;

			Vector3 oldPosition = playerPos;
			newPosition = playerPos + movement;// +new Vector3(0, 0, 0);
			Vector3 polyPoint;
			caveCollision.PointMove(
				// Always add a little height to the player position just
				// to make sure we never fall out of the level!
				playerPos,// + new Vector3(0, 0, 0.00005f),
				newPosition, 1.0f, 0.1f, 3,
				out newPosition, ref newVelocity, out polyPoint);

			float groundDist = playerPos.Z - polyPoint.Z;
			playerPos = newPosition;

			// Reached ground? Then stop flying mode and stop moving
			// if no movement is used.
			if (obj.isFlyingCounter > 0 &&
				//(isMonster == false || groundDist > 0) &&
				groundDist > 0 &&
				Math.Abs(groundDist) < 0.05f * 50 * BaseGame.MoveFactorPerSecond)
				obj.isFlyingCounter--;
				// Flying again? Then allow dropping to the ground
			else if (obj.isFlyingCounter == 0 &&
				Math.Abs(groundDist) > 0.5f)
				obj.isFlyingCounter = 1;

			// If we are below the ground, fix it.
			if (obj.isFlyingCounter == 0 &&
				groundDist < -0.25f)
				playerPos.Z += -groundDist +0.0025f;
		} // UpdatePlayerPosition(playerPos, movement, isMonster)
		#endregion

		#region BounceOff
		/// <summary>
		/// Bounce off
		/// </summary>
		/// <param name="playerPos">Player position</param>
		/// <param name="targetPos">Target position</param>
		/// <param name="keepDistance">Keep distance</param>
		public static void BounceOff(Vector3 playerPos, Vector3 targetPos,
			float keepDistance, ref Vector3 movement)
		{
			float remMovementZ = movement.Z;
			// Only continue if we are in the inner distance
			if (Vector3.DistanceSquared(playerPos, targetPos) <
				keepDistance * keepDistance)
			{
				float distance = Vector3.Distance(playerPos, targetPos);
				if (distance > 0)
				{
					// Allow some movement in the outer region, but make it harder
					// as we move to the middle
					float strength = 1.0f;
					if (distance > keepDistance / 2)
						strength -= (distance - keepDistance / 2) / (keepDistance / 2);
					// Move away from the center
					movement += strength * (playerPos - targetPos);

					// Make sure we never move down or up (z), gravitation is handled
					// after this. Move up a little too because collisions sometimes
					// push us into the ground (not good).
					movement.Z = remMovementZ + 0.01f;
				} // if (distance)
			} // if (Vector3.DistanceSquared)
		} // BounceOff(playerPos, targetPos, keepDistance)
		#endregion
	} // class CaveCollision
} // namespace DungeonQuest.Collision
