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
            
            var texture = new Texture2D((int)wid, (int)hei, TextureFormat.ARGB32, true);
            texture.LoadImage(textureData);


            return texture;
        }
		
		public static void RecalculateTangents(Mesh theMesh)
        {


            int vertexCount = theMesh.vertexCount;
            Vector3[] vertices = theMesh.vertices;
            Vector3[] normals = theMesh.normals;
            Vector2[] texcoords = theMesh.uv;
            int[] triangles = theMesh.triangles;
            int triangleCount = triangles.Length/3;


            var tangents = new Vector4[vertexCount];
            var tan1 = new Vector3[vertexCount];
            var tan2 = new Vector3[vertexCount];


            int tri = 0;


            for (int i = 0; i < (triangleCount); i++)
            {


                int i1 = triangles[tri];
                int i2 = triangles[tri + 1];
                int i3 = triangles[tri + 2];


                Vector3 v1 = vertices[i1];
                Vector3 v2 = vertices[i2];
                Vector3 v3 = vertices[i3];


                Vector2 w1 = texcoords[i1];
                Vector2 w2 = texcoords[i2];
                Vector2 w3 = texcoords[i3];


                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;


                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;


                float r = 1.0f/(s1*t2 - s2*t1);
                var sdir = new Vector3((t2*x1 - t1*x2)*r, (t2*y1 - t1*y2)*r, (t2*z1 - t1*z2)*r);
                var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);


                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;


                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;


                tri += 3;
            }
            for (int i = 0; i < (vertexCount); i++)
            {
                Vector3 n = normals[i];
                Vector3 t = tan1[i];


                // Gram-Schmidt orthogonalize
                Vector3.OrthoNormalize(ref n, ref t);


                tangents[i].x = t.x;
                tangents[i].y = t.y;
                tangents[i].z = t.z;


                // Calculate handedness
                tangents[i].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0f) ? -1.0f : 1.0f;
            }
            theMesh.tangents = tangents;
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
		
		public static void ExportPlanetMaps( string TemplateName, Texture2D[] texture )
		{
			byte[] ExportColourMap = texture[0].EncodeToPNG();
			if( File.Exists( "GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/colourMap.png" ) )
			{
    			File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/colourMap.png",  ExportColourMap);
			}
			else
			{
				File.Create( "GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/colourMap.png" );
				File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/colourMap.png",  ExportColourMap);
			}
			
			ExportColourMap = texture[1].EncodeToPNG();
			
			if( File.Exists( "GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/bumpMap.png" ) )
			{
				File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/bumpMap.png",  ExportColourMap);
			}
			else
			{
				File.Create( "GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/bumpMap.png" );
				File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/bumpMap.png",  ExportColourMap);
			}
		}
		
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
		
	}
}

