// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Collision
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:38

#region Using directives
using Microsoft.Xna.Framework;
using System;
#endregion

namespace DungeonQuest.Collision
{
	/// <summary>
	/// Collision tris
	/// </summary>
	public class CollisionPolygon : BaseCollisionObject
	{
		#region Variables
		/// <summary>
		/// Indices for the three face vertices
		/// </summary>
		int[] vertexIndices;
		#endregion

		#region CollisionPolygon
		/// <summary>
		/// Create collision polygon
		/// </summary>
		/// <param name="offset">Offset</param>
		/// <param name="vert_indx">Vert _indx</param>
		/// <param name="vert_offset">Vert _offset</param>
		/// <param name="vertexPositions">Vert _pos</param>
		public CollisionPolygon(int offset, int[] vert_indx, int vert_offset,
			Vector3[] vertexPositions)
		{
			vertexIndices = new int[3];
			box = new BoxHelper(float.MaxValue, -float.MaxValue);
			for (int i = 0; i < 3; i++)
			{
				vertexIndices[i] = vert_indx[i + offset] + vert_offset;
				box.AddPoint(vertexPositions[vertexIndices[i]]);
			} // for (int)
		} // CollisionTris(offset, vert_indx, vert_offset)
		#endregion

		#region Vector3RemoveComponent
		/// <summary>
		/// remove vector component (vector3 to vector2)
		/// </summary>
		/// <param name="v">V texture coordinate</param>
		/// <param name="i">I</param>
		/// <returns>Vector 2</returns>
		public static Vector2 Vector3RemoveComponent(Vector3 v, uint i)
		{
			switch (i)
			{
				case 0: return new Vector2(v.Y, v.Z);
				case 1: return new Vector2(v.X, v.Z);
				case 2: return new Vector2(v.X, v.Y);
				default: return Vector2.Zero;
			} // switch
		} // Vector3RemoveComponent(v, i)
		#endregion

		#region EdgeIntersect
		/// <summary>
		/// Intersect edge (p1,p2) moving in direction (dir) colliding with edge
		/// (p3,p4). Return true on a collision with collision distance (dist) and
		/// intersection point (ip)
		/// </summary>
		/// <param name="p1">P 1</param>
		/// <param name="p2">P 2</param>
		/// <param name="dir">Dir</param>
		/// <param name="p3">P 3</param>
		/// <param name="p4">P 4</param>
		/// <param name="dist">Dist</param>
		/// <param name="ip">Ip</param>
		/// <returns>Bool</returns>
		public static bool EdgeIntersect(Vector3 p1, Vector3 p2, Vector3 dir,
			Vector3 p3, Vector3 p4, out float dist, out Vector3 ip)
		{
			dist = 0;
			ip = Vector3.Zero;

			// Edge vectors
			Vector3 v1 = p2 - p1;
			Vector3 v2 = p4 - p3;

			// Build plane based on edge (p1,p2) and move direction (dir)
			Vector3 planeDirection;
			float planeDistance;
			planeDirection = Vector3.Cross(v1, dir);
			planeDirection.Normalize();
			planeDistance = Vector3.Dot(planeDirection, p1);

			// If colliding edge (p3,p4) does not cross plane return no collision
			// same as if p3 and p4 on same side of plane return 0
			float temp = (Vector3.Dot(planeDirection, p3) - planeDistance) *
				(Vector3.Dot(planeDirection, p4) - planeDistance);
			if (temp > 0)
				return false;

			// If colliding edge (p3,p4) and plane are paralell return no collision
			v2.Normalize();
			temp = Vector3.Dot(planeDirection, v2);
			if (temp == 0)
				return false;

			// Compute intersection point of plane and colliding edge (p3,p4)
			ip = p3 + v2 * ((planeDistance - Vector3.Dot(planeDirection, p3)) / temp);

			// Get largest 2D plane projection
			planeDirection.X = Math.Abs(planeDirection.X);
			planeDirection.Y = Math.Abs(planeDirection.Y);
			planeDirection.Z = Math.Abs(planeDirection.Z);
			uint i;
			if (planeDirection.X > planeDirection.Y)
			{
				i = 0;
				if (planeDirection.X < planeDirection.Z)
					i = 2;
			} // if (planeDirection.X)
			else
			{
				i = 1;
				if (planeDirection.Y < planeDirection.Z)
					i = 2;
			} // else

			// Remove component with largest absolute value 
			Vector2 p1_2d = CollisionPolygon.Vector3RemoveComponent(p1, i);
			Vector2 v1_2d = CollisionPolygon.Vector3RemoveComponent(v1, i);
			Vector2 ip_2d = CollisionPolygon.Vector3RemoveComponent(ip, i);
			Vector2 dir_2d = CollisionPolygon.Vector3RemoveComponent(dir, i);

			// Compute distance of intersection from line (ip,-dir) to line (p1,p2)
			dist =
				(v1_2d.X * (ip_2d.Y - p1_2d.Y) - v1_2d.Y * (ip_2d.X - p1_2d.X)) /
				(v1_2d.X * dir_2d.Y - v1_2d.Y * dir_2d.X);
			if (dist < 0)
				return false;

			// Compute intesection point on edge (p1,p2)
			ip -= dist * dir;

			// Check if intersection point (ip) is between egde (p1,p2) vertices
			temp = Vector3.Dot(p1 - ip, p2 - ip);
			if (temp >= 0)
				return false; // no collision

			return true; // collision found!
		} // EdgeIntersect(p1, p2, dir)
		#endregion

