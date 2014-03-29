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
	/// and rotation values. SkinnedTangentVertex is used to store the vertices.
	/// </summary>
	public class AnimatedColladaModel : ColladaModel
	{
		#region Bone helper class
		/// <summary>
		/// Bone
		/// </summary>
		public class Bone
		{
			#region Variables
			/// <summary>
			/// Parent bone, very important to get all parent matrices when
			/// building the finalMatrix for rendering.
			/// </summary>
			public Bone parent = null;

			/// <summary>
			/// Children bones, not really used anywhere except for the ShowBones
			/// helper method, but also useful for debugging.
			/// </summary>
			public List<Bone> children = new List<Bone>();

			/// <summary>
			/// Position, very useful to position bones to show bones in 3D, also
			/// only used for debugging and testing purposes.
			/// </summary>
			public Vector3 pos;

			/// <summary>
			/// Initial matrix we get from loading the collada model, it contains
			/// the start position and is used for the calculation to get the
			/// absolute and final matrices (see below).
			/// </summary>
			public Matrix initialMatrix;

			/// <summary>
			/// Bone number for the skinning process. This is just our internal
			/// number and children do always have higher numbers, this way going
			/// through the bones list is quicker and easier. The collada file
			/// might use a different order, see LoadAnimation for details.
			/// </summary>
			public int num;

			/// <summary>
			/// Id and name of this bone, makes debugging and testing easier, but
			/// it is also used to identify this bone later on in the loading process
			/// because our bone order might be different from the one in the file.
			/// </summary>
			public string id;

			/// <summary>
			/// Animation matrices for the precalculated bone animations.
			/// These matrices must be set each frame (use time) in order
			/// for the animation to work.
			/// </summary>
			public List<Matrix> animationMatrices = new List<Matrix>();

			/// <summary>
			/// invBoneMatrix is a special helper matrix loaded directly from
			/// the collada file. It is used to transform the final matrix
			/// back to a relative format after transforming and rotating each
			/// bone with the current animation frame. This is very important
			/// because else we would always move and rotate vertices around the
			/// center, but thanks to this inverted skin matrix the correct
			/// rotation points are used.
			/// </summary>
			public Matrix invBoneSkinMatrix;

			/// <summary>
			/// Final absolute matrix, which is calculated in UpdateAnimation each
			/// frame after all the loading is done. It can directly be used to
			/// find out the current bone positions, but for rendering we have
			/// to apply the invBoneSkinMatrix first to transform all vertices into
			/// the local space.
			/// </summary>
			public Matrix finalMixMatrix;

			/// <summary>
			/// Blend matrices for each animation we got, they will be blended
			/// together later into finalMixMatrix
			/// </summary>
			public Matrix[] finalBlendMatrix = new Matrix[NumberOfAnimationTypes];
			#endregion

			#region Constructor
			/// <summary>
			/// Create bone
			/// </summary>
			/// <param name="setMatrix">Set matrix</param>
			/// <param name="setParentBone">Set parent bone</param>
			/// <param name="setNum">Set number</param>
			/// <param name="setId">Set id name</param>
			public Bone(Matrix setMatrix, Bone setParentBone, int setNum,
				string setId)
			{
				initialMatrix = setMatrix;
				pos = initialMatrix.Translation;
				parent = setParentBone;
				num = setNum;
				id = setId;

				invBoneSkinMatrix = Matrix.Identity;
			} // Bone(setMatrix, setParentBone, setNum)
			#endregion

			#region Get matrix recursively helper method
			/// <summary>
			/// Get matrix recursively
			/// </summary>
			/// <returns>Matrix</returns>
			public Matrix GetMatrixRecursively()
			{
				Matrix ret = initialMatrix;

				// If we have a parent mesh, we have to multiply the matrix with the
				// parent matrix.
				if (parent != null)
					ret *= parent.GetMatrixRecursively();

				return ret;
			} // GetMatrixRecursively()
			#endregion

			#region To string
			/// <summary>
			/// To string, useful for debugging and testing.
			/// </summary>
			/// <returns>String</returns>
			public override string ToString()
			{
				return "Bone: Id=" + id + ", Num=" + num + ", Position=" + pos;
			} // ToString()
			#endregion
		} // class Bone
		#endregion

		#region Variables
		/// <summary>
		/// Vertices for the main mesh (we only support one mesh here!).
		/// This is a new member and just used for the AnimatedColladaModel.
		/// All the rest of the required variables (vertex and index buffer, etc.)
		/// are already defined in ColladaModel.
		/// </summary>
		private List<SkinnedTangentVertex> skinnedVertices =
			new List<SkinnedTangentVertex>();

		/// <summary>
		/// Flat list of bones, the first bone is always the root bone, all
		/// children can be accessed from here. The main reason for having a flat
		/// list is easy access to all bones for showing bone previous and of
		/// course to quickly access all animation matrices.
		/// </summary>
		List<Bone> bones = new List<Bone>();

		/// <summary>
		/// Number of values in the animationMatrices in each bone.
		/// Split the animations up into several states (stay, stay to walk,
		/// walk, fight, etc.), see variables below.
		/// </summary>
		int numOfAnimations = 1;

		/// <summary>
		/// Get frame rate from Collada file, should always be 30, but sometimes
		/// test models might have different times (like 24).
		/// </summary>
		float frameRate = 30;

		/// <summary>
		/// Animation types for all characters we support here (player, goblin,
		/// ogre).
		/// </summary>
		enum AnimationTypes
		{
			Run,
			Idle,
			Hit1,
			Hit2,
			Die,
		} // enum AnimationTypes

		/// <summary>
		/// Number of animation types each animated model has (and must have).
		/// </summary>
		const int NumberOfAnimationTypes = 5;

		/// <summary>
		/// Animation length of each animation type.
		/// </summary>
		public static readonly int[] AnimationLengths =
			{ 21, 21, 31, 23, 164 };
			//{ 21, 21, 45, 29, 233 };
			//{ 21, 21, 21, 13 };
		/// <summary>
		/// Offsets for each animation type.
		/// </summary>
		int[] animationOffsets =
			{ 0, 21, 42, 73, 97 };
			//{ 0, 21, 42, 88, 117 };
			//{ 0, 21, 42, 63 };

		/// <summary>
		/// Only used for the player model to place the flare.
		/// </summary>
		Bone playerFlareBone = null;
		/// <summary>
		/// Only used for the player model to place the flare with help of
		/// playerFlareBone.
		/// </summary>
		Matrix playerFlareMatrix = Matrix.Identity;

		/// <summary>
		/// Player weapon bone, only used for the player model.
		/// </summary>
		Bone playerWeaponBone = null;
		/// <summary>
		/// Player weapon matrix, only used for the player model.
		/// </summary>
		Matrix playerWeaponMatrix = Matrix.Identity;

		/// <summary>
		/// Is this the goblin wizard unit?
		/// </summary>
		bool isWizard = false,
			isPlayerModel = false;
		#endregion

		#region Properties
		/// <summary>
		/// Total number of bones
		/// </summary>
		/// <returns>Int</returns>
		public int TotalNumberOfBones
		{
			get
			{
				return bones.Count;
			} // get
		} // TotalNumberOfBones
		#endregion

		#region Constructor
		/// <summary>
		/// Create a model from a collada file
		/// </summary>
		/// <param name="setName">Set name</param>
		public AnimatedColladaModel(string setName)
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

			isWizard = filename.Contains("Wizard");
			isPlayerModel = filename.Contains("Hero");

			// Load material (we only support one)
			//Not used, always autoassign: LoadMaterial(colladaFile);
			AutoAssignMaterial();
			
			// Load bones
			LoadBones(colladaFile);

			// Load mesh (vertices data, combine with bone weights)
			LoadMesh(colladaFile);

			// And finally load bone animation data
			LoadAnimation(colladaFile);

			// Close file, we are done.
			file.Close();
		} // AnimatedColladaModel(setFilename)
		#endregion

		#region Load material
		/*unused
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
		} // LoadMaterial(colladaFile, TextureIDDictionary, MaterialIDDictionary)
		 */
		#endregion

		#region Load bones
		/// <summary>
		/// Load bones
		/// </summary>
		/// <param name="colladaFile">Collada file</param>
		private void LoadBones(XmlNode colladaFile)
		{
			// We need to find the bones in the visual scene
			XmlNode visualSceneNode = XmlHelper.GetChildNode(
				XmlHelper.GetChildNode(colladaFile, "library_visual_scenes"),
				"visual_scene");

			// Just go through library_visual_scenes and collect all bones
			// +hierachy from there!
			FillBoneNodes(null, visualSceneNode);
		} // LoadBones(colladaFile)

		/// <summary>
		/// Fill bone nodes helper method for LoadBones.
		/// </summary>
		/// <param name="parentBone">Parent bone</param>
		/// <param name="boneNodes">Bone nodes as XmlNodes</param>
		private void FillBoneNodes(Bone parentBone, XmlNode boneNodes)
		{
			foreach (XmlNode boneNode in boneNodes)
				if (boneNode.Name == "node")
				{
					if (XmlHelper.GetXmlAttribute(boneNode, "id").Contains("Bone") ||
						XmlHelper.GetXmlAttribute(boneNode, "type").Contains("JOINT"))
					{
						Matrix matrix = Matrix.Identity;

						// Get all sub nodes for the matrix, sorry translate and rotate nodes
						// are not supported here yet. Reconstructing the matrices is a
						// little bit complicated because we have to reconstruct the animation
						// data too and I don't want to overcomplicate this test project.
						foreach (XmlNode subnode in boneNode)
						{
							switch (subnode.Name)
							{
								case "translate":
								case "rotate":
									throw new InvalidOperationException(
										"Unsupported bone data found for bone " + bones.Count +
										". Please make sure you save the collada file with baked " +
										"matrices!");
								case "matrix":
									matrix = LoadColladaMatrix(
										StringHelper.ConvertStringToFloatArray(subnode.InnerText), 0);
									break;
							} // switch
						} // foreach (subnode)

						// Create this node, use the current number of bones as number.
						Bone newBone = new Bone(matrix, parentBone, bones.Count,
							XmlHelper.GetXmlAttribute(boneNode, "sid"));

						// Add to our global bones list
						bones.Add(newBone);
						// And to our parent, this way we have a tree and a flat list in
						// the bones list :)
						if (parentBone != null)
							parentBone.children.Add(newBone);

						// Create all children (will do nothing if there are no sub bones)
						FillBoneNodes(newBone, boneNode);
					} // if (XmlHelper.GetXmlAttribute)
					// Also check for extra helpers (used only for the player)
					else if (XmlHelper.GetXmlAttribute(boneNode, "name").
						Contains("flare"))
					{
						playerFlareBone = parentBone;
						playerFlareMatrix = LoadColladaMatrix(
							StringHelper.ConvertStringToFloatArray(
							XmlHelper.GetChildNode(boneNode, "matrix").InnerText), 0);
					} // else
					else if (XmlHelper.GetXmlAttribute(boneNode, "name").
						Contains("weapon"))
					{
						playerWeaponBone = parentBone;
						playerWeaponMatrix = LoadColladaMatrix(
							StringHelper.ConvertStringToFloatArray(
							XmlHelper.GetChildNode(boneNode, "matrix").InnerText), 0);
					} // else
				} // foreach if (boneNode.Name)
		} // FillBoneNodes(parentBone, boneNodes)
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
						XmlHelper.GetChildNode(colladaFile, "mesh"));

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
		/// Load mesh geometry
		/// </summary>
		/// <param name="geometry"></param>
		private void LoadMeshGeometry(XmlNode colladaFile, XmlNode meshNode)
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

			#region Load helper matrixes (bind shape and all bind poses matrices)
			// We only support 1 skinning, so just load the first skin node!
			XmlNode skinNode = XmlHelper.GetChildNode(
				XmlHelper.GetChildNode(
				XmlHelper.GetChildNode(colladaFile, "library_controllers"),
				"controller"), "skin");
			objectMatrix = LoadColladaMatrix(
				StringHelper.ConvertStringToFloatArray(
				XmlHelper.GetChildNode(skinNode, "bind_shape_matrix").InnerText), 0) *
				Matrix.CreateScale(globalScaling);

			// Get the order of the bones used in collada (can be different than ours)
			int[] boneArrayOrder = new int[bones.Count];
			int[] invBoneArrayOrder = new int[bones.Count];
			string boneNameArray =
				XmlHelper.GetChildNode(skinNode, "Name_array").InnerText;
			int arrayIndex = 0;
			foreach (string boneName in boneNameArray.Split(' '))
			{
				boneArrayOrder[arrayIndex] = -1;
				foreach (Bone bone in bones)
					if (bone.id == boneName)
					{
						boneArrayOrder[arrayIndex] = bone.num;
						invBoneArrayOrder[bone.num] = arrayIndex;
						break;
					} // foreach

				if (boneArrayOrder[arrayIndex] == -1)
					throw new InvalidOperationException(
						"Unable to find boneName=" + boneName +
						" in our bones array for skinning!");
				arrayIndex++;
			} // foreach
			#endregion

			#region Load weights
			float[] weights = null;
			foreach (XmlNode sourceNode in skinNode)
			{
				// Get all inv bone skin matrices
				if (sourceNode.Name == "source" &&
					XmlHelper.GetXmlAttribute(sourceNode, "id").Contains("bind_poses"))
				{
					// Get inner float array
					float[] mat = StringHelper.ConvertStringToFloatArray(
						XmlHelper.GetChildNode(sourceNode, "float_array").InnerText);
					for (int boneNum = 0; boneNum < bones.Count; boneNum++)
						if (mat.Length / 16 > boneNum)
						{
							bones[boneArrayOrder[boneNum]].invBoneSkinMatrix =
								LoadColladaMatrix(mat, boneNum * 16);
						} // for if
				} // if

				// Get all weights
				if (sourceNode.Name == "source" &&
					XmlHelper.GetXmlAttribute(sourceNode, "id").Contains("skin-weights"))
				{
					// Get inner float array
					weights = StringHelper.ConvertStringToFloatArray(
						XmlHelper.GetChildNode(sourceNode, "float_array").InnerText);
				} // if
			} // foreach

			if (weights == null)
				throw new InvalidOperationException(
					"No weights were found in our skin, unable to continue!");
			#endregion

			#region Prepare weights and joint indices, we only want the top 3!
			// Helper to access the bones (first index) and weights (second index).
			// If we got more than 2 indices for an entry here, there are multiple
			// weights used (usually 1 to 3, if more, we only use the strongest).
			XmlNode vertexWeightsNode =
				XmlHelper.GetChildNode(skinNode, "vertex_weights");
			int[] vcountArray = StringHelper.ConvertStringToIntArray(
				XmlHelper.GetChildNode(skinNode, "vcount").InnerText);
			int[] vArray = StringHelper.ConvertStringToIntArray(
				XmlHelper.GetChildNode(skinNode, "v").InnerText);

			// Build vertexSkinJoints and vertexSkinWeights for easier access.
			List<Vector3> vertexSkinJoints = new List<Vector3>();
			List<Vector3> vertexSkinWeights = new List<Vector3>();
			int vArrayIndex = 0;
			for (int num = 0; num < vcountArray.Length; num++)
			{
				int vcount = vcountArray[num];
				List<int> jointIndices = new List<int>();
				List<int> weightIndices = new List<int>();
				for (int i = 0; i < vcount; i++)
				{
					// Make sure we convert the internal number to our bone numbers!
					jointIndices.Add(boneArrayOrder[vArray[vArrayIndex]]);
					weightIndices.Add(vArray[vArrayIndex + 1]);
					vArrayIndex += 2;
				} // for

				// If we got less than 3 values, add until we have enough,
				// this makes the computation easier below
				while (jointIndices.Count < 3)
					jointIndices.Add(0);
				while (weightIndices.Count < 3)
					weightIndices.Add(-1);
				
				// Find out top 3 weights
				float[] weightValues = new float[weightIndices.Count];
				int[] bestWeights = { 0, 1, 2 };
				for (int i = 0; i < weightIndices.Count; i++)
				{
					// Use weight of zero for invalid indices.
					if (weightIndices[i] < 0 ||
						weightValues[i] >= weights.Length)
						weightValues[i] = 0;
					else
						weightValues[i] = weights[weightIndices[i]];

					// Got 4 or more weights? Then just select the top 3!
					if (i >= 3)
					{
						float lowestWeight = 1.0f;
						int lowestWeightOverride = 2;
						for (int b = 0; b < bestWeights.Length; b++)
							if (lowestWeight > weightValues[bestWeights[b]])
							{
								lowestWeight = weightValues[bestWeights[b]];
								lowestWeightOverride = b;
							} // for if
						// Replace lowest weight
						bestWeights[lowestWeightOverride] = i;
					} // if
				} // for

				// Now build 2 vectors from the best weights
				Vector3 boneIndicesVec = new Vector3(
					jointIndices[bestWeights[0]],
					jointIndices[bestWeights[1]],
					jointIndices[bestWeights[2]]);
				Vector3 weightsVec = new Vector3(
					weightValues[bestWeights[0]],
					weightValues[bestWeights[1]],
					weightValues[bestWeights[2]]);
				// Renormalize weight, important if we got more entries before
				// and always good to do!
				float totalWeights = weightsVec.X + weightsVec.Y + weightsVec.Z;
				if (totalWeights == 0)
					weightsVec.X = 1.0f;
				else
				{
					weightsVec.X /= totalWeights;
					weightsVec.Y /= totalWeights;
					weightsVec.Z /= totalWeights;
				} // else

				vertexSkinJoints.Add(boneIndicesVec);
				vertexSkinWeights.Add(weightsVec);
			} // for
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
				skinnedVertices.Clear();
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

					// Get vertex blending stuff (uses pos too)
					Vector3 blendWeights = vertexSkinWeights[pos/3];
					Vector3 blendIndices = vertexSkinJoints[pos/3];
					// Pre-multiply all indices with 3, this way the shader
					// code gets a little bit simpler and faster
					blendIndices = new Vector3(
						blendIndices.X * 3, blendIndices.Y * 3, blendIndices.Z * 3);

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
					skinnedVertices.Add(new SkinnedTangentVertex(
						position, blendWeights, blendIndices, u, v, normal, tangent));

					// Remember pos for optimizing the vertices later more easily.
					reuseVertexPositions[i] = pos / 3;
					reverseReuseVertexPositions[pos / 3].Add(i);
				} // for (ushort)

				// Only support one mesh for now, get outta here.
				return;
			} // foreach (trianglenode)

			throw new InvalidOperationException(
				"No mesh found in this collada file, unable to continue!");
			#endregion
		} // LoadMeshGeometry(geometry)
		#endregion

		#region Load animation
		#region Load animation targets
		/// <summary>
		/// Animation target values to help us loading animations a little.
		/// Thanks to this dictionary we are able to load all float arrays
		/// at once and then use them wherever we need them later to fill in
		/// the animation matrices for every animated bone.
		/// </summary>
		Dictionary<string, float[]> animationTargetValues =
			new Dictionary<string, float[]>();

		/// <summary>
		/// Load Animation data from a collada file, ignoring timesteps,
		/// interpolation and multiple animations
		/// </summary>
		/// <param name="colladaFile"></param>
		/// <param name="AnimationTargetValue"></param>
		private void LoadAnimationTargets(XmlNode colladaFile)
		{
			// Get global frame rate
			try
			{
				frameRate = Convert.ToSingle(
					XmlHelper.GetChildNode(colladaFile, "frame_rate").InnerText,
                    NumberFormatInfo.InvariantInfo);
			} // try
			catch { } // ignore if that fails

			XmlNode libraryanimation = XmlHelper.GetChildNode(colladaFile,
				"library_animations");
			if (libraryanimation == null)
				return;

			LoadAnimationHelper(libraryanimation);
		} // LoadAnimation(colladaFile, AnimationTargetValues)

		/// <summary>
		/// Load animation helper, goes over all animations in the animationnode,
		/// calls itself recursively for sub-animation-nodes
		/// </summary>
		/// <param name="animationnode">Animationnode</param>
		private void LoadAnimationHelper(XmlNode animationnode)
		{
			// go over all animation elements
			foreach (XmlNode node in animationnode)
			{
				if (node.Name == "animation")
				//not a channel but another animation node
				{
					LoadAnimationHelper(node);
					continue;
				} // if (animation.name)

				if (node.Name != "channel")
					continue;
				string samplername =
					XmlHelper.GetXmlAttribute(node, "source").Substring(1);
				string targetname = XmlHelper.GetXmlAttribute(node, "target");

				// Find the sampler for the animation values.
				XmlNode sampler = XmlHelper.GetChildNode(animationnode, "id",
					samplername);

				// Find value xml node
				string valuename = XmlHelper.GetXmlAttribute(
					XmlHelper.GetChildNode(sampler, "semantic", "OUTPUT"),
					"source").Substring(1) + "-array";
				XmlNode valuenode = XmlHelper.GetChildNode(animationnode, "id",
					valuename);

				// Parse values and add to dictionary
				float[] values =
					StringHelper.ConvertStringToFloatArray(valuenode.InnerText);
				animationTargetValues.Add(targetname, values);

				// Set number of animation we will use in all bones.
				// Leave last animation value out later, but make filling array
				// a little easier (last matrix is just unused then).
				numOfAnimations = values.Length;
				// If these are matrix values, devide by 16!
				if (XmlHelper.GetXmlAttribute(valuenode, "id").Contains("transform"))
					numOfAnimations /= 16;
			} // foreach (node)
		} // LoadAnimationHelper(animationnode, AnimationTargetValues)
		#endregion

		#region FillBoneAnimations
		/// <summary>
		/// Fill bone animations, called from LoadAnimation after we got all
		/// animationTargetValues.
		/// </summary>
		/// <param name="colladaFile">Collada file</param>
		private void FillBoneAnimations(XmlNode colladaFile)
		{
			foreach (Bone bone in bones)
			{
				// Loads animation data from bone node sid, links them
				// automatically, also generates animation matrices.
				// Note: We only support the transform node here, "RotX", "RotY",
				// etc. will only be used if we don't got baked matrices, but that
				// is catched already when loading the initial bone matrices above.

				// Build sid the way it is used in the collada file.
				string sid = bone.id + "/" + "transform";

				int framecount = 0;
				if (animationTargetValues.ContainsKey(sid))
					// Transformation contains whole matrix (always 4x4).
					framecount = animationTargetValues[sid].Length / 16;

				// Expand array and duplicate the initial matrix in case
				// there is no animation data present (often the case).
				for (int i = 0; i < numOfAnimations; i++)
					bone.animationMatrices.Add(bone.initialMatrix);

				if (framecount > 0)
				{
					float[] mat = animationTargetValues[sid];
					// Load all absolute animation matrices. If you want relative
					// data here you can use the invBoneMatrix (invert initialMatrix),
					// but this won't be required here because all animations are
					// already computed. Maybe you need it when doing your own animations.
					for (int num = 0; num < bone.animationMatrices.Count &&
						num < framecount; num++)
					{
						bone.animationMatrices[num] = LoadColladaMatrix(mat, num * 16);
					} // for (num)
				} // if (framecount)
			} // foreach (bone)
		} // FillBoneAnimations(colladaFile)
		#endregion

		#region CalculateAbsoluteBoneMatrices
		/// <summary>
		/// Calculate absolute bone matrices for finalMatrix. Not really required,
		/// but this way we can start using the bones without having to call
		/// UpdateAnimation (maybe we don't want to animate yet).
		/// </summary>
		private void CalculateAbsoluteBoneMatrices()
		{
			foreach (Bone bone in bones)
			{
				// Get absolute matrices and also use them for the initial finalMatrix
				// of each bone, which is used for rendering.
				bone.finalMixMatrix = bone.GetMatrixRecursively();
			} // foreach (bone)
		} // CalculateAbsoluteBoneMatrices()
		#endregion

		#region MixAndFixIdleAnimation
		/// <summary>
		/// Mix and fix idle animation
		/// </summary>
		private void MixAndFixIdleAnimation()
		{
			// Go through all bones
			foreach (Bone bone in bones)
			{
				// Fix the idle animation (last 5 frames)
				int start = animationOffsets[(int)AnimationTypes.Idle];
				int length = AnimationLengths[(int)AnimationTypes.Idle];
				int end = start+length-1;
				// Mix start with end
				bone.animationMatrices[start] =
					bone.animationMatrices[start] * 0.2f +
					bone.animationMatrices[end] * 0.8f;
				bone.animationMatrices[start + 1] =
					bone.animationMatrices[start + 1] * 0.4f +
					bone.animationMatrices[start] * 0.6f;
				bone.animationMatrices[start + 2] =
					bone.animationMatrices[start + 2] * 0.6f +
					bone.animationMatrices[start + 1] * 0.4f;
				bone.animationMatrices[start + 3] =
					bone.animationMatrices[start + 3] * 0.6f +
					bone.animationMatrices[start + 2] * 0.4f;
				// Mix end with start
				bone.animationMatrices[end] =
					bone.animationMatrices[end] * 0.2f +
					bone.animationMatrices[start] * 0.8f;
				bone.animationMatrices[end - 1] =
					bone.animationMatrices[end - 1] * 0.4f +
					bone.animationMatrices[end] * 0.6f;
				bone.animationMatrices[end - 2] =
					bone.animationMatrices[end - 2] * 0.6f +
					bone.animationMatrices[end - 1] * 0.4f;
				bone.animationMatrices[end - 3] =
					bone.animationMatrices[end - 3] * 0.6f +
					bone.animationMatrices[end - 2] * 0.4f;
			} // foreach (bone)
		} // MixAndFixIdleAnimation()
		#endregion

		/// <summary>
		/// Load animation
		/// </summary>
		/// <param name="colladaFile">Collada file</param>
		private void LoadAnimation(XmlNode colladaFile)
		{
			// Little helper to load and store all animation values
			// before assigning them to the bones where they are used.
			LoadAnimationTargets(colladaFile);

			// Fill animation matrices in each bone (if used or not, always fill).
			FillBoneAnimations(colladaFile);

			// Mix idle animation, christoph exported it wrongly!
			MixAndFixIdleAnimation();

			// Calculate all absolute matrices. We only got relative ones in
			// initialMatrix for each bone right now.
			CalculateAbsoluteBoneMatrices();
		} // LoadAnimation(colladaFile)
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
				return (ushort)(oldIndex + 1);
			else //if (polygonIndex == 2)
				return (ushort)(oldIndex - 1);
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
		/// <returns>ushort array for the optimized indices</returns>
		private ushort[] OptimizeVertexBuffer()
		{
			List<SkinnedTangentVertex> newVertices =
				new List<SkinnedTangentVertex>();
			List<ushort> newIndices = new List<ushort>();

			// Helper to only search already added newVertices and for checking the
			// old position indices by transforming them into newVertices indices.
			List<int> newVerticesPositions = new List<int>();

			// Go over all vertices (indices are currently 1:1 with the vertices)
			for (int num = 0; num < skinnedVertices.Count; num++)
			{
				// Get current vertex
				SkinnedTangentVertex currentVertex = skinnedVertices[num];
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
						SkinnedTangentVertex.NearlyEquals(
						currentVertex, newVertices[newIndices[otherVertexIndex]]))
					{
						// Reuse the existing vertex, don't add it again, just
						// add another index for it!
						newIndices.Add((ushort)newIndices[otherVertexIndex]);
						reusedExistingVertex = true;
						break;
					} // if (TangentVertex.NearlyEquals)
				} // foreach (otherVertexIndex)

				if (reusedExistingVertex == false)
				{
					// Add the currentVertex and set it as the current index
					newIndices.Add((ushort)newVertices.Count);
					newVertices.Add(currentVertex);
				} // if (reusedExistingVertex)
			} // for (num)

			// Finally flip order of all triangles to allow us rendering
			// with CullCounterClockwiseFace (default for XNA) because all the data
			// is in CullClockwiseFace format right now!
			for (int num = 0; num < newIndices.Count / 3; num++)
			{
				ushort swap = newIndices[num * 3 + 1];
				newIndices[num * 3 + 1] = newIndices[num * 3 + 2];
				newIndices[num * 3 + 2] = swap;
			} // for

			// Reassign the vertices, we might have deleted some duplicates!
			skinnedVertices = newVertices;

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
		/// <returns>ushort array for the optimized indices</returns>
		private ushort[] OptimizeVertexBufferSlow()
		{
			List<SkinnedTangentVertex> newVertices =
				new List<SkinnedTangentVertex>();
			List<ushort> newIndices = new List<ushort>();

			// Go over all vertices (indices are currently 1:1 with the vertices)
			for (int num = 0; num < skinnedVertices.Count; num++)
			{
				// Try to find already existing vertex in newVertices list that
				// matches the vertex of the current index.
				SkinnedTangentVertex currentVertex =
					skinnedVertices[FlipIndexOrder(num)];
				bool reusedExistingVertex = false;
				for (int checkNum = 0; checkNum < newVertices.Count; checkNum++)
				{
					if (SkinnedTangentVertex.NearlyEquals(
						currentVertex, newVertices[checkNum]))
					{
						// Reuse the existing vertex, don't add it again, just
						// add another index for it!
						newIndices.Add((ushort)checkNum);
						reusedExistingVertex = true;
						break;
					} // if (TangentVertex.NearlyEquals)
				} // for (checkNum)

				if (reusedExistingVertex == false)
				{
					// Add the currentVertex and set it as the current index
					newIndices.Add((ushort)newVertices.Count);
					newVertices.Add(currentVertex);
				} // if (reusedExistingVertex)
			} // for (num)

			// Reassign the vertices, we might have deleted some duplicates!
			skinnedVertices = newVertices;

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
			ushort[] indices = OptimizeVertexBuffer();
			// For testing, this one is REALLY slow (see method summary)!
			//ushort[] indices = OptimizeVertexBufferSlow();

			// Create the vertex buffer from our vertices.
			vertexBuffer = new VertexBuffer(
				BaseGame.Device,
				typeof(SkinnedTangentVertex),
				//typeof(TangentVertex),
				skinnedVertices.Count,
				BufferUsage.WriteOnly);
			vertexBuffer.SetData(skinnedVertices.ToArray());
			numOfVertices = skinnedVertices.Count;

			// We only support max. 65535 optimized vertices, which is really a
			// lot, but more would require a int index buffer (twice as big) and
			// I have never seen any realtime 3D model that needs more vertices ^^
			if (skinnedVertices.Count > ushort.MaxValue)
				throw new InvalidOperationException(
					"Too much vertices to index, optimize vertices or use "+
					"fewer vertices. Vertices="+skinnedVertices.Count+
					", Max Vertices for Index Buffer="+ushort.MaxValue);

			// Create the index buffer from our indices (Note: While the indices
			// will point only to 16bit (ushort) vertices, we can have a lot
			// more indices in this list than just 65535).
			indexBuffer = new IndexBuffer(
				BaseGame.Device,
				typeof(ushort),
				indices.Length,
				BufferUsage.WriteOnly);
			indexBuffer.SetData(indices);
			numOfIndices = indices.Length;
		} // GenerateVertexAndIndexBuffers()
		#endregion

		#region Update animation
		/*obs
		/// <summary>
		/// Was this animation data already constructed last time we called
		/// UpdateAnimation? Will not optimize much if you render several models
		/// of this type (maybe use a instance class that holds this animation
		/// data and just calls this class for rendering to optimize it further),
		/// but if you just render a single model, it gets a lot faster this way.
		/// </summary>
		private int lastAniMatrixNum = -1;
		 */
		public static AnimatedGameObject currentAnimatedObject = null;

		/// <summary>
		/// Update animation. Will do nothing if animation stayed the same since
		/// last time we called this method.
		/// </summary>
		/// <param name="renderMatrix">Render matrix just for adding some
		/// offset value to the animation time, remove if you allow moving
		/// objects, this is just for testing!</param>
		private void UpdateAnimation(Matrix renderMatrix, int animationToUpdate)
		{
			float addTime = 0.0f;
			if (currentAnimatedObject != null)
				addTime = currentAnimatedObject.addAnimationTime;

			float localFrameRate = frameRate;
			// Use slower framerate for wizard fire (else shoots too much and moves
			// way too quick).
			if (isWizard &&
				animationToUpdate == (int)AnimationTypes.Hit1)
				localFrameRate /= 2.0f;

			// Add some time to the animation depending on the position.
			int aniMatrixNum = ((int)((Player.GameTime + addTime) * localFrameRate)) %
				//complete animation: numOfAnimations;
				AnimationLengths[animationToUpdate];
			if (currentAnimatedObject != null &&
				currentAnimatedObject.state == AnimatedGameObject.States.Die)
			{
				aniMatrixNum = ((int)(currentAnimatedObject.timeTillDeath * frameRate));
				if (aniMatrixNum >= AnimationLengths[animationToUpdate])
					aniMatrixNum = AnimationLengths[animationToUpdate] - 1;
			} // if (currentAnimatedObject)

			if (aniMatrixNum < 0)
				aniMatrixNum = 0;
			aniMatrixNum += animationOffsets[animationToUpdate];

			/*can never happen, we update NumberOfAnimationTypes at the same time
			// No need to update if everything stayed the same
			if (aniMatrixNum == lastAniMatrixNum)
				return;
			lastAniMatrixNum = aniMatrixNum;
			 */

			foreach (Bone bone in bones)
			{
				// Just assign the final matrix from the animation matrices.
				bone.finalBlendMatrix[animationToUpdate] =
					bone.animationMatrices[aniMatrixNum];

				// Also use parent matrix if we got one
				// This will always work because all the bones are in order.
				if (bone.parent != null)
					bone.finalBlendMatrix[animationToUpdate] *=
						bone.parent.finalBlendMatrix[animationToUpdate];
			} // foreach
		} // UpdateAnimation()
		#endregion

		#region GetBoneMatrices
		/// <summary>
		/// 40 bones is sufficiant for our little game here.
		/// </summary>
		const int MaxNumberOfBones = 40;//80;

		bool updateBoneMatricesForPlayerOnlyOnce = true;

		/// <summary>
		/// Get bone matrices for the shader. We have to apply the invBoneSkinMatrix
		/// to each final matrix, which is the recursively created matrix from
		/// all the animation data (see UpdateAnimation).
		/// </summary>
		/// <returns></returns>
		private Matrix[] GetBoneMatrices(Matrix renderMatrix, float[] blendedStates)
		{
			// Only update player once per frame
			if (isPlayerModel == false ||
				updateBoneMatricesForPlayerOnlyOnce)
			{
				updateBoneMatricesForPlayerOnlyOnce = false;

				// Update all the matrices and blend them into each other.
				for (int i = 0; i < NumberOfAnimationTypes; i++)
				{
					// Update the animation data in case it is not up to date anymore.
					UpdateAnimation(renderMatrix, i);
				} // for (int)

				// Now mix in all animations for the finalMixMatrix (was done before
				// in UpdateAnimation for just one animation and was just called
				// finalMatrix).
				for (int num = 0; num < bones.Count; num++)
				{
					bones[num].finalMixMatrix =
						bones[num].finalBlendMatrix[0] * blendedStates[0] +
						bones[num].finalBlendMatrix[1] * blendedStates[1] +
						bones[num].finalBlendMatrix[2] * blendedStates[2] +
						bones[num].finalBlendMatrix[3] * blendedStates[3] +
						bones[num].finalBlendMatrix[4] * blendedStates[4];
				} // for (num)
			} // if (updateBoneMatricesForPlayerOnlyOnce)

			// And get all bone matrices, we support max. 40 (was 80) (see shader).
			Matrix[] matrices = new Matrix[Math.Min(MaxNumberOfBones, bones.Count)];
			for (int num = 0; num < matrices.Length; num++)
				// The matrices are constructed from the invBoneSkinMatrix and
				// the finalMatrix, which holds the recursively added animation matrices
				// and finally we add the render matrix too here.
				matrices[num] =
					bones[num].invBoneSkinMatrix * bones[num].finalMixMatrix *
					OnlyScaleTransformationInverse(renderMatrix);

			return matrices;
		} // GetBoneMatrices()
		#endregion

		#region Render
		/// <summary>
		/// Render the animated model (will call UpdateAnimation internally,
		/// but if you do that yourself before calling this method, it gets
		/// optimized out). Rendering always uses the skinnedNormalMapping shader
		/// with the DiffuseSpecular30 technique.
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public void Render(Matrix renderMatrix, float[] blendedStates)
		{
			// Make sure we use the correct vertex declaration for our shader.
			BaseGame.Device.VertexDeclaration =
				SkinnedTangentVertex.VertexDeclaration;
			// Set the world matrix for this object (often Identity).
			// The renderMatrix is directly applied to the matrices we use
			// as bone matrices for the shader (has to be done this way because
			// the bone matrices are transmitted transposed and we could lose
			// important render matrix translation data if we do not apply it there).
			BaseGame.WorldMatrix =
				//fix bug ... note, 3ds max transformations will not work!
				Matrix.CreateScale(0.01f);
				// objectMatrix;

			// And set all bone matrices (Goblin has 40, but we support up to 80).
			ShaderEffect.skinnedNormalMapping.SetBoneMatrices(
				GetBoneMatrices(renderMatrix, blendedStates));

			// Rendering is pretty straight forward (if you know how anyway).
			ShaderEffect.skinnedNormalMapping.RenderSinglePassShader(
				material,
				RenderVertices);

			// Update bone matrices next frame again!
			//TODO: if we got 2 players, this gets more complicated!
			updateBoneMatricesForPlayerOnlyOnce = true;
		} // Render(renderMatrix)

		/// <summary>
		/// Default animation for Render, GenerateShadow and UseShadow.
		/// </summary>
		static readonly float[] DefaultAnimation = new float[] { 0, 0, 1, 0, 0 };

		/// <summary>
		/// Render
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public override void Render(Matrix renderMatrix)
		{
			Render(renderMatrix, DefaultAnimation);
		} // Render(renderMatrix)

		/// <summary>
		/// Render vertices
		/// </summary>
		private void RenderVertices()
		{
			BaseGame.Device.Vertices[0].SetSource(vertexBuffer, 0,
				SkinnedTangentVertex.SizeInBytes);
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
		public void GenerateShadow(Matrix renderMatrix,
			float[] blendedStates)
		{
			// Set bone matrices and the world matrix for the shader.
			ShaderEffect.shadowMapping.SetBoneMatrices(
				GetBoneMatrices(renderMatrix, blendedStates));
			ShaderEffect.shadowMapping.UpdateGenerateShadowWorldMatrix(objectMatrix);

			// And render
			BaseGame.Device.VertexDeclaration =
				SkinnedTangentVertex.VertexDeclaration;
			ShaderEffect.shadowMapping.UseAnimatedShadowShader();
			RenderVertices();
		} // Render(shader, shaderTechnique)

		/// <summary>
		/// Generate shadow
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public override void GenerateShadow(Matrix renderMatrix)
		{
			GenerateShadow(renderMatrix, DefaultAnimation);
		} // GenerateShadow(renderMatrix)
		#endregion

		#region Use shadow
		/// <summary>
		/// Use shadow on the plane, useful for our unit tests. The plane does not
		/// throw shadows, so we don't need a GenerateShadow method.
		/// </summary>
		public void UseShadow(Matrix renderMatrix,
			float[] blendedStates)
		{
			// Set bone matrices and the world matrix for the shader.
			ShaderEffect.shadowMapping.SetBoneMatrices(
				GetBoneMatrices(renderMatrix, blendedStates));
			ShaderEffect.shadowMapping.UpdateCalcShadowWorldMatrix(objectMatrix);

			// And render
			BaseGame.Device.VertexDeclaration =
				SkinnedTangentVertex.VertexDeclaration;
			ShaderEffect.shadowMapping.UseAnimatedShadowShader();
			RenderVertices();
		} // UseShadow()

		/// <summary>
		/// Use shadow
		/// </summary>
		/// <param name="renderMatrix">Render matrix</param>
		public override void UseShadow(Matrix renderMatrix)
		{
			UseShadow(renderMatrix, DefaultAnimation);
		} // UseShadow(renderMatrix)
		#endregion

		#region HandlePlayerFlareAndWeapon
		public static Vector3 finalPlayerFlarePos = Vector3.Zero;
		static Vector3 lastSmokePos = Vector3.Zero;
		/// <summary>
		/// Handle player flare and weapon. This is a helper method to handle
		/// the animated flare and weapon positions, which are bound to bones.
		/// </summary>
		public Matrix HandlePlayerFlareAndWeapon()
		{
			if (playerFlareBone != null)
			{
				// Calc finalPlayerFlarePos, which is used in LightManager
				finalPlayerFlarePos = OnlyScaleTransformation(
					playerFlareMatrix * playerFlareBone.finalMixMatrix *
					Matrix.CreateRotationZ(BaseGame.camera.PlayerRotation)).
					Translation;

				// Add smoke for flare, only add it if some time has passed and
				// if we have moved, else just don't add anything most of the time!
				Vector3 newSmokePos = BaseGame.camera.PlayerPos + finalPlayerFlarePos;
				if (BaseGame.EveryMs(50) &&
					(Vector3.Distance(newSmokePos, lastSmokePos) > 0.05f ||
					BaseGame.EveryMs(222)))
				{
					lastSmokePos = newSmokePos;
					EffectManager.AddEffect(
						newSmokePos +
						new Vector3(0, 0, 0.1f) +
						RandomHelper.GetRandomVector3(-0.1f, 0.1f),
						EffectManager.EffectType.Smoke, 0.156789f,
						RandomHelper.GetRandomFloat(0, (float)Math.PI * 2.0f));
				} // if (BaseGame.EveryMs)
			} // if (playerFlareBone)

			// Render the weapon of the player
			if (playerWeaponBone != null)
				return OnlyScaleTransformation(
					playerWeaponMatrix * playerWeaponBone.finalMixMatrix);
			else
				return Matrix.Identity;
		} // HandlePlayerFlareAndWeapon()

		/// <summary>
		/// Get weapon position
		/// </summary>
		/// <returns>Matrix</returns>
		public Vector3 GetWeaponPos(Matrix modelMatrix)
		{
			// Calc the weapon of the player
			if (playerWeaponBone != null)
				return (OnlyScaleTransformation(
					playerWeaponMatrix * playerWeaponBone.finalMixMatrix) *
					modelMatrix).Translation;
			else
				return new Vector3(0, 0.5f, 1.5f);
		} // GetWeaponPos()
		#endregion

		#region Unit Testing
		// Note: Allow calling all this even in release mode (see Program.cs)
		#region TestShowBones
		/*TODO
		/// <summary>
		/// TestShowBones
		/// </summary>
		public static void TestShowBones()
		{
			AnimatedColladaModel model = null;
			PlaneRenderer groundPlane = null;
			// Bone colors for displaying bone lines.
			Color[] BoneColors = new Color[]
				{ Color.Blue, Color.Red, Color.Yellow, Color.White, Color.Teal,
				Color.RosyBrown, Color.Orange, Color.Olive, Color.Maroon, Color.Lime,
				Color.LightBlue, Color.LightGreen, Color.Lavender, Color.Green,
				Color.Firebrick, Color.DarkKhaki, Color.BlueViolet, Color.Beige };

			TestGame.Start("TestLoadColladaModel",
				delegate
				{
					// Load our goblin here, you can also load one of my test models!
					model = new AnimatedColladaModel(
						//"Goblin");
						//"test_bones_simple_baked");
						//"test_bones_advanced_baked");
						"test_man_baked");

					// And load ground plane
					groundPlane = new PlaneRenderer(
						new Vector3(0, 0, -0.001f),
						new Plane(new Vector3(0, 0, 1), 0),
						new Material(
							"GroundStone", "GroundStoneNormal", "GroundStoneHeight"),
						50);
				},
				delegate
				{
					// Show ground
					groundPlane.Render();//obs: ShaderEffect.parallaxMapping, "DiffuseSpecular30");

					// Show bones without rendering the model itself
					if (model.bones.Count == 0)
						return;

					// Update bone animation.
					model.UpdateAnimation(Matrix.Identity, 0);

					// Show bones (all endpoints)
					foreach (Bone bone in model.bones)
					{
						foreach (Bone childBone in bone.children)
							BaseGame.DrawLine(
								bone.finalMatrix.Translation,
								childBone.finalMatrix.Translation,
								BoneColors[bone.num % BoneColors.Length]);
					} // foreach (bone)
				});
		} // TestLoadColladaModel()
		 */
		#endregion

		#region TestPlayerColladaModelScene
		/// <summary>
		/// TestPlayerColladaModelScene
		/// </summary>
		public static void TestPlayerColladaModelScene()
		{
			AnimatedGameObject player = null;
			AnimatedColladaModel playerModel = null;
			//obs: PlaneRenderer groundPlane = null;

			TestGame.Start("TestPlayerColladaModelScene",
				delegate
				{
					// Load Player
					player = new AnimatedGameObject(
						GameManager.AnimatedTypes.Hero,
						Matrix.Identity);
					playerModel = new AnimatedColladaModel(
						//"Hero");
						//"Goblin");
						//"GoblinMaster");
						"GoblinWizard");
						//"Ogre");
						//"BigOgre");

					// Play background music :)
					//Sound.StartMusic();

          /*obs
					// Create ground plane
					groundPlane = new PlaneRenderer(
						new Vector3(0, 0, -0.001f),
						new Plane(new Vector3(0, 0, 1), 0),
						new Material(
							"CaveDetailGround", "CaveDetailGroundNormal",
							"CaveDetailGroundHeight"),
						28);
          */

					// Set light direction (light is coming from the front right pos).
					BaseGame.LightDirection = new Vector3(-18, -20, 16);
				},
				delegate
				{
					// Start glow shader
					//BaseGame.GlowShader.Start();

					// Clear background with white color, looks much cooler for the
					// post screen glow effect.
					//BaseGame.Device.Clear(Color.White);

					// Render goblin always in center, but he is really big, bring him
					// down to a more normal size that fits better in our test scene.
					Matrix renderMatrix =
						Input.Keyboard.IsKeyDown(Keys.LeftControl) ?
						Matrix.CreateScale(0.8f) *
						Matrix.CreateRotationZ(1.4f) *
						Matrix.CreateTranslation(1, 1, 0) :
						Input.Keyboard.IsKeyDown(Keys.LeftAlt) ?
						Matrix.CreateTranslation(1, 1, 0) :
						Matrix.CreateTranslation(0, 0, -1);
						//Matrix.Identity; // should work!
						//Matrix.CreateScale(0.01f);

					// Restore z buffer state
					BaseGame.Device.RenderState.DepthBufferEnable = true;
					BaseGame.Device.RenderState.DepthBufferWriteEnable = true;
//TODO: when we got more time to test
					/*
					// Make sure we use skinned tangent vertex format for shadow mapping
					BaseGame.Device.VertexDeclaration =
						SkinnedTangentVertex.VertexDeclaration;

					// Generate shadows
					ShaderEffect.shadowMapping.GenerateShadows(
						delegate
						{
							playerModel.GenerateShadow(renderMatrix);
						});

					// Render shadows
					ShaderEffect.shadowMapping.RenderShadows(
						delegate
						{
							playerModel.UseShadow(renderMatrix);
							groundPlane.UseShadow();
						});
					//*/

                    /*obs
					// Show ground with DiffuseSpecular material and use parallax mapping!
					groundPlane.Render(
						//ShaderEffect.normalMapping, "DiffuseSpecular30");
						);//just used for testing, looks good: ShaderEffect.parallaxMapping);
                     */

					// Update game time
					Player.SetGameTimeMs(BaseGame.TotalTimeMs);

					// Cycle around with Z/X
					if (Input.KeyboardKeyJustPressed(Keys.Z))
						player.state = (AnimatedGameObject.States)
							(((int)player.state + 1) % NumberOfAnimationTypes);
					else if (Input.KeyboardKeyJustPressed(Keys.X))
						player.state = (AnimatedGameObject.States)
							(((int)player.state + NumberOfAnimationTypes - 1) %
							NumberOfAnimationTypes);
					// And update all blend states
					player.UpdateState();

					// Render the model with the current animation
					playerModel.Render(
						renderMatrix,
						//Matrix.Identity,
						player.blendedStates);
						//new float[] { 1, 0, 0, 0, 0 });

					TextureFont.WriteText(2, 30, "Press Z/X to change the animation");

					TextureFont.WriteText(2, 60, "Blended states: " +
						StringHelper.WriteArrayData(player.blendedStates));

					// show fire flare at weapon for testing, use wizard!
					Vector3 weaponPos = playerModel.GetWeaponPos(renderMatrix);
					EffectManager.AddFireBallEffect(weaponPos, 0, 0.25f);
					BaseGame.effectManager.HandleAllEffects();

					/*obs
					// And show all goblins
					for (int x = 0; x < 2; x++)
						for (int y = 0; y < 3; y++)
							playerModel.Render(
								renderMatrix *
								Matrix.CreateTranslation(-5 + 10 * x, -10 + 10 * y, 0));
					 */
					
					// And show shadows on top of the scene (with shadow blur effect).
					//TODO: ShaderEffect.shadowMapping.ShowShadows();

					// And finally show glow shader on top of everything
					/*BaseGame.GlowShader.Show();
					 */

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
						PostScreenGlow.sceneMapTexture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10 + 256 + 10, 10, 256, 256));
						// Line 2 (3 render targets, 2 post screen blurs, 1 final scene)
						PostScreenGlow.downsampleMapTexture.RenderOnScreenNoAlpha(
							new Rectangle(10, 10 + 256 + 10, 256, 256));
						PostScreenGlow.blurMap1Texture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10, 10 + 256 + 10, 256, 256));
						PostScreenGlow.blurMap2Texture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10 + 256 + 10, 10 + 256 + 10, 256, 256));
					} // if (Input.MouseRightButtonPressed)
					//*/
				});
		} // TestPlayerColladaModelScene()
		#endregion

		#region TestGoblinColladaModelScene
        /*obs!
		/// <summary>
		/// TestGoblinColladaModelScene
		/// </summary>
		public static void TestGoblinColladaModelScene()
		{
			AnimatedColladaModel goblinModel = null;
			PlaneRenderer groundPlane = null;

			TestGame.Start("TestLoadColladaModel",
				delegate
				{
					// Load Goblin
					goblinModel = new AnimatedColladaModel("Goblin");
					goblinModel.material.ambientColor = Material.DefaultAmbientColor;

					// Play background music :)
					Sound.StartMusic();

					// Create ground plane
					groundPlane = new PlaneRenderer(
						new Vector3(0, 0, -0.001f),
						new Plane(new Vector3(0, 0, 1), 0),
						new Material(
							"CaveDetailGround", "CaveDetailGroundNormal",
							"CaveDetailGroundHeight"),
						28);

					// Set light direction (light is coming from the front right pos).
					BaseGame.LightDirection = new Vector3(-18, -20, 16);
				},
				delegate
				{
					// Start glow shader
					BaseGame.GlowShader.Start();

					// Clear background with white color, looks much cooler for the
					// post screen glow effect.
					BaseGame.Device.Clear(Color.White);

					// Render goblin always in center, but he is really big, bring him
					// down to a more normal size that fits better in our test scene.
					Matrix renderMatrix = Matrix.CreateScale(0.1f);

					// Make sure we use skinned tangent vertex format for shadow mapping
					BaseGame.Device.VertexDeclaration =
						SkinnedTangentVertex.VertexDeclaration;

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

					// Show ground with DiffuseSpecular material and use parallax mapping!
					groundPlane.Render();//obs: ShaderEffect.parallaxMapping, "DiffuseSpecular30");

					// And show all goblins
					for (int x = 0; x < 2; x++)
						for (int y = 0; y < 3; y++)
							goblinModel.Render(
								renderMatrix *
								Matrix.CreateTranslation(-5 + 10 * x, -10 + 10 * y, 0));
					
					// And show shadows on top of the scene (with shadow blur effect).
					ShaderEffect.shadowMapping.ShowShadows();

					// And finally show glow shader on top of everything
					BaseGame.GlowShader.Show();

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
						PostScreenGlow.sceneMapTexture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10 + 256 + 10, 10, 256, 256));
						// Line 2 (3 render targets, 2 post screen blurs, 1 final scene)
						PostScreenGlow.downsampleMapTexture.RenderOnScreenNoAlpha(
							new Rectangle(10, 10 + 256 + 10, 256, 256));
						PostScreenGlow.blurMap1Texture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10, 10 + 256 + 10, 256, 256));
						PostScreenGlow.blurMap2Texture.RenderOnScreenNoAlpha(
							new Rectangle(10 + 256 + 10 + 256 + 10, 10 + 256 + 10, 256, 256));
					} // if (Input.MouseRightButtonPressed)
				});
		} // TestLoadColladaModel()
         */
		#endregion
		#endregion
	} // class ColladaModel
} // namespace DungeonQuest.Graphics