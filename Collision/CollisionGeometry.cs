// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Collision
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:37

#region Using directives
using DungeonQuest.Helpers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
#endregion

namespace DungeonQuest.Collision
{
	/// <summary>
	/// Collision mesh
	/// </summary>
	class CollisionGeometry
	{
		#region Constants
		/// <summary>
		/// Default Extension for Collada files exported with 3ds max.
		/// </summary>
		public const string ColladaDirectory = "Content\\Models",
			ColladaExtension = "DAE";
		#endregion

		#region Variables
		/// <summary>
		/// Vectors for this mesh, we don't care about anything else!
		/// </summary>
		private Vector3[] vectors;
		/// <summary>
		/// Indices for all the triangles. Should be optimized together with
		/// the vectors (no duplicates there) for the best performance.
		/// This array is just used for loading, for processing the collision
		/// we use the collision faces and the collision tree!
		/// </summary>
		private int[] indices;

		/// <summary>
		/// Collision mesh faces
		/// </summary>
		CollisionPolygon[] faces;
		/// <summary>
		/// Collision tree with meshes faces
		/// </summary>
		CollisionHelper tree;
		#endregion

		#region Constructor
		/// <summary>
		/// Create a collision mesh from a collada file
		/// </summary>
		/// <param name="setName">Set name</param>
		public CollisionGeometry(string setName)
		{
			// Set name to identify this model and build the filename
			string filename = Path.Combine(ColladaDirectory,
				StringHelper.ExtractFilename(setName, true) + "." +
				ColladaExtension);

			// Load file
			Stream file = File.OpenRead(filename);
			string colladaXml = new StreamReader(file).ReadToEnd();
			XmlNode colladaFile = XmlHelper.LoadXmlFromText(colladaXml);

			// Load mesh (vectors and indices)
			LoadMesh(colladaFile);

			// Close file, we are done.
			file.Close();
		} // CollisionMesh(setName)
		#endregion

		#region Load collada matrix helper method
		/// <summary>
		/// Create matrix from collada float value array. The matrices in collada
		/// are stored differently from the xna format (row based instead of
		/// columns).
		/// </summary>
		/// <param name="mat">mat</param>
		/// <returns>Matrix</returns>
		protected Matrix LoadColladaMatrix(float[] mat, int offset)
		{
			return new Matrix(
				mat[offset + 0], mat[offset + 4], mat[offset + 8], mat[offset + 12],
				mat[offset + 1], mat[offset + 5], mat[offset + 9], mat[offset + 13],
				mat[offset + 2], mat[offset + 6], mat[offset + 10], mat[offset + 14],
				mat[offset + 3], mat[offset + 7], mat[offset + 11], mat[offset + 15]);
		} // LoadColladaMatrix(mat, offset)

		/// <summary>
		/// Only scale transformation down to globalScaling, left rest of
		/// the matrix intact. This is required for rendering because all the
		/// models have their own scaling!
		/// </summary>
		/// <param name="mat">Matrix</param>
		/// <returns>Matrix</returns>
		protected Matrix OnlyScaleTransformation(Matrix mat)
		{
			mat.Translation = mat.Translation * globalScaling;
			return mat;
		} // OnlyScaleTransformation(mat)

		/// <summary>
		/// Only scale transformation inverse
		/// </summary>
		/// <param name="mat">Matrix</param>
		/// <returns>Matrix</returns>
		protected Matrix OnlyScaleTransformationInverse(Matrix mat)
		{
			mat.Translation = mat.Translation / globalScaling;
			return mat;
		} // OnlyScaleTransformationInverse(mat)
		#endregion

