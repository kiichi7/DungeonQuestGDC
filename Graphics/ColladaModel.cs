// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Graphics
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:40

#region Using directives
using DungeonQuest.Game;
using DungeonQuest.GameLogic;
using DungeonQuest.Helpers;
using DungeonQuest.Shaders;
using DungeonQuest.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
#endregion

namespace DungeonQuest.Graphics
{
	/// <summary>
	/// Collada model. Supports bones and animation for collada (.dae) exported
	/// 3D Models from 3D Studio Max (8 or 9).
	/// This class is just for testing and it will only display one single mesh
	/// with bone and skinning support, the mesh also can only have one single
	/// material. Bones can be either in matrix mode or stored with transform
	/// and rotation values. TangentVertex is used to store the vertices,
	/// use derived classes for different formats.
	/// </summary>
	public class ColladaModel : IDisposable
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
		/// Name of this model
		/// </summary>
		protected internal string name;

		/// <summary>
		/// Material for the main mesh. Only one material is supported!
		/// </summary>
		internal Material material = null;

		/// <summary>
		/// Vertices for the main mesh (we only support one mesh here!).
		/// </summary>
		private List<TangentVertex> vertices = new List<TangentVertex>();

		/// <summary>
		/// Number of vertices and number of indices we got in the
		/// vertex and index buffers.
		/// </summary>
		protected int numOfVertices = 0,
			numOfIndices = 0;

		/// <summary>
		/// Object matrix for our mesh. Often used to fix mesh to bone skeleton.
		/// </summary>
		protected Matrix objectMatrix = Matrix.Identity;

		/// <summary>
		/// Vertex buffer for the mesh.
		/// </summary>
		protected VertexBuffer vertexBuffer = null;
		/// <summary>
		/// Index buffer for the mesh.
		/// </summary>
		protected IndexBuffer indexBuffer = null;

		/// <summary>
		/// Helper to quickly check if this is the cave.
		/// </summary>
		bool isCave = false;
		#endregion

		#region Properties
		/// <summary>
		/// Get model name, currently just the filename without path and extension
		/// </summary>
		public string Name
		{
			get
			{
				return name;
			} // get
		} // name

		/// <summary>
		/// Was the model loaded successfully?
		/// </summary>
		public bool Loaded
		{
			get
			{
				return vertexBuffer != null &&
					indexBuffer != null;
			} // get
		} // Loaded
		#endregion

		#region Constructor
		/// <summary>
		/// Empty constructor to allow deriving from this class.
		/// </summary>
		protected ColladaModel()
		{
		} // ColladaModel()

		/// <summary>
		/// Create a model from a collada file
		/// </summary>
		/// <param name="setName">Set name</param>
		public ColladaModel(string setName)
		{
			// Set name to identify this model and build the filename
			name = setName;
			string filename = Path.Combine(ColladaDirectory,
				StringHelper.ExtractFilename(name, true) + "." +
				ColladaExtension);

			// Load file
			Stream file = File.OpenRead(filename);
			string colladaXml = new StreamReader(file).ReadToEnd();
			XmlNode colladaFile = XmlHelper.LoadXmlFromText(colladaXml);

			isCave = name == "Cave";

			// Load material (we only support one)
			//Not used, always autoassign: LoadMaterial(colladaFile);
			AutoAssignMaterial();

			// Load mesh (vertices data, combine with bone weights)
			LoadMesh(colladaFile);

			// If this is our level (the cave), load all lights and give them
			// to the LightManager.
			if (name == "Cave")
			{
				LightManager.allLights = LoadAllLights(colladaFile);
				// Find 6 closest lights, just in case we are in some unit test
				// that does not do this by itself.
				LightManager.FindClosestLights(BaseGame.CameraPos);
				// Also load all helpers (also build flares from light positions)
				LoadAllHelpers(colladaFile, LightManager.allLights);
			} // if (name)

			// Close file, we are done.
			file.Close();
		} // ColladaModel(setFilename)
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose stuff that need to be disposed in XNA.
		/// </summary>
		public void Dispose()
		{
			if (vertexBuffer != null)
				vertexBuffer.Dispose();
			vertexBuffer = null;
			if (indexBuffer != null)
				indexBuffer.Dispose();
			indexBuffer = null;
		} // Dispose()
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

