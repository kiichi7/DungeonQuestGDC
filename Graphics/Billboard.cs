// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Graphics
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:40

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using DungeonQuest.Game;
using DungeonQuest.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DungeonQuest.Shaders;
using XnaTexture = Microsoft.Xna.Framework.Graphics.Texture2D;
#endregion

namespace DungeonQuest.Graphics
{
	/// <summary>
	/// Billboard
	/// </summary>
	class Billboard
	{
		#region Variables
		/// <summary>
		/// Right and up vectors for billboard calculations, will be
		/// set by Billboard.CalcVectors each frame (called always by Graphics)!
		/// </summary>
		internal static Vector3 vecRight = new Vector3(1.0f, 0.0f, 0.0f),
			vecUp = new Vector3(0.0f, 1.0f, 0.0f);

		/// <summary>
		/// For rendering billboard on ground (on z plane, e.g. for ring effect)
		/// </summary>
		internal static Vector3
			// Do it on xy plane instead of xz plane (better visibility)
			vecGroundRight = new Vector3(0.0f, 1.0f, 0.0f),
			vecGroundUp = new Vector3(1.0f, 0.0f, 0.0f);//,
			//obs: vecGroundDepth = new Vector3(0.0f, 0.0f, 1.0f);

		/// <summary>
		/// Blend mode
		/// </summary>
		public enum BlendMode
		{
			/// <summary>
			/// Use normal alpha blending (src alpha, inv src alpha)
			/// </summary>
			NormalAlphaBlending,
			/// <summary>
			/// Light effect with help of destination color and inv src alpha.
			/// </summary>
			LightEffect,
			/// <summary>
			/// Additive blending effect, just adding color with one, one blending.
			/// </summary>
			AdditiveEffect,
		} // enum BlendMode

		/// <summary>
		/// Texture billboard list
		/// </summary>
		class TextureBillboardList
		{
			/// <summary>
			/// Tex
			/// </summary>
			public XnaTexture tex;
			/// <summary>
			/// Light blend mode, usually NormalAlphaBlending
			/// </summary>
			public BlendMode lightBlendMode;
			/// <summary>
			/// Billboard vertices
			/// </summary>
			public List<VertexPositionColorTexture> vertices =
				new List<VertexPositionColorTexture>();
			/// <summary>
			/// Billboard indices
			/// </summary>
			public List<short> indices = new List<short>();

			/// <summary>
			/// Create texture billboard list
			/// </summary>
			/// <param name="setTex">Set tex</param>
			/// <param name="setLightBlendMode">Set light blend mode</param>
			public TextureBillboardList(XnaTexture setTex,
				BlendMode setLightBlendMode)
			{
				tex = setTex;
				lightBlendMode = setLightBlendMode;
			} // TextureBillboardList(setTex, setLightBlendMode)
		} // class TextureBillboardList

		static List<TextureBillboardList> billboards =
			new List<TextureBillboardList>();

		/// <summary>
		/// Get texture billboard
		/// </summary>
		/// <param name="tex">Tex</param>
		/// <returns>Texture billboard list</returns>
		static private TextureBillboardList GetTextureBillboard(
			XnaTexture tex, BlendMode lightBlendMode)
		{
			if (billboards.Count > 0 &&
				billboards[billboards.Count - 1].tex == tex &&
				billboards[billboards.Count - 1].lightBlendMode == lightBlendMode)
				return billboards[billboards.Count - 1];

			// Not found? Then create new and add to billboards list
			TextureBillboardList texBillboard =
				new TextureBillboardList(tex, lightBlendMode);
			billboards.Add(texBillboard);
			
			// Return result
			return texBillboard;
		} // GetTextureBillboard(tex)
		#endregion

		#region Calc vectors
		/// <summary>
		/// Calc vectors for billboards, will create helper vectors for
		/// billboard rendering, should just be called every frame.
		/// </summary>
		public static void CalcVectors()
		{
			// Only use the inverse view matrix, world matrix is assumed to be
			// Idendity, simply grab the values out of the inverse view matrix.
			Matrix invViewMatrix = BaseGame.InverseViewMatrix;
			vecRight = new Vector3(
				invViewMatrix.M11, invViewMatrix.M12, invViewMatrix.M13);
			vecUp = new Vector3(
				invViewMatrix.M21, invViewMatrix.M22, invViewMatrix.M23);
		} // CalcVectors()
		#endregion

