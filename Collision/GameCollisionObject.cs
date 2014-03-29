// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Collision
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:38

#region Using directives
using System;
using System.Collections.Generic;
using System.Text;
#endregion

namespace DungeonQuest.Collision
{
	/// <summary>
	/// Collision game object. Contains a tree of dynamic elements.
	/// </summary>
	public class GameCollisionObject : BaseCollisionObject
	{
		#region Variables
		/// <summary>
		/// All tree nodes the dynamic element is included.
		/// </summary>
		List<CollisionNode> nodes;
		#endregion

		#region GameCollisionObject
		/// <summary>
		/// Create collision tree elem dynamic
		/// </summary>
		public GameCollisionObject()
			: base()
		{
		} // GameCollisionObject()
		#endregion

		#region AddToNode
		/// <summary>
		/// Add to node
		/// </summary>
		/// <param name="node">Node</param>
		public override void AddToNode(CollisionNode node)
		{
			nodes.Add(node);
		} // AddToNode()
		#endregion

		#region RemoveFromNodes
		/// <summary>
		/// Remove from nodes
		/// </summary>
		public void RemoveFromNodes()
		{
			foreach (CollisionNode node in nodes)
			{
				node.RemoveElement(this);
			} // foreach
			nodes = null;
		} // RemoveFromNodes()
		#endregion
	} // class GameCollisionObject
} // namespace DungeonQuest.Collision
