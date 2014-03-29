// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

#region Using directives
using DungeonQuest.Collision;
using DungeonQuest.GameLogic;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Moveable game model
	/// </summary>
	public class AnimatedGameObject //not required: StaticGameModel
	{
		#region Variables
		/// <summary>
		/// Type for this model
		/// </summary>
		public GameManager.AnimatedTypes type;

		/// <summary>
		/// Position matrix for positioning this entry, scale is unused
		/// and rotation is kinda important too.
		/// </summary>
		public Matrix positionMatrix, initialPositionMatrix;

		/// <summary>
		/// Movement speed for this entry.
		/// </summary>
		public Vector3 movement;

		/// <summary>
		/// Monster rotation (see AI)
		/// </summary>
		/// <returns>0</returns>
		public float wannaHaveMonsterRotation, monsterRotationZ = 0;

		/// <summary>
		/// States the player can be in, the animation will actually be blended
		/// and is a little bit more complicated than just the States.
		/// </summary>
		public enum States // same as AnimatedColladaModel.AnimationTypes
		{
			Run,
			Idle,
			Hit1,
			Hit2,
			Die,
		} // enum States

		/// <summary>
		/// Current state
		/// </summary>
		public States state = States.Idle;

		/// <summary>
		/// Number of states we can manage here.
		/// 0 = Idle, 1 = Run, 2 = Hit 1, 3 = Hit 2, 4 = Die
		/// </summary>
		const int NumberOfStates = 5;
		/// <summary>
		/// Animation blending (see AnimatedColladaModel)
		/// </summary>
		public float[] blendedStates = new float[NumberOfStates];

		/// <summary>
		/// Size variation for the monsters.
		/// </summary>
		public float sizeVariation = 1.0f;

		/// <summary>
		/// Hitpoints for this monster, decreases when we manage to hit him.
		/// </summary>
		public float hitpoints = 1.0f;

		/// <summary>
		/// Little helper to make all models be animated differently.
		/// </summary>
		public float addAnimationTime = 0.0f;

		/// <summary>
		/// Time till death, used for the die animation (stops at end).
		/// Also important to let player fall down after game is over!
		/// </summary>
		public float timeTillDeath = 0.0f;

		/// <summary>
		/// Drop object for monsters.
		/// </summary>
		public enum DropObject
		{
			None,
			Club,
			Sword,
			BigSword,
			Key,
		} // enum DropObject

		/// <summary>
		/// Drop object for monsters. Will be randomly filled for monsters
		/// in the constructor by the GameManager.
		/// </summary>
		public DropObject dropObject;

		/// <summary>
		/// Helper for collision testing. At the beginning everything is
		/// flying, when we reach the ground the flying is set to false and
		/// we do not move or strafe around.
		/// Counter is set to 3 to check 3 times if we are above ground,
		/// else we miss sometimes for some monsters!
		/// </summary>
		//obs: public bool wasAboveGroundLastFrame = true;
		public int isFlyingCounter = 8;//25;

		/// <summary>
		/// Each monster crys when he comes near to us.
		/// </summary>
		bool hasCried = false;

		/// <summary>
		/// Weapon pos, currently only used for wizards. Used to show fireball
		/// effect. Updated in GameManager each time we render!
		/// </summary>
		public Vector3 weaponPos = Vector3.Zero;
		#endregion

		#region Reset
		/// <summary>
		/// Reset
		/// </summary>
		public void Reset()
		{
			positionMatrix = initialPositionMatrix;
			wannaHaveMonsterRotation = monsterRotationZ = 0;
			state = States.Idle;
			hitpoints = 1.0f;
			addAnimationTime = 0.0f;
			timeTillDeath = 0.0f;
			isFlyingCounter = 8;// 25;
			hasCried = false;

			// Use default state of not moving and idling around
			movement = Vector3.Zero;
			state = States.Idle;
			blendedStates[0] = 1.0f;
			blendedStates[1] = 0.0f;
			blendedStates[2] = 0.0f;
			blendedStates[3] = 0.0f;
			blendedStates[4] = 0.0f;
			sizeVariation = RandomHelper.GetRandomFloat(0.9f, 1.1f);
			addAnimationTime = RandomHelper.GetRandomFloat(0, 100);

			// Goblins are smaller
			if (type == GameManager.AnimatedTypes.Goblin)
				sizeVariation *= 0.85f;
			else if (type == GameManager.AnimatedTypes.GoblinMaster)
				sizeVariation *= 1.1f;
			else if (type == GameManager.AnimatedTypes.GoblinWizard)
				sizeVariation *= 0.92f;
			else if (type == GameManager.AnimatedTypes.Ogre)
				sizeVariation *= 1.5f;
			else if (type == GameManager.AnimatedTypes.BigOgre)
				sizeVariation *= 2.5f;

			hitpoints = GameManager.MonsterHitpoints[(int)type];
		} // Reset()
		#endregion

		#region MoveableGameModel
		/// <summary>
		/// Create moveable game model
		/// </summary>
		/// <param name="setType">Set type</param>
		/// <param name="setPositionMatrix">Set position matrix</param>
		public AnimatedGameObject(GameManager.AnimatedTypes setType,
			Matrix setPositionMatrix, DropObject setDropObject)
		{
			type = setType;

			initialPositionMatrix = positionMatrix = setPositionMatrix;
			// Use default state of not moving and idling around
			movement = Vector3.Zero;
			state = States.Idle;
			blendedStates[0] = 1.0f;
			blendedStates[1] = 0.0f;
			blendedStates[2] = 0.0f;
			blendedStates[3] = 0.0f;
			blendedStates[4] = 0.0f;
			sizeVariation = RandomHelper.GetRandomFloat(0.9f, 1.1f);
			addAnimationTime = RandomHelper.GetRandomFloat(0, 100);

			// Randomly let monsters drop one of the weapons if we did not
			// specify a drop object (like the key) yet.
			if (setDropObject == DropObject.None)
				// Use the percentages from GameManager
				dropObject = GameManager.GetDropPercentages(type);
			else
				dropObject = setDropObject;
			//Log.Write("Monster: " + type + ": dropObject=" + dropObject);

			// Goblins are smaller, ogres are a lot bigger!
			if (type == GameManager.AnimatedTypes.Goblin)
				sizeVariation *= 0.85f;
			else if (type == GameManager.AnimatedTypes.GoblinMaster)
				sizeVariation *= 1.1f;
			else if (type == GameManager.AnimatedTypes.GoblinWizard)
				sizeVariation *= 0.92f;
			else if (type == GameManager.AnimatedTypes.Ogre)
				sizeVariation *= 1.5f;
			else if (type == GameManager.AnimatedTypes.BigOgre)
				sizeVariation *= 2.5f;

			hitpoints = GameManager.MonsterHitpoints[(int)type];
		} // AnimatedGameObject(setType, setPositionMatrix)
		
		/// <summary>
		/// Create moveable game model
		/// </summary>
		/// <param name="setType">Set type</param>
		/// <param name="setPositionMatrix">Set position matrix</param>
		public AnimatedGameObject(GameManager.AnimatedTypes setType,
			Matrix setPositionMatrix)
			: this(setType, setPositionMatrix, DropObject.None)
		{
		} // AnimatedGameObject(setType, setPositionMatrix)
		#endregion

		#region GetAnimationElapsedTimeFrame
		/// <summary>
		/// Get animation elapsed time frame. This is a very important method
		/// to find out if we reached a certain animation number, it works both
		/// if we got high frame rates (and hit an animation step for multiple
		/// frames) and for low frame rates (when we might not hit it at all).
		/// </summary>
		/// <param name="aniNum">Ani number</param>
		/// <returns>Bool</returns>
		public bool GetAnimationElapsedTimeFrame(int aniNum)
		{
			int animationLength = AnimatedColladaModel.AnimationLengths[(int)state];
			//obs: return (int)(Player.GameTime * 30) % animationLength == aniNum);
			int currentFrame = (int)(Player.GameTime * 30) % animationLength;
			int lastFrame = (int)(Player.LastGameTime * 30) % animationLength;
			// Last frame we must have been below and this frame we must have reached
			// our aniNum goal frame! In all other cases return false.
			return (lastFrame < aniNum &&
				currentFrame >= aniNum) ||
				// Special case for switching hit animations (only change at frame 0)
				(aniNum == -1 &&
				lastFrame < animationLength - 1 &&
				currentFrame == animationLength - 1);
		} // GetAnimationElapsedTimeFrame(aniNum)
		#endregion

		#region UpdateState
		/// <summary>
		/// Update state, will update the blendedStates array depending on
		/// the current state of this object. All states are faded into each other!
		/// </summary>
		public void UpdateState()
		{
			if (state == States.Die)
				timeTillDeath += BaseGame.MoveFactorPerSecond;

			// Go through all states
			float totalBlending = 0;
			for (int num = 0; num < NumberOfStates; num++)
			{
				// Is this state currently active?
				bool selected = num == (int)state;

				// Increase size if selected, decrease otherwise
				// Blend in a matter of a quarter second!
				// Update: half a second because we got longer animations now!
				// Update 2: Looks often too strange, its better with fast changes!
				blendedStates[num] +=
					(selected ? 1 : -1) * BaseGame.MoveFactorPerSecond * 5;// 2;// 4;
				if (blendedStates[num] < 0)
					blendedStates[num] = 0;
				if (blendedStates[num] > 1)
					blendedStates[num] = 1;
				totalBlending += blendedStates[num];
			} // for (num)

			// Renormalize all states (else bones get rotated too extreme)
			for (int num = 0; num < NumberOfStates; num++)
				blendedStates[num] /= totalBlending;
		} // UpdateState()
		#endregion

		#region UpdateMonsterAI
		/// <summary>
		/// Update monster AI
		/// </summary>
		public void UpdateMonsterAI()//GameManager gameManager)
		{
			// Don't update anymore if this one is dead
			if (state == States.Die)
				return;

			Vector3 monsterMovement = Vector3.Zero;
			Vector3 wannaHaveMonsterPos = positionMatrix.Translation;

			Vector3 movementVector =
				BaseGame.camera.PlayerPos - wannaHaveMonsterPos;
			// Initially all monsters are in the idle state, just change
			// to the move state if they see the player and then change to the
			// attack state once we reached him.
			//TODO: AI for 2 players
			if (type == GameManager.AnimatedTypes.GoblinWizard)
			{
				// Always show little fire flare effect at weapon
				Vector3 flarePos = weaponPos;
					//in GameManager now:
					//gameManager.animatedModels[(int)type].GetWeaponPos(
					//positionMatrix);// +wannaHaveMonsterPos;
				//EffectManager.AddSmoke(flarePos, 0.25f);
				EffectManager.AddFireBallEffect(flarePos, 0, 0.15f);

				if ((wannaHaveMonsterPos - BaseGame.camera.PlayerPos).Length() < 20)
					state = States.Hit1; // throw ball mode

				if (state == States.Hit1)
				{
					// * 2 to make it less often (1.5 secs)
					if (BaseGame.EveryMs((int)(2 * 1000 * 31 / 30.0f),
						(int)(addAnimationTime * 2 * 1000)))
					{
						GameManager.AddWeaponProjectile(flarePos);
						EffectManager.AddEffect(flarePos,
							EffectManager.EffectType.LightShort, 2.5f, 0); ;
						Sound.Play(Sound.Sounds.WizardFire);
					} // if (BaseGame.EveryMs)
					// Also make whosh sound
						/*too loud, sounds strange
					else if (BaseGame.EveryMs(-400 + (int)(2 * 1000 * 21.0f / 30.0f)))
					{
						Sound.Play(Sound.Sounds.Whosh);
					} // else if
						 */
				} // if (state)
			} // if (type)
			else
			{
				if ((wannaHaveMonsterPos - BaseGame.camera.PlayerPos).Length() < 1.75f)
				{
					if (state != States.Hit1 &&
						state != States.Hit2)
						state =
							RandomHelper.GetRandomInt(2) == 0 ? States.Hit1 : States.Hit2;
				} // else
				else if ((wannaHaveMonsterPos - BaseGame.camera.PlayerPos).Length() < 15 &&
					hasCried == false)
				{
					hasCried = true;
					if (type == GameManager.AnimatedTypes.Ogre ||
						type == GameManager.AnimatedTypes.BigOgre)
						Sound.Play(Sound.Sounds.OgreCry);
					else
						Sound.Play(Sound.Sounds.GoblinCry);

					// Also alert surrounding monsters that player is here!
					foreach (AnimatedGameObject otherMonster in GameManager.animatedObjects)
						if (Vector3.Distance(wannaHaveMonsterPos,
							otherMonster.positionMatrix.Translation) <= 15.0f &&
							otherMonster.state == States.Idle)
							// Start running too!
							otherMonster.state = States.Run;
				} // else if
				else if ((wannaHaveMonsterPos - BaseGame.camera.PlayerPos).Length() < 20 &&
					(state == States.Idle ||
					(wannaHaveMonsterPos - BaseGame.camera.PlayerPos).Length() > 2.25f))
				{
					if (state == States.Idle)
						Sound.Play(Sound.Sounds.MonsterSteps);

					state = States.Run;
				} // else if
				if (state == States.Run)
				{
					movementVector.Normalize();
					monsterMovement +=
						movementVector * BaseGame.MoveFactorPerSecond *
						// Increase speed of monsters as the level increases
						(2 + Player.level / 4.5f);
				} // if (state)
			} // else

			if (hitpoints <= 0)
			{
				state = States.Die;
				// Give as much points as this guy has hitpoints
				Player.score += (int)GameManager.MonsterHitpoints[(int)type];
				Sound.Play(
					type == GameManager.AnimatedTypes.Ogre ||
					type == GameManager.AnimatedTypes.BigOgre ?
					Sound.Sounds.OgreDie : Sound.Sounds.GoblinDie);

				// If this monster had some item, drop it now!
				if (dropObject != DropObject.None)
				{
					Matrix objectMatrix = Matrix.CreateTranslation(
						positionMatrix.Translation + new Vector3(0, 0, 0.75f));
					GameManager.Add(
						dropObject == DropObject.Club ? GameManager.StaticTypes.Club :
						dropObject == DropObject.Sword ? GameManager.StaticTypes.Sword :
						dropObject == DropObject.BigSword ?
						GameManager.StaticTypes.BigSword :
						GameManager.StaticTypes.Key, objectMatrix);
				} // if (dropObject)
			} // if (hitpoints)

			// If the movement is big enough, update the rotation!
			/*always rotate to player, looks better!
			if (monsterMovement.Length() > 0.0001f)
				wannaHaveMonsterRotation =
					MathHelper.PiOver2 +
					(float)Math.Atan2(monsterMovement.Y, monsterMovement.X);
			 */
			wannaHaveMonsterRotation =
				MathHelper.PiOver2 +
				(float)Math.Atan2(movementVector.Y, movementVector.X);

			// Make sure we got gravity and collision detection
			CaveCollision.UpdatePlayerPosition(
				ref wannaHaveMonsterPos, monsterMovement, true, this);

			monsterRotationZ =
				//Problematic because 0-360, just assign.
				wannaHaveMonsterRotation;
				//monsterRotationZ * 0.925f + wannaHaveMonsterRotation * 0.075f;

			// Always hit at frame 11
			if ((state == States.Hit1 || state == States.Hit2) &&
				type != GameManager.AnimatedTypes.GoblinWizard)
			{
				// Use 49 for animation length for Hit1 and 29 for Hit2,
				// see AnimatedColladaModel.cs:218 for details.
				//int animationLength =
				//	state == AnimatedGameObject.States.Hit1 ? 45 : 29;

				//if ((int)(Player.GameTime * 30) % animationLength == 10)
				if (GetAnimationElapsedTimeFrame(16))//22))
				{
					// Hurt player if he is close.
					Vector3 effectPos = Vector3.Transform(
						new Vector3(0.2f, -0.82f, 1.2f) +
						RandomHelper.GetRandomVector3(-0.3f, 0.3f),
						Matrix.CreateRotationZ(wannaHaveMonsterRotation));
					EffectManager.AddBlood(wannaHaveMonsterPos + effectPos, 0.15f);

					float damage = GameManager.MonsterDamages[(int)type] -
						Player.defenseIncrease;
					// Don't heal
					if (damage < 0)
						damage = 0;
					Player.health -= damage;
					if (damage > 0)
					{
						// Do not always play a player cry
						if (RandomHelper.GetRandomInt(2) == 0)
							Sound.Play(RandomHelper.GetRandomInt(3) == 0 ?
								Sound.Sounds.PlayerWasHit : Sound.Sounds.HitFlesh);

						// Rumble controller!
						Input.GamePadRumble(
							RandomHelper.GetRandomFloat(0.1f, 0.5f),
							RandomHelper.GetRandomFloat(0.2f, 0.75f));
					} // if (damage)
					//don't play, too confusing: else
						// No damage? Then don't cry
						//Sound.Play(Sound.Sounds.HitFlesh);

					GameManager.bloodBorderEffect = 0.5f;
					GameManager.bloodBorderColor = Color.Red;
				} // if (int)
				else if //((int)(Player.GameTime * 30) % animationLength == 11 ||
					//(int)(Player.GameTime * 30) % animationLength == 12)
					(GetAnimationElapsedTimeFrame(8) ||
					GetAnimationElapsedTimeFrame(20))
				{
					Vector3 effectPos = Vector3.Transform(
						new Vector3(0.2f, -0.82f, 1.2f) +
						RandomHelper.GetRandomVector3(-0.3f, 0.3f),
						Matrix.CreateRotationZ(wannaHaveMonsterRotation));
					EffectManager.AddBlood(wannaHaveMonsterPos + effectPos, 0.15f);
				} // else if
				else //if ((int)(Player.GameTime * 30) % animationLength == 2 ||
					// 30 is only used for Hit1, Hit2 has only 29 ani steps
					//(int)(Player.GameTime * 30) % animationLength == 30)
					if (GetAnimationElapsedTimeFrame(1) ||
					GetAnimationElapsedTimeFrame(8) ||
					GetAnimationElapsedTimeFrame(9))
				{
					Sound.Play(Sound.Sounds.Whosh);
				} // else if

				// At end of animation?
				//if ((int)(Player.GameTime * 30) % animationLength == animationLength - 1)
				if (GetAnimationElapsedTimeFrame(-1))
				{
					// Set state back to idle, next frame it will be replaced
					// with a randomly choosen new hit animation!
					state =
						RandomHelper.GetRandomInt(2) == 0 ?
						AnimatedGameObject.States.Hit1 : AnimatedGameObject.States.Hit2;
				} // else if
			} // if (state)

			positionMatrix =
				Matrix.CreateRotationZ(monsterRotationZ) *
				Matrix.CreateScale(sizeVariation) *
				Matrix.CreateTranslation(
				wannaHaveMonsterPos + new Vector3(0, 0, 0.00001f));
		} // UpdateMonsterAI()
		#endregion
	} // class AnimatedGameObject
} // namespace DungeonQuest.Game