		#region Render billboard
		static VertexDeclaration decl = null;
    /// <summary>
    /// Render billboards
    /// </summary>
		public static void RenderBillboards()
		{
			if (billboards.Count == 0)
				return;

			// Update billboard vectors
			CalcVectors();

			// Reset world space
			BaseGame.WorldMatrix = Matrix.Identity;

			// Enable alpha blending
			BaseGame.EnableAlphaBlending();

			// Disable z buffer to prevent effects to be disabling each other!
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = false;

			// Never go outside of billboard texture
			BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Clamp;
			BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Clamp;

			// No culling
			BaseGame.Device.RenderState.CullMode =
				CullMode.None;

			// Always use VertexPositionColorTexture format!
			if (decl == null)
				decl = new VertexDeclaration(
					BaseGame.Device, VertexPositionColorTexture.VertexElements);
			BaseGame.Device.VertexDeclaration = decl;

			BlendMode useLightBlendMode = BlendMode.NormalAlphaBlending;
			// Render all billboards
			ShaderEffect.effectShader.SetParametersOptimizedGeneral();
			ShaderEffect.effectShader.Effect.Begin();
			ShaderEffect.effectShader.Effect.CurrentTechnique.Passes[0].Begin();

			//foreach (TextureBillboardList billboard in billboards)
			for (int num=0; num<billboards.Count; num++)
			{
				TextureBillboardList billboard = billboards[num];

				if (billboard.vertices.Count > 0 &&
					billboard.indices.Count > 0)
				{
					ShaderEffect.effectShader.SetParameters(billboard.tex);
					ShaderEffect.effectShader.Update();

					if (billboard.lightBlendMode == BlendMode.LightEffect)
					{
						if (useLightBlendMode != BlendMode.LightEffect)
						{
							useLightBlendMode = BlendMode.LightEffect;
							BaseGame.Device.RenderState.SourceBlend =
								Blend.DestinationColor;
							BaseGame.Device.RenderState.DestinationBlend =
								Blend.SourceAlpha;
						} // if (useLightBlendMode)
					} // if (billboard.lightBlendMode)
					else if (billboard.lightBlendMode == BlendMode.AdditiveEffect)
					{
						if (useLightBlendMode != BlendMode.AdditiveEffect)
						{
							useLightBlendMode = BlendMode.AdditiveEffect;
							BaseGame.Device.RenderState.SourceBlend =
								Blend.SourceAlpha;
							BaseGame.Device.RenderState.DestinationBlend =
								Blend.One;
						} // if (useLightBlendMode)
					} // else if
					else
					{
						if (useLightBlendMode != BlendMode.NormalAlphaBlending)
						{
							useLightBlendMode = BlendMode.NormalAlphaBlending;
							BaseGame.Device.RenderState.SourceBlend =
								Blend.SourceAlpha;
							BaseGame.Device.RenderState.DestinationBlend =
								Blend.InverseSourceAlpha;
						} // if (useLightBlendMode)
					} // else

					//Log.Write("rendering " + billboard.indices.Count / 3 +
					//  ", of type=" + billboard.tex);
					VertexPositionColorTexture[] vertices =
						billboard.vertices.ToArray();
					short[] indices = billboard.indices.ToArray();
					BaseGame.Device.DrawUserIndexedPrimitives<VertexPositionColorTexture>(
						PrimitiveType.TriangleList,
						vertices, 0, vertices.Length,
						indices, 0, indices.Length / 3);
				} // if (billboardVertices.Count)
			} // foreach (billboard)

			ShaderEffect.effectShader.Effect.CurrentTechnique.Passes[0].End();
			ShaderEffect.effectShader.Effect.End();

			//*/
			// Remove all billboards
			billboards.Clear();

			//BaseGame.Device.RenderState.AlphaTestEnable = false;
			BaseGame.Device.RenderState.DepthBufferEnable = true;
			BaseGame.Device.RenderState.DepthBufferWriteEnable = true;

			BaseGame.EnableAlphaBlending();

			BaseGame.Device.RenderState.CullMode =
				CullMode.CullCounterClockwiseFace;//.None;

			// Reset texturing
			BaseGame.Device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
			BaseGame.Device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;
		} // EndRendering()
		#endregion

