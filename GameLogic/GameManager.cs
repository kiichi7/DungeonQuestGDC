// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\GameLogic
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:39

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
using DungeonQuest.Shaders;
#endregion

namespace DungeonQuest.GameLogic
{
	/// <summary>
	/// Game manager
	/// </summary>
	public class GameManager
	{
		#region Types enum
		/// <summary>
		/// Animated types
		/// </summary>
		public enum AnimatedTypes
		{
			// Player
			Hero,
			// Monsters
			Goblin,
			GoblinMaster,
			GoblinWizard,
			Ogre,
			BigOgre,
		} // enum AnimatedTypes

		/// <summary>
		/// Static Types
		/// </summary>
		public enum StaticTypes
		{
			// Static geometry
			Flare,
			DoorWall,
			Door,
			Treasure,
			// Note: We can collect the key, club, sword and big sword!
			Key,
			// Weapons
			Club,
			Sword,
			BigSword,
		} // enum StaticTypes
		#endregion

		#region Health, Damage, etc. settings
		/// <summary>
		/// Monster hitpoints
		/// </summary>
		public static readonly float[] MonsterHitpoints = new float[]
			{
				100, // Hero (unused, just to match enum)
				10,//10, // Goblin
				15,//15, // GoblinMaster
				8,//5,  // GoblinWizard,
				32,//30, // Ogre
				60,//50, // BigOgre
			};

		/// <summary>
		/// Monster damages
		/// </summary>
		public static readonly float[] MonsterDamages = new float[]
			{
				10, // Hero (unused, just to match enum)
				5,//6,//4, // Goblin
				7,//8,//6, // GoblinMaster
				0,//not used here: 10, // GoblinWizard,
				9,//10,//8, // Ogre
				15,//16,//14, // BigOgre
			};

    /// <summary>
    /// Monster weapon drop percentages
    /// </summary>
		public static readonly float[,] MonsterWeaponDropPercentages = new float[,]
			{
				{0, 0, 0}, // Hero (unused, just to match enum)
				{0.14f, 0.02f, 0.001f}, // Goblin
				{0.05f, 0.5f, 0.006f}, // GoblinMaster
				{0.085f, 0.25f, 0.05f}, // GoblinWizard,
				{0.02f, 0.15f, 0.35f}, // Ogre
				{0.01f, 0.125f, 0.625f}, // BigOgre
			};

		/// <summary>
		/// Get drop percentages
		/// </summary>
		/// <param name="type">Type</param>
    /// <returns>Drop object</returns>
		public static AnimatedGameObject.DropObject GetDropPercentages(
				AnimatedTypes type)
		{
			for (int i = 0; i < 3; i++)
				if (RandomHelper.GetRandomFloat(0, 1) <
						MonsterWeaponDropPercentages[(int)type, i])
					return (AnimatedGameObject.DropObject)
							((int)AnimatedGameObject.DropObject.Club + i);

			return AnimatedGameObject.DropObject.None;
		} // GetDropPercentages(type)
    #endregion

		#region Variables
		/// <summary>
		/// Cave model
		/// </summary>
		ColladaModel caveModel = null;
		/// <summary>
		/// Static models
		/// </summary>
		public List<ColladaModel> staticModels =
				new List<ColladaModel>();
		/// <summary>
		/// Animated models
		/// </summary>
		public List<AnimatedColladaModel> animatedModels =
				new List<AnimatedColladaModel>();

		/// <summary>
		/// Static objects
		/// </summary>
		public static List<StaticGameObject> staticObjects =
				new List<StaticGameObject>();
		/// <summary>
		/// Animated objects
		/// </summary>
		public static List<AnimatedGameObject> animatedObjects =
				new List<AnimatedGameObject>();

		/// <summary>
		/// Helper texture for the Gears of War blood effect :D
		/// </summary>
		public static Texture screenBorder = null;
		/// <summary>
		/// Blood border effect, just set it to 1 and it will be bloody ^^
		/// </summary>
		public static float bloodBorderEffect = 0;
		/// <summary>
		/// Blood border color, can be replaced by other effects (like new level).
		/// </summary>
		public static Color bloodBorderColor = Color.Red;
		#endregion