		#region Load mesh
		/// <summary>
		/// Load mesh, must be called after we got all bones. Will also create
		/// the vertex and index buffers and optimize the vertices as much as
		/// we can.
		/// </summary>
		/// <param name="colladaFile">Collada file</param>
		private void LoadMesh(XmlNode colladaFile)
		{
			XmlNode geometrys =
				XmlHelper.GetChildNode(colladaFile, "library_geometries");
			if (geometrys == null)
				throw new InvalidOperationException(
					"library_geometries node not found in collision file");

			foreach (XmlNode geometry in geometrys)
				if (geometry.Name == "geometry")
				{
					// Load everything from the mesh node
					LoadMeshGeometry(colladaFile,
						XmlHelper.GetChildNode(colladaFile, "mesh"),
						XmlHelper.GetXmlAttribute(geometry, "name"));

					// Optimize vertices first and build index buffer from that!
					indices = OptimizeVertexBuffer();

					// Copy and create everything to CollisionFace
					faces = new CollisionPolygon[indices.Length / 3];
					for (int i = 0; i < indices.Length / 3; i++)
					{
						faces[i] = new CollisionPolygon(i * 3, indices, 0, vectors);
					} // for (int)

					BoxHelper box = new BoxHelper(float.MaxValue, -float.MaxValue);
					for (int i = 0; i < vectors.Length; i++)
						box.AddPoint(vectors[i]);

					uint subdivLevel = 4; // max 8^6 nodes
					tree = new CollisionHelper(box, subdivLevel);
					for (int i = 0; i < faces.Length; i++)
						tree.AddElement(faces[i]);

					// Get outa here, we currently only support one single mesh!
					return;
				} // foreach if (geometry.Name)
		} // LoadMesh(colladaFile)

		/// <summary>
		/// Helpers to remember how we can reuse vertices for OptimizeVertexBuffer.
		/// See below for more details.
		/// </summary>
		int[] reuseVertexPositions;
		/// <summary>
		/// Reverse reuse vertex positions, this one is even more important because
		/// this way we can get a list of used vertices for a shared vertex pos.
		/// </summary>
		List<int>[] reverseReuseVertexPositions;

		/// <summary>
		/// Global scaling we get for importing the mesh and all positions.
		/// This is important because 3DS max might use different units than we are.
		/// </summary>
		protected float globalScaling = 1.0f;

		/// <summary>
		/// Object matrix. Not really needed here.
		/// </summary>
		Matrix objectMatrix = Matrix.Identity;

