// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Sounds
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:47

#region Using directives
#if DEBUG
//using NUnit.Framework;
#endif
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using DungeonQuest.Game;
using DungeonQuest.Graphics;
using DungeonQuest.Helpers;
using Microsoft.Xna.Framework.Input;
using System.Threading;
using Microsoft.Xna.Framework;
using System.IO;
using DungeonQuest.Properties;
#endregion

namespace DungeonQuest.Sounds
{
	/// <summary>
	/// Sound
	/// </summary>
	class Sound
	{
		#region Variables
		/// <summary>
		/// Sound stuff for XAct
		/// </summary>
		static AudioEngine audioEngine;
		/// <summary>
		/// Wave bank
		/// </summary>
		static WaveBank waveBank;
		/// <summary>
		/// Sound bank
		/// </summary>
		static SoundBank soundBank;
		/// <summary>
		/// Special category for music, we can only play one music at a time
		/// and to stop we use this category. For play just use play.
		/// Adding stepping category for dungeon quest.
		/// </summary>
		static AudioCategory musicCategory,
			stepsCategory, soundCategory;
		#endregion

		#region Enums
		/// <summary>
		/// Sounds we use in this game.
		/// </summary>
		/// <returns>Enum</returns>
		public enum Sounds
		{
			Click,
			Defeat,
			Victory,
			GameMusic,
			Steps,
			Whosh,
			LevelUp,
			GoblinDie,
			GoblinWasHit,
			OgreDie,
			OgreWasHit,
			PlayerWasHit,
			HitClub,
			HitFlesh,
			Athmosphere,
			MonsterSteps,
			GoblinCry,
			OgreCry,
			WizardFire,
            ButtonClick,
            Highlight
		} // enum Sounds
		#endregion

		#region Constructor
		/// <summary>
		/// Create sound
		/// </summary>
		static Sound()
		{
			try
			{
				string dir = Directories.SoundsDirectory;
				audioEngine = new AudioEngine(
					Path.Combine(dir, "DungeonQuest.xgs"));
				waveBank = new WaveBank(audioEngine,
					Path.Combine(dir, "Wave Bank.xwb"));
				// Use streaming for music to save memory
				//waveBank2 = new WaveBank(audioEngine,
				//	Path.Combine(dir, "Wave Bank 2.xwb"), 0, 16);

				// Dummy wavebank call to get rid of the warning that waveBank is
				// never used (well it is used, but only inside of XNA).
				if (waveBank != null)
					soundBank = new SoundBank(audioEngine,
						Path.Combine(dir, "Sound Bank.xsb"));
				musicCategory = audioEngine.GetCategory("Music");
                SetMusicVolume(GameSettings.Default.MusicVolume);
				stepsCategory = audioEngine.GetCategory("Steps");
                soundCategory = audioEngine.GetCategory("Default");
                SetSoundVolume(GameSettings.Default.SoundVolume);
			} // try
			catch (Exception ex)
			{
				// Audio creation crashes in early xna versions, log it and ignore it!
				Log.Write("Failed to create sound class: " + ex.ToString());
			} // catch
		} // Sound()
		#endregion

		#region Play
		/// <summary>
		/// Play
		/// </summary>
		/// <param name="soundName">Sound name</param>
		public static void Play(string soundName)
		{
			if (soundBank == null)
				return;

			try
			{
				soundBank.PlayCue(soundName);
			} // try
			catch (Exception ex)
			{
				Log.Write("Playing sound " + soundName + " failed: " + ex.ToString());
			} // catch
		} // Play(soundName)

		/// <summary>
		/// Play
		/// </summary>
		/// <param name="sound">Sound</param>
		public static void Play(Sounds sound)
		{
			Play(sound.ToString());
		} // Play(sound)

		/// <summary>
		/// Play defeat sound
		/// </summary>
		public static void PlayDefeatSound()
		{
			Play(Sounds.Defeat);
		} // PlayDefeatSound()

		/// <summary>
		/// Play victory sound
		/// </summary>
		public static void PlayVictorySound()
		{
			Play(Sounds.Victory);
		} // PlayVictorySound()

