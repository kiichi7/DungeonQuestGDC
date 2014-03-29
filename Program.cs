// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:47

#region Using directives
using System;
using DungeonQuest.Sounds;
using DungeonQuest.Shaders;
using DungeonQuest.Graphics;
using DungeonQuest.GameScreens;
using DungeonQuest.Game;
using DungeonQuest.Helpers;
#endregion

namespace DungeonQuest
{
	static class Program
	{
    #region RestartGameAfterOptionsChange
#if !XBOX360
    /// <summary>
    /// Restart if user changes something in options.
    /// </summary>
    public static bool RestartGameAfterOptionsChange = false;
#endif
    #endregion

		#region Main
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			//*
			// Run the actual game
			using (DungeonQuestGame game = new DungeonQuestGame())
			{
				game.Run();
			} // using (game)

			/*/

			// Some unit tests that were written during the development of this game
			//UIManager.TestUI();
			//ShaderTests.TestNormalMappingShader();
			//ColladaModel.TestCaveColladaModelScene();
			//ColladaModel.TestLoadStaticModel();
			//AnimatedColladaModel.TestPlayerColladaModelScene();
			//PostScreenGlow.TestPostScreenGlow();

			//PlaneRenderer.TestRenderingPlaneXY();
			//ShadowMapShader.TestShadowMapping();

			//EffectManager.TestEffects();
			//StringHelper.TestConvertStringToFloatArray();
		  //Sound.TestPlaySounds();
 
			//Note: This test only works if you enable the depth buffer setting
			// and reseting in RenderToTexture and BaseGame:
			//RenderToTexture.TestCreateRenderToTexture();

			//Note: Tested split screen support for the Xbox 360 here, but there
			// was no time to finish the game logic + handling input from 2nd player.
			// But it should not be too much work to finish this up if anyone is
			// intererested in that feature!
			//ColladaModel.TestCaveColladaModelSceneSplitScreen();
			//*/
		} // Main(args)
		#endregion
	} // class Program
} // namespace DungeonQuest