		#region Load material
/*not used anymore, works fine, but we had problems at the GDC exporting
 * shader material data in 3DS Max 9 (always crashes the exporter).
 * Instead the AutoAssignMaterial method below is used.
		/// <summary>
		/// Load material information from a collada file
		/// </summary>
		/// <param name="colladaFile">Collada file xml node</param>
		private void LoadMaterial(XmlNode colladaFile)
		{
			// First get all textures, if there are none, ignore and quit.
			XmlNode texturesNode =
				XmlHelper.GetChildNode(colladaFile, "library_images");
			if (texturesNode == null)
				return;

			// Get all used texture images
			Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
			foreach (XmlNode textureNode in texturesNode)
			{
				if (textureNode.Name == "image")
				{
					string filename =
						XmlHelper.GetChildNode(textureNode, "init_from").InnerText;
					textures.Add(
						XmlHelper.GetXmlAttribute(textureNode, "id"),
						new Texture(StringHelper.ExtractFilename(filename, true)));
				} // if (texture.name)
				else
					throw new InvalidOperationException(
						"Unknown Node " + textureNode.Name + " found in library_images");
			} // foreach (texture)

			// Load all effects to find out the used effect in our material
			XmlNode effectsNode =
				XmlHelper.GetChildNode(colladaFile, "library_effects");
			if (effectsNode == null)
				throw new InvalidOperationException(
					"library_effects node not found while loading Collada file " +name);

			Dictionary<string, string> effectIds = new Dictionary<string, string>();
			foreach (XmlNode effectNode in effectsNode)
			{
				XmlNode fileNode = XmlHelper.GetChildNode(effectNode, "import");
				if (fileNode == null)
				{
					// Use "include" node instead, this one is used when we got valid
					// techniques selected in the material (was sometimes used collada
					// 1.4 and different versions of collada exporters than ColladaMax).
					fileNode = XmlHelper.GetChildNode(effectNode, "include");

					// Still null?
					if (fileNode == null)
						throw new InvalidOperationException(
							"Import or include node not found in effect node, file: " +name);
				} // if (filenode)

				effectIds.Add(
					XmlHelper.GetXmlAttribute(effectNode, "id"),
					XmlHelper.GetXmlAttribute(fileNode, "url"));
			} // foreach (effect)

			// And finally get all material nodes, but we only support one material
			// here, we will use the first one that uses shaders and ignore the rest.
			XmlNode materialsNode =
				XmlHelper.GetChildNode(colladaFile, "library_materials");
			if (materialsNode == null)
				throw new InvalidOperationException(
					"library_materials node not found while loading Collada file "+name);

			foreach (XmlNode materialNode in materialsNode)
			{
				if (materialNode.Name == "material")
				{
					Material newMaterial =
						new Material(materialNode, textures, effectIds);
					if (material == null ||
						newMaterial.shader != null)
						material = newMaterial;
				} // if (materialNode.name)
				else
					throw new InvalidOperationException("Unknown Node " +
						materialNode.Name + " found in library_materials");
			} // foreach (material)
		} // LoadMaterial(colladaFile, TextureIDDictionary, MaterialIDDictionary)'
 */
		/// <summary>
		/// Auto assign material
		/// </summary>
		protected void AutoAssignMaterial()
		{
			// Just load the material from the name of this model
			material = new Material(
				name,
				name + "Normal",
				name + "Specular");

			// Custom material stuff for Cave (detail maps and different shader)
			// is handled in the CavePointNormalMapping class.
			// But make sure to use black for ambient, very bright yellow for
			// diffuse and white for specular.
			if (isCave)
			{
				material.ambientColor = Color.Black;
				material.diffuseColor = new Color(255, 255, 240);
				material.specularColor = Color.White;
				material.specularPower = 22;
			} // if (name)
			else if (name.Contains("Goblin") ||
				name.Contains("Orge"))
			{
				// Use a higher specular for the monsters.
				material.specularPower = 16;
				material.specularColor = new Color(180, 180, 180);
				// Increase ambient a little (better visibility of monsters)
				material.ambientColor = new Color(80, 80, 80);
			} // else if
		} // AutoAssignMaterial()
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
					"library_geometries node not found in collada file " + name);

			foreach (XmlNode geometry in geometrys)
				if (geometry.Name == "geometry")
				{
					// Load everything from the mesh node
					LoadMeshGeometry(colladaFile,
						XmlHelper.GetChildNode(colladaFile, "mesh"),
						XmlHelper.GetXmlAttribute(geometry, "name"));

					// Generate vertex buffer for rendering
					GenerateVertexAndIndexBuffers();

					// Get outa here, we currently only support one single mesh!
					return;
				} // foreach if (geometry.name)
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
			} // foreach
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
					"library_visual_scenes node not found in collada file " + name);

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
				XmlNode normalsnode = XmlHelper.GetChildNode(trianglenode,
					"semantic", "NORMAL");
				XmlNode texcoordsnode = XmlHelper.GetChildNode(trianglenode,
					"semantic", "TEXCOORD");
				XmlNode tangentsnode = XmlHelper.GetChildNode(trianglenode,
					"semantic", "TEXTANGENT");

				// Get the data of the sources
				List<float> positions = sources[XmlHelper.GetXmlAttribute(
					positionsnode, "source").Substring(1)];
				List<float> normals = sources[XmlHelper.GetXmlAttribute(normalsnode,
					"source").Substring(1)];
				List<float> texcoords = sources[XmlHelper.GetXmlAttribute(
					texcoordsnode, "source").Substring(1)];
				List<float> tangents = sources[XmlHelper.GetXmlAttribute(tangentsnode,
					"source").Substring(1)];

				// Find the Offsets
				int positionsoffset = Convert.ToInt32(XmlHelper.GetXmlAttribute(
					positionsnode, "offset"), NumberFormatInfo.InvariantInfo);
				int normalsoffset = Convert.ToInt32(XmlHelper.GetXmlAttribute(
					normalsnode, "offset"), NumberFormatInfo.InvariantInfo);
				int texcoordsoffset = Convert.ToInt32(XmlHelper.GetXmlAttribute(
					texcoordsnode, "offset"), NumberFormatInfo.InvariantInfo);
				int tangentsoffset = Convert.ToInt32(XmlHelper.GetXmlAttribute(
					tangentsnode, "offset"), NumberFormatInfo.InvariantInfo);

				// Get the indexlist
				XmlNode p = XmlHelper.GetChildNode(trianglenode, "p");
				int[] pints = StringHelper.ConvertStringToIntArray(p.InnerText);
				int trianglecount = Convert.ToInt32(XmlHelper.GetXmlAttribute(
					trianglenode, "count"), NumberFormatInfo.InvariantInfo);
				// The number of ints that form one vertex:
				int vertexcomponentcount = pints.Length / trianglecount / 3;

				// Construct data
				vertices.Clear();
				// Initialize reuseVertexPositions and reverseReuseVertexPositions
				// to make it easier to use them below
				reuseVertexPositions = new int[trianglecount * 3];
				reverseReuseVertexPositions = new List<int>[positions.Count / 3];
				for (int i = 0; i < reverseReuseVertexPositions.Length; i++)
					reverseReuseVertexPositions[i] = new List<int>();

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
					if (isCave)
						position *= globalScaling;

					// Normal
					int nor = pints[i * vertexcomponentcount + normalsoffset] * 3;
					Vector3 normal = new Vector3(
						normals[nor], normals[nor + 1], normals[nor + 2]);

					// Texture Coordinates
					int tex = pints[i * vertexcomponentcount + texcoordsoffset] * 3;
					float u = texcoords[tex];
					// V coordinate is inverted in max
					float v = 1.0f - texcoords[tex + 1];

					// Tangent
					int tan = pints[i * vertexcomponentcount + tangentsoffset] * 3;
					Vector3 tangent = new Vector3(
						tangents[tan], tangents[tan + 1], tangents[tan + 2]);

					// Set the vertex
					vertices.Add(new TangentVertex(
						position, u, v, normal, tangent));

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
		} // LoadMeshGeometry(geometry)
		#endregion

		#region LoadAllLights
		/// <summary>
		/// Load all lights
		/// </summary>
		/// <param name="colladaFile">Collada file</param>
		private List<Matrix> LoadAllLights(XmlNode colladaFile)
		{
			XmlNode sceneStuff = XmlHelper.GetChildNode(
				XmlHelper.GetChildNode(colladaFile, "library_visual_scenes"),
				"visual_scene");
			if (sceneStuff == null)
				throw new InvalidOperationException(
					"library_visual_scenes node not found in collada file " + name);

			List<Matrix> ret = new List<Matrix>();
			foreach (XmlNode node in sceneStuff)
				// Search for "Omni" nodes, this are the lights from 3DS Max.
				if (node.Name == "node" &&
					XmlHelper.GetXmlAttribute(node, "name").Contains("Omni"))
				{
					// Get the matrix for this light, we need it for the flares.
					ret.Add(OnlyScaleTransformation(LoadColladaMatrix(
						StringHelper.ConvertStringToFloatArray(
						XmlHelper.GetChildNode(node, "matrix").InnerText), 0)));
				} // foreach if (geometry.name)

			return ret;
		} // LoadAllLights(colladaFile)
		#endregion

		#region LoadAllHelpers
		/// <summary>
		/// Load all helpers
		/// </summary>
		/// <param name="colladaFile">Collada file</param>
		private void LoadAllHelpers(XmlNode colladaFile,
			List<Matrix> lightMatrices)
		{
			XmlNode sceneStuff = XmlHelper.GetChildNode(
				XmlHelper.GetChildNode(colladaFile, "library_visual_scenes"),
				"visual_scene");
			if (sceneStuff == null)
				throw new InvalidOperationException(
					"library_visual_scenes node not found in collada file " + name);

			// Add all flares automatically from the light positions.
			foreach (Matrix mat in lightMatrices)
			{
				GameManager.Add(GameManager.StaticTypes.Flare, mat);
			} // foreach (mat)

			foreach (XmlNode node in sceneStuff)
				// Search for all nodes, we check for helpers with the names
				if (node.Name == "node")
				{
					string nodeName = XmlHelper.GetXmlAttribute(node, "name").ToLower();
					Matrix positionMatrix = Matrix.Identity;
					if (XmlHelper.GetChildNode(node, "matrix") != null)
						positionMatrix = OnlyScaleTransformation(LoadColladaMatrix(
						 StringHelper.ConvertStringToFloatArray(
						 XmlHelper.GetChildNode(node, "matrix").InnerText), 0));
					if (nodeName.Contains("door"))
					{
						GameManager.doorPosition = positionMatrix.Translation;
						GameManager.Add(GameManager.StaticTypes.Door, positionMatrix);
						GameManager.Add(GameManager.StaticTypes.DoorWall, positionMatrix);
					} // if (nodeName.Contains)
					else if (nodeName.Contains("key"))
						// Actually place a monster here that got the key (randomly)
						GameManager.Add(GameManager.AnimatedTypes.Ogre, positionMatrix,
							// Give the key to the orge (overwrite the weapon if he would
							// drop one).
							AnimatedGameObject.DropObject.Key);
						//obs: GameManager.Add(GameManager.StaticTypes.Key, positionMatrix);
					else if (nodeName.Contains("treasure"))
					{
						// Move down a little (fix error with CaveCollision)
						positionMatrix = positionMatrix *
							Matrix.CreateTranslation(new Vector3(0, 0, -1));

						GameManager.treasurePosition = positionMatrix.Translation;
						GameManager.Add(GameManager.StaticTypes.Treasure, positionMatrix);
					} // else if
					else if (nodeName.Contains("bigogre"))
						GameManager.Add(GameManager.AnimatedTypes.BigOgre, positionMatrix);
					else if (nodeName.Contains("ogre"))
						GameManager.Add(GameManager.AnimatedTypes.Ogre, positionMatrix);
					else if (nodeName.Contains("goblinwizard"))
						GameManager.Add(GameManager.AnimatedTypes.GoblinWizard,
							positionMatrix);
					else if (nodeName.Contains("goblinmaster"))
						GameManager.Add(GameManager.AnimatedTypes.GoblinMaster,
							positionMatrix);
					else if (nodeName.Contains("goblin"))
						GameManager.Add(GameManager.AnimatedTypes.Goblin,
							positionMatrix);
				} // foreach if (geometry.name)
		} // LoadAllLights(colladaFile)
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
			List<TangentVertex> newVertices =
				new List<TangentVertex>();
			List<int> newIndices = new List<int>();

			// Helper to only search already added newVertices and for checking the
			// old position indices by transforming them into newVertices indices.
			List<int> newVerticesPositions = new List<int>();

			// Go over all vertices (indices are currently 1:1 with the vertices)
			for (int num = 0; num < vertices.Count; num++)
			{
				// Get current vertex
				TangentVertex currentVertex = vertices[num];
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
						TangentVertex.NearlyEquals(
						currentVertex, newVertices[newIndices[otherVertexIndex]]))
					{
						// Reuse the existing vertex, don't add it again, just
						// add another index for it!
						newIndices.Add(newIndices[otherVertexIndex]);
						reusedExistingVertex = true;
						break;
					} // if (TangentVertex.NearlyEquals)
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
			} // for

			// Reassign the vertices, we might have deleted some duplicates!
			vertices = newVertices;

			// And return index list for the caller
			return newIndices.ToArray();
		} // OptimizeVertexBuffer()
		#endregion

		#region OptimizeVertexBufferSlow
		/// <summary>
		/// Optimize vertex buffer. Note: The vertices list array will be changed
		/// and shorted quite a lot here. We are also going to create the indices
		/// for the index buffer here (we don't have them yet, they are just
		/// sequential from the loading process above).
		/// 
		/// Note: Slow version because we have to check each vertex against
		/// each other vertex, which makes this method exponentially slower
		/// the more vertices we have. Takes 10 seconds for 30k vertices,
		/// and over 40 seconds for 60k vertices. It is much easier to understand,
		/// but it produces the same output as the fast OptimizeVertexBuffer
		/// method and you should always use that one (it only requires a couple
		/// of miliseconds instead of the many seconds this method will spend).
		/// </summary>
		/// <returns>int array for the optimized indices</returns>
		private int[] OptimizeVertexBufferSlow()
		{
			List<TangentVertex> newVertices =
				new List<TangentVertex>();
			List<int> newIndices = new List<int>();

			// Go over all vertices (indices are currently 1:1 with the vertices)
			for (int num = 0; num < vertices.Count; num++)
			{
				// Try to find already existing vertex in newVertices list that
				// matches the vertex of the current index.
				TangentVertex currentVertex = vertices[FlipIndexOrder(num)];
				bool reusedExistingVertex = false;
				for (int checkNum = 0; checkNum < newVertices.Count; checkNum++)
				{
					if (TangentVertex.NearlyEquals(
						currentVertex, newVertices[checkNum]))
					{
						// Reuse the existing vertex, don't add it again, just
						// add another index for it!
						newIndices.Add(checkNum);
						reusedExistingVertex = true;
						break;
					} // if (TangentVertex.NearlyEquals)
				} // for (checkNum)

				if (reusedExistingVertex == false)
				{
					// Add the currentVertex and set it as the current index
					newIndices.Add(newVertices.Count);
					newVertices.Add(currentVertex);
				} // if (reusedExistingVertex)
			} // for (num)

			// Reassign the vertices, we might have deleted some duplicates!
			vertices = newVertices;

			// And return index list for the caller
			return newIndices.ToArray();
		} // OptimizeVertexBufferSlow()
		#endregion
		#endregion

		#region Generate vertex and index buffers
		/// <summary>
		/// Generate vertex and index buffers
		/// </summary>
		private void GenerateVertexAndIndexBuffers()
		{
			// Optimize vertices first and build index buffer from that!
			int[] indices = OptimizeVertexBuffer();
			// For testing, this one is REALLY slow (see method summary)!
			//int[] indices = OptimizeVertexBufferSlow();

			// Create the vertex buffer from our vertices.
			vertexBuffer = new VertexBuffer(
				BaseGame.Device,
				typeof(TangentVertex),
				vertices.Count,
				BufferUsage.WriteOnly);
			vertexBuffer.SetData(vertices.ToArray());
			numOfVertices = vertices.Count;

			// Create the index buffer from our indices.
			// To fully support the complex cave (200,000+ polys) we are using
			// int values here.
			indexBuffer = new IndexBuffer(
				BaseGame.Device,
				typeof(int),
				indices.Length,
				BufferUsage.WriteOnly);				
			indexBuffer.SetData(indices);
			numOfIndices = indices.Length;
		} // GenerateVertexAndIndexBuffers()
		#endregion

		#region Render
		/// <summary>
		/// Render the animated model (will call UpdateAnimation internally,
		/// but if you do that yourself before calling this method, it gets
		/// optimized out). Rendering always uses the skinnedNormalMapping shader
		/// with the DiffuseSpecular30 technique.
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public virtual void Render(Matrix renderMatrix)
		{
			// Make sure we use the correct vertex declaration for our shader.
			BaseGame.Device.VertexDeclaration =
				TangentVertex.VertexDeclaration;
			// Set the world matrix for this object (often Identity).
			// The renderMatrix is directly applied to the matrices we use
			// as bone matrices for the shader (has to be done this way because
			// the bone matrices are transmitted transposed and we could lose
			// important render matrix translation data if we do not apply it there).
			BaseGame.WorldMatrix = objectMatrix * renderMatrix;

			// Rendering is pretty straight forward (if you know how anyway).
			// For the cave we are going to use the CavePointNormalMapping shader.
			if (isCave)
			{
				ShaderEffect.caveMapping.RenderCave(
					material,
					//auto: LightManager.closestLightsForRendering,
					RenderVertices);
			} // if (name)
			else
			{
				// Just render with normal mapping
				//TODO: use lights here too!
				ShaderEffect.normalMapping.RenderSinglePassShader(
				//ShaderEffect.caveMapping.Render(
					material,
					//material,
					//auto: LightManager.closestLightsForRendering,
					RenderVertices);
			} // else
		} // Render(renderMatrix)

		/// <summary>
		/// Render vertices
		/// </summary>
		private void RenderVertices()
		{
			BaseGame.Device.Vertices[0].SetSource(vertexBuffer, 0,
				TangentVertex.SizeInBytes);
			BaseGame.Device.Indices = indexBuffer;
			BaseGame.Device.DrawIndexedPrimitives(
				PrimitiveType.TriangleList,
				0, 0, numOfVertices,
				0, numOfIndices / 3);
		} // RenderVertices()
		#endregion

		#region Generate shadow
		/// <summary>
		/// Generate shadow for this model in the generate shadow pass of our
		/// shadow mapping shader. All objects rendered here will cast shadows to
		/// our scene (if they are in range of the light).
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public virtual void GenerateShadow(Matrix renderMatrix)
		{
			// Set bone matrices and the world matrix for the shader.
			ShaderEffect.shadowMapping.UpdateGenerateShadowWorldMatrix(
				objectMatrix * renderMatrix);

			/*obs, using static shader now
			// Reset all bone matrices, we will use some random data that
			// is currently set as blendWeights and blendIndices.
			Matrix[] defaultMatrices = new Matrix[80];
			for (int i = 0; i < defaultMatrices.Length; i++)
			{
				defaultMatrices[i] = Matrix.Identity;
			} // for
			ShaderEffect.shadowMapping.SetBoneMatrices(defaultMatrices);
			 */

			// And render
			BaseGame.Device.VertexDeclaration =
				TangentVertex.VertexDeclaration;
				//SkinnedTangentVertex.VertexDeclaration;
			ShaderEffect.shadowMapping.UseStaticShadowShader();
			RenderVertices();
		} // Render(shader, shaderTechnique)
		#endregion

		#region Use shadow
		/// <summary>
		/// Use shadow on the plane, useful for our unit tests. The plane does not
		/// throw shadows, so we don't need a GenerateShadow method.
		/// </summary>
		public virtual void UseShadow(Matrix renderMatrix)
		{
			// Set bone matrices and the world matrix for the shader.
			ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(
				objectMatrix * renderMatrix);

			/*obs, using static shader now
			// Reset all bone matrices, we will use some random data that
			// is currently set as blendWeights and blendIndices.
			Matrix[] defaultMatrices = new Matrix[80];
			for (int i = 0; i < defaultMatrices.Length; i++)
			{
				defaultMatrices[i] = Matrix.Identity;
			} // for
			ShaderEffect.shadowMapping.SetBoneMatrices(defaultMatrices);
			 */

			// And render
			BaseGame.Device.VertexDeclaration =
				TangentVertex.VertexDeclaration;
			ShaderEffect.shadowMapping.UseStaticShadowShader();
			RenderVertices();
		} // UseShadow()
		#endregion

		#region Unit Testing
