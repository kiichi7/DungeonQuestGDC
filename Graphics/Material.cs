// Author: abi
// Project: DungeonQuest
// Path: C:\code\Xna\DungeonQuest\Graphics
// Creation date: 28.03.2007 01:01
// Last modified: 31.07.2007 04:40

#region Using directives
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using DungeonQuest.Helpers;
using DungeonQuest.Shaders;
using System.Globalization;
using System.Net;
using System.IO;
#endregion

namespace DungeonQuest.Graphics
{
	/// <summary>
	/// Material class for Xna materials used for Models. Consists of
	/// normal Xna material settings (ambient, diffuse, specular),
	/// the diffuse texture and optionally of normal map, height map and shader
	/// parameters.
	/// </summary>
	public class Material : IDisposable
	{
		#region Constants
		/// <summary>
		/// Default color values are:
		/// 0.15f for ambient and 1.0f for diffuse and 1.0f specular.
		/// </summary>
		public static readonly Color
			DefaultAmbientColor =
				//new Color(80, 80, 80),
				//new Color(64, 64, 64),
				//new Color(40, 40, 40),
				//new Color(0, 0, 0),
				// Use 5%
				//new Color(12, 12, 12),
				// Use 11%
				new Color(28, 28, 28),
			DefaultDiffuseColor = new Color(230, 230, 230),
			DefaultSpecularColor = new Color(255, 255, 255);

		/// <summary>
		/// Default specular power (24)
		/// </summary>
		const float DefaultSpecularPower = 4;//24.0f;

		/// <summary>
		/// Parallax amount for parallax and offset shaders.
		/// </summary>
		public const float DefaultParallaxAmount = 0.024f;//0.04f;//0.07f;
		#endregion

		#region Variables
		/// <summary>
		/// Colors
		/// </summary>
		public Color diffuseColor = DefaultDiffuseColor,
			ambientColor = DefaultAmbientColor,
			specularColor = DefaultSpecularColor;

		/// <summary>
		/// Specular power
		/// </summary>
		public float specularPower = DefaultSpecularPower;

		/// <summary>
		/// Diffuse texture for the material. Can be null for unused.
		/// </summary>
		public Texture diffuseTexture = null;
		/// <summary>
		/// Normal texture in case we use normal mapping. Can be null for unused.
		/// </summary>
		public Texture normalTexture = null;
		/// <summary>
		/// Specular texture in case we use normal mapping. Can be null for unused.
		/// </summary>
		public Texture specularTexture = null;
		#endregion

		#region Properties
		/// <summary>
		/// Checks if the diffuse texture has alpha
		/// </summary>
		public bool HasAlpha
		{
			get
			{
				if (diffuseTexture != null)
					return diffuseTexture.HasAlphaPixels;
				else
					return false;
			} // get
		} // HasAlpha
		#endregion

		#region Constructors
		#region Default Constructors
		/// <summary>
		/// Create material, just using default values.
		/// </summary>
		public Material()
		{
		} // Material()

