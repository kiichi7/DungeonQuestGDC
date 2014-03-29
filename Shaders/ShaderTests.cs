// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Shaders
// Creation date: 31.07.2007 04:29
// Last modified: 31.07.2007 04:46

#if DEBUG
#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using DungeonQuest.Game;
using DungeonQuest.Graphics;
#endregion

namespace DungeonQuest.Shaders
{
	/// <summary>
	/// Shader tests. Extra class to make sure we can call shaders
	/// here without instantiating ShaderEffect first because
	/// the graphic engine is not initialized yet.
	/// </summary>
	class ShaderTests
	{
		#region Unit Testing
#if DEBUG
		#region Test NormalMapping shader
		/// <summary>
		/// Test NormalMapping shader
		/// </summary>
		public static void TestNormalMappingShader()
		{
			PlaneRenderer testPlane = null;

			TestGame.Start("TestNormalMappingShader",
				delegate
				{
					testPlane = new PlaneRenderer(
						Vector3.Zero, new Plane(new Vector3(0, 0, 1), 0),
						new Material("CaveDetailGround", "CaveDetailGroundNormal"), 25.0f);
				},
				delegate
				{
					testPlane.Render(ShaderEffect.normalMapping);//, "DiffuseSpecular20");
				});
		} // TestNormalMappingShader()
		#endregion
#endif
		#endregion
	} // class ShaderTests
} // namespace RacingGame.Shaders
#endif