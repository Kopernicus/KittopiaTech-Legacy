//This code is copyright Kragrathea with all rights reserved. Used with permission
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using System.IO;


namespace PFUtilityAddon
{
	public class Utils
	{
		private static GameObject _localSpace;


        public static GameObject LocalSpace
        {
            get
            {
                if (_localSpace == null)
                    _localSpace = GameObject.Find("localSpace");
                return (_localSpace);
            }
        }


        public static GameObject FindLocal(string name)
        {
            try
            {
                return (LocalSpace.transform.FindChild(name).gameObject);
            }
            catch
            {
            }
            return null;
        }


        public static GameObject FindScaled(string name)
        {
            try
            {
                return (ScaledSpace.Instance.transform.FindChild(name).gameObject);
            }
            catch
            {
            }


            return null;
        }


        public static CelestialBody FindCB(string name)
        {
            try
            {
                return (FindLocal(name).GetComponent<CelestialBody>());
            }
            catch
            {
            }


            return null;
        }
		
		public static PQS FindPQS( string name )
        {
			foreach( PSystemBody body in PlanetToolsUiController.Templates )
			{
				foreach( PQS pqs in Utils.FindLocal(body.celestialBody.name).GetComponentsInChildren(typeof( PQS )) )
				{
					if( pqs.gameObject.name == name )
					{
						return pqs;
					}
				}
			}
			
			return null;
		}
		
        private static Texture2D _defaultTexture=null;
        public static Texture2D defaultTexture { get{
            if(_defaultTexture==null)
                _defaultTexture=LoadTexture("PFUtilityAddon.Resources.ringTex.png",true);
            return _defaultTexture;
        } }


        public static Texture2D LoadTexture(string name,bool embedded=false)
        {
            byte[] textureData = null;
            if (!embedded)
            {
                if (!File.Exists(name))
                    return defaultTexture;
                textureData = File.ReadAllBytes(name);
            }
            else
            {
                System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
                Stream myStream = myAssembly.GetManifestResourceStream(name);
                var br = new BinaryReader(myStream);
                textureData = br.ReadBytes((int)myStream.Length);
            }
            var bytes = textureData.Skip(16).Take(4);
            ulong wid = bytes.Aggregate<byte, ulong>(0, (current, b) => (current * 0x100) + b);


            bytes = textureData.Skip(16 + 4).Take(4);
            ulong hei = bytes.Aggregate<byte, ulong>(0, (current, b) => (current * 0x100) + b);


            //Console.WriteLine("Loading Texture:"+name+"("+wid+"x"+hei+")");
            
            //var texture = new Texture2D((int)wid, (int)hei, TextureFormat.ARGB32, true);
			var texture = new Texture2D(4, 4, TextureFormat.ARGB32, true);
            texture.LoadImage(textureData);


            return texture;
        }

		// This function was taken straight from RSS. Credit goes to NathanKell.
		public static bool LoadTexture(string path, ref Texture2D map, bool compress, bool upload, bool unreadable)
		{
			map = null;
			//MonoBehaviour.print("LoadTextureRSS: Searching for texture " + path);
			// first try in GDB
			Texture2D[] textures = Resources.FindObjectsOfTypeAll(typeof(Texture2D)) as Texture2D[];
			foreach (Texture2D tex in textures)
			{
				if (tex.name.Equals(path))
				{
					map = tex;
					break;
				}
			}
			if ((object)map == null)
			{
				//MonoBehaviour.print("LoadTextureRSS: Loading local texture " + path);
				path = KSPUtil.ApplicationRootPath + path;
				if (File.Exists(path))
				{
					try
					{
						map = new Texture2D(4, 4, TextureFormat.RGB24, true);
						if (path.ToLower().Contains(".dds"))
						{
							GameDatabase.TextureInfo tInfo = DDSLoader.DatabaseLoaderTexture_DDS.LoadDDS(path, !unreadable, path.ToLower().Contains("normal"), -1, upload);
							map = tInfo.texture;
						}
						else
						{
							map.LoadImage(System.IO.File.ReadAllBytes(path));
							if (compress)
								map.Compress(true);
							if (upload)
								map.Apply(true, unreadable);
						}
						return true;
					}
					catch (Exception e)
					{
						Debug.Log("LoadTextureRSS: Failed to load " + path + " with exception " + e.Message);
					}
				}
				else
					MonoBehaviour.print("LoadTextureRSS: Texture does not exist! " + path);
			}
			return false;
		}

		public static bool FileExists( string path )
		{
			return (File.Exists( path ));
		}
		
		public static void LoadScaledPlanetAtmoShader(string Name, Texture2D Tex)
        {
            GameObject ScaledPlanet = Utils.FindScaled(Name);
            MeshRenderer SmallPlanetMeshRenderer = (MeshRenderer)ScaledPlanet.GetComponentInChildren((typeof(MeshRenderer)));
            SmallPlanetMeshRenderer.material.SetTexture("_rimColorRamp", Tex);
        }
		
		/*
		public static void ExportPlanetMaps( string TemplateName, Texture2D[] texture )
		{
			Directory.CreateDirectory("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName);

			byte[] ExportColourMap = texture[0].EncodeToPNG();
			File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/colourMap.png", ExportColourMap);

			ExportColourMap = texture[1].EncodeToPNG();
			File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/bumpMap.png", ExportColourMap);
		}
		 * */
		
		public static void CreateTextFile( string dir, string io )
		{
			if( File.Exists( dir ) )
			{
				File.WriteAllText( dir, io );
			}
			else
			{
				File.Create( dir );
				File.WriteAllText( dir, io );
			}
		}
		
		public static Color ParseColour( string input )
		{
			string tempColourString = input;
			tempColourString = tempColourString.Replace( "RGBA(" , "" );
			tempColourString = tempColourString.Replace( ")" , "" );
			return ConfigNode.ParseColor(tempColourString);
		}

		// Credit goes to Kragrathea.
		public static Texture2D BumpToNormalMap(Texture2D source, float strength)
		{
			strength = Mathf.Clamp(strength, 0.0F, 10.0F);
			var result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);
			for (int by = 0; by < result.height; by++)
			{
				for (var bx = 0; bx < result.width; bx++)
				{
					var xLeft = source.GetPixel(bx - 1, by).grayscale * strength;
					var xRight = source.GetPixel(bx + 1, by).grayscale * strength;
					var yUp = source.GetPixel(bx, by - 1).grayscale * strength;
					var yDown = source.GetPixel(bx, by + 1).grayscale * strength;
					var xDelta = ((xLeft - xRight) + 1) * 0.5f;
					var yDelta = ((yUp - yDown) + 1) * 0.5f;
					result.SetPixel(bx, by, new Color(xDelta, yDelta, 1.0f, xDelta));
				}
			}
			result.Apply();
			return result;
		}
		
	}
}

