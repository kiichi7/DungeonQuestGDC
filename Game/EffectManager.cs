// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Game
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:38

#region Using directives
using DungeonQuest.Game;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using DungeonQuest.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using Texture = DungeonQuest.Graphics.Texture;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Effect manager. Used in 2 ways: As instance class by DungeonQuestGame
	/// to load and show all used effect textures. And as a static class to
	/// add and manage all new effects (which makes it much easier for external
	/// use).
	/// </summary>
	public class EffectManager
	{
		#region Variables
		private static string[] EffectTextureFilenames = new string[]
			{
				"MgHit",
				"Plasma",
				"Smoke",
				"EnemyRing",
				"LightEffect",
				"FireBall",
				"FireStar",
				"SimpleFlare",
				"BloodSplatter",
				"BloodPoints",
			};
		private Texture[] effectTextures = null;

		/// <summary>
		/// Effect texture types
		/// </summary>
		enum EffectTextureType
		{
			MgHit,
			Plasma,
			Smoke,
			EnemyRing,
			LightEffect,
			FireBall,
			FireStar,
			SimpleFlare,
			BloodSplatter,
			BloodPoints,
		} // enum EffectTextureTypes

		/*obs
		private static string[] ExplosionTextureFilenames = new string[]
			{
				//TODO: fire?
				"BigExplosion",
			};
		private AnimatedTexture[] explosionTextures = null;
		/// <summary>
		/// Explosion texture type
		/// </summary>
		enum ExplosionTextureType
		{
			BigExplosion,
		} // enum ExplosionTextureType
		 */

		/*unused, use Sonuds directly
		public enum EffectSoundType
		{
			Health,
			NewWeapon,
			MgShoot,
			MgHit,
			PlasmaShoot,
			RocketShoot,
			RocketHit,
			GattlingShoot,
			EnemyShoot,
			ShipHit,
			//obs: BigExplosion,
			MgShootEnemy,
			Blood
		} // enum ExplosionTextureType
		 */
		#endregion

		#region Constructor
		/// <summary>
		/// Create effect manager
		/// </summary>
		public EffectManager()
		{
			// Load textures
			effectTextures = new Texture[EffectTextureFilenames.Length];
			for (int num = 0; num < effectTextures.Length; num++)
			{
				effectTextures[num] = new Texture(EffectTextureFilenames[num]);
			} // for (num)

			/*obs
			explosionTextures =
				new AnimatedTexture[ExplosionTextureFilenames.Length];
			for (int num = 0; num < explosionTextures.Length; num++)
			{
				explosionTextures[num] = new AnimatedTexture(
					ExplosionTextureFilenames[num]);
			} // for (num)
			 */
		} // EffectManager()
		#endregion

		#region Effect types
		/// <summary>
		/// Effect types, only used internally. Use the Add methods to
		/// add specific effects, which often contain multiple internal effects.
		/// </summary>
		public enum EffectType
		{
			/// <summary>
			/// Machine Gun Shoot effect. Used for Raptor and Hunter guns.
			/// </summary>
			MgShoot,
			/// <summary>
			/// Machine Gun Hit effect. Also used for MiniGun and added to
			/// big explosions, rotate and use alpha, very small,
			/// also don't play all the time.
			/// </summary>
			MgHit,
			/// <summary>
			/// Plasma effects are used for Mammuts (primary weapon),
			/// they are very similar to the Quake3 Plasma Gun.
			/// </summary>
			Plasma,
			/// <summary>
			/// Same effect as MgShoot, just longer
			/// </summary>
			PlasmaShoot,
			/// <summary>
			/// Fireball effect ala mg shoot.
			/// </summary>
			RocketShoot,
			/// <summary>
			/// Fire ball effect for bombers in game. Slow flying particles.
			/// </summary>
			FireBall,
			/// <summary>
			/// Smoke effect for rockts and ships
			/// </summary>
			Smoke,
			/// <summary>
			/// Flare effect for rockets and ships to indicate burning eginiton.
			/// Basically just a red/yellow spot (heat).
			/// </summary>
			Flare,
			/// <summary>
			/// Big explosion when units are dying.
			/// </summary>
			//obs: BigExplosion,
			/// <summary>
			/// Fire star effect for big explosion.
			/// </summary>
			FireStar,
			/// <summary>
			/// Ring effect for bigger explosions to indicate heavy explosions.
			/// </summary>
			EnemyRing,
			/// <summary>
			/// Light effect (same texture as the one from the Lens flare).
			/// This effect makes the screen more bright, so do this effect
			/// after the other effects (kinda post screen). Looks very good
			/// in combination with glow, burn and explosions.
			/// We get 3 kinds: LightShort for a short burst of light (500 ms),
			/// LightLong (1.5 seconds) for big explosions and stuff,
			/// LightInstant for immediate effects, no fade in/out.
			/// </summary>
			LightVeryShort,
			LightShort,
			LightLong,
			LightInstant,
			/// <summary>
			/// Simple glow effect (same as glow behind items).
			/// Used to indicate enemies+weapons (red) and friends (green).
			/// </summary>
			FriendlyGlow,
			/// <summary>
			/// 2 enums to save storing color values in effect class.
			/// </summary>
			EnemyGlow,
			/// <summary>
			/// Blood effect if we hit someone.
			/// </summary>
			BloodSplatter,
			BloodPoints,
			/// <summary>
			/// No effect, just for assigning no effect to a variable.
			/// </summary>
			None,
		} // enum EffectType

		/// <summary>
		/// Helper list for all effects. Holds the max. time the effect is
		/// active. It will fade in from 0-10% and fade out from 50-100%.
		/// Explosion effects don't fade in, they just fade out at end (80-100%)
		/// </summary>
		static int[] EffectMaxTime = new int[]
			{
				35, // MgShoot,
				100, // MgHit,
				0, // Plasma,
				89, // PlasmaShoot
				250, // RocketShoot
				0, // FireBall,
				1500,//2000,//4250, // Smoke,
				0, // Flare,
				//1800,//2100, // BigExplosion,
				500, // FireStar,
				0,//2000,//2500, // EnemyRing,
				200, // LightVeryShort,
				575,//625,//750, // LightShort,
				1300,//1500, // LightLong,
				0, // LightInstant,
				0, // FriendlyGlow,
				0, // EnemyGlow,
				125, // BloodSplatter,
				200, // BloodPoints,
			};

		/// <summary>
		/// Effect texture nums, links to effectTextures except for the
		/// explosions, which use explosionTextures.
		/// </summary>
		static int[] EffectTextureNum = new int[]
			{
				(int)EffectTextureType.FireStar,
				(int)EffectTextureType.MgHit,
				(int)EffectTextureType.Plasma,
				(int)EffectTextureType.FireStar,
				(int)EffectTextureType.FireStar,
				(int)EffectTextureType.FireBall,
				(int)EffectTextureType.Smoke,
				(int)EffectTextureType.SimpleFlare,
				//(int)ExplosionTextureType.BigExplosion,
				(int)EffectTextureType.FireStar,
				(int)EffectTextureType.EnemyRing,
				(int)EffectTextureType.LightEffect,
				(int)EffectTextureType.LightEffect,
				(int)EffectTextureType.LightEffect,
				(int)EffectTextureType.LightEffect,
				(int)EffectTextureType.LightEffect,
				(int)EffectTextureType.LightEffect,
				(int)EffectTextureType.BloodSplatter,
				(int)EffectTextureType.BloodPoints,
			};
		#endregion

		#region Line Handling
		/*obs
		/// <summary>
		/// Line helper class. Remembers all the lines we added.
		/// </summary>
		class Line
		{
			public Vector3 startPos, endPos;
			public Color startColor, endColor;
			float time, maxTime;
			public int startPosId, endPosId;

			/// <summary>
			/// Create line
			/// </summary>
			/// <param name="setStartPos">Set start position</param>
			/// <param name="setEndPos">Set end position</param>
			/// <param name="setColor">Set color</param>
			/// <param name="setMaxTime">Set maximum time</param>
			public Line(Vector3 setStartPos, Vector3 setEndPos,
				Color setStartColor, Color setEndColor,
				int setMaxTime, int setStartPosId, int setEndPosId)
			{
				startPos = setStartPos;
				endPos = setEndPos;
				startColor = setStartColor;
				endColor = setEndColor;
				time = 0;
				maxTime = setMaxTime;
				startPosId = setStartPosId;
				endPosId = setEndPosId;
			} // Line(setStartPos, setEndPos, setColor)

			/// <summary>
			/// Render
			/// </summary>
			/// <returns>Returns true if time is over and line should be removed
			/// from the lines list.</returns>
			public bool Render()
			{
				time += BaseGame.ElapsedTimeThisFrameInMs;// ElapsedTime.MsLastFrame;
				if (time >= maxTime)
					return true;

				BaseGame.DrawLine(startPos, endPos,
					// Looks better than with alpha fading
					startColor, endColor);
				//Color.FromArgb(
				//(byte)(col.A * (1.0f - ((float)time / (float)maxTime))), col));
				return false;
			} // Render()
		} // class Line
		static List<Line> lines = new List<Line>();

		/// <summary>
		/// Add line from startPoint to endPoint with specific color.
		/// Used for MG effects (randomly every 10 shoots), alpha lines
		/// with dark yellow colors (see WeaponManager).
		/// Lines are always rendered instantly and fade out quickly.
		/// </summary>
		/// <param name="startPoint">Start point</param>
		/// <param name="endPoint">End point</param>
		/// <param name="col">Color</param>
		private static void AddLine(Vector3 startPoint, Vector3 endPoint,
			Color startColor, Color endColor, int startPosId, int endPosId)
		{
			lines.Add(new Line(startPoint, endPoint,
				startColor, endColor, 100, startPosId, endPosId));
		} // AddLine(startPoint, endPoint, col)
		 */
		#endregion

		#region Effect helper class
		/// <summary>
		/// Effect
		/// </summary>
		class Effect
		{
			#region Variables
			/// <summary>
			/// Position of effect, moving is not supported, use instant effects
			/// for moving stuff (e.g. weapon projectiles).
			/// </summary>
			public Vector3 pos;
			/// <summary>
			/// Size and endSize, usually the same, but some effects like
			/// ExplosionRing will expand over time (from 1.0f to 50.0f very quickly)
			/// </summary>
			public float size, endSize;
			/// <summary>
			/// Effect type, see above.
			/// </summary>
			public EffectType type;
			/// <summary>
			/// Time and max time, if time reaches maxTime, the effect is over.
			/// </summary>
			public float time, maxTime;
			/// <summary>
			/// Rotation, set to a specific value for more different looks of
			/// same effects. Will stay rotated this way. Most effects just use 0.
			/// But plasma and fireball effects look much better rotated this way :)
			/// </summary>
			public float rotation;
			/// <summary>
			/// Color to colorizise effects, often used to darken down or
			/// make effect more transparent. Sometimes we color effects like
			/// glow or light to indicate player colors (or enemy/friendly status).
			/// </summary>
			public Color color;
			/// <summary>
			/// Id for this effect, used to update stuff.
			/// </summary>
			public int id;

			/// <summary>
			/// Helpers for RenderBillboardOnGround, used for Ring effect.
			/// This allows us to rotate the ring effect in space.
			/// </summary>
			public Vector3 vecGroundRight, vecGroundUp;
			#endregion

			#region Properties
			/// <summary>
			/// Is light effect
			/// </summary>
			/// <returns>Bool</returns>
			public bool IsLightEffect
			{
				get
				{
					return type == EffectType.LightVeryShort ||
						type == EffectType.LightShort ||
						type == EffectType.LightLong ||
						type == EffectType.LightInstant;
				} // get
			} // IsLightEffect

			/*obs
			/// <summary>
			/// Is explosion
			/// </summary>
			/// <returns>Bool</returns>
			public bool IsExplosion
			{
				get
				{
					return type == EffectType.BigExplosion;
				} // get
			} // IsExplosion
			 */

			/// <summary>
			/// Is ring
			/// </summary>
			/// <returns>Bool</returns>
			public bool IsRing
			{
				get
				{
					return type == EffectType.EnemyRing;
				} // get
			} // IsRing
			#endregion

			#region Constructor
			/// <summary>
			/// Create effect
			/// </summary>
			/// <param name="setPos">Set position</param>
			/// <param name="setSize">Set size</param>
			/// <param name="setType">Set type</param>
			public Effect(Vector3 setPos, float setSize, EffectType setType,
				int setId)
			{
				position =
				pos = setPos;
				size = endSize = setSize;
				type = setType;
				time = 0;
				maxTime = EffectMaxTime[(int)type];
				rotation = 0;
				color = Color.White;
				id = setId;
			} // Effect(setPos, setSize, setType)
			#endregion

			#region Distance
			/// <summary>
			/// Helper for calculating distance.
			/// </summary>
			public float distance;
			//*obs
			/// <summary>
			/// Also precalculate position here, will be changed for light effects
			/// to add little offset to fix z buffer errors.
			/// </summary>
			private Vector3 position;
			//*/
			/// <summary>
			/// Calc distance, will be called before sorting (it is faster to
			/// calculate it only once and then compare multiple times).
			/// </summary>
			public void CalcDistance(Vector3 cameraPos)
			{
				//*obs
				// First get position and offset it for lights (always on top)
				position = pos;
				// Reduce position nearer to camera for light effects.
				if (IsLightEffect)
				{
					Vector3 distVector = pos - cameraPos;
					distVector.Normalize();
					position -= distVector * 2.0f;// 2.5f;
				} // if (IsLightEffect)
				//*/

				distance = (position - cameraPos).Length();
			} // CalcDistance(cameraPos)
			#endregion

			#region Render
			/// <summary>
			/// Render
			/// </summary>
			/// <returns>True if effect is at end and has to be removed</returns>
			public bool Render(Vector3 camPos, EffectManager effectManager)
			{
				// Max. distance for visible effects.
				const float MaxDistance = BaseGame.FarPlane;// *0.66f;

				// Calculate current size
				float timePercent = maxTime == 0 ? 0 : (float)time / (float)maxTime;
				float currentSize = size + (endSize - size) * timePercent;

				// Calculate alpha, always 100% for instant effects.
				// Else it will fade in from 0-10% and fade out from 75-100%.
				// Explosion effects don't fade in, they just fade out at end (80-100%)
				Color currentColor = color;

				/*obs
				if (IsExplosion)
				{
					if (timePercent > 0.8f)
						currentColor = ColorHelper.FromArgb((byte)
							(color.A * (1.0f - (timePercent - 0.8f) * 5.0f)), color);
					AnimatedTexture aniTex =
						effectManager.explosionTextures[EffectTextureNum[(int)type]];
					//obs: aniTex.Select((int)(timePercent * aniTex.AnimationLength));
					Billboard.Render(aniTex,
						(int)(timePercent * aniTex.AnimationLength),
						Billboard.BlendMode.NormalAlphaBlending,
						position, currentSize, rotation, currentColor);
				} // if (IsExplosion)
				else
				 */
				{
					float alpha = 1.0f;
					if (IsLightEffect)
					{
						// Don't change color, but change size
						if (timePercent > 0.5f)
							currentSize = currentSize *
								(1.0f - (timePercent - 0.5f) * 1.9f);
					} // if (IsLightEffect)
					else if (maxTime > 0 &&
						timePercent < 0.1f)
						alpha = timePercent * 10.0f;
					else if (type == EffectType.Smoke)
						alpha = 1.0f - (timePercent - 0.1f) / 0.9f;
					else if (timePercent > 0.75f)
						alpha = 1.0f - (timePercent - 0.75f) * 4.0f;

					// Fade out if really far away!
					if (distance > MaxDistance * 0.75f)
						alpha *= 1.0f -
							((distance - MaxDistance * 0.75f) / (MaxDistance * 0.25f));

					if (alpha != 1.0f)
					{
						if (alpha > 1.0f)
							alpha = 1.0f;
						if (alpha < 0.0f)
							alpha = 0.0f;

						currentColor = ColorHelper.FromArgb((byte)(color.A * alpha), color);
					} // if (alpha)

					//obs:
					//effectManager.effectTextures[EffectTextureNum[(int)type]].Select();
					if (IsRing)
						Billboard.RenderOnGround(
							effectManager.effectTextures[EffectTextureNum[(int)type]],
							position, currentSize, rotation, currentColor,
							vecGroundRight, vecGroundUp);
					else if (IsLightEffect)
						Billboard.Render(
							effectManager.effectTextures[EffectTextureNum[(int)type]].
							XnaTexture, Billboard.BlendMode.LightEffect,
							position, currentSize, currentColor);
					else
						Billboard.Render(
							effectManager.effectTextures[EffectTextureNum[(int)type]],
							position, currentSize, rotation, currentColor);
				} // else

				// Increase time
				time += BaseGame.ElapsedTimeThisFrameInMs;//.MsLastFrame;
				// Quit if time is over, also quit if out of visible area!
				if (time >= maxTime ||
					distance > MaxDistance)
					return true;

				return false;
			} // Render()
			#endregion
		} // class Effect

		static List<Effect> effects = new List<Effect>();

		public static int NumberOfEffects
		{
			get
			{
				return effects.Count;
			} // get
		} // NumberOfEffects

		/// <summary>
		/// Compare effects for effects.Sort in HandleAllEffects.
		/// </summary>
		/// <param name="effect1">Effect 1</param>
		/// <param name="effect2">Effect 2</param>
		/// <returns>Int</returns>
		private static int CompareEffects(Effect effect1, Effect effect2)
		{
			// Uncompareable because x or y is invalid, don't change stuff!
			if (effect1 == null || effect2 == null)
				return 0;
			else if (effect1.distance == effect2.distance)
				return 0;
			else if (effect1.distance < effect2.distance)
				return 1;
			else
				return -1;
		} // CompareEffects(effect1, effect2)
		#endregion

		#region Add single effect
		static int offsetCounter = 0;
		/// <summary>
		/// Add effect at a specific position.
		/// Effects are either one-frame-only (e.g. plasma) or are animated and
		/// fade out after a while (explosions, smoke). Usually effects get
		/// bigger when they fade out (ring effect). Most effects are for
		/// big weapons like rockets or explosions when killing something.
		/// </summary>
		/// <param name="position">Position</param>
		/// <param name="type">Effect type</param>
		/// <param name="scaleFactor">Additional scale factor, usually
		/// 1.0f, but sometimes we want bigger smokes or explosions.</param>
		/// <param name="useRotationForPlasmaAndFireball">Use this rotation</param>
		/// <param name="setId">Set id</param>
		public static void AddEffect(Vector3 position, EffectType type,
			float scaleFactor, float useRotationForPlasmaAndFireball, int setId)
		{
			Effect effect = new Effect(
				position,
				scaleFactor,
				type,
				setId);
			
			// Add little offset for effects to prevent painting them on top of each
			// other and producing polygon clipping errors.
			offsetCounter = (offsetCounter + 1) % 18;
			effect.pos += new Vector3(0.004f, 0.006f, 0.008f) * offsetCounter;

			// All additonal sizes, timeouts, etc. are hardcoded.
			if (type == EffectType.Smoke)
			{
				effect.endSize *= 2.0f;
				// Randomize smoke position a bit
				//already done:
				//effect.pos += RandomHelper.GetRandomVector3(-1.35f, +1.35f);
				// Use very low alpha for smoke effects (put many on top of each other)
				effect.color = ColorHelper.FromArgb(40, effect.color);
			} // if (type)
			else if (type == EffectType.Plasma ||
				type == EffectType.FireBall)
			{
				effect.rotation = useRotationForPlasmaAndFireball;
				effect.color = ColorHelper.FromArgb(220, 190, 190, 190);
			} // else if
			else if (type == EffectType.FireStar ||
				type == EffectType.MgShoot ||
				type == EffectType.MgHit ||
				type == EffectType.PlasmaShoot)
			{
				effect.rotation = RandomHelper.GetRandomFloat(
					0, (float)Math.PI * 2.0f);
				effect.endSize *= 1.7f;
				effect.color = ColorHelper.FromArgb(155, 255, 255, 255);
			} // else if
			else if (type == EffectType.BloodSplatter)
			{
				effect.endSize *= 15;// 25.0f;
				effect.color = ColorHelper.FromArgb(100, 255, 255, 255);
			} // else if
			else if (type == EffectType.BloodPoints)
			{
				effect.endSize *= 4.0f;
				effect.color = ColorHelper.FromArgb(100, 255, 255, 255);
			} // else if
			else if (type == EffectType.EnemyRing)
			{
				//ignored: effect.endSize *= 15.0f;
				effect.color = new Color(255, 255, 128, 80);//128);
				// Randomize rotation for ring
				Matrix rotMatrix = Matrix.Identity;
					/*obsMatrix.CreateRotationX(
					RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f)) *
					Matrix.CreateRotationY(
					RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f)) *
					Matrix.CreateRotationZ(
					RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
													  */
				effect.vecGroundRight = Vector3.TransformNormal(
					Billboard.vecGroundRight, rotMatrix);
				effect.vecGroundUp = Vector3.TransformNormal(
					Billboard.vecGroundUp, rotMatrix);

				effect.rotation = Player.GameTime / 1.496f;
			} // else if
			//obs: else if (effect.IsExplosion)
			//	effect.endSize *= 1.3f;
			else if (effect.IsLightEffect)
			{
				effect.endSize *= 1.6f;
				effect.color = Color.White;// Color.FromArgb(255, 255, 255, 255);
			} // else if
			else if (type == EffectType.FriendlyGlow ||
				type == EffectType.EnemyGlow)
			{
				// Put glow always behind objects, not in front
				Vector3 distVector = effect.pos - BaseGame.CameraPos;
				distVector.Normalize();
				effect.pos += distVector * 10.0f;

				if (type == EffectType.FriendlyGlow)
					effect.color = ColorHelper.FromArgb(120, 100, 255, 100);
				else
					effect.color = ColorHelper.FromArgb(160, 255, 40, 40);
			} // else if

			// And finally add to effects list
			effects.Add(effect);
		} // AddEffect(position, type)
		
		/// <summary>
		/// Add effect at a specific position.
		/// Effects are either one-frame-only (e.g. plasma) or are animated and
		/// fade out after a while (explosions, smoke). Usually effects get
		/// bigger when they fade out (ring effect). Most effects are for
		/// big weapons like rockets or explosions when killing something.
		/// </summary>
		/// <param name="position">Position</param>
		/// <param name="type">Effect type</param>
		/// <param name="scaleFactor">Additional scale factor, usually
		/// 1.0f, but sometimes we want bigger smokes or explosions.</param>
		/// <param name="useRotationForPlasmaAndFireball">Use this rotation</param>
		/// <param name="setId">Set id</param>
		public static void AddEffect(Vector3 position, EffectType type,
			float scaleFactor, float useRotationForPlasmaAndFireball)
		{
			AddEffect(position, type, scaleFactor, useRotationForPlasmaAndFireball,
				-1);
		} // AddEffect(position, type, scaleFactor)
		#endregion

		#region Play sound effects
		/*unused
		/// <summary>
		/// Play sound effect, adds 3d sound effect depending on the position.
		/// </summary>
		/// <param name="soundType">Sound type</param>
		public static void PlaySoundEffect(EffectSoundType soundType)
		{
			Sound.Play(soundType.ToString());
		} // PlaySoundEffect(soundType, volume)
		 */
		#endregion

		#region Add effect helper methods
		#region AddMgEffect
		/*obs
		static readonly Color MgLineStartColor = ColorHelper.FromArgb(255, 206, 180, 73);
		static readonly Color MgLineEndColor = ColorHelper.FromArgb(255, 126, 110, 53);

		/// <summary>
		/// Add mg effect.
		/// First we will render a line from startPos to endPos in MgLineColor.
		/// Then we also display a startPos effect (small fire star and light)
		/// and an endPos effect (mg hit effect and light).
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		/// <param name="startPosId">Start position id</param>
		/// <param name="endPosId">End position id</param>
		/// <param name="showTargetEffects">Show target effects only if
		/// we hit something, don't show if shoot goes out of the screen.</param>
		public static void AddMgEffect(Vector3 startPos, Vector3 endPos,
			int startPosId, int endPosId,
			bool showTargetEffects, bool playSound)
		{
			//obs: AddLine(startPos, endPos,
			//	MgLineStartColor, MgLineEndColor,
			//	startPosId, endPosId);

			// Play sound effect, but not always and not so loud (we play many)
			if (RandomHelper.GetRandomInt(4) == 0 &&
				BaseGame.EveryMs(50) ||
				playSound)
				PlaySoundEffect(EffectSoundType.MgShoot);

			//if (BaseGame.EveryMs(150))
			//{
				AddEffect(startPos, EffectType.MgShoot, 2.25f, 0.0f, startPosId);
				AddEffect(startPos, EffectType.LightShort, 3.0f, 0.0f, startPosId);
			//} // if (BaseGame.EveryMs)

			if (showTargetEffects)
			{
				if (RandomHelper.GetRandomInt(5) < 3)
					PlaySoundEffect(EffectSoundType.MgHit);

				AddEffect(endPos, EffectType.MgHit, 4.0f, 0.0f, endPosId);
				AddEffect(endPos, EffectType.LightVeryShort, 6.0f, 0.0f, endPosId);
			} // if (showTargetEffects)
		} // AddMgEffect(startPos, endPos)

		/// <summary>
		/// Add gattling effect.
		/// First we will render a line from startPos to endPos in MgLineColor.
		/// Then we also display a startPos effect (small fire star and light)
		/// and an endPos effect (mg hit effect and light).
		/// </summary>
		/// <param name="startPos">Start position</param>
		/// <param name="endPos">End position</param>
		/// <param name="startPosId">Start position id</param>
		/// <param name="endPosId">End position id</param>
		/// <param name="showTargetEffects">Show target effects only if
		/// we hit something, don't show if shoot goes out of the screen.</param>
		public static void AddGattlingEffect(Vector3 startPos, Vector3 endPos,
			int startPosId, int endPosId,
			bool showTargetEffects, bool playSound)
		{
			//obs: AddLine(startPos, endPos,
			//	MgLineStartColor, MgLineEndColor,
			//	startPosId, endPosId);

			// Play sound effect, but not always and not so loud (we play many)
			if (RandomHelper.GetRandomInt(4) == 0 &&
				BaseGame.EveryMs(150) ||
				playSound)
				PlaySoundEffect(EffectSoundType.GattlingShoot);

			if (RandomHelper.GetRandomInt(3) == 0)//BaseGame.EveryMs(150))
			{
				AddEffect(startPos, EffectType.MgShoot, 2.5f, 0.0f, startPosId);
				AddEffect(startPos, EffectType.LightShort, 3.5f, 0.0f, startPosId);
			} // if (BaseGame.EveryMs)

			if (showTargetEffects)
			{
				if (RandomHelper.GetRandomInt(5) < 3)
					PlaySoundEffect(EffectSoundType.MgHit);

				AddEffect(endPos, EffectType.MgHit, 7.0f, 0.0f, endPosId);
				AddEffect(endPos, EffectType.LightVeryShort, 8.0f, 0.0f, endPosId);
			} // if (showTargetEffects)
		} // AddGattlingEffect(startPos, endPos)

		/// <summary>
		/// Update mg effect
		/// </summary>
		/// <param name="updatedPos">Updated position</param>
		/// <param name="id">Id</param>
		public static void UpdateMgEffect(Vector3 updatedPos, int id)
		{
			//foreach (Line line in lines)
			for (int num=0; num<lines.Count; num++)
			{
				Line line = lines[num];
				if (line.startPosId == id)
					line.startPos = updatedPos;
				if (line.endPosId == id)
					line.endPos = updatedPos;
			} // foreach (line)

			//foreach (Effect effect in effects)
			for (int num = 0; num < effects.Count; num++)
			{
				Effect effect = effects[num];
				if (effect.id == id)
					effect.pos = updatedPos;
			} // for
		} // UpdateMgEffect(updatedPos, id)

		/*
		/// <summary>
		/// Update effect
		/// </summary>
		/// <param name="updatedPos">Updated position</param>
		/// <param name="id">Id</param>
		public static void UpdateEffect(Vector3 updatedPos, int id)
		{
			foreach (Effect effect in effects)
				if (effect.id == id)
					effect.pos = updatedPos;
		} // UpdateEffect(updatePos, id)
		 */
		#endregion

		#region AddFireFlash
		public static void AddFireFlash(Vector3 pos)
		{
			AddEffect(pos, EffectType.MgShoot, 2.5f, 0.0f, -1);
			AddEffect(pos, EffectType.LightLong, 5.0f, 0.0f, -1);
		} // AddFireFlash(pos)
		#endregion

		#region AddPlasmaEffect
		/// <summary>
		/// Add plasma effect.
		/// Shows plasma ball and light effect (both instant only).
		/// </summary>
		/// <param name="position">Position</param>
		/// <param name="size">Size</param>
		public static void AddPlasmaEffect(Vector3 pos,
			float rotation, float size)
		{
			AddEffect(pos, EffectType.Plasma, size, rotation);
			AddEffect(pos, EffectType.LightInstant, size * 2.5f, 0.0f);
		} // AddPlasmaEffect(position)
		#endregion

		#region AddFireBallEffect
		/// <summary>
		/// Add fire ball effect.
		/// Shows fire ball and light effect (both instant only).
		/// Also adds small smoke behind fireball (every 1/10 seconds).
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		public static void AddFireBallEffect(Vector3 pos,
			float rotation, float size)
		{
			AddEffect(pos, EffectType.FireBall, size, rotation);
			AddEffect(pos, EffectType.LightInstant,
				//size * 2.5f, 0.0f);
				// Make really big ^^
				//Update: Make small, else way too big effect
				size * 1.0f, 0.0f);

			if (BaseGame.EveryMs(125))//75))
			{
				AddEffect(
					pos + RandomHelper.GetRandomVector3(-size * 0.275f, +size * 0.275f),
					EffectType.Smoke, size,
					RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
			} // if (BaseGame.EveryMs)
		} // AddFireBallEffect(pos, rotation, size)

		/// <summary>
		/// Add light and smoke
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="size">Size</param>
		public static void AddSmoke(Vector3 pos, float size)
		{
			if (BaseGame.EveryMs(125))//75))
			{
				AddEffect(
					pos + RandomHelper.GetRandomVector3(-size * 0.275f, +size * 0.275f),
					EffectType.Smoke, size,
					RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
			} // if (BaseGame.EveryMs)
		} // AddLightAndSmoke(pos, size)
		#endregion

		#region AddRocketOrShipFlareAndSmoke
		/// <summary>
		/// Add rocket or ship flare and smoke.
		/// First we show an instant flare effect at pos, then we add a
		/// smoke effect every 1/10 seconds.
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		/// <param name="speed">Speed of unit per second,
		/// very important for determinting  how much smoke we should generate.
		/// </param>
		public static void AddRocketOrShipFlareAndSmoke(
			Vector3 pos, float size, float speed)
		{
			AddEffect(pos, EffectType.Flare, size, 0);
			AddEffect(pos, EffectType.LightInstant, size, 0);
			// Raptor speed: 220 -> smoke: 60ms
			// Lizard rocket speed: 250 -> smoke: 50ms
			// Mammut speed: 150 -> smoke: 100ms
			// Carrier speed: 20 -> smoke: >500ms
			// Formular: (1/speed)*1000*15 ms
			int msToCheck = (int)(speed > 0 ?
				(1 / speed) * 1000 * 15 : 1000.0f);
			// Don't go above 500, looks bad
			if (msToCheck > 500)
				msToCheck = 500;
			//*tst disabling
			if (BaseGame.EveryMs(msToCheck))//75))
			{
				AddEffect(
					pos + RandomHelper.GetRandomVector3(-size * 0.275f, +size * 0.275f),
					EffectType.Smoke, size,// 1.5f,
					RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
			} // if (BaseGame.EveryMs)
			//*/
		} // AddRocketOrShipFlareAndSmoke(pos, size)
		#endregion

		#region AddExplosion
		/// <summary>
		/// Add explosion, will use small or big explosion and several other
		/// effects depending on the size (small explosions only use
		/// SmallExplosion and a little light effect, big explosions use
		/// BigExplosion, ExplosionRing, Light effects, MgHit, FireStar and Smoke).
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		public static void AddExplosion(Vector3 pos, float size)
		{
			/*TODO
			if (size <= 8 &&
				RandomHelper.GetRandomInt(2) == 0)
			{
				AddEffect(pos, EffectType.SmallExplosion, size,
					RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
				AddEffect(pos, EffectType.LightLong, size * 1.75f, 0);
				PlaySoundEffect(EffectSoundType.Explosion);
			} // if (size)
			else
			{
				// Skip this big explosion if already 4 or more are active,
				// too many big explosions look weird and cause problems
				// for too much overlapping graphic and sound effects.
				int numOfBigExplosions = 0;
				foreach (Effect effect in effects)
					if (effect.type == EffectType.BigExplosion)
						numOfBigExplosions++;
				if (numOfBigExplosions >= 4)
					return;

				AddEffect(pos, EffectType.BigExplosion, size,
					RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
				if (size > 6)
					AddEffect(pos, EffectType.ExplosionRing, size / 6.0f, 0);
				else
					AddEffect(pos, EffectType.ExplosionRing, size / 8.0f, 0);

				AddEffect(pos, EffectType.LightLong, size * 2.5f, 0);
				PlaySoundEffect(size > 9 ?
					EffectSoundType.BigExplosion :
					EffectSoundType.Explosion);
			} // else

			for (int i = 0; i < (int)size/2; i++)
			{
				Vector3 offset = RandomHelper.GetRandomVector3(-size, +size);
				AddEffect(pos + offset, EffectType.Smoke, size / 2.66f, 0);
			} // for (int)
	*/
		} // AddExplosion(pos, size)

		/// <summary>
		/// Add hit explosion
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		public static void AddHitExplosion(Vector3 pos, float size)
		{
			/*TODO
			AddEffect(pos, EffectType.SmallExplosion, size,
				RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
			AddEffect(pos, EffectType.LightLong, size * 1.75f, 0);
			PlaySoundEffect(EffectSoundType.Explosion);

			for (int i = 0; i < (int)size / 4; i++)
			{
				Vector3 offset = RandomHelper.GetRandomVector3(-size, +size);
				AddEffect(pos + offset, EffectType.Smoke, size / 1.66f, 0);
			} // for (int)
			 */
		} // AddHitExplosion(pos, size)
		#endregion

		#region AddFlameExplosion
		/// <summary>
		/// Add flame explosion for the lizard shoot, high splash value.
		/// </summary>
		/// <param name="pos">Position</param>
		public static void AddFlameExplosion(Vector3 pos, bool playSound)
		{
			/*TODO
			AddEffect(pos, EffectType.FlameExplosion, 2.5f, 0);
			AddEffect(pos, EffectType.LightLong, 5.5f, 0);
			if (playSound)
				PlaySoundEffect(EffectSoundType.RocketHit);

			float size = 8;// 120;//40;
			for (int i = 0; i < 2; i++)
			{
				Vector3 offset = RandomHelper.GetRandomVector3(-size/2, +size/2);
				AddEffect(pos + offset, EffectType.FlameExplosion, size / 4.5f, 0);
			} // for (int)

			for (int i = 0; i < 2; i++)
			{
				Vector3 offset = RandomHelper.GetRandomVector3(-size, +size);
				AddEffect(pos + offset, EffectType.Smoke, size / 2.85f, 0);//1.66f, 0);
			} // for (int)
			 */
		} // AddFlameExplosion(pos)
		
		/// <summary>
		/// Add flame explosion for the lizard shoot, high splash value.
		/// </summary>
		/// <param name="pos">Position</param>
		public static void AddFlameExplosion(Vector3 pos)
		{
			AddFlameExplosion(pos, true);
		} // AddFlameExplosion(pos)
		#endregion

		#region AddBlood
		/// <summary>
		/// Add blood
		/// </summary>
		public static void AddBlood(Vector3 pos, float size)
		{
			/*too strong
			EffectManager.AddEffect(
				pos,
				EffectManager.EffectType.LightShort,
				size, 0);
			 */
			EffectManager.AddEffect(
				pos,
				EffectManager.EffectType.BloodPoints,
				size, RandomHelper.GetRandomFloat(0, MathHelper.Pi*2));
			EffectManager.AddEffect(
				pos,
				EffectManager.EffectType.BloodSplatter,
				size, 0);
		} // AddBlood()
		#endregion

		#region AddEnemyRing
		/// <summary>
		/// Add enemy ring
		/// </summary>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		public static void AddEnemyRing(Vector3 pos, float size)
		{
			EffectManager.AddEffect(
				pos,
				EffectManager.EffectType.LightInstant,
				size, 0);
			EffectManager.AddEffect(
				pos,
				EffectManager.EffectType.EnemyRing,
				size, 0);
		} // AddEnemyRing(pos, size)
		#endregion
		#endregion

		#region Handle all effects
		/// <summary>
		/// Handle all effects
		/// </summary>
		public void HandleAllEffects()
		{
			/*obs
			// First draw lines
			List<Line> linesToRemove = new List<Line>();
			foreach (Line line in lines)
				if (line.Render())
					linesToRemove.Add(line);

			// Now remove all lines we don't need anymore
			foreach (Line lineToBeRemoved in linesToRemove)
				lines.Remove(lineToBeRemoved);

			// Render all lines. Not done automatically by manager because of
			// z buffer issues and we want to paint over the lines.
			//obs: BaseGame.FlushLineManager3D();
			 */

			Vector3 camPos = BaseGame.CameraPos;
			/*
			// Sort effects by distance! First calculate all distances
			foreach (Effect effect in effects)
				effect.CalcDistance(camPos);

			// Sort whole effects list based on the distances
			effects.Sort(CompareEffects);
			 */

			// Use temp list for removing, then remove after the foreach loop.
			List<Effect> remEffectsToRemove = new List<Effect>();
			//foreach (Effect effect in effects)
			for (int num=0; num<effects.Count; num++)
			{
				Effect effect = effects[num];
				if (effect.Render(camPos, this))
					remEffectsToRemove.Add(effect);
			} // foreach (effect)

			// Finish
			Billboard.RenderBillboards();

			// Remove elements that returned true in Render.
			//foreach (Effect effectToRemove in remEffectsToRemove)
			for (int num=0; num<remEffectsToRemove.Count; num++)
				effects.Remove(remEffectsToRemove[num]);
		} // HandleAllEffects()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Effect tests
		/// </summary>
		public static void TestEffects()
		{
			TestGame.Start(
				"TestEffects",
				delegate
				{
				},
				delegate
				{
					//GraphicForm.LineManager3D.AddLine(
					//  new Vector3(0, 0, 0), new Vector3(100, 100, 100),
					//  Color.White);

					BaseGame.Device.Clear(Color.DarkBlue);

					if (Input.Keyboard.IsKeyDown(Keys.D1))
						EffectManager.AddEffect(
							new Vector3(0, 0, 0),
							EffectManager.EffectType.LightInstant,
							1.0f, 0);

					if (Input.KeyboardKeyJustPressed(Keys.D2))
					{
						EffectManager.AddBlood(Vector3.Zero, 0.2f);
					} // if (Input.Keyboard.IsKeyDown)

					/*obs
					// Press 1-0 for creating effects (in center of scene)
					if (Input.Keyboard.IsKeyDown(Keys.D1) &&
						BaseGame.EveryMs(200))
						AddMgEffect(
							new Vector3(-10.0f, 0, -10),
							new Vector3((BaseGame.TotalTimeMs % 3592) / 100.0f, 25, +100),
							0, 1, true, true);//0, 1, true, true, true);

					if (Input.Keyboard.IsKeyDown(Keys.D2))
					{
						AddPlasmaEffect(new Vector3(-50.0f, 0.0f, 0.0f), 0.5f, 5);
						AddPlasmaEffect(new Vector3(0.0f, 0.0f, 0.0f), 1.5f, 5);
						AddPlasmaEffect(new Vector3(50.0f, 0.0f, 0.0f), 0.0f, 5);
					} // if (Input.Keyboard.IsKeyDown(Keys.D2])

					if (Input.Keyboard.IsKeyDown(Keys.D3))
					{
						AddFireBallEffect(new Vector3(-50.0f, +10.0f, 0.0f), 0.0f, 10);
						AddFireBallEffect(new Vector3(0.0f, +10.0f, 0.0f),
							(float)Math.PI / 8, 10);
						AddFireBallEffect(new Vector3(50.0f, +10.0f, 0.0f),
							(float)Math.PI * 3 / 8, 10);
					} // if (Input.Keyboard.IsKeyDown(Keys.D3])

					if (Input.Keyboard.IsKeyDown(Keys.D4))
						AddRocketOrShipFlareAndSmoke(
							new Vector3((BaseGame.TotalTimeMs % 4000) / 40.0f, 0, 0),
							5.0f, 150.0f);

					if (Input.Keyboard.IsKeyDown(Keys.D5) &&
						BaseGame.EveryMs(1000))
						AddExplosion(Vector3.Zero, 9.0f);
					if (Input.Keyboard.IsKeyDown(Keys.D6) &&
						BaseGame.EveryMs(1500))
						AddExplosion(Vector3.Zero, 18.0f);
					if (Input.Keyboard.IsKeyDown(Keys.D7) &&
						BaseGame.EveryMs(2000))
						AddExplosion(Vector3.Zero, 32.0f);
					if (Input.Keyboard.IsKeyDown(Keys.D8) &&
					  BaseGame.EveryMs(1500))
						AddFlameExplosion(Vector3.Zero);
					
					// Play a couple of sound effects
					if (Input.Keyboard.IsKeyDown(Keys.P) &&
						BaseGame.EveryMs(500))
						PlaySoundEffect(EffectSoundType.PlasmaShoot);
					if (Input.Keyboard.IsKeyDown(Keys.F) &&
						BaseGame.EveryMs(500))
						PlaySoundEffect(EffectSoundType.EnemyShoot);
					if (Input.Keyboard.IsKeyDown(Keys.B) &&
						BaseGame.EveryMs(250))
						PlaySoundEffect(EffectSoundType.NewWeapon);
					if (Input.Keyboard.IsKeyDown(Keys.L) &&
						BaseGame.EveryMs(1500))
						PlaySoundEffect(EffectSoundType.RocketShoot);
					if (Input.Keyboard.IsKeyDown(Keys.H) &&
						BaseGame.EveryMs(500))
						PlaySoundEffect(EffectSoundType.RocketHit);
					if (Input.Keyboard.IsKeyDown(Keys.T) &&
						BaseGame.EveryMs(500))
						PlaySoundEffect(EffectSoundType.Health);
					 */

					// We have to render the effects ourselfs because
					// it is usually done in DungeonQuestGame!
					// Finally render all effects (before applying post screen effects)
					BaseGame.effectManager.HandleAllEffects();

					TextureFont.WriteText(0, 30, 
						"totalTimeMs="+BaseGame.TotalTimeMs);
				});
		} // TestEffects()
#endif
		#endregion
	} // class EffectManager
} // namespace DungeonQuest.Game