		/// <summary>
		/// Load mesh geometry
		/// </summary>
		/// <param name="geometry"></param>
		private void LoadMeshGeometry(XmlNode colladaFile,
			XmlNode meshNode, string meshName)
		{
			#region Load all source nodes
			Dictionary<string, List<float>> sources = new Dictionary<string,
				List<float>>();
			foreach (XmlNode node in meshNode)
			{
				if (node.Name != "source")
					continue;
				XmlNode floatArray = XmlHelper.GetChildNode(node, "float_array");
				List<float> floats = new List<float>(
					StringHelper.ConvertStringToFloatArray(floatArray.InnerText));

				// Fill the array up
				int count = Convert.ToInt32(XmlHelper.GetXmlAttribute(floatArray,
					"count"), NumberFormatInfo.InvariantInfo);
				while (floats.Count < count)
					floats.Add(0.0f);

				sources.Add(XmlHelper.GetXmlAttribute(node, "id"), floats);
			} // foreach (node)
			#endregion

			#region Vertices
			// Also add vertices node, redirected to position node into sources
			XmlNode verticesNode = XmlHelper.GetChildNode(meshNode, "vertices");
			XmlNode posInput = XmlHelper.GetChildNode(verticesNode, "input");
			if (XmlHelper.GetXmlAttribute(posInput, "semantic").ToLower(
				CultureInfo.InvariantCulture) != "position")
				throw new InvalidOperationException(
					"unsupported feature found in collada \"vertices\" node");
			string verticesValueName = XmlHelper.GetXmlAttribute(posInput,
				"source").Substring(1);
			sources.Add(XmlHelper.GetXmlAttribute(verticesNode, "id"),
				sources[verticesValueName]);
			#endregion

			#region Get the global scaling from the exported units to meters!
			XmlNode unitNode = XmlHelper.GetChildNode(
				XmlHelper.GetChildNode(colladaFile, "asset"), "unit");
			globalScaling = Convert.ToSingle(
				XmlHelper.GetXmlAttribute(unitNode, "meter"),
                NumberFormatInfo.InvariantInfo);
			#endregion

			#region Get the object matrix (its in the visual_scene node at the end)
			XmlNode sceneStuff = XmlHelper.GetChildNode(
				XmlHelper.GetChildNode(colladaFile, "library_visual_scenes"),
				"visual_scene");
			if (sceneStuff == null)
				throw new InvalidOperationException(
					"library_visual_scenes node not found in collision file");

			// Search for the node with the name of this geometry.
			foreach (XmlNode node in sceneStuff)
				if (node.Name == "node" &&
					XmlHelper.GetXmlAttribute(node, "name") == meshName &&
					XmlHelper.GetChildNode(node, "matrix") != null)
				{
					// Get the object matrix
					objectMatrix = LoadColladaMatrix(
						StringHelper.ConvertStringToFloatArray(
						XmlHelper.GetChildNode(node, "matrix").InnerText), 0) *
						Matrix.CreateScale(globalScaling);
					break;
				} // foreach if (node.Name)
			#endregion

			#region Construct all triangle polygons from the vertex data
			// Construct and generate vertices lists. Every 3 vertices will
			// span one triangle polygon, but everything is optimized later.
			foreach (XmlNode trianglenode in meshNode)
			{
				if (trianglenode.Name != "triangles")
					continue;

				// Find data source nodes
				XmlNode positionsnode = XmlHelper.GetChildNode(trianglenode,
					"semantic", "VERTEX");
				// All the rest is ignored

				// Get the data of the sources
				List<float> positions = sources[XmlHelper.GetXmlAttribute(
					positionsnode, "source").Substring(1)];

				// Find the Offsets
				int positionsoffset = Convert.ToInt32(XmlHelper.GetXmlAttribute(
					positionsnode, "offset"), NumberFormatInfo.InvariantInfo);

				// Get the indexlist
				XmlNode p = XmlHelper.GetChildNode(trianglenode, "p");
				int[] pints = StringHelper.ConvertStringToIntArray(p.InnerText);
				int trianglecount = Convert.ToInt32(XmlHelper.GetXmlAttribute(
					trianglenode, "count"), NumberFormatInfo.InvariantInfo);
				// The number of ints that form one vertex:
				int vertexcomponentcount = pints.Length / trianglecount / 3;

				// Construct data
				// Initialize reuseVertexPositions and reverseReuseVertexPositions
				// to make it easier to use them below
				reuseVertexPositions = new int[trianglecount * 3];
				reverseReuseVertexPositions = new List<int>[positions.Count / 3];
				for (int i = 0; i < reverseReuseVertexPositions.Length; i++)
					reverseReuseVertexPositions[i] = new List<int>();

				vectors = new Vector3[trianglecount * 3];

				// We have to use int indices here because we often have models
				// with more than 64k triangles (even if that gets optimized later).
				for (int i = 0; i < trianglecount * 3; i++)
				{
					// Position
					int pos = pints[i * vertexcomponentcount + positionsoffset] * 3;
					Vector3 position = new Vector3(
						positions[pos], positions[pos + 1], positions[pos + 2]);
					// For the cave mesh we are not going to use a world matrix,
					// Just scale everything correctly right here!
					position *= globalScaling;

					// Set the vertex
					vectors[i] = position;

					// Remember pos for optimizing the vertices later more easily.
					reuseVertexPositions[i] = pos / 3;
					reverseReuseVertexPositions[pos / 3].Add(i);
				} // for (int)

				// Only support one mesh for now, get outta here.
				return;
			} // foreach (trianglenode)

			throw new InvalidOperationException(
				"No mesh found in this collada file, unable to continue!");
			#endregion
		} // LoadMeshGeometry(colladaFile, meshNode, meshName)
		#endregion