		#region Billboard rendering for effects
		/// <summary>
		/// Render 3D Billboard into scene. Used for 3d effects.
		/// Set texture before hand, this is required to support both
		/// normal textures and animated textures.
		/// </summary>
		/// <param name="pos">Position in world space</param>
		/// <param name="size">Size in world coordinates</param>
		/// <param name="rotation">Rotation around billboard z axis</param>
		/// <param name="col">Color, usually white</param>
		public static void Render(XnaTexture tex, BlendMode lightBlendMode,
			Vector3 pos, float size, float rotation, Color col)
		{
			// Invisible?
			if (col.A == 0)
				return;

			TextureBillboardList texBillboard =
				GetTextureBillboard(tex, lightBlendMode);
			
			// Rotation
			Matrix rotMatrix =
				Matrix.CreateRotationZ(rotation) *
				BaseGame.InverseViewMatrix;
			Vector3 upLeft = new Vector3(-1, -1, 0);
			upLeft = Vector3.TransformNormal(upLeft, rotMatrix);
			Vector3 upRight = new Vector3(+1, -1, 0);
			upRight = Vector3.TransformNormal(upRight, rotMatrix);
			Vector3 downRight = new Vector3(+1, +1, 0);
			downRight = Vector3.TransformNormal(downRight, rotMatrix);
			Vector3 downLeft = new Vector3(-1, +1, 0);
			downLeft = Vector3.TransformNormal(downLeft, rotMatrix);

			Vector3 vec;
			vec = pos + upLeft * size;
			int index = texBillboard.vertices.Count;
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(0.0f, 0.0f)));
			vec = pos + downLeft * size;
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(0.0f, 1.0f)));
			vec = pos + downRight * size;
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(1.0f, 1.0f)));
			vec = pos + upRight * size;
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(1.0f, 0.0f)));

			texBillboard.indices.AddRange(new short[]
				{
					(short)(index+0), (short)(index+1), (short)(index+2),
					(short)(index+0), (short)(index+2), (short)(index+3),
				});
		} // Render(tex, pos, size)

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="tex">Tex</param>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="col">Color</param>
		public static void Render(XnaTexture tex,
			Vector3 pos, float size, float rotation, Color col)
		{
			Render(tex, BlendMode.NormalAlphaBlending,
				pos, size, rotation, col);
		} // Render(tex, pos, size)

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="tex">Tex</param>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="col">Color</param>
		public static void Render(Texture tex,
			Vector3 pos, float size, float rotation, Color col)
		{
			Render(tex.XnaTexture, BlendMode.NormalAlphaBlending,
				pos, size, rotation, col);
		} // Render(tex, pos, size)
		/*obs
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="tex">Tex</param>
		/// <param name="animationStep">Animation step</param>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="col">Color</param>
		public static void Render(AnimatedTexture tex, int animationStep,
			BlendMode lightBlendMode, Vector3 pos, float size, float rotation,
			Color col)
		{
			Render(tex.GetAnimatedTexture(animationStep), lightBlendMode,
				pos, size, rotation, col);
		} // Render(tex, animationStep, pos)
		 */

		/// <summary>
		/// Render 3D Billboard into scene. Used for 3d effects.
		/// Set texture before hand, this is required to support both
		/// normal textures and animated textures.
		/// This method does not support rotation (it is a bit faster).
		/// </summary>
		/// <param name="pos">Position in world space</param>
		/// <param name="size">Size in world coordinates</param>
		/// <param name="col">Color, usually white</param>
		public static void Render(XnaTexture tex, BlendMode lightBlendMode,
			Vector3 pos, float size, Color col)
		{
			// Invisible?
			if (col.A == 0)
				return;

			TextureBillboardList texBillboard =
				GetTextureBillboard(tex, lightBlendMode);
			
			Vector3 vec;
			int index = texBillboard.vertices.Count;
			vec = pos + ((-vecRight + vecUp) * size);
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(0.0f, 0.0f)));
			vec = pos + ((-vecRight - vecUp) * size);
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(0.0f, 1.0f)));
			vec = pos + ((vecRight - vecUp) * size);
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(1.0f, 1.0f)));
			vec = pos + ((vecRight + vecUp) * size);
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(1.0f, 0.0f)));

			texBillboard.indices.AddRange(new short[]
				{
					(short)(index+0), (short)(index+1), (short)(index+2),
					(short)(index+0), (short)(index+2), (short)(index+3),
				});
		} // Render(tex, pos, size)

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="tex">Tex</param>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		/// <param name="col">Color</param>
		public static void Render(Texture tex,
			Vector3 pos, float size, Color col)
		{
			Render(tex.XnaTexture, BlendMode.NormalAlphaBlending, pos, size, col);
		} // Render(tex, pos, size)

		/*
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="tex">Tex</param>
		/// <param name="animationStep">Animation step</param>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="col">Color</param>
		public static void Render(AnimatedTexture tex, int animationStep,
			BlendMode lightBlendMode, Vector3 pos, float size, Color col)
		{
			Render(tex.GetAnimatedTexture(animationStep), lightBlendMode,
				pos, size, col);
		} // Render(tex, animationStep, pos)
		 */

		/// <summary>
		/// Render on ground (Z/Y Plane), used for effects which are
		/// rendered on the ground/landscape.
		/// Does also support coloring for blending in/out effects.
		/// Set texture before hand, this is required to support both
		/// normal textures and animated textures.
		/// </summary>
		public static void RenderOnGround(XnaTexture tex,
			BlendMode blendMode,
			Vector3 pos, float size, float rotation, Color col,
			Vector3 vecGroundRight, Vector3 vecGroundUp)
		{
			// Invisible?
			if (col.A == 0)
				return;

			TextureBillboardList texBillboard =
				GetTextureBillboard(tex, blendMode);//BlendMode.NormalAlphaBlending);

			Vector3 right = vecGroundRight;
			right = Vector3.TransformNormal(right, Matrix.CreateRotationZ(rotation));
			Vector3 up = vecGroundUp;
			up = Vector3.TransformNormal(up, Matrix.CreateRotationZ(rotation));

			Vector3 vec = pos + ((-right - up) * size);
			//obs: CustomVertex.PositionColoredTextured[] billboardVertices =
			//	new CustomVertex.PositionColoredTextured[4];
			int index = texBillboard.vertices.Count;
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(0.0f, 1.0f)));
			vec = pos + ((right - up) * size);
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(1.0f, 1.0f)));
			vec = pos + ((right + up) * size);
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(1.0f, 0.0f)));
			vec = pos + ((-right + up) * size);
			texBillboard.vertices.Add(
				new VertexPositionColorTexture(
				vec, col, new Vector2(0.0f, 0.0f)));

			texBillboard.indices.AddRange(new short[]
				{
					(short)(index+0), (short)(index+1), (short)(index+2),
					(short)(index+0), (short)(index+2), (short)(index+3),
				});
		} // RenderOnGround(pos, size, col)

		/// <summary>
		/// Render on ground
		/// </summary>
		/// <param name="tex">Tex</param>
		/// <param name="pos">Position</param>
		/// <param name="size">Size</param>
		/// <param name="rotation">Rotation</param>
		/// <param name="col">Color</param>
		public static void RenderOnGround(Texture tex,
			Vector3 pos, float size, float rotation, Color col,
			Vector3 vecGroundRight, Vector3 vecGroundUp)
		{
			RenderOnGround(tex.XnaTexture, BlendMode.NormalAlphaBlending,
				pos, size, rotation, col, vecGroundRight, vecGroundUp);
		} // RenderOnGround(tex, pos, size)
		#endregion

		#region Unit testing
