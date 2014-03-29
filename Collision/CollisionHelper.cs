// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Collision
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:37

#region Using directives
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
#endregion

namespace DungeonQuest.Collision
{
	/// <summary>
	/// Helper class to manage all the collision testing, keeps the tree
	/// Collision node root node and got helper methods to
	/// add and remove elments as well as the top level collision move methods.
	/// </summary>
	public class CollisionHelper
	{
		#region Variables
		/// <summary>
		/// The tree root node
		/// </summary>
		CollisionNode root;
		/// <summary>
		/// Id for this object for identificating collision objects
		/// </summary>
		uint id = 0;
		#endregion

		#region CollisionHelper
		/// <summary>
		/// Create collision helper
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="subdivLevel">Subdiv _level</param>
		public CollisionHelper(BoxHelper box, uint subdivLevel)
		{
			root = new CollisionNode(box, subdivLevel);
			id = 0;
		} // CollisionHelper(box, subdivLevel)
		#endregion

		#region AddElement
		/// <summary>
		/// Add element
		/// </summary>
		/// <param name="elem">Elem</param>
		public void AddElement(BaseCollisionObject elem)
		{
			root.AddElement(elem);
		} // AddElement(elem)
		#endregion

		#region RemoveElement
		/// <summary>
		/// Remove element
		/// </summary>
		/// <param name="dyn_elem">Dyn _elem</param>
		public void RemoveElement(GameCollisionObject gameElement)
		{
			gameElement.RemoveFromNodes();
		} // RemoveElement(gameElement)
		#endregion

		#region GetElements
		/// <summary>
		/// Get elements
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="elements">Elements</param>
		public void GetElements(BoxHelper box, List<BaseCollisionObject> elements)
		{
			root.GetElements(box, elements, ++id);
		} // GetElements(box, elements)
		#endregion

		#region PointMove
		/// <summary>
		/// Point move
		/// </summary>
		/// <param name="pointStart">Point start</param>
		/// <param name="pointEnd">Point end</param>
		/// <param name="vertexPositions">Vertex positions</param>
		/// <param name="frictionFactor">Friction factor</param>
		/// <param name="bumpFactor">Bump mapping factor</param>
		/// <param name="recurseLevel">Recurse level</param>
		/// <param name="pointResult">Point result</param>
		/// <param name="velocityResult">Velocity result</param>
		/// <param name="polyPoint">Poly point</param>
		public void PointMove(Vector3 pointStart, Vector3 pointEnd,
			Vector3[] vertexPositions, float frictionFactor, float bumpFactor,
			uint recurseLevel, out Vector3 pointResult, ref Vector3 velocityResult,
			out Vector3 polyPoint)
		{
			pointResult = pointStart;
			polyPoint = Vector3.Zero;

			Vector3 delta = pointEnd - pointStart;
			float deltaLength = delta.Length();
			if (deltaLength < 0.00001f)
				return;

			float total_dist = deltaLength;
			delta *= 1.0f / deltaLength;

			while (recurseLevel > 0)
			{
				float dist;
				Vector3 pos, norm;
				if (false == DoesRayIntersect(
					Ray.FromStartAndEnd(pointStart, pointEnd),
					vertexPositions, out dist, out pos, out norm))
				{
					pointStart = pointEnd;
					break;
				} // if (false)

				if (dist > 0.01f)
				{
					dist -= 0.01f;

					pointStart += delta * dist;
					total_dist -= dist;
				} // if (dist)
				else
					dist = 0.0f;

				Vector3 reflect_dir = Vector3.Reflect(delta, norm);

				Vector3 n = norm * Vector3.Dot(reflect_dir, norm);
				Vector3 t = reflect_dir - n;

				reflect_dir = frictionFactor * t + bumpFactor * n;
				if (polyPoint == Vector3.Zero)
					polyPoint = pointStart;
				pointEnd = pointStart + reflect_dir * total_dist;

				delta = pointEnd - pointStart;
				deltaLength = delta.Length();
				if (deltaLength < 0.00001f)
					break;
				delta *= 1.0f / deltaLength;

				recurseLevel--;
			} // while (recurseLevel)

			pointResult = pointStart;
			velocityResult = delta * velocityResult.Length();
		} // PointMove(pointStart, pointEnd, vertexPositions)
		#endregion