		#region Optimize vertex buffer
		#region Flip index order
		/// <summary>
		/// Little helper method to flip indices from 0, 1, 2 to 0, 2, 1.
		/// This way we can render with CullClockwiseFace (default for XNA).
		/// </summary>
		/// <param name="oldIndex"></param>
		/// <returns></returns>
		private int FlipIndexOrder(int oldIndex)
		{
			int polygonIndex = oldIndex % 3;
			if (polygonIndex == 0)
				return oldIndex;
			else if (polygonIndex == 1)
				return oldIndex + 1;
			else //if (polygonIndex == 2)
				return oldIndex - 1;
		} // FlipIndexOrder(oldIndex)
		#endregion

		#region OptimizeVertexBuffer
		/// <summary>
		/// Optimize vertex buffer. Note: The vertices list array will be changed
		/// and shorted quite a lot here. We are also going to create the indices
		/// for the index buffer here (we don't have them yet, they are just
		/// sequential from the loading process above).
		/// 
		/// Note: This method is highly optimized for speed, it performs
		/// hundred of times faster than OptimizeVertexBufferSlow, see below!
		/// </summary>
		/// <returns>int array for the optimized indices</returns>
		private int[] OptimizeVertexBuffer()
		{
			List<Vector3> newVertices =
				new List<Vector3>();
			List<int> newIndices = new List<int>();

			// Helper to only search already added newVertices and for checking the
			// old position indices by transforming them into newVertices indices.
			List<int> newVerticesPositions = new List<int>();

			// Go over all vertices (indices are currently 1:1 with the vertices)
			for (int num = 0; num < vectors.Length; num++)
			{
				// Get current vertex
				Vector3 currentVertex = vectors[num];
				bool reusedExistingVertex = false;

				// Find out which position index was used, then we can compare
				// all other vertices that share this position. They will not
				// all be equal, but some of them can be merged.
				int sharedPos = reuseVertexPositions[num];
				foreach (int otherVertexIndex in reverseReuseVertexPositions[sharedPos])
				{
					// Only check the indices that have already been added!
					if (otherVertexIndex != num &&
						// Make sure we already are that far in our new index list
						otherVertexIndex < newIndices.Count &&
						// And make sure this index has been added to newVertices yet!
						newIndices[otherVertexIndex] < newVertices.Count &&
						// Then finally compare vertices (this call is slow, but thanks to
						// all the other optimizations we don't have to call it that often)
						currentVertex == newVertices[newIndices[otherVertexIndex]])
					{
						// Reuse the existing vertex, don't add it again, just
						// add another index for it!
						newIndices.Add(newIndices[otherVertexIndex]);
						reusedExistingVertex = true;
						break;
					} // if (otherVertexIndex)
				} // foreach (otherVertexIndex)

				if (reusedExistingVertex == false)
				{
					// Add the currentVertex and set it as the current index
					newIndices.Add(newVertices.Count);
					newVertices.Add(currentVertex);
				} // if (reusedExistingVertex)
			} // for (num)

			// Finally flip order of all triangles to allow us rendering
			// with CullCounterClockwiseFace (default for XNA) because all the data
			// is in CullClockwiseFace format right now!
			for (int num = 0; num < newIndices.Count / 3; num++)
			{
				int swap = newIndices[num * 3 + 1];
				newIndices[num * 3 + 1] = newIndices[num * 3 + 2];
				newIndices[num * 3 + 2] = swap;
			} // for (num)

			// Reassign the vertices, we might have deleted some duplicates!
			vectors = newVertices.ToArray();

			// And return index list for the caller
			return newIndices.ToArray();
		} // OptimizeVertexBuffer()
		#endregion
		#endregion