		#region Reset
		/// <summary>
		/// Reset
		/// </summary>
		public static void Reset()
		{
				foreach (AnimatedGameObject obj in animatedObjects)
					obj.Reset();

				bloodBorderEffect = 0;
				bloodBorderColor = Color.Red;
		} // Reset()
		#endregion

		#region Weapon projectiles
		/// <summary>
		/// Current weapon projectiles.
		/// </summary>
		static List<Projectile> weaponProjectiles = new List<Projectile>();

		/// <summary>
		/// Add weapon projectile
		/// </summary>
		/// <param name="weaponType">Weapon</param>
		/// <param name="position">Pos</param>
		/// <param name="ownProjectile">True for player, false for enemy</param>
		public static void AddWeaponProjectile(
			//Projectile.WeaponTypes weaponType,
			Vector3 position)
			//bool ownProjectile)
		{
			weaponProjectiles.Add(
				new Projectile(//weaponType,
				position));//, ownProjectile));
		} // AddWeaponProjectile(weaponType, position, direction)

		/// <summary>
		/// Render weapon projectiles
		/// </summary>
		private void RenderWeaponProjectiles()
		{
			for (int num = 0; num < weaponProjectiles.Count; num++)
			{
				// Remove weapon projectile if we are done
				if (weaponProjectiles[num].Render())
				{
					weaponProjectiles.RemoveAt(num);
					num--;
				} // if
			} // for
		} // RenderWeaponProjectiles()
		#endregion

		#region Constructor
		/// <summary>
		/// Create game manager
		/// </summary>
		public GameManager()
		{
			// Load the cave and fill in all static game items (will be done
			// there in the loading process).
			caveModel = new ColladaModel("Cave");
			caveModel.material.ambientColor = Material.DefaultAmbientColor;

			// Load all animated models (player and monsters)
			for (int i = 0; i <= (int)AnimatedTypes.BigOgre; i++)
				animatedModels.Add(new AnimatedColladaModel(
					//tst: "GoblinWizard"));
					((AnimatedTypes)i).ToString()));

			// Also load all static models
			for (int i = 0; i <= (int)StaticTypes.BigSword; i++)
				staticModels.Add(new ColladaModel(
					((StaticTypes)i).ToString()));

			screenBorder = new Texture("ScreenBorder");
		} // GameManager()
		#endregion

		#region Add text message
		/// <summary>
		/// Text types
		/// </summary>
		public enum TextTypes
		{
			Normal,
			LevelUp,
			GotWeapon,
			GotKey,
			Damage,
			Died,
		} // enum TextTypes

		/// <summary>
		/// Colors for each text type
		/// </summary>
		static readonly Color[] TextTypeColors = new Color[]
			{
				new Color(200, 200, 200),//Color.White,
				Color.Yellow,
				Color.Orange,
				Color.LightBlue,
				Color.Red,
				Color.Red,
			};

		class FadeupText
		{
			public string text;
			public Color color;
			public float showTimeMs;
			public const float MaxShowTimeMs = 3750;//3000;

			public FadeupText(string setText, Color setColor)
			{
				text = setText;
				color = setColor;
				showTimeMs = MaxShowTimeMs;
			} // TimeFadeupText(setText, setShowTimeMs)
		} // TimeFadeupText
		static List<FadeupText> fadeupTexts = new List<FadeupText>();

		/// <summary>
		/// Add text message
		/// </summary>
		/// <param name="message">Message</param>
		/// <param name="type">Type</param>
		public static void AddTextMessage(string message, TextTypes type)
		{
			// Check number of texts that were added in the last 400 ms!
			int numOfRecentTexts = 0;
			for (int num = 0; num < fadeupTexts.Count; num++)
			{
				FadeupText fadeupText = fadeupTexts[num];
				if (fadeupText.showTimeMs > FadeupText.MaxShowTimeMs - 400)
					numOfRecentTexts++;
			} // for (num)
			
			fadeupTexts.Add(new FadeupText(message, TextTypeColors[(int)type]));
			// Add offset to this text to be displayed below the already existing
			// texts! This fixes the overlapping texts!
			fadeupTexts[fadeupTexts.Count - 1].showTimeMs += numOfRecentTexts * 400;
		} // AddTextMessage(message, type)

