// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Collision
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:37

#region Using directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace DungeonQuest.Collision
{
	/// <summary>
	/// Box helper
	/// </summary>
	public class BoxHelper
	{
		#region Variables
		/// <summary>
		/// Bounding box minimum point
		/// </summary>
		public Vector3 min;
		/// <summary>
		/// Bounding box maximum point
		/// </summary>
		public Vector3 max;

		/// <summary>
		/// Cosinus of 45 degrees
		/// </summary>
		const float Cosinus45 = 0.70710678f;
		/// <summary>
		/// Invert square root of 3 (1/sqrt(3))
		/// </summary>
		const float InvertSquareRoot3 = 0.57735027f;

		/// <summary>
		/// Vertex buffer and declaration for drawing debug box
		/// </summary>
		VertexBuffer vertexBuffer;
		/// <summary>
		/// Vertex declaration
		/// </summary>
		VertexDeclaration vd;

		/// <summary>
		/// Normals for each vertex
		/// </summary>
		public static Vector3[] vertexNormals = new Vector3[8]
      {
        new Vector3(-InvertSquareRoot3,-InvertSquareRoot3,-InvertSquareRoot3),
        new Vector3( InvertSquareRoot3, InvertSquareRoot3, InvertSquareRoot3),
        new Vector3( InvertSquareRoot3,-InvertSquareRoot3,-InvertSquareRoot3),
        new Vector3(-InvertSquareRoot3, InvertSquareRoot3, InvertSquareRoot3),
        new Vector3( InvertSquareRoot3, InvertSquareRoot3,-InvertSquareRoot3),
        new Vector3(-InvertSquareRoot3,-InvertSquareRoot3, InvertSquareRoot3),
        new Vector3(-InvertSquareRoot3, InvertSquareRoot3,-InvertSquareRoot3),
        new Vector3( InvertSquareRoot3,-InvertSquareRoot3, InvertSquareRoot3)
      };

		/// <summary>
		/// Normals for each edge
		/// </summary>
		public static Vector3[] edgeNormals = new Vector3[12]
      {
        new Vector3(-Cosinus45, 0, -Cosinus45),
        new Vector3(0, Cosinus45, -Cosinus45),
        new Vector3(Cosinus45, 0, -Cosinus45),
        new Vector3(0, -Cosinus45, -Cosinus45),
        new Vector3(0, Cosinus45, Cosinus45),
        new Vector3(-Cosinus45, 0, Cosinus45),
        new Vector3(0, -Cosinus45, Cosinus45),
        new Vector3( Cosinus45, 0, Cosinus45),
        new Vector3(-Cosinus45,-Cosinus45, 0),
        new Vector3(-Cosinus45, Cosinus45, 0),
        new Vector3( Cosinus45, Cosinus45, 0),
        new Vector3( Cosinus45,-Cosinus45, 0)
      };

		/// <summary>
		/// Normals for each face
		/// </summary>
		public static Vector3[] faceNormals = new Vector3[6]
      {
        new Vector3(1,0,0),
        new Vector3(0,1,0),
        new Vector3(0,0,1),
        new Vector3(-1,0,0),
        new Vector3(0,-1,0),
        new Vector3(0,0,-1)
      };
		#endregion

		#region Constructors
		/// <summary>
		/// Create box helper
		/// </summary>
		/// <param name="min">Minimum</param>
		/// <param name="max">Maximum</param>
		public BoxHelper(float min, float max)
		{
			this.min = new Vector3(min, min, min);
			this.max = new Vector3(max, max, max);
		} // BoxHelper(min, max)

		/// <summary>
		/// Create box helper
		/// </summary>
		/// <param name="min">Minimum</param>
		/// <param name="max">Maximum</param>
		public BoxHelper(Vector3 min, Vector3 max)
		{
			this.min = min;
			this.max = max;
		} // BoxHelper(min, max)

		/// <summary>
		/// Constructor from another collision box
		/// </summary>
		/// <param name="bb">box</param>
		public BoxHelper(BoxHelper bb)
		{
			min = bb.min;
			max = bb.max;
		} // BoxHelper()
		#endregion

		#region AddPoint
		/// <summary>
		/// Adds a point to the bounding box extending it if needed
		/// </summary>
		/// <param name="p">P</param>
		public void AddPoint(Vector3 p)
		{
			if (p.X >= max.X)
				max.X = p.X;
			if (p.Y >= max.Y)
				max.Y = p.Y;
			if (p.Z >= max.Z)
				max.Z = p.Z;

			if (p.X <= min.X)
				min.X = p.X;
			if (p.Y <= min.Y)
				min.Y = p.Y;
			if (p.Z <= min.Z)
				min.Z = p.Z;
		} // AddPoint()
		#endregion

		#region GetCenter
		/// <summary>
		/// Get the bounding box center point
		/// </summary>
		/// <returns>Vector 3</returns>
		public Vector3 GetCenter()
		{
			return 0.5f * (min + max);
		} // GetCenter()
		#endregion

		#region DoesBoxIntersect
		/// <summary>
		/// Check if two bounding boxes have any intersection
		/// </summary>
		/// <param name="bb">Bb</param>
		/// <returns>Bool</returns>
		public bool DoesBoxIntersect(BoxHelper bb)
		{
			if (max.X >= bb.min.X && min.X <= bb.max.X &&
					max.Y >= bb.min.Y && min.Y <= bb.max.Y &&
					max.Z >= bb.min.Z && min.Z <= bb.max.Z)
				return true;
			return false;
		} // DoesBoxIntersect()
		#endregion

		#region PointInside
		/// <summary>
		/// Check if a point in inside the bounding box
		/// </summary>
		/// <param name="p">P</param>
		/// <returns>Bool</returns>
		public bool PointInside(Vector3 p)
		{
			return p.X > min.X && p.X <= max.X &&
				p.Y > min.Y && p.Y <= max.Y &&
				p.Z > min.Z && p.Z <= max.Z;
		} // PointInside()
		#endregion

		#region GetChilds
		/// <summary>
		/// Split in middle point creating 8 childs
		/// </summary>
		public BoxHelper[] GetChilds()
		{
			Vector3 center = 0.5f * (min + max);

			BoxHelper[] childs = new BoxHelper[8];

			childs[0] = new BoxHelper(min, center);
			childs[1] = new BoxHelper(new Vector3(center.X, min.Y, min.Z),
				new Vector3(max.X, center.Y, center.Z));
			childs[2] = new BoxHelper(new Vector3(min.X, center.Y, min.Z),
				new Vector3(center.X, max.Y, center.Z));
			childs[3] = new BoxHelper(new Vector3(center.X, center.Y, min.Z),
				new Vector3(max.X, max.Y, center.Z));
			childs[4] = new BoxHelper(new Vector3(min.X, min.Y, center.Z),
				new Vector3(center.X, center.Y, max.Z));
			childs[5] = new BoxHelper(new Vector3(center.X, min.Y, center.Z),
				new Vector3(max.X, center.Y, max.Z));
			childs[6] = new BoxHelper(new Vector3(min.X, center.Y, center.Z),
				new Vector3(center.X, max.Y, max.Z));
			childs[7] = new BoxHelper(center, max);

			return childs;
		} // GetChilds()
		#endregion

		#region GetVertices
		/// <summary>
		/// Get the 8 bounding box vertices
		/// </summary>
		public Vector3[] GetVertices()
		{
			Vector3[] vertices = new Vector3[8];

			vertices[0] = min;
			vertices[1] = max;
			vertices[2] = new Vector3(max.X, min.Y, min.Z);
			vertices[3] = new Vector3(min.X, max.Y, max.Z);
			vertices[4] = new Vector3(max.X, max.Y, min.Z);
			vertices[5] = new Vector3(min.X, min.Y, max.Z);
			vertices[6] = new Vector3(min.X, max.Y, min.Z);
			vertices[7] = new Vector3(max.X, min.Y, max.Z);

			return vertices;
		} // GetVertices()
		#endregion

		#region GetEdges
		/// <summary>
		/// Get the 12 edges (each edge in list made of two 3D points)
		/// </summary>
		public Vector3[] GetEdges()
		{
			Vector3[] vertices = GetVertices();

			Vector3[] edges = new Vector3[24];

			edges[0] = vertices[0]; edges[1] = vertices[6];
			edges[2] = vertices[6]; edges[3] = vertices[4];
			edges[4] = vertices[4]; edges[5] = vertices[2];
			edges[6] = vertices[2]; edges[7] = vertices[0];
			edges[8] = vertices[1]; edges[9] = vertices[3];
			edges[10] = vertices[3]; edges[11] = vertices[5];
			edges[12] = vertices[5]; edges[13] = vertices[7];
			edges[14] = vertices[7]; edges[15] = vertices[1];
			edges[16] = vertices[0]; edges[17] = vertices[5];
			edges[18] = vertices[3]; edges[19] = vertices[6];
			edges[20] = vertices[4]; edges[21] = vertices[1];
			edges[22] = vertices[7]; edges[23] = vertices[2];

			return edges;
		} // GetEdges()
		#endregion

		#region RayIntersect
		/// <summary>
		/// Collide ray defined by ray origin (ro) and ray direction (rd) with
		/// the bound box. Returns -1 on no collision and the face index (0 to 5)
		/// if a collision is found together with the distances to the collision
		/// points.
		/// </summary>
		/// <param name="ro">Ro</param>
		/// <param name="rd">Rd</param>
		/// <param name="tnear">Tnear</param>
		/// <param name="tfar">Tfar</param>
		/// <returns>Int</returns>
		public int RayIntersect(Vector3 ro, Vector3 rd, out float tnear,
			out float tfar)
		{
			float t1, t2, t;

			tnear = -float.MaxValue;
			tfar = float.MaxValue;

			int face, i = -1, j = -1;

			// intersect in X
			if (rd.X > -0.00001f && rd.X < -0.00001f)
			{
				if (ro.X < min.X || ro.X > max.X)
					return -1;
			} // if (rd.X)
			else
			{
				t = 1.0f / rd.X;
				t1 = (min.X - ro.X) * t;
				t2 = (max.X - ro.X) * t;

				if (t1 > t2)
				{
					t = t1; t1 = t2; t2 = t;
					face = 0;
				} // if (t1)
				else
					face = 3;

				if (t1 > tnear)
				{
					tnear = t1;
					i = face;
				} // if (t1)
				if (t2 < tfar)
				{
					tfar = t2;
					if (face > 2)
						j = face - 3;
					else
						j = face + 3;
				} // if (t2)

				if (tnear > tfar || tfar < 0.00001f)
					return -1;
			} // else

			// intersect in Y
			if (rd.Y > -0.00001f && rd.Y < -0.00001f)
			{
				if (ro.Y < min.Y || ro.Y > max.Y)
					return -1;
			} // if (rd.Y)
			else
			{
				t = 1.0f / rd.Y;
				t1 = (min.Y - ro.Y) * t;
				t2 = (max.Y - ro.Y) * t;

				if (t1 > t2)
				{
					t = t1; t1 = t2; t2 = t;
					face = 1;
				} // if (t1)
				else
					face = 4;

				if (t1 > tnear)
				{
					tnear = t1;
					i = face;
				} // if (t1)
				if (t2 < tfar)
				{
					tfar = t2;
					if (face > 2)
						j = face - 3;
					else
						j = face + 3;
				} // if (t2)

				if (tnear > tfar || tfar < 0.00001f)
					return -1;
			} // else

			// intersect in Z
			if (rd.Z > -0.00001f && rd.Z < -0.00001f)
			{
				if (ro.Z < min.Z || ro.Z > max.Z)
					return -1;
			} // if (rd.Z)
			else
			{
				t = 1.0f / rd.Z;
				t1 = (min.Z - ro.Z) * t;
				t2 = (max.Z - ro.Z) * t;

				if (t1 > t2)
				{
					t = t1; t1 = t2; t2 = t;
					face = 2;
				} // if (t1)
				else
					face = 5;

				if (t1 > tnear)
				{
					tnear = t1;
					i = face;
				} // if (t1)
				if (t2 < tfar)
				{
					tfar = t2;
					if (face > 2)
						j = face - 3;
					else
						j = face + 3;
				} // if (t2)
			} // else

			if (tnear > tfar || tfar < 0.00001f)
				return -1;

			if (tnear < 0.0f)
				return j;
			else
				return i;
		} // RayIntersect(ro, rd, tnear)
		#endregion

		#region Draw
		/// <summary>
		/// Render the bounding box as wireframe
		/// </summary>
		/// <param name="gd">Gd</param>
		public void Draw(GraphicsDevice gd)
		{
			Vector3[] edges = GetEdges();
			if (vertexBuffer == null)
			{
				vertexBuffer = new VertexBuffer(gd,
					VertexPositionColor.SizeInBytes * 24,
					BufferUsage.WriteOnly);
				vd = new VertexDeclaration(gd,
					VertexPositionColor.VertexElements);
			} // if (vertexBuffer)

			VertexPositionColor[] v = new VertexPositionColor[24];
			for (int i = 0; i < 24; i += 2)
			{
				v[i].Position = edges[i];
				v[i].Color = Color.Red;
				v[i + 1].Position = edges[i + 1];
				v[i + 1].Color = Color.Red;
			} // for (int)
			vertexBuffer.SetData<VertexPositionColor>(v);

			gd.RenderState.DepthBias = -0.1f;

			gd.VertexDeclaration = vd;
			gd.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);
			gd.DrawPrimitives(PrimitiveType.LineList, 0, 12);

			gd.RenderState.DepthBias = 0.0f;
		} // Draw()
		#endregion
	} // class BoxHelper
} // namespace DungeonQuest.Collision
