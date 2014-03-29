// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Game
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:38

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using DungeonQuest.Graphics;
#endregion

namespace DungeonQuest.Game
{
	/// <summary>
	/// Global light manager class to manage all lights we got from the
	/// cave level. They are mostly used by CavePointNormalMapping for rendering
	/// the cave itself, but we also need the lights for the static models
	/// and the animated characters.
	/// 
	/// The level itself has many lights, like 20-30, but we can only render
	/// exactly 6 because the shaders are optimized this way.
	/// </summary>
	class LightManager
	{
		#region Variables
		/// <summary>
		/// Static list of all lights as they were imported from the cave
		/// model. We need to get the closest 6 ones for rendering.
		/// Note: These are not just positions, but the matrices we get from
		/// the collada file. This is important to set the flare object
		/// below the light and might be useful for later spot light effects
		/// or shadows.
		/// </summary>
		public static List<Matrix> allLights = new List<Matrix>();

		/// <summary>
		/// Number of lights we can render at a time, this does not mean
		/// our cave cant have more lights. We just going to search for the 6
		/// closest ones.
		/// </summary>
		public const int NumberOfLights = 6;

		/// <summary>
		/// Closest lights for rendering. This has to be exactly the number
		/// of lights we support for all shaders (6), see NumberOfLights.
		/// </summary>
		public static List<Vector3> closestLightsForRendering = new List<Vector3>();
		#endregion

		#region Static constructor
		/// <summary>
		/// Create light manager
		/// </summary>
		static LightManager()
		{
			// Make sure we always got at least 3 lights
			LightManager.closestLightsForRendering.Add(
				Vector3.Zero);
			LightManager.closestLightsForRendering.Add(
				new Vector3(1, 2, 4));
			LightManager.closestLightsForRendering.Add(
				new Vector3(1.6f, -2, 3));
			// same again just to fix 6 light exception if we don't set any lights!
			LightManager.closestLightsForRendering.Add(
				Vector3.Zero);
			LightManager.closestLightsForRendering.Add(
				new Vector3(1, 2, 4));
			LightManager.closestLightsForRendering.Add(
				new Vector3(1.6f, -2, 3));
		} // LightManager()
    #endregion

		#region Find closest lights
		/// <summary>
		/// Light position and distance helper class to figure out which
		/// lights are the 6 closest ones.
		/// </summary>
		class LightPosAndDistance : IComparable<LightPosAndDistance>
		{
			/// <summary>
			/// Position
			/// </summary>
			public Vector3 pos;
			/// <summary>
			/// Distance
			/// </summary>
			public float distance;

			/// <summary>
			/// Create light position and distance
			/// </summary>
			/// <param name="setPos">Set position</param>
			/// <param name="setDistance">Set distance</param>
			public LightPosAndDistance(Vector3 setPos, float setDistance)
			{
				pos = setPos;
				distance = setDistance;
			} // LightPosAndDistance(setPos, setDistance)

			#region IComparable<LightPosAndDistance> Members
			/// <summary>
			/// Compare to
			/// </summary>
			/// <param name="other">Other</param>
			/// <returns>Int</returns>
			public int CompareTo(LightPosAndDistance other)
			{
				if (other.distance > distance)
					return -1;
				else if (other.distance < distance)
					return 1;
				else
					return 0;
			} // CompareTo(other)
			#endregion
		} // class LightPosAndDistance

		/// <summary>
		/// Find closest lights. Little helper method to find out about all the
		/// lights that are close to the player. Our cave can have many more lights.
		/// We just going to search for the 6 closest ones.
		/// </summary>
		public static void FindClosestLights(Vector3 camPos)
		{
			if (allLights.Count < NumberOfLights)
				throw new InvalidOperationException(
					"We need at least "+NumberOfLights+" set in order for the "+
					"LightManager to work. Make sure you load a level with lights!");

			// Create and fill in array of light distances we need for sorting
			List<LightPosAndDistance> lightDistances =
				new List<LightPosAndDistance>();
			foreach (Matrix mat in allLights)
			{
				Vector3 pos = mat.Translation;
				lightDistances.Add(new LightPosAndDistance(pos,
					(camPos - pos).Length()));
			} // foreach (mat)

			// Add one extra light for the player himself.
			// This position is the flare in the players left arm.
			lightDistances.Add(new LightPosAndDistance(
				BaseGame.camera.PlayerPos +
				AnimatedColladaModel.finalPlayerFlarePos, 0));

			// Ok, now sort
			lightDistances.Sort();

			// And return the top 6 ones, store it in closestLightsForRendering
			closestLightsForRendering.Clear();
			for (int i = 0; i < NumberOfLights; i++)
			{
				closestLightsForRendering.Add(lightDistances[i].pos);
			} // for (int) 
		} // FindClosestLights()
		#endregion

		#region RenderAllCloseLightEffects
		/// <summary>
		/// Render all close light effects
		/// </summary>
		public static void RenderAllCloseLightEffects()
		{
			bool playerFlare = true;
			foreach (Vector3 pos in closestLightsForRendering)
			{
				// Add a couple of effects on top of each other
				//TODO: make smaller for player flare?
				float size = playerFlare ? 0.3f : 1.05f;
				playerFlare = false;
				EffectManager.AddEffect(
					pos, EffectManager.EffectType.Flare, size, 0);
				EffectManager.AddEffect(
					pos, EffectManager.EffectType.LightInstant, size * 2.5f, 0);
				//EffectManager.AddEffect(
				//	pos, EffectManager.EffectType.LightInstant, size, 0);
			} // foreach (pos)
		} // RenderAllCloseLightEffects()
		#endregion
	} // class LightManager
} // namespace DungeonQuest.Game
