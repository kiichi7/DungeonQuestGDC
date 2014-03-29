// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using DungeonQuest.Shaders;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.GameScreens;
using DungeonQuest.Game;
using Material = DungeonQuest.Graphics.Material;
using Microsoft.Xna.Framework;
using DungeonQuest.Sounds;
using DungeonQuest.Collision;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Unit values, used for both the GameAsteroidManager to keep all
	/// enemy units in the current level and in UnitManager for all active
	/// units we are currently rendering.
	/// </summary>
	public class Projectile
	{
		#region Variables
		/// <summary>
		/// Damage this unit currently does. Copied from unit settings,
		/// but increase as the level advances.
		/// </summary>
		public float damage = 5;

		/// <summary>
		/// Helper for plasma and fireball effects, rotation is calculated
		/// once in constructor and used for the effects.
		/// </summary>
		public float effectRotation;

		/// <summary>
		/// Max speed for this unit (calculated from unit settings, increased
		/// a little for each level). Any movement is limited by this value.
		/// Units don't have to fly as fast as this value, this is just the limit.
		/// </summary>
		public float maxSpeed;
		/// <summary>
		/// Current position of this unit, will be updated each frame.
		/// Absolute position in 3d space.
		/// </summary>
		public Vector3 position;

		/// <summary>
		/// Move direction, not used for plasma projectiles, but fireballs will
		/// be moving towards the player (but not change direction during flight).
		/// Rockets on the other hand will slowly adjust to the player position.
		/// </summary>
		public Vector3 moveDirection;

		/// <summary>
		/// Own projectile? Then it flys up and damages enemies, else it
		/// is an enemy projectile!
		/// </summary>
		public bool ownProjectile = false;

		/// <summary>
		/// Life time of this projectile, will die after 6 seconds!
		/// </summary>
		private float lifeTimeMs = 0;
		#endregion

		#region Constructor
		/// <summary>
		/// Create unit of specific type at specific location.
		/// </summary>
		/// <param name="setType">Set type</param>
		/// <param name="setPosition">Set position</param>
		public Projectile(//WeaponTypes setType,
			Vector3 setPosition)//,
			//bool setOwnProjectile)
		{
			//obs: weaponType = setType;
			position = setPosition;
			//obs: ownProjectile = setOwnProjectile;

			// 300 dmg for plasma, 1000 for rockets
			//see above: damage = weaponType == WeaponTypes.Plasma ? 300 : 1000;
			/*
			if (ownProjectile == false)
			{
				if (weaponType == WeaponTypes.Fireball)
				{
			 */
					Vector3 distVec = BaseGame.camera.PlayerPos - position;
					moveDirection = Vector3.Normalize(
						BaseGame.camera.PlayerPos +
						//new Vector3(0, distVec.Length() / 3, 0) -
						new Vector3(0, 0, 1.25f) -
						position);
					//see above: damage = 70;// 75;
			/*
				} // if
				else if (weaponType == WeaponTypes.Rocket)
				{
					moveDirection = new Vector3(0, -1, 0);
					damage = 110;// 125;
				} // else if
			} // if
			 */

			/*tst
			if (weaponType == WeaponTypes.Rocket)
				maxSpeed = 30;//80;
			else if (weaponType == WeaponTypes.Plasma)
				maxSpeed = 75;//150;
			else if (weaponType == WeaponTypes.Fireball)
			 */
					maxSpeed = 8;// 20;//45;

			//if (weaponType == WeaponTypes.Plasma ||
			//	weaponType == WeaponTypes.Fireball)
				effectRotation = RandomHelper.GetRandomFloat(
					0, (float)Math.PI * 2.0f);
		} // Unit(setType, setPosition, setLevel)
		#endregion

		#region Get rotation angle
		private float GetRotationAngle(Vector3 pos1, Vector3 pos2)
		{
			// See http://en.wikipedia.org/wiki/Vector_(spatial)
			// for help and check out the Dot Product section ^^
			// Both vectors are normalized so we can save deviding through the
			// lengths.
			Vector3 vec1 = new Vector3(0, 1, 0);
			Vector3 vec2 = pos1 - pos2;
			vec2.Normalize();
			return (float)Math.Acos(Vector3.Dot(vec1, vec2));
		} // GetRotationAngle(pos1, pos2)

		private float GetRotationAngle(Vector3 vec)
		{
			return (float)Math.Atan2(vec.Y, vec.X);
		} // GetRotationAngle(pos1, pos2)
		#endregion

		#region Render
		/// <summary>
		/// Render projectile, returns false if we are done with it.
		/// Has to be removed then. Else it updates just position.
		/// </summary>
		/// <returns>True if done, false otherwise</returns>
		public bool Render()
		{
			#region Update movement
			lifeTimeMs += BaseGame.ElapsedTimeThisFrameInMs;
			float moveSpeed = BaseGame.MoveFactorPerSecond;
			if (Player.GameOver)
				moveSpeed = 0;
			/*
			switch (weaponType)
			{
				case WeaponTypes.Fireball:
			 */
			Vector3 oldPosition = position;

					position += moveDirection * maxSpeed * moveSpeed;
			/*
					break;
				case WeaponTypes.Rocket:
					if (ownProjectile)
						position += new Vector3(0, +1, 0) * maxSpeed * moveSpeed * 1.1f;
					else
					{
						// Fly to player
						Vector3 targetMovement = Player.shipPos - position;
						targetMovement.Normalize();
						moveDirection = moveDirection * 0.95f + targetMovement * 0.05f;
						moveDirection.Normalize();
						position += moveDirection * maxSpeed * moveSpeed;
					} // else
					break;
				case WeaponTypes.Plasma:
					if (ownProjectile)
						position += new Vector3(0, +1, 0) * maxSpeed * moveSpeed * 1.25f;
					else
						position += new Vector3(0, -1, 0) * maxSpeed * moveSpeed;
					break;
				// Rest are items, they just stay around
			} // switch(movementPattern)
			 */
			#endregion

			#region Skip if out of visible range
			float distance = (position - BaseGame.camera.PlayerPos).Length();
			const float MaxUnitDistance = BaseGame.FarPlane;

			// Remove unit if it is out of visible range!
			if (distance > MaxUnitDistance)/*tst ||
				distance < -MaxUnitDistance * 2 ||
				position.Z < 0 ||
				lifeTimeMs > 6000)*/
				return true;

			// Also check if fireball is hitting a wall
			Vector3 newPosition = position;
			Vector3 newVelocity = moveDirection * maxSpeed * moveSpeed;
			Vector3 polyPoint;
			CaveCollision.caveCollision.PointMove(
				oldPosition, position, 0.0f, 1.0f, 3,
				out newPosition, ref newVelocity, out polyPoint);
			if (Vector3.Distance(newPosition, position) > 0.01f)
				return true;
			#endregion

			#region Render
			/*
			switch (weaponType)
			{
				case WeaponTypes.Rocket:
					float rocketRotation = MathHelper.Pi;
					if (ownProjectile == false)
						rocketRotation = MathHelper.PiOver2 + GetRotationAngle(moveDirection);
					mission.AddModelToRender(
						mission.shipModels[(int)Mission.ShipModelTypes.Rocket],
						Matrix.CreateScale(
						(ownProjectile ? 1.25f : 0.75f) *
						Mission.ShipModelSize[(int)Mission.ShipModelTypes.Rocket]) *
						Matrix.CreateRotationZ(rocketRotation) *
						Matrix.CreateTranslation(position));
					// Add rocket smoke
					EffectManager.AddRocketOrShipFlareAndSmoke(position, 1.5f, 6 * maxSpeed);
					break;
				case WeaponTypes.Plasma:
					EffectManager.AddPlasmaEffect(position, effectRotation, 1.25f);
					break;
				case WeaponTypes.Fireball:
			 */
					EffectManager.AddFireBallEffect(position, effectRotation, 0.25f);
			/*		break;
			} // switch
			 */
			#endregion

			#region Explode if hitting unit
			/*
			// Own projectile?
			if (ownProjectile)
			{
				// Hit enemy units, check all of them
				for (int num = 0; num < Mission.units.Count; num++)
				{
					Unit enemyUnit = Mission.units[num];
					// Near enough to enemy ship?
					Vector2 distVec =
						new Vector2(enemyUnit.position.X, enemyUnit.position.Y) -
						new Vector2(position.X, position.Y);
					if (distVec.Length() < 7 &&
						(enemyUnit.position.Y - Player.shipPos.Y) < 60)
					{
						// Explode and do damage!
						EffectManager.AddFlameExplosion(position);
						Player.score += (int)enemyUnit.hitpoints / 10;
						enemyUnit.hitpoints -= damage;
						return true;
					} // if
				} // for
			} // if
			// Else this is an enemy projectile?
			else
			{
			 */
      // Near enough to our player?
      Vector2 distVec =
        new Vector2(BaseGame.camera.PlayerPos.X, BaseGame.camera.PlayerPos.Y) -
        new Vector2(position.X, position.Y);
      if (distVec.Length() < 0.275f)//0.25f)
			{
				// Explode and do damage!
				EffectManager.AddFlameExplosion(position);
				Player.health -= 10+Player.level/2-Player.defenseIncrease;//14;
				Sound.Play(Sound.Sounds.PlayerWasHit);
				return true;
			} // if
			//} // else
			#endregion

			// Don't remove unit from units list
			return false;
		} // Render()
		#endregion
	} // class Unit
} // namespace DungeonQuest.Game