		#region RayTriangleIntersect
		/// <summary>
		/// Triangle intersect from
		/// http://www.graphics.cornell.edu/pubs/1997/MT97.pdf
		/// </summary>
		/// <param name="ray.origin">Ray _origin</param>
		/// <param name="ray.direction">Ray _direction</param>
		/// <param name="vert0">Vert 0</param>
		/// <param name="vert1">Vert 1</param>
		/// <param name="vert2">Vert 2</param>
		/// <param name="t">T</param>
		/// <param name="u">U texture coordinate</param>
		/// <param name="v">V texture coordinate</param>
		/// <returns>Bool</returns>
		public static bool RayTriangleIntersect(Ray ray, Vector3 vert0,
			Vector3 vert1, Vector3 vert2,	out float t, out float u, out float v)
		{
			t = 0; u = 0; v = 0;

			Vector3 edge1 = vert1 - vert0;
			Vector3 edge2 = vert2 - vert0;

			Vector3 tvec, pvec, qvec;
			float det, inv_det;

			pvec = Vector3.Cross(ray.direction, edge2);
			det = Vector3.Dot(edge1, pvec);

			if (det > -0.00001f)
				return false;

			inv_det = 1.0f / det;

			tvec = ray.origin - vert0;

			u = Vector3.Dot(tvec, pvec) * inv_det;
			if (u < -0.0001f || u > 1.0001f)
				return false;

			qvec = Vector3.Cross(tvec, edge1);

			v = Vector3.Dot(ray.direction, qvec) * inv_det;
			if (v < -0.0001f || u + v > 1.0001f)
				return false;

			t = Vector3.Dot(edge2, qvec) * inv_det;

			if (t <= 0)
				return false;

			return true;
		} // RayTriangleIntersect(ray.origin, ray.direction, vert0)
		#endregion

		#region DoesRayIntersect
		/// <summary>
		/// ray intersect face and return intersection distance, point and normal
		/// </summary>
		/// <param name="ray.origin">Ray _origin</param>
		/// <param name="ray.direction">Ray _direction</param>
		/// <param name="vertexPositions">Vert _pos</param>
		/// <param name="distance">Intersect _distance</param>
		/// <param name="collisionPosition">Intersect _position</param>
		/// <param name="collisionNormal">Intersect _normal</param>
		/// <returns>Bool</returns>
		public override bool DoesRayIntersect(Ray ray, Vector3[] vertexPositions,
			out float distance, out Vector3 collisionPosition,
			out Vector3 collisionNormal)
		{
			distance = 0.0f;
			collisionPosition = ray.origin;
			collisionNormal = Vector3.Zero;

			Vector3 v1 = vertexPositions[vertexIndices[0]];
			Vector3 v2 = vertexPositions[vertexIndices[1]];
			Vector3 v3 = vertexPositions[vertexIndices[2]];

			Vector3 uvt;
			if (CollisionPolygon.RayTriangleIntersect(ray,
				v1, v2, v3, out uvt.Z, out uvt.X, out uvt.Y))
			{
				distance = uvt.Z;
				collisionPosition = (1.0f - uvt.X - uvt.Y) * v1 +
					uvt.X * v2 + uvt.Y * v3;
				collisionNormal = Vector3.Normalize(
					Vector3.Cross(v3 - v1, v2 - v1));
				return true;
			} // if (CollisionTris.RayTriangleIntersect)
			return false;
		} // DoesRayIntersect(ray.origin, ray.direction, vertexPositions)
		#endregion

