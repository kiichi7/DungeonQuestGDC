// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Graphics
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:40

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using DungeonQuest.Shaders;
using DungeonQuest.Game;
#endregion

namespace DungeonQuest.Graphics
{
	/// <summary>
	/// Helper class to render a simple plane, used in some unit tests,
	/// especially for testing the physics engine.
	/// </summary>
	class PlaneRenderer
	{
		#region Variables
		Vector3 pos;
		Plane plane;
		Material material;
		float size;
		const float Tiling = 20.0f;//10.0f;
		#endregion

		#region Constructor
		/// <summary>
		/// Create plane renderer
		/// </summary>
		/// <param name="setPos">Set position</param>
		/// <param name="setPlane">Set plane</param>
		/// <param name="setMaterial">Set material</param>
		/// <param name="setSize">Set size</param>
		public PlaneRenderer(Vector3 setPos,
			Plane setPlane, Material setMaterial, float setSize)
		{
			pos = setPos;
			plane = setPlane;
			material = setMaterial;
			size = setSize;
		} // PlaneRenderer(setPos, setPlane, setMaterial)
		#endregion

		#region Render
		/// <summary>
		/// Draw plane vertices
		/// </summary>
		private void DrawPlaneVertices()
		{
			// Calculate right and dir vectors for constructing the plane.
			// The following code might look strange, but we have to make sure
			// that we always get correct up, right and dir vectors. Cross products
			// can return (0, 0, 0) if the vectors are parallel!
			Vector3 up = plane.Normal;
			if (up.Length() == 0)
				up = new Vector3(0, 0, 1);
			Vector3 helperVec = Vector3.Cross(up, new Vector3(1, 0, 0));
			if (helperVec.Length() == 0)
				helperVec = new Vector3(0, 1, 0);
			Vector3 right = Vector3.Cross(helperVec, up);
			Vector3 dir = Vector3.Cross(up, right);
			float dist = plane.D;

			TangentVertex[] vertices = new TangentVertex[]
			{
				// Make plane VERY big and tile texture every 10 meters
				new TangentVertex(
					(-right-dir)*size+up*dist, -size/Tiling, -size/Tiling, up, right),
				new TangentVertex(
					(-right+dir)*size+up*dist, -size/Tiling, +size/Tiling, up, right),
				new TangentVertex(
					(right-dir)*size+up*dist, +size/Tiling, -size/Tiling, up, right),
				new TangentVertex(
					(right+dir)*size+up*dist, +size/Tiling, +size/Tiling, up, right),
			};

			// Draw the plane (just 2 simple triangles)
			BaseGame.Device.DrawUserPrimitives(
				PrimitiveType.TriangleStrip, vertices, 0, 2);
		} // DrawPlaneVertices()

		/// <summary>
		/// Just renders the plane with the given material.
		/// </summary>
		public void Render()
		{
			BaseGame.WorldMatrix = Matrix.CreateTranslation(pos);
			BaseGame.Device.VertexDeclaration = TangentVertex.VertexDeclaration;
			ShaderEffect.normalMapping.RenderSinglePassShader(
				material,
				//obs: "DiffuseSpecular20",
				new BaseGame.RenderDelegate(DrawPlaneVertices));
			BaseGame.WorldMatrix = Matrix.Identity;
		} // Render()
		
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="shader">Shader</param>
		/// <param name="shaderTechnique">Shader technique</param>
		public void Render(ShaderEffect shader, //obs: string shaderTechnique,
			Matrix worldMatrix)
		{
			BaseGame.WorldMatrix = worldMatrix;
			BaseGame.Device.VertexDeclaration = TangentVertex.VertexDeclaration;
			shader.RenderSinglePassShader(
				material,
				//obs: shaderTechnique,
				new BaseGame.RenderDelegate(DrawPlaneVertices));
			BaseGame.WorldMatrix = Matrix.Identity;
		} // Render(shader, shaderTechnique)

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="shader">Shader</param>
		/// <param name="shaderTechnique">Shader technique</param>
		public void Render(ShaderEffect shader)
		{
			Render(shader, Matrix.CreateTranslation(pos));
		} // Render(shader, shaderTechnique)

		/*obs
		/// <summary>
		/// Render
		/// </summary>
		/// <param name="shader">Shader</param>
		/// <param name="shaderTechnique">Shader technique</param>
		public void Render(ShaderEffect shader, string shaderTechnique)
		{
			Render(shader, //obs: shaderTechnique, 
				Matrix.CreateTranslation(pos));
		} // Render(shader, shaderTechnique)
		 */
		#endregion

		#region Use shadow
		/// <summary>
		/// Use shadow on the plane, useful for our unit tests. The plane does not
		/// throw shadows, so we don't need a GenerateShadow method.
		/// </summary>
		public void UseShadow()
		{
			// And proceed with normal rendering for UseShadow
			ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(
				Matrix.CreateTranslation(pos));

			BaseGame.Device.VertexDeclaration =
				TangentVertex.VertexDeclaration;
			ShaderEffect.shadowMapping.UseStaticShadowShader();
			DrawPlaneVertices();
		} // UseShadow()
		#endregion

		#region Unit Testing
#if DEBUG
		/// <summary>
		/// Test rendering plane xy
		/// </summary>
		static public void TestRenderingPlaneXY()
		{
			PlaneRenderer plane = null;
			TestGame.Start("TestRenderingPlaneXY",
				delegate
				{
					plane = new PlaneRenderer(
						Vector3.Zero,
						new Plane(new Vector3(0, 0, 1), 0),
						//try1: new Material("RoadCement", "RoadCementNormal"));
						new Material("CaveDetailGround", "CaveDetailGroundNormal"),
						10.0f);
				},
				delegate
				{
					//TrackLine.ShowGroundGrid();
					plane.Render();
				});
		} // TestRenderingPlaneXY()
		
		/// <summary>
		/// Test rendering plane xy
		/// </summary>
		static public void TestRenderingPlaneWithVector111()
		{
			PlaneRenderer plane = null;
			TestGame.Start("TestRenderingPlaneWithVector111",
				delegate
				{
					plane = new PlaneRenderer(
						Vector3.Zero,
						new Plane(new Vector3(1, 1, 1), 1),
						//try1: new Material("RoadCement", "RoadCementNormal"));
						new Material("CaveDetailGround", "CaveDetailGroundNormal"),
						500.0f);
				},
				delegate
				{
					//TrackLine.ShowGroundGrid();
					plane.Render();
				});
		} // TestRenderingPlaneXY()
#endif
		#endregion
	} // class PlaneRenderer
} // namespace DungeonQuest.Graphics