		#region BoxMove
		/// <summary>
		/// Box move
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="pointStart">Point start</param>
		/// <param name="pointEnd">Point end</param>
		/// <param name="vertexPositions">Vertex positions</param>
		/// <param name="frictionFactor">Friction factor</param>
		/// <param name="bumpFactor">Bump mapping factor</param>
		/// <param name="recurseLevel">Recurse level</param>
		/// <param name="pointResult">Point result</param>
		/// <param name="velocityResult">Velocity result</param>
		public void BoxMove(BoxHelper box, Vector3 pointStart, Vector3 pointEnd,
			Vector3[] vertexPositions, float frictionFactor, float bumpFactor,
			uint recurseLevel, out Vector3 pointResult, ref Vector3 velocityResult)
		{
			pointResult = pointStart;

			Vector3 delta = pointEnd - pointStart;
			float deltaLength = delta.Length();
			if (deltaLength < 0.00001f)
				return;

			float total_dist = deltaLength;
			delta *= 1.0f / deltaLength;

			Vector3 main_dir = delta;

			while (recurseLevel > 0)
			{
				float dist;
				Vector3 pos, norm;
				if (false == DoesBoxIntersect(box,
					Ray.FromStartAndEnd(pointStart, pointEnd),
					vertexPositions, out dist, out pos, out norm))
				{
					pointStart = pointEnd;
					break;
				} // if (false)

				if (dist > 0.01f)
				{
					pointStart += delta * (dist - 0.01f);
					total_dist -= dist;
				} // if (dist)

				Vector3 reflect_dir = Vector3.Reflect(delta, norm);

				Vector3 n = norm * Vector3.Dot(reflect_dir, norm);
				Vector3 t = reflect_dir - n;

				reflect_dir = frictionFactor * t + bumpFactor * n;

				pointEnd = pointStart + reflect_dir * total_dist;

				delta = pointEnd - pointStart;
				deltaLength = delta.Length();
				if (deltaLength < 0.00001f)
					break;
				delta *= 1.0f / deltaLength;

				recurseLevel--;
			} // while (recurseLevel)

			pointResult = pointStart;
			velocityResult = delta * velocityResult.Length();
		} // BoxMove(box, pointStart, pointEnd)
		#endregion

		#region DoesRayIntersect
		/// <summary>
		/// Point intersect
		/// </summary>
		/// <param name="ray">Ray</param>
		/// <param name="vertexPositions">Vertex positions</param>
		/// <param name="distance">Distance</param>
		/// <param name="collisionPosition">Collision position</param>
		/// <param name="collisionNormal">Collision normal</param>
		/// <returns>Bool</returns>
		public bool DoesRayIntersect(Ray ray, Vector3[] vertexPositions,
			out float distance, out Vector3 collisionPosition,
			out Vector3 collisionNormal)
		{
			distance = 0.0f;
			collisionPosition = ray.origin;
			collisionNormal = Vector3.Zero;

			if (ray.Length == 0)
				return false;

			BoxHelper rayBox = new BoxHelper(float.MaxValue, -float.MaxValue);
			rayBox.AddPoint(ray.origin);
			rayBox.AddPoint(ray.EndPosition);
			Vector3 inflate = new Vector3(0.001f, 0.001f, 0.001f);
			rayBox.min -= inflate;
			rayBox.max += inflate;

			List<BaseCollisionObject> elems = new List<BaseCollisionObject>();
			root.GetElements(rayBox, elems, ++id);

			ray.direction *= 1.0f / ray.Length;
			distance = ray.Length;

			bool intersected = false;

			foreach (BaseCollisionObject collisionObject in elems)
			{
				float checkDistance;
				Vector3 position;
				Vector3 normal;
				if (true == collisionObject.DoesRayIntersect(ray,
					vertexPositions, out checkDistance, out position, out normal))
				{
					if (checkDistance < distance)
					{
						distance = checkDistance;
						collisionPosition = position;
						collisionNormal = normal;
						intersected = true;
					} // if (checkDistance)
				} // if (true)
			} // foreach (collisionObject)

			return intersected;
		} // DoesRayIntersect(ray, vertexPositions, distance)
		#endregion

		#region DoesBoxIntersect
		/// <summary>
		/// Box intersect
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="ray">Ray</param>
		/// <param name="vertexPositions">Vertex positions</param>
		/// <param name="distance">Distance</param>
		/// <param name="collisionPosition">Collision position</param>
		/// <param name="collisionNormal">Collision normal</param>
		/// <returns>Bool</returns>
		public bool DoesBoxIntersect(BoxHelper box, Ray ray,
			Vector3[] vertexPositions, out float distance,
			out Vector3 collisionPosition, out Vector3 collisionNormal)
		{
			distance = 0.0f;
			collisionPosition = ray.origin;
			collisionNormal = Vector3.Zero;

			if (ray.Length == 0)
				return false;

			BoxHelper rayBox = new BoxHelper(box.min + ray.origin,
				box.max + ray.origin);
			rayBox.AddPoint(rayBox.min + ray.direction);
			rayBox.AddPoint(rayBox.max + ray.direction);
			Vector3 inflate = new Vector3(0.001f, 0.001f, 0.001f);
			rayBox.min -= inflate;
			rayBox.max += inflate;

			List<BaseCollisionObject> elems = new List<BaseCollisionObject>();
			root.GetElements(rayBox, elems, ++id);

			ray.direction *= 1.0f / ray.Length;
			distance = ray.Length;

			bool intersected = false;

			foreach (BaseCollisionObject e in elems)
			{
				float checkDistance;
				Vector3 position;
				Vector3 normal;
				if (true == e.DoesBoxIntersect(box, ray,
					vertexPositions, out checkDistance, out position, out normal))
				{
					if (checkDistance < distance)
					{
						distance = checkDistance;
						collisionPosition = position;
						collisionNormal = normal;
						intersected = true;
					} // if (checkDistance)
				} // if (true)
			} // foreach

			return intersected;
		} // DoesBoxIntersect(box, ray, vertexPositions)
		#endregion
	} // class CollisionHelper
} // namespace DungeonQuest.Collision