		#region DoesBoxIntersect
		/// <summary>
		/// Box intersect face and return intersection distance, point and normal
		/// </summary>
		/// <param name="rayBox">Ray box</param>
		/// <param name="ray">Ray</param>
		/// <param name="vertexPositions">Vertex positions</param>
		/// <param name="distance">Distance</param>
		/// <param name="collisionPosition">Collision position</param>
		/// <param name="collisionNormal">Collision normal</param>
		/// <returns>Bool</returns>
		public override bool DoesBoxIntersect(BoxHelper rayBox, Ray ray,
			Vector3[] vertexPositions, out float distance,
			out Vector3 collisionPosition, out Vector3 collisionNormal)
		{
			distance = float.MaxValue;
			collisionPosition = ray.origin;
			collisionNormal = Vector3.Zero;

			bool intersected = false;
			Vector3 p1, p2, p3, p4;
			uint i, j;

			BoxHelper world_box = new BoxHelper(rayBox.min + ray.origin,
				rayBox.max + ray.origin);

			Vector3[] box_verts = world_box.GetVertices();
			Vector3[] box_edges = world_box.GetEdges();

			// Intersect box edges to face edges
			for (i = 0; i < 12; i++)
			{
				// Cull edges with normal more than 135 degree from moving 
				// direction
				if (Vector3.Dot(BoxHelper.edgeNormals[i], ray.direction) <
					-0.70710678)
					continue;

				p1 = box_edges[i * 2];
				p2 = box_edges[i * 2 + 1];
				p4 = vertexPositions[vertexIndices[0]];
				for (j = 0; j < vertexIndices.Length; j++)
				{
					p3 = p4;
					p4 = vertexPositions[vertexIndices[(j + 1) % vertexIndices.Length]];

					float checkDistance;
					Vector3 position;
					if (CollisionPolygon.EdgeIntersect(p1, p2, ray.direction,
						p3, p4, out checkDistance, out position))
					{
						if (checkDistance < distance)
						{
							distance = checkDistance;
							collisionPosition = position;
							collisionNormal = Vector3.Normalize(
								Vector3.Cross(p2 - p1, p3 - p4));
							if (Vector3.Dot(ray.direction, collisionNormal) > 0)
								collisionNormal =
									Vector3.Negate(collisionNormal);
							intersected = true;
						} // if (checkDistance)
					} // if (CollisionTris.EdgeIntersect)
				} // for (j)
			} // for (i)

			// Intersect from face vertices to box
			for (i = 0; i < 3; i++)
			{
				float tnear, tfar;
				p1 = vertexPositions[vertexIndices[i]];
				int box_face_id = world_box.RayIntersect(p1, -ray.direction,
					out tnear, out tfar);
				if (box_face_id > -1)
				{
					if (tnear < distance)
					{
						distance = tnear;
						collisionPosition = p1;
						collisionNormal = -BoxHelper.faceNormals[box_face_id];
						intersected = true;
					} // if (tnear)
				} // if (box_face_id)
			} // for (i)

			// Intersect from box vertices to face polygon
			Vector3 v1 = vertexPositions[vertexIndices[0]];
			Vector3 v2 = vertexPositions[vertexIndices[1]];
			Vector3 v3 = vertexPositions[vertexIndices[2]];
			for (i = 0; i < 8; i++)
			{
				// Cull vertices with normal more than 135 degree from moving
				// direction
				if (Vector3.Dot(BoxHelper.vertexNormals[i], ray.direction) <
					-0.70710678)
					continue;

				Vector3 uvt;
				if (CollisionPolygon.RayTriangleIntersect(
					new Ray(box_verts[i], ray.direction),
					v1, v2, v3, out uvt.Z, out uvt.X, out uvt.Y))
				{
					if (uvt.Z < distance)
					{
						distance = uvt.Z;
						collisionPosition = (1.0f - uvt.X - uvt.Y) * v1 +
							uvt.X * v2 + uvt.Y * v3;
						collisionNormal = Vector3.Normalize(
							Vector3.Cross(v3 - v1, v2 - v1));
						intersected = true;
					} // if (uvt.Z)
				} // if (CollisionTris.RayTriangleIntersect)
			} // for (i)

			return intersected;
		} // DoesBoxIntersect(rayBox, ray, vertexPositions)
		#endregion
	} // class CollisionTris
} // namespace DungeonQuest.Collision