#if DEBUG
		/// <summary>
		/// Test render billboards
		/// </summary>
		public static void TestRenderBillboards()
		{
			Texture plasma = null, fireball = null, ring = null, smoke = null;

			TestGame.Start("Test render billboards",
				delegate
				{
					plasma = new Texture("Plasma");
					fireball = new Texture("FireBall");
					ring = new Texture("ExplosionRing");
					smoke = new Texture("Smoke");
				},
				delegate
				{
					BaseGame.Device.Clear(Color.Blue);

					/*TODO
					for (int num = 0; num < 200; num++)
					{
						BaseGame.DrawLine(
							new Vector3(-12.0f + num / 4.0f, 13.0f, 0),
							new Vector3(-17.0f + num / 4.0f, -13.0f, 0),
							new Color((byte)(255 - num), 14, (byte)num));
					} // for
					 */

					TextureFont.WriteText(2, 30,
						"cam pos=" + BaseGame.CameraPos);

					Billboard.Render(plasma,
						new Vector3(-40.0f, 0.0f, 0.0f), 5.0f,
						BaseGame.TotalTimeMs * (float)Math.PI / 1000.0f,
						Color.White);
					Billboard.Render(plasma,
						new Vector3(0.0f, 0.0f, 0.0f), 10.0f, (float)Math.PI / 8,
						Color.Gray);
					Billboard.Render(plasma,
						new Vector3(40.0f, 0.0f, 0.0f), 20.0f,
						BaseGame.TotalTimeMs * (float)Math.PI / 5000.0f,
						Color.Red);

					Billboard.Render(fireball,
						new Vector3(-40.0f, +50.0f, 0.0f), 5.0f, 0,
						Color.White);
					Billboard.Render(fireball,
						new Vector3(0.0f, +50.0f, 0.0f), 10.0f, (float)Math.PI / 8,
						Color.Yellow);
					Billboard.Render(fireball,
						new Vector3(40.0f, +50.0f, 0.0f), 20.0f, (float)Math.PI * 3 / 8,
						Color.Red);

					Billboard.RenderOnGround(ring,
						new Vector3(-25.0f, 0.0f, -100.0f), 5.0f, 0, Color.White,
						vecGroundRight, vecGroundUp);
					Billboard.RenderOnGround(ring,
						new Vector3(0.0f, 0.0f, -100.0f), 10.0f, (float)Math.PI / 8,
						Color.Blue,
						vecGroundRight, vecGroundUp);
					Billboard.RenderOnGround(ring,
						new Vector3(25.0f, 0.0f, -100.0f), 20.0f,
						BaseGame.TotalTimeMs * (float)Math.PI / 5000.0f, Color.White,
						vecGroundRight, vecGroundUp);

					Billboard.RenderBillboards();
				});
		} // TestRenderBillboards()
		//*/
#endif
		#endregion
	} // class Billboard
} // namespace DungeonQuest.Graphics