		/// <summary>
		/// Render all time fadeup effects, move them up and fade them out.
		/// </summary>
		public void RenderTimeFadeupEffects()
		{
			for (int num = 0; num < fadeupTexts.Count; num++)
			{
				FadeupText fadeupText = fadeupTexts[num];
				fadeupText.showTimeMs -= BaseGame.ElapsedTimeThisFrameInMs;
				if (fadeupText.showTimeMs < 0)
				{
					fadeupTexts.Remove(fadeupText);
					num--;
				} // if
				else
				{
					// Fade out
					float alpha = 1.0f;
					if (fadeupText.showTimeMs < 1500)
						alpha = fadeupText.showTimeMs / 1500.0f;
					// Move up
					float moveUpAmount =
						(FadeupText.MaxShowTimeMs - fadeupText.showTimeMs) /
						FadeupText.MaxShowTimeMs;

					// Calculate screen position
					TextureFont.WriteTextCentered(BaseGame.Width / 2,
						BaseGame.Height / 3 - (int)(moveUpAmount * BaseGame.Height / 3),
						fadeupText.text,
						//ColorHelper.ApplyAlphaToColor(fadeupText.color, alpha),
						//2.25f);
						fadeupText.color, alpha);
				} // else
			} // for
		} // RenderTimeFadeupEffects()
		#endregion

		#region Run
		//obs: bool firstTimeWeaponHelp = false;
		public static Vector3 doorPosition = Vector3.Zero;
		public static Vector3 treasurePosition = Vector3.Zero;
		/// <summary>
		/// Run
		/// </summary>
		public void Run(bool showUI)
		{
			#region Init glow
			//if (Input.Keyboard.IsKeyDown(Keys.LeftAlt) == false)
			{
				// Start glow shader
				BaseGame.GlowShader.Start();

				// Clear background with white color, looks much cooler for the
				// post screen glow effect.
				//BaseGame.Device.Clear(Color.Black);
				PostScreenGlow.sceneMapTexture.Clear(Color.Black);
			} // if (Input.Keyboard.IsKeyDown)
			#endregion

			#region Init variables
			// Restart with start or space
			if (Input.GamePad.Buttons.Start == ButtonState.Pressed ||
				Input.KeyboardEnterJustPressed)
			{
				// Reset everything
				Player.Reset();
				BaseGame.camera.Reset();
				GameManager.Reset();
			} // if

			// Render goblin always in center, but he is really big, bring him
			// down to a more normal size that fits better in our test scene.
			Matrix renderMatrix = Matrix.Identity;// Matrix.CreateScale(0.1f);

			// Restore z buffer state
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;

			// Make sure we got the closest 6 lights, only has to be checked
			// once per frame.
			Vector3 camPos = BaseGame.CameraPos;
			LightManager.FindClosestLights(camPos);

			// Render the player first
			AnimatedColladaModel playerModel =
				animatedModels[(int)AnimatedTypes.Hero];
			Matrix playerMatrix =
				Matrix.CreateRotationZ(BaseGame.camera.PlayerRotation) *
				Matrix.CreateTranslation(BaseGame.camera.PlayerPos);
			AnimatedColladaModel.currentAnimatedObject = BaseGame.camera.playerObject;
			Vector3 playerPos = BaseGame.camera.PlayerPos + new Vector3(0, 0, 0.75f);

			// Is player close to the door and he got the key?
			if (Vector3.Distance(playerPos, doorPosition) < 4.5f &&
				Player.gotKey)
			{
				// Then remove door, allow walking through!
				foreach (StaticGameObject model in staticObjects)
					if (model.type == StaticTypes.Door)
					{
						staticObjects.Remove(model);
						Sound.Play(Sound.Sounds.HitClub);
						Sound.Play(Sound.Sounds.Click);

						GameManager.AddTextMessage("Entering Level 2 ..",
							GameManager.TextTypes.LevelUp);
						GameManager.AddTextMessage("Find the Treasure!",
							GameManager.TextTypes.LevelUp);
						break;
					} // foreach if (model.type)
			} // if (Vector3.Distance)

			int weaponModelNum = Player.currentWeapon == Player.WeaponTypes.Club ?
				(int)StaticTypes.Club :
				Player.currentWeapon == Player.WeaponTypes.Sword ?
				(int)StaticTypes.Sword : (int)StaticTypes.BigSword;
			Matrix weaponMatrix =
				playerModel.HandlePlayerFlareAndWeapon() *
				playerMatrix;
			#endregion

			#region Generate and render shadows
			// Generate shadows
			ShaderEffect.shadowMapping.GenerateShadows(
				delegate
				{
					// First render all animated models
					playerModel.GenerateShadow(playerMatrix,
						BaseGame.camera.PlayerBlendStates);
					foreach (AnimatedGameObject model in animatedObjects)
						if ((camPos - model.positionMatrix.Translation).LengthSquared() <
							15 * 15)//60 * 60)
						{
							AnimatedColladaModel.currentAnimatedObject = model;
							if ((int)model.type < animatedModels.Count)
								animatedModels[(int)model.type].GenerateShadow(
									model.positionMatrix,
									model.blendedStates);
						} // foreach if (camPos)

					// And then all static models!
					staticModels[weaponModelNum].GenerateShadow(weaponMatrix);
					foreach (StaticGameObject model in staticObjects)
						if ((camPos - model.positionMatrix.Translation).LengthSquared() <
							15 * 15)//60 * 60)
						{
							Matrix objectMatrix = model.positionMatrix;
							// For collectable items replace matrix, rotate around
							if (model.IsCollectable)
								objectMatrix =
									Matrix.CreateScale(1.5f) *
									Matrix.CreateRotationZ(Player.GameTime / 0.556f) *
									Matrix.CreateTranslation(objectMatrix.Translation +
									new Vector3(0, 0,
									(float)Math.Sin(Player.GameTime / 0.856f) * 0.15f));
							staticModels[(int)model.type].GenerateShadow(objectMatrix);
						} // foreach if (camPos)
				});

			// Render shadows
			ShaderEffect.shadowMapping.RenderShadows(
				delegate
				{
					// Throw shadows on the cave
					caveModel.UseShadow(
						// Note: We have to rescale ourselfs, we use the world matrix here!
						// Below it does not matter for Render because the shader handles
						// everything. Also move up a little to fix ground error.
						Matrix.CreateScale(100)*
						Matrix.CreateTranslation(new Vector3(0, 0, 0.05f)));
					
					// And all animated and static models
					playerModel.UseShadow(playerMatrix,
						BaseGame.camera.PlayerBlendStates);
					foreach (AnimatedGameObject model in animatedObjects)
						if ((camPos - model.positionMatrix.Translation).LengthSquared() <
							15 * 15)//20 * 20)
						{
							AnimatedColladaModel.currentAnimatedObject = model;
							if ((int)model.type < animatedModels.Count)
								animatedModels[(int)model.type].UseShadow(
									model.positionMatrix,
									model.blendedStates);
						} // foreach if (camPos)

					// And finally all static models!
					staticModels[weaponModelNum].UseShadow(weaponMatrix);
					foreach (StaticGameObject model in staticObjects)
						if ((camPos - model.positionMatrix.Translation).LengthSquared() <
							15 * 15)//20 * 20)
						{
							Matrix objectMatrix = model.positionMatrix;
							// For collectable items replace matrix, rotate around
							if (model.IsCollectable)
								objectMatrix =
									Matrix.CreateScale(1.5f) *
									Matrix.CreateRotationZ(Player.GameTime / 0.556f) *
									Matrix.CreateTranslation(objectMatrix.Translation +
									new Vector3(0, 0,
									(float)Math.Sin(Player.GameTime / 0.856f) * 0.15f));
							staticModels[(int)model.type].UseShadow(objectMatrix);
						} // foreach if (camPos)
				});
			#endregion

			#region Render cave
			// Enable culling for the cave (faster rendering!)
			BaseGame.Device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

			//*tst
			caveModel.Render(Matrix.Identity);//Matrix.CreateTranslation(0, 0, +1));
			//*/

			// Disable culling, some models use backsides (for swords, etc.)
			// This should be fixed on the model level and not here, but there is no
			// time.
			BaseGame.Device.RenderState.CullMode = CullMode.None;
			#endregion

			#region Render player and his weapon
			// Render player
			playerModel.Render(playerMatrix, BaseGame.camera.PlayerBlendStates);

			// Handle player flare and weapon, which have to be updated with
			// the bones.
			staticModels[weaponModelNum].Render(weaponMatrix);

			// Set our flare light as shadow mapping light position.
			// This is very important for our shadow mapping.
			ShaderEffect.shadowMapping.SetVirtualLightPos(
				AnimatedColladaModel.finalPlayerFlarePos);
			#endregion

			#region Render static models
			// Render all static and animated models and update the closest lights
			// for each of them. Note: We only have to render the visible stuff,
			// which is basically everything in a 60m radius.
			foreach (StaticGameObject model in staticObjects)
				if ((camPos - model.positionMatrix.Translation).LengthSquared() <
					50 * 50)//60 * 60)
				{
					LightManager.FindClosestLights(model.positionMatrix.Translation);
					Matrix objectMatrix = model.positionMatrix;
					// For collectable items replace matrix, rotate around
					if (model.IsCollectable)
						objectMatrix =
							Matrix.CreateScale(1.5f) *
							Matrix.CreateRotationZ(Player.GameTime/0.556f) *
							Matrix.CreateTranslation(objectMatrix.Translation+
							new Vector3(0, 0,
							(float)Math.Sin(Player.GameTime/0.856f)*0.15f));
					staticModels[(int)model.type].Render(objectMatrix);

					// If player is close enough, collect this item (if it is a key,
					// club, sword or big sword).
					if (model.IsCollectable && Vector3.Distance(
						playerPos, model.positionMatrix.Translation) < 1.0f)//1.25f)
					{
						if (model.type == StaticTypes.Key)
						{
							Player.gotKey = true;
							
							// Show text message
							GameManager.AddTextMessage("You got the key, find "+
								"the door to the second level!", TextTypes.GotKey);
							// Play level up sound effect for got key
							Sound.Play(Sound.Sounds.LevelUp);
						} // if (model.type)
						else
						{
							if (model.type == StaticTypes.Club)
								Player.gotWeapons[0] = true;
							else if (model.type == StaticTypes.Sword &&
								Player.gotWeapons[1] == false)
							{
								Player.gotWeapons[1] = true;
								Player.currentWeapon = Player.WeaponTypes.Sword;
							} // else if
							else if (model.type == StaticTypes.BigSword &&
								Player.gotWeapons[2] == false)
							{
								Player.gotWeapons[2] = true;
								Player.currentWeapon = Player.WeaponTypes.BigSword;
							} // else if

							// Show text message
							GameManager.AddTextMessage("You collected a "+
								StringHelper.SplitFunctionNameToWordString(
								model.type.ToString()) + ".", TextTypes.GotWeapon);
							
							//Too annoying every time!
							/*ignore, we autochange weapon now!
							if (firstTimeWeaponHelp == false)
							{
								firstTimeWeaponHelp = true;
								GameManager.AddTextMessage("Change weapon with " +
									"Q/E/MouseWheel or shoulder buttons", TextTypes.Normal);
							} // if (firstTimeWeaponHelp)
							 */
							// Play one of the whosh sounds for the new weapon
							Sound.Play(Sound.Sounds.Whosh);
							// And add a click effect to make it sound a little different!
							Sound.Play(Sound.Sounds.Click);
						} // else

						// Remove this object
						staticObjects.Remove(model);

						// Get outta here, we modified the collection
						break;
					} // if (Vector3.Distance)
				} // foreach if (camPos)
			#endregion

			#region Render animated models
			//int goblinNum = 0;
			foreach (AnimatedGameObject model in animatedObjects)
				if ((camPos - model.positionMatrix.Translation).LengthSquared() <
					50 * 50)//60 * 60)
				{
					//goblinNum++;
					//TextureFont.WriteText(100, 100+goblinNum*25,
					//	"state=" + animatedObjects[goblinNum].state);

					LightManager.FindClosestLights(model.positionMatrix.Translation);
					if (Player.GameOver == false &&
						Player.GameTime >= 0)
					{
						model.UpdateMonsterAI();
						model.UpdateState();
					} // if (Player.GameOver)
					//TextureFont.WriteText(300, 100 + goblinNum * 25,
					//	"state=" + animatedObjects[goblinNum].state);
					AnimatedColladaModel.currentAnimatedObject = model;
					if ((int)model.type < animatedModels.Count)
						animatedModels[(int)model.type].Render(model.positionMatrix,
							model.blendedStates);

					// Update weapon pos (important for the wizard)
					model.weaponPos =
						animatedModels[(int)model.type].GetWeaponPos(model.positionMatrix);
				} // foreach if (camPos)

			if (bloodBorderEffect > 0)
			{				
				// Show also on border
				GameManager.screenBorder.RenderOnScreen(
					BaseGame.ResolutionRect,
					GameManager.screenBorder.GfxRectangle,
					ColorHelper.ApplyAlphaToColor(bloodBorderColor, bloodBorderEffect));

				if (bloodBorderColor == Color.Green)
				{
					// Formular: (1/speed)*1000*15 ms
					float speed = 10;
					int msToCheck = (int)(speed > 0 ?
						(1 / speed) * 1000 * 15 : 1000.0f);
					// Don't go above 500, looks bad
					if (msToCheck > 500)
						msToCheck = 500;
					//*tst disabling
					if (BaseGame.EveryMs(msToCheck))//75))
					{
						EffectManager.AddEffect(
							BaseGame.camera.PlayerPos + new Vector3(0, 0, 1.5f) +
							RandomHelper.GetRandomVector3(-0.275f, 0.275f),
							EffectManager.EffectType.Smoke, 1.5f,
							RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
					} // if (BaseGame.EveryMs)
				} // if (bloodBorderColor)

				bloodBorderEffect -= BaseGame.MoveFactorPerSecond;// *2.5f;
			} // if (bloodBorderEffect)
			#endregion

			#region Render shadow map
			ShaderEffect.shadowMapping.ShowShadows();
			#endregion

			#region Render lights and effects
			LightManager.RenderAllCloseLightEffects();

			RenderWeaponProjectiles();

			// We have to render the effects ourselfs because
			// it is usually done in DungeonQuestGame!
			// Finally render all effects (before applying post screen effects)
			BaseGame.effectManager.HandleAllEffects();
			#endregion

			#region Handle UI and rest of game logic
			if (showUI)
			{
				// Show UI
				UIManager.DrawUI();

				// Show extra text messages in the center
				RenderTimeFadeupEffects();

				// Handle the game logic and show the victory/defeat screen.
				Player.HandleGameLogic();
			} // if (showUI)
			else
			{
				// Stop everything
				Player.StopTime();
			} // else

			/*tst
			if (Input.Keyboard.IsKeyDown(Keys.LeftShift) ||
				Input.GamePadAPressed)
			{
				ShaderEffect.shadowMapping.shadowMapTexture.RenderOnScreen(
					new Rectangle(10, 10, 256, 256));
				ShaderEffect.shadowMapping.shadowMapBlur.SceneMapTexture.
					RenderOnScreen(
					new Rectangle(10 + 256 + 10, 10, 256, 256));
			} // if (Input.Keyboard.IsKeyDown)
			 */
			#endregion

			#region Finish with glow
			//if (Input.Keyboard.IsKeyDown(Keys.LeftAlt) == false)
			{
				// And finally show glow shader on top of everything
				BaseGame.GlowShader.Show();
			} // if (Input.Keyboard.IsKeyDown)
			#endregion
		} // Run()
		#endregion

		#region Add
		/// <summary>
		/// Add
		/// </summary>
		/// <param name="type">Type</param>
		/// <param name="positionMatrix">Position matrix</param>
		public static void Add(StaticTypes type, Matrix positionMatrix)
		{
			staticObjects.Add(new StaticGameObject(type, positionMatrix));
		} // Add(type, positionMatrix)

		/// <summary>
		/// Add
		/// </summary>
		/// <param name="type">Type</param>
		/// <param name="positionMatrix">Position matrix</param>
		public static void Add(AnimatedTypes type, Matrix positionMatrix)
		{
			animatedObjects.Add(new AnimatedGameObject(type, positionMatrix));
		} // Add(type, positionMatrix)

		/// <summary>
		/// Add
		/// </summary>
		/// <param name="type">Type</param>
		/// <param name="positionMatrix">Position matrix</param>
		public static void Add(AnimatedTypes type, Matrix positionMatrix,
			AnimatedGameObject.DropObject dropObject)
		{
			animatedObjects.Add(new AnimatedGameObject(type, positionMatrix,
				dropObject));
		} // Add(type, positionMatrix)
		#endregion
	} // class GameManager
} // namespace DungeonQuest.GameLogic