		#region Collision testing
		/// <summary>
		/// Point intersect
		/// </summary>
		/// <param name="ray_start">Ray _start</param>
		/// <param name="ray_end">Ray _end</param>
		/// <param name="distance">Intersect _distance</param>
		/// <param name="collisionPosition">Intersect _position</param>
		/// <param name="collisionNormal">Intersect _normal</param>
		/// <returns>Bool</returns>
		public bool DoesRayIntersect(Vector3 ray_start, Vector3 ray_end,
			out float distance, out Vector3 collisionPosition,
			out Vector3 collisionNormal)
		{
			return tree.DoesRayIntersect(
				Ray.FromStartAndEnd(ray_start, ray_end), vectors,
				out distance, out collisionPosition, out collisionNormal);
		} // DoesRayIntersect(ray_start, ray_end, distance)

		/// <summary>
		/// Box intersect
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="ray_start">Ray _start</param>
		/// <param name="ray_end">Ray _end</param>
		/// <param name="distance">Intersect _distance</param>
		/// <param name="collisionPosition">Intersect _position</param>
		/// <param name="collisionNormal">Intersect _normal</param>
		/// <returns>Bool</returns>
		public bool DoesBoxIntersect(BoxHelper box, Ray ray, out float distance,
			out Vector3 collisionPosition, out Vector3 collisionNormal)
		{
			return tree.DoesBoxIntersect(box, ray, vectors,
				out distance, out collisionPosition, out collisionNormal);
		} // DoesBoxIntersect(box, ray_start, ray_end)

		/// <summary>
		/// Point move
		/// </summary>
		/// <param name="pointStart">Point _start</param>
		/// <param name="pointEnd">Point _end</param>
		/// <param name="frictionFactor">Friction _factor</param>
		/// <param name="bumpFactor">Bump mapping _factor</param>
		/// <param name="recurseLevel">Recurse _level</param>
		/// <param name="pointResult">Point _result</param>
		/// <param name="velocityResult">Velocity _result</param>
		public void PointMove(Vector3 pointStart, Vector3 pointEnd,
			float frictionFactor, float bumpFactor, uint recurseLevel,
			out Vector3 pointResult, ref Vector3 velocityResult,
			out Vector3 polyPoint)
		{
			tree.PointMove(pointStart, pointEnd, vectors, frictionFactor,
				bumpFactor, recurseLevel, out pointResult, ref velocityResult,
				out polyPoint);
		} // PointMove(pointStart, pointEnd, frictionFactor)

		/// <summary>
		/// Box move
		/// </summary>
		/// <param name="box">Box</param>
		/// <param name="pointStart">Point _start</param>
		/// <param name="pointEnd">Point _end</param>
		/// <param name="frictionFactor">Friction _factor</param>
		/// <param name="bumpFactor">Bump mapping _factor</param>
		/// <param name="recurseLevel">Recurse _level</param>
		/// <param name="pointResult">Point _result</param>
		/// <param name="velocityResult">Velocity _result</param>
		public void BoxMove(BoxHelper box, Vector3 pointStart,
			Vector3 pointEnd, float frictionFactor, float bumpFactor,
			uint recurseLevel, out Vector3 pointResult, ref Vector3 velocityResult)
		{
			tree.BoxMove(box, pointStart, pointEnd, vectors, frictionFactor,
				bumpFactor, recurseLevel, out pointResult, ref velocityResult);
		} // BoxMove(box, pointStart, pointEnd)

		/// <summary>
		/// Get elements
		/// </summary>
		/// <param name="b">B</param>
		/// <param name="e">E</param>
		public void GetElements(BoxHelper b, List<BaseCollisionObject> e)
		{
			tree.GetElements(b, e);
		} // GetElements(b, e)
		#endregion

		#region Unit Testing
#if DEBUG
		//TODO?
#endif
		#endregion
	} // class CollisionMesh
} // namespace DungeonQuest.Collision