		public static void PlayWhosh(float volume)
		{
			//*lags a little!
			// Sound bank must be valid
			if (soundBank == null)
				return;

			// Get whosh cue, will either create cue or reuse existing cue
			Cue newWhoshCue = soundBank.GetCue(Sounds.Whosh.ToString());//"Whosh");
			// Set volume
			newWhoshCue.SetVariable("Volume", volume * 100);
			// And play
			newWhoshCue.Play();
			//*/

			//Play(Sounds.Whosh);
		} // PlayWhosh(volume)
		#endregion

		#region Music
		/// <summary>
		/// Start music
		/// </summary>
		public static void StartMusic()
		{
			Play(Sounds.GameMusic);
		} // StartMusic()

		/// <summary>
		/// Stop music
		/// </summary>
		public static void StopMusic()
		{
			musicCategory.Stop(AudioStopOptions.Immediate);
		} // StopMusic()
		#endregion

		#region Music
		static bool playingStepsSounds = false;
		/// <summary>
		/// Start music
		/// </summary>
		public static void StartSteps()
		{
			// Already playing?
			if (playingStepsSounds)
				return;

			Play(Sounds.Steps);
			playingStepsSounds = true;
		} // StartMusic()

		/// <summary>
		/// Stop music
		/// </summary>
		public static void StopSteps()
		{
			// Not playing?
			if (playingStepsSounds == false)
				return;
            if (stepsCategory.Name != null)
			    stepsCategory.Stop(AudioStopOptions.Immediate);
			playingStepsSounds = false;
		} // StopMusic()
		#endregion

		#region Update
		static bool startMusicPlayback =
#if DEBUG
			false;
#else
			true;
#endif
		/// <summary>
		/// Update, just calls audioEngine.Update!
		/// </summary>
		public static void Update()
		{
            if (audioEngine != null)
            {
                audioEngine.Update();
            }

			if (startMusicPlayback)
			{
				startMusicPlayback = false;
				StartMusic();
			} // if
		} // Update()
		#endregion

        /// <summary>
        /// Set Music Volume
        /// </summary>
        /// <param name="volume"></param>
        public static void SetMusicVolume(float volume)
        {
            if (musicCategory != null)
            {                
                musicCategory.SetVolume(volume);
            }
        }

        /// <summary>
        /// Set Sounds Volume
        /// </summary>
        /// <param name="volume"></param>
        public static void SetSoundVolume(float volume)
        {
            if (stepsCategory != null && soundCategory != null)
            {
                stepsCategory.SetVolume(volume);
                soundCategory.SetVolume(volume);
            }
        }

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test play sounds
		/// </summary>
		//[Test]
		public static void TestPlaySounds()
		{
			//int crazyCounter = 0;

			TestGame.Start(
				delegate
				{
					if (Input.MouseLeftButtonJustPressed ||
						Input.GamePadAJustPressed)
						Sound.Play(Sounds.Click);
					else if (Input.MouseRightButtonJustPressed ||
						Input.GamePadBJustPressed)
						Sound.Play(Sounds.GameMusic);
					else if (Input.KeyboardKeyJustPressed(Keys.D1))
						Sound.Play(Sounds.Victory);
					else if (Input.KeyboardKeyJustPressed(Keys.D2))
						Sound.Play(Sounds.Defeat);
					//else if (Input.KeyboardKeyJustPressed(Keys.D3))
					//	Sound.Play(Sounds.Speed);
					else if (Input.KeyboardKeyJustPressed(Keys.D4))
						Sound.Play(Sounds.Steps);
					else if (Input.KeyboardKeyJustPressed(Keys.D5))
						Sound.Play(Sounds.Whosh);
					else if (Input.KeyboardKeyJustPressed(Keys.D6))
						Sound.Play(Sounds.LevelUp);
					else if (Input.KeyboardKeyJustPressed(Keys.D7))
						Sound.Play(Sounds.GoblinDie);
					else if (Input.KeyboardKeyJustPressed(Keys.D8))
						Sound.Play(Sounds.GoblinWasHit);
					else if (Input.KeyboardKeyJustPressed(Keys.D9))
						Sound.Play(Sounds.OgreDie);

					TextureFont.WriteText(2, 30,
						"Press 1-8 or A/B or left/right mouse buttons to play back "+
						"sounds!");
				});
		} // TestPlaySounds()
#endif
		#endregion
	} // class Sound
} // DungeonQuest.Sounds
