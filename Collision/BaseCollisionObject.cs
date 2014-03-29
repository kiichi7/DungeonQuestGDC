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
	/// Base collision object. Some of the collision code is based on
	/// XNA Box Collider by Fabio Policarpo.
	/// http://fabio.policarpo.nom.br/xna/index.htm
	/// Most of the code is heavily refactored, there is not much original code
	/// left ^^
	/// 
	/// Other helpful links for XNA collision detection:
	/// http://www.ziggyware.com/readarticle.php?article_id=58
	/// http://www.riemers.net/eng/Tutorials/XNA/Csharp/Series2/
	/// Collision_detection.php
	/// http://xbox360homebrew.com/blogs/homebrew360/archive/2006/10/25/1177.aspx
	/// http://sharky.bluecog.co.nz/?page_id=113
	/// http://graphicdna.blogspot.com/2007/03/xna-collision-detection-part-i.html
	/// http://channel9.msdn.com/ShowPost.aspx?PostID=276041
	/// </summary>
	public class BaseCollisionObject
	{
		#region Variables
		/// <summary>
		/// Bounding box this collison object. Kinda important because we always
		/// check if something intersects with this box, there is no polygon
		/// collision checking in this game!
		/// </summary>
		public BoxHelper box;
		/// <summary>
		/// Id for this object for identificating collision objects
		/// </summary>
		public uint id = 0;
		#endregion

		#region CollisionObject
		/// <summary>
		/// Create collision object
		/// </summary>
		public BaseCollisionObject()
		{
		} // CollisionObject()
		#endregion

		#region DoesRayIntersect
		/// <summary>
		/// Does ray intersect with our bounding box?
		/// </summary>
		/// <param name="ray">Ray</param>
		/// <param name="vertexPositions">Vertex positions</param>
		/// <param name="distance">Distance</param>
		/// <param name="collisionPosition">Collision position</param>
		/// <param name="collisionNormal">Collision normal</param>
		/// <returns>Bool</returns>
		public virtual bool DoesRayIntersect(Ray ray, Vector3[] vertexPositions,
			out float distance, out Vector3 collisionPosition,
			out Vector3 collisionNormal)
		{
			distance = 0;
			collisionPosition = Vector3.Zero;
			collisionNormal = Vector3.Zero;
			return false;
		} // DoesRayIntersect(ray, vertexPositions, distance)
		#endregion

		#region DoesBoxIntersect
		/// <summary>
		/// Box intersect
		/// </summary>
		/// <param name="rayBox">Ray box</param>
		/// <param name="ray">Ray</param>
		/// <param name="vertexPositions">Vertex positions</param>
		/// <param name="distance">Distance</param>
		/// <param name="collisionPosition">Collision position</param>
		/// <param name="collisionNormal">Collision normal</param>
		/// <returns>Bool</returns>
		public virtual bool DoesBoxIntersect(BoxHelper rayBox, Ray ray,
			Vector3[] vertexPositions, out float distance,
			out Vector3 collisionPosition, out Vector3 collisionNormal)
		{
			distance = 0;
			collisionPosition = Vector3.Zero;
			collisionNormal = Vector3.Zero;
			return false;
		} // DoesBoxIntersect(rayBox, ray, vertexPositions)
		#endregion

		#region AddToNode
		/// <summary>
		/// Add to node
		/// </summary>
		/// <param name="node">Node</param>
		public virtual void AddToNode(CollisionNode node)
		{
		} // AddToNode(node)
		#endregion
	} // class BaseCollisionObject
} // namespace DungeonQuest.Collision