		/// <summary>
		/// Create material, just using default color values.
		/// </summary>
		public Material(string setDiffuseTexture)
		{
			diffuseTexture = new Texture(setDiffuseTexture);
		} // Material(setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(Color setAmbientColor, Color setDiffuseColor,
			string setDiffuseTexture)
		{
			ambientColor = setAmbientColor;
			diffuseColor = setDiffuseColor;
			diffuseTexture = new Texture(setDiffuseTexture);
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(Color setAmbientColor, Color setDiffuseColor,
			Texture setDiffuseTexture)
		{
			ambientColor = setAmbientColor;
			diffuseColor = setDiffuseColor;
			diffuseTexture = setDiffuseTexture;
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(string setDiffuseTexture, string setNormalTexture)
		{
			diffuseTexture = new Texture(setDiffuseTexture);
			normalTexture = new Texture(setNormalTexture);
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(string setDiffuseTexture, string setNormalTexture,
			string setSpecularTexture)
		{
			diffuseTexture = new Texture(setDiffuseTexture);
			if (File.Exists(Path.Combine(
				Directories.ContentDirectory, setNormalTexture + ".xnb")) ||
				File.Exists(Path.Combine(
				Directories.ContentDirectory, setNormalTexture + ".dds")))
				normalTexture = new Texture(setNormalTexture);
			if (File.Exists(Path.Combine(
				Directories.ContentDirectory, setSpecularTexture + ".xnb")) ||
				File.Exists(Path.Combine(
				Directories.ContentDirectory, setSpecularTexture + ".dds")))
				specularTexture = new Texture(setSpecularTexture);
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)

		/// <summary>
		/// Create material
		/// </summary>
		public Material(Color setAmbientColor, Color setDiffuseColor,
			Color setSpecularColor, string setDiffuseTexture,
			string setNormalTexture, string setSpecularTexture,
			string setDetailTexture)
		{
			ambientColor = setAmbientColor;
			diffuseColor = setDiffuseColor;
			specularColor = setSpecularColor;
			diffuseTexture = new Texture(setDiffuseTexture);
			if (String.IsNullOrEmpty(setNormalTexture) == false)
				normalTexture = new Texture(setNormalTexture);
			if (String.IsNullOrEmpty(setSpecularTexture) == false)
				specularTexture = new Texture(setSpecularTexture);
			// Leave rest to default
		} // Material(ambientColor, diffuseColor, setDiffuseTexture)
		#endregion

		#region Helpers for creating material from shader parameters
		/*TODO
		/// <summary>
		/// Search effect parameter
		/// </summary>
		/// <param name="parameters">Parameters</param>
		/// <param name="paramName">Param name</param>
		/// <returns>Object</returns>
		private static object SearchEffectParameter(
			EffectDefault[] parameters, string paramName)
		{
			foreach (EffectDefault param in parameters)
			{
				if (StringHelper.Compare(param.ParameterName, paramName))
				{
					return param.Data;
				} // if (StringHelper.Compare)
			} // foreach (param in parameters)
			// Not found
			return null;
		} // SearchEffectParameter(parameters, paramName)

		/// <summary>
		/// Search effect float parameter
		/// </summary>
		/// <param name="parameters">Parameters</param>
		/// <param name="paramName">Param name</param>
		/// <param name="defaultValue">Default value</param>
		/// <returns>Float</returns>
		private static float SearchEffectFloatParameter(
			EffectDefault[] parameters, string paramName, float defaultValue)
		{
			object ret = SearchEffectParameter(parameters, paramName);
			if (ret != null &&
				ret.GetType() == typeof(float))
				return (float)ret;
			// Not found? Then just return default value.
			return defaultValue;
		} // SearchEffectFloatParameter(parameters, paramName, defaultValue)

		/// <summary>
		/// Search effect color parameter
		/// </summary>
		/// <param name="parameters">Parameters</param>
		/// <param name="paramName">Param name</param>
		/// <param name="defaultColor">Default color</param>
		/// <returns>Color</returns>
		private static Color SearchEffectColorParameter(
			EffectDefault[] parameters, string paramName, Color defaultColor)
		{
			object ret = SearchEffectParameter(parameters, paramName);
			if (ret != null &&
				ret.GetType() == typeof(float[]))
			{
				float[] data = (float[])ret;
				if (data.Length >= 4)
				{
					byte red = (byte)(data[0] * 255.0f);
					byte green = (byte)(data[1] * 255.0f);
					byte blue = (byte)(data[2] * 255.0f);
					byte alpha = (byte)(data[3] * 255.0f);
					return ColorHelper.FromArgb(alpha, red, green, blue);
				} // if (data.Length)
			} // if (ret)
			// Not found? Then just return default value.
			return defaultColor;
		} // SearchEffectColorParameter(parameters, paramName, defaultColor)

		/// <summary>
		/// Search effect texture parameter
		/// </summary>
		/// <param name="parameters">Parameters</param>
		/// <param name="paramName">Param name</param>
		/// <param name="defaultTexture">Default texture</param>
		/// <returns>Texture</returns>
		private static Texture SearchEffectTextureParameter(
			EffectDefault[] parameters, string paramName, Texture defaultTexture)
		{
			object ret = SearchEffectParameter(parameters, paramName);
			if (ret != null &&
				ret.GetType() == typeof(string))
			{
				// Use the models directory
				return new Texture(
					Directories.TextureModelsSubDirectory + "\\" +
					StringHelper.ExtractFilename((string)ret, true));
			} // if (ret)
			// Not found? Then just return default value.
			return defaultTexture;
		} // SearchEffectTextureParameter(parameters, paramName, defaultTexture)
		 */
		#endregion

		#region Constructor for creating material from EffectInstance from x file
		/*TODO
		/// <summary>
		/// Material
		/// </summary>
		public Material(EffectInstance modelEffectInstance,
			ExtendedMaterial dxMaterial)
		{
			EffectDefault[] parameters = modelEffectInstance.GetDefaults();

			// If shader could not be loaded or is missing, we can't set
			// any shader parameters, load material normally without shaders.
			if (ShaderEffect.normalMapping.Valid == false)
			{
				// Load material like a normal extended material.
				LoadExtendedMaterial(dxMaterial);

				// Leave rest to default, only load diffuseTexture from shader
				// if none is set in the extended material.
				if (diffuseTexture == null)
					diffuseTexture = SearchEffectTextureParameter(
						parameters, "diffuseTexture", null);

				// Get outta here, all the advanced shader stuff is not required.
				return;
			} // if (ShaderEffect.normalMapping.Valid)

			d3dMaterial.Ambient = SearchEffectColorParameter(
				parameters, "ambientColor", DefaultAmbientColor);
			d3dMaterial.Diffuse = SearchEffectColorParameter(
				parameters, "diffuseColor", DefaultDiffuseColor);
			d3dMaterial.Specular = SearchEffectColorParameter(
				parameters, "specularColor", DefaultSpecularColor);
			d3dMaterial.SpecularSharpness = SearchEffectFloatParameter(
				parameters, "specularPower", DefaultShininess);

			// If diffuse is white, reduce it to nearly white!
			if (d3dMaterial.Diffuse == Color.White)
				d3dMaterial.Diffuse = ColorHelper.FromArgb(255, 230, 230, 230);
			// Same for specular color
			if (d3dMaterial.Specular == Color.White)
				d3dMaterial.Specular = ColorHelper.FromArgb(255, 230, 230, 230);

			diffuseTexture = SearchEffectTextureParameter(
				parameters, "diffuseTexture", null);
			normalTexture = SearchEffectTextureParameter(
				parameters, "normalTexture", null);
			heightTexture = SearchEffectTextureParameter(
				parameters, "heightTexture", null);

			parallaxAmount = SearchEffectFloatParameter(
				parameters, "parallaxAmount", DefaultParallaxAmount);
		} // Material(modelEffectInstance, dxMaterial)
		 */
		#endregion

		#region Create material from effect settings
		/// <summary>
		/// Create material
		/// </summary>
		/// <param name="effect">Effect</param>
		public Material(Effect effect)
		{
			EffectParameter diffuseTextureParameter =
				effect.Parameters["diffuseTexture"];
			if (diffuseTextureParameter != null)
				diffuseTexture = new Texture(
					diffuseTextureParameter.GetValueTexture2D());
			
			EffectParameter normalTextureParameter =
				effect.Parameters["normalTexture"];
			if (normalTextureParameter != null)
				normalTexture = new Texture(
					normalTextureParameter.GetValueTexture2D());

			EffectParameter specularTextureParameter =
				effect.Parameters["heightTexture"];
			if (specularTextureParameter != null)
				specularTexture = new Texture(
					specularTextureParameter.GetValueTexture2D());

			EffectParameter diffuseColorParameter =
				effect.Parameters["diffuseColor"];
			if (diffuseColorParameter != null)
				diffuseColor = new Color(diffuseColorParameter.GetValueVector4());
			
			EffectParameter ambientColorParameter =
				effect.Parameters["ambientColor"];
			if (ambientColorParameter != null)
				ambientColor = new Color(ambientColorParameter.GetValueVector4());
			// Make sure ambientColor is not darker than DefaultAmbientColor
			if (ambientColor.R < DefaultAmbientColor.R)
				ambientColor = DefaultAmbientColor;
			
			EffectParameter specularColorParameter =
				effect.Parameters["specularColor"];
			if (specularColorParameter != null)
				specularColor = new Color(specularColorParameter.GetValueVector4());

			EffectParameter specularPowerParameter =
				effect.Parameters["specularPower"];
			if (specularPowerParameter != null)
				specularPower = specularPowerParameter.GetValueSingle();
		} // Material(effect)
		#endregion

		#region Create material from xml node
		/// <summary>
		/// Shader effect used here, only used by ColladaModel right now!
		/// </summary>
		public ShaderEffect shader = null;

		/// <summary>
		/// Name of this material, only used for ColladaModel.
		/// </summary>
		string name = "";

		/// <summary>
		/// Load a material from inside of a material node for loading collada
		/// models.
		/// </summary>
		/// <param name="materialNode">Material node</param>
		/// <param name="textures">Textures</param>
		/// <param name="effectsIds">Effects ids</param>
		internal Material(XmlNode materialNode,
			Dictionary<string, Texture> textures,
			Dictionary<string, string> effectIds)
		{
			name = XmlHelper.GetXmlAttribute(materialNode, "id");

			// Get all instance effects
			XmlNode instEffect =
				XmlHelper.GetChildNode(materialNode, "instance_effect");
			foreach (XmlNode setparam in instEffect)
			{
				float[] vec;
				switch (XmlHelper.GetXmlAttribute(setparam, "ref"))
				{
					case "ambientColor":
						vec = StringHelper.ConvertStringToFloatArray(
							XmlHelper.GetChildNode(setparam, "float4").InnerText);
						ambientColor = ColorHelper.ColorFromFloatArray(vec);
						// Make sure ambientColor is at least DefaultAmbientColor.
						// Modelers often save way to dark ambient colors.
						if (ambientColor.R < DefaultAmbientColor.R)
							ambientColor = DefaultAmbientColor;
						break;
					case "diffuseColor":
						vec = StringHelper.ConvertStringToFloatArray(
							XmlHelper.GetChildNode(setparam, "float4").InnerText);
						diffuseColor = ColorHelper.ColorFromFloatArray(vec);
						break;
					case "specularColor":
						vec = StringHelper.ConvertStringToFloatArray(
							XmlHelper.GetChildNode(setparam, "float4").InnerText);
						specularColor = ColorHelper.ColorFromFloatArray(vec);
						break;
					case "specularPower":
						specularPower = Convert.ToSingle(
							XmlHelper.GetChildNode(setparam, "float").InnerText,
							NumberFormatInfo.InvariantInfo);
						break;
					case "diffuseTexture":
						XmlNode initFromNode =
							XmlHelper.GetChildNode(setparam, "init_from");
						if (initFromNode != null)
							diffuseTexture = textures[initFromNode.InnerText];
						break;
					case "normalTexture":
						initFromNode = XmlHelper.GetChildNode(setparam, "init_from");
						if (initFromNode != null)
							normalTexture = textures[initFromNode.InnerText];
						break;
					default:
						//silent ignore
						break;
				} // switch
			} // foreach (setparam)

			string effectnodename =
				XmlHelper.GetXmlAttribute(instEffect, "url").Substring(1);
			if (effectnodename.Length > 0 &&
				effectIds.ContainsKey(effectnodename))
			{
				string shaderName =
					StringHelper.ExtractFilename(effectIds[effectnodename], true);
				if (String.IsNullOrEmpty(shaderName) == false)
				{
					shader = new ShaderEffect(shaderName);
				} // if (String.IsNullOrEmpty)
			} // if (effectnodename.Length)
		} // Material(materialNode, textures, effectsIds)
		#endregion
		#endregion

		#region Dispose
		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			if (diffuseTexture != null)
				diffuseTexture.Dispose();
			if (normalTexture != null)
				normalTexture.Dispose();
			if (specularTexture != null)
				specularTexture.Dispose();
		} // Dispose()
		#endregion
	} // class Material
} // namespace DungeonQuest.Graphics