#if DEBUG
		#region TestCaveColladaModelScene
		/// <summary>
		/// TestCaveColladaModelScene
		/// </summary>
		public static void TestCaveColladaModelScene()
		{
			ColladaModel caveModel = null;
			//ColladaModel playerModel = null;
			//PlaneRenderer groundPlane = null;

			TestGame.Start("TestCaveColladaModelScene",
				delegate
				{
					// Load Cave
					caveModel = new ColladaModel("Cave");
					caveModel.material.ambientColor = Material.DefaultAmbientColor;

					//TODO: playerModel = new ColladaModel("Hero");

					// Play background music :)
					//Sound.StartMusic();

					/*not needed here
					// Create ground plane
					groundPlane = new PlaneRenderer(
						new Vector3(0, 0, -0.001f),
						new Plane(new Vector3(0, 0, 1), 0),
						new Material(
							"CaveDetailGround",
							"CaveDetailGroundNormal"),
						28);
					 */

					// Set light direction (light is coming from the front right pos).
					BaseGame.LightDirection = new Vector3(-18, -20, 16);
				},
				delegate
				{
					if (Input.Keyboard.IsKeyDown(Keys.LeftAlt))
					{
						// Start glow shader
						BaseGame.GlowShader.Start();

						// Clear background with white color, looks much cooler for the
						// post screen glow effect.
						BaseGame.Device.Clear(Color.Black);
					} // if (Input.Keyboard.IsKeyDown)

					//BaseGame.Device.Clear(new Color(86, 86, 60));

					// Render goblin always in center, but he is really big, bring him
					// down to a more normal size that fits better in our test scene.
					Matrix renderMatrix = Matrix.Identity;// Matrix.CreateScale(0.1f);

					// Restore z buffer state
					BaseGame.Device.RenderState.DepthBufferEnable = true;
					BaseGame.Device.RenderState.DepthBufferWriteEnable = true;

					/*TODO
					// Make sure we use skinned tangent vertex format for shadow mapping
					BaseGame.Device.VertexDeclaration =
						TangentVertex.VertexDeclaration;

					// Generate shadows
					ShaderEffect.shadowMapping.GenerateShadows(
						delegate
						{
							for (int x = 0; x < 2; x++)
								for (int y = 0; y < 3; y++)
									goblinModel.GenerateShadow(renderMatrix *
										Matrix.CreateTranslation(-5 + 10 * x, -10 + 10 * y, 0));
						});

					// Render shadows
					ShaderEffect.shadowMapping.RenderShadows(
						delegate
						{
							for (int x = 0; x < 2; x++)
								for (int y = 0; y < 3; y++)
									goblinModel.UseShadow(renderMatrix *
										Matrix.CreateTranslation(-5 + 10 * x, -10 + 10 * y, 0));
							groundPlane.UseShadow();
						});
					 */

					// Show ground with DiffuseSpecular material and use parallax mapping!
					//not needed: groundPlane.Render(ShaderEffect.normalMapping, "Specular30");
					/*obs
					// And show all goblins
					for (int x = 0; x < 2; x++)
						for (int y = 0; y < 3; y++)
							goblinModel.Render(
								renderMatrix *
								Matrix.CreateTranslation(-5 + 10 * x, -10 + 10 * y, 0));
					 */
					caveModel.Render(
						Matrix.CreateTranslation(0, 0, +1));

					LightManager.RenderAllCloseLightEffects();

					// We have to render the effects ourselfs because
					// it is usually done in DungeonQuestGame!
					// Finally render all effects (before applying post screen effects)
					BaseGame.effectManager.HandleAllEffects();

					/*TODO
					if (Input.Keyboard.IsKeyDown(Keys.LeftControl))
						playerModel.Render(
							Matrix.CreateTranslation(BaseGame.camera.PlayerPos));
					 */
					
					// And show shadows on top of the scene (with shadow blur effect).
					//TODO: ShaderEffect.shadowMapping.ShowShadows();

					if (Input.Keyboard.IsKeyDown(Keys.LeftAlt))
					{
						// And finally show glow shader on top of everything
						BaseGame.GlowShader.Show();
					} // if (Input.Keyboard.IsKeyDown)

					/*TODO
					// If you press the right mouse button or B you can see all
					// shadow map and post screen render targets (for testing/debugging)
					if (Input.MouseRightButtonPressed ||
						Input.GamePadBPressed)
					{
						BaseGame.AlphaBlending = false;
						BaseGame.Device.RenderState.AlphaTestEnable = false;
						// Line 1 (3 render targets, 2 shadow mapping, 1 post screen)
						ShaderEffect.shadowMapping.shadowMapTexture.RenderOnScreen(
							new Rectangle(10, 10, 256, 256));
						ShaderEffect.shadowMapping.shadowMapBlur.SceneMapTexture.
							RenderOnScreen(
							new Rectangle(10 + 256 + 10, 10, 256, 256));
						PostScreenMenu.sceneMapTexture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10 + 256 + 10, 10, 256, 256));
						// Line 2 (3 render targets, 2 post screen blurs, 1 final scene)
						PostScreenMenu.downsampleMapTexture.RenderOnScreenNoAlpha(
							new Rectangle(10, 10 + 256 + 10, 256, 256));
						PostScreenMenu.blurMap1Texture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10, 10 + 256 + 10, 256, 256));
						PostScreenMenu.blurMap2Texture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10 + 256 + 10, 10 + 256 + 10, 256, 256));
					} // if (Input.MouseRightButtonPressed)
					 */
				});
		} // TestLoadColladaModel()
		#endregion

		#region TestCaveColladaModelSceneSplitScreen
		/// <summary>
		/// TestCaveColladaModelScene
		/// </summary>
		public static void TestCaveColladaModelSceneSplitScreen()
		{
			ColladaModel caveModel = null;
			Viewport originalViewport = new Viewport(),
				viewport1 = new Viewport(),
				viewport2 = new Viewport();

			TestGame.Start("TestCaveColladaModelScene",
				delegate
				{
					// Load Cave
					caveModel = new ColladaModel("Cave");
					caveModel.material.ambientColor = Material.DefaultAmbientColor;

					// Set light direction (light is coming from the front right pos).
					BaseGame.LightDirection = new Vector3(-18, -20, 16);

					// Create both viewports
					originalViewport = BaseGame.Device.Viewport;

					viewport1 = new Viewport();
					viewport1.Width = BaseGame.Width / 2;
					viewport1.Height = BaseGame.Height;
					viewport1.X = 0;
					viewport1.Y = 0;

					viewport2 = new Viewport();
					viewport2.Width = BaseGame.Width / 2;
					viewport2.Height = BaseGame.Height;
					viewport2.X = BaseGame.Width / 2;
					viewport2.Y = 0;

					// Fix projection matrix for 2 views
					BaseGame.ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(
						BaseGame.FieldOfView, BaseGame.aspectRatio / 2.0f,
						BaseGame.NearPlane, BaseGame.FarPlane);
				},
				delegate
				{
					// Render goblin always in center, but he is really big, bring him
					// down to a more normal size that fits better in our test scene.
					Matrix renderMatrix = Matrix.Identity;// Matrix.CreateScale(0.1f);

					// Restore z buffer state
					BaseGame.Device.RenderState.DepthBufferEnable = true;
					BaseGame.Device.RenderState.DepthBufferWriteEnable = true;

					// Render viewport 1
					BaseGame.Device.Viewport = viewport1;
					BaseGame.Device.Clear(BaseGame.BackgroundColor);
					caveModel.Render(
						Matrix.CreateTranslation(0, 0, +1));
					LightManager.RenderAllCloseLightEffects();
					BaseGame.effectManager.HandleAllEffects();

					// Render viewport 2
					BaseGame.Device.Viewport = viewport2;
					BaseGame.ViewMatrix = BaseGame.ViewMatrix *
						Matrix.CreateRotationY(1.4f);
					BaseGame.Device.Clear(BaseGame.BackgroundColor);

					caveModel.Render(
						Matrix.CreateTranslation(0, 0, +1));
					LightManager.RenderAllCloseLightEffects();
					BaseGame.effectManager.HandleAllEffects();

					// Restore
					BaseGame.Device.Viewport = originalViewport;

					// Draw seperation line
					//BaseGame.Device.RenderState.DepthBufferEnable = true;
					//BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
					//TODO
					UIManager.DrawUI();
				});
		} // TestCaveColladaModelSceneSplitScreen()
		#endregion

		#region TestLoadStaticModel
		/// <summary>
		/// Test load static model
		/// </summary>
		static public void TestLoadStaticModel()
		{
			ColladaModel someModel = null;

			TestGame.Start("TestLoadStaticModel",
				delegate
				{
					// Load the cave for all the lights (else everything will be dark)
					someModel = new ColladaModel("Cave");
					someModel = new ColladaModel(
						//"Cave");
						//"Flare");
						//"Club");
						"Sword");
						//"Door");
				},
				delegate
				{
					someModel.Render(
						//Matrix.CreateTranslation(
						//BaseGame.camera.PlayerPos+new Vector3(0, 0, 1)));
						//Matrix.CreateScale(100));
						Matrix.Identity);
						//Matrix.CreateTranslation(new Vector3(2, 0, 0)));
				});
		} // TestLoadStaticModel()
		#endregion
#endif
		#endregion
	} // class ColladaModel
} // namespace DungeonQuest.Graphics
