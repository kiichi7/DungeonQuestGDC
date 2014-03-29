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
	/// Collision node helper class.
	/// </summary>
	public class CollisionNode
	{
		#region Variables
		/// <summary>
		/// Bounding box for this node.
		/// </summary>
		BoxHelper box;
		/// <summary>
		/// The node childs (if null node is a leaf)
		/// </summary>
		CollisionNode[] childs;
		/// <summary>
		/// list with elements included in the node (only created on leaf nodes)
		/// </summary>
		List<BaseCollisionObject> elems;
		#endregion

		#region CollisionNode
		/// <summary>
		/// Create collision node
		/// </summary>
		/// <param name="b">B</param>
		/// <param name="subdivLevel">Subdiv _level</param>
		public CollisionNode(BoxHelper setBox, uint subdivLevel)
		{
			box = setBox;
			if (subdivLevel > 0)
			{
				subdivLevel--;
				childs = new CollisionNode[8];
				BoxHelper[] childs_box = box.GetChilds();
				for (uint i = 0; i < 8; i++)
					childs[i] = new CollisionNode(childs_box[i], subdivLevel);
			} // if (subdivLevel)
		} // CollisionNode(b, subdivLevel)
		#endregion

		#region AddElement
		/// <summary>
		/// Add element
		/// </summary>
		/// <param name="collisionObject">Collision object</param>
		public void AddElement(BaseCollisionObject collisionObject)
		{
			if (collisionObject.box.DoesBoxIntersect(box) == false)
				return;

			if (childs == null)
			{
				if (elems == null)
					elems = new List<BaseCollisionObject>();
				elems.Add(collisionObject);
				collisionObject.AddToNode(this);
			} // if (childs)
			else
			{
				foreach (CollisionNode node in childs)
					node.AddElement(collisionObject);
			} // else
		} // AddElement(collisionObject)
		#endregion

		#region RemoveElement
		/// <summary>
		/// Remove element
		/// </summary>
		/// <param name="collisionObject">Collision object</param>
		public void RemoveElement(BaseCollisionObject collisionObject)
		{
			if (elems != null)
				elems.Remove(collisionObject);
		} // RemoveElement(collisionObject)
		#endregion

		#region GetElements
		/// <summary>
		/// Get elements
		/// </summary>
		/// <param name="b">B</param>
		/// <param name="e">E</param>
		/// <param name="recurse_id">Recurse _id</param>
		public void GetElements(BoxHelper checkBox,
			List<BaseCollisionObject> elements, uint id)
		{
			if (checkBox.DoesBoxIntersect(box) == false)
				return;

			if (elems != null)
			{
				foreach (BaseCollisionObject elem in elems)
				{
					if (elem.id < id)
					{
						if (elem.box.DoesBoxIntersect(checkBox))
							elements.Add(elem);
						elem.id = id;
					} // if (elem.id)
				} // foreach (elem)
			} // if (elems)

			if (childs != null)
			{
				foreach (CollisionNode node in childs)
					node.GetElements(checkBox, elements, id);
			} // if (childs)
		} // GetElements(b, e, recurse_id)
		#endregion
	} // class CollisionNode
} // namespace DungeonQuest.Collision
