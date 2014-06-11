
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;

namespace PFUtilityAddon
{
	//Helper class!
	public class CustomStar : Sun
	{
		public bool Toggle;
		
		public CustomStar()
		{
			useLocalSpaceSunLight = true;
		}

		public void Awake()
		{
			//Override to stop Bad Things(tm) from happening!
		}
	}
	
	public class PlanetUtils
	{
		public static void AddAtmoFX( string Planetname, float inputnum, Color waveColour, float radiusAddition )
		{
			GameObject Kerbin = Utils.FindScaled( "Kerbin" );
			GameObject PlanetAtmoAdd = Utils.FindScaled( Planetname );
			
			GameObject Atmo = Kerbin.GetComponentInChildren<AtmosphereFromGround>().gameObject;
			GameObject NewAtmo = (GameObject)GameObject.Instantiate( Atmo );
			NewAtmo.transform.position = PlanetAtmoAdd.transform.position;
			NewAtmo.transform.parent = PlanetAtmoAdd.transform;
			NewAtmo.name = "Atmosphere";
			
			var body = Utils.FindCB( Planetname );
			
            var afg = NewAtmo.GetComponent<AtmosphereFromGround>();
			
			afg.planet = body;
			
			float newRadius = (float)body.Radius + radiusAddition;
			
            afg.waveLength = waveColour;
			
			afg.outerRadius = newRadius * 1.025f * ScaledSpace.InverseScaleFactor;
			
			//afg.outerRadius *= radiusAddition;
			
            afg.innerRadius = afg.outerRadius * 0.975f;
            
            afg.outerRadius2 = afg.outerRadius * afg.outerRadius;
            afg.innerRadius2 = afg.innerRadius * afg.innerRadius;
            afg.scale = 1f / (afg.outerRadius - afg.innerRadius);
            afg.scaleDepth = -0.25f;
            afg.scaleOverScaleDepth = afg.scale / afg.scaleDepth;
			afg.DEBUG_alwaysUpdateAll = true;
			
			//afg.transform.localScale = Vector3.one * ((float)(body.Radius + body.maxAtmosphereAltitude) / (float)body.Radius);
			
			afg.transform.localScale *= inputnum;
		}
		public static void RecalculateAtmo( string Planetname, float inputnum )
		{
			var body = Utils.FindCB( Planetname );
            var afg = Utils.FindScaled( Planetname ).GetComponentInChildren<AtmosphereFromGround>();
			
			float newRadius = (float)body.Radius + inputnum;
			
			afg.outerRadius = newRadius * 1.025f * ScaledSpace.InverseScaleFactor;
			
			//afg.outerRadius *= radiusAddition;
			
            afg.innerRadius = afg.outerRadius * 0.975f;
            
            afg.outerRadius2 = afg.outerRadius * afg.outerRadius;
            afg.innerRadius2 = afg.innerRadius * afg.innerRadius;
            afg.scale = 1f / (afg.outerRadius - afg.innerRadius);
            afg.scaleDepth = -0.25f;
            afg.scaleOverScaleDepth = afg.scale / afg.scaleDepth;
			
		}
		//Scale atmo FX (Does not work correctly)
		public static void AtmoScaler( string Planetname, float inputnum )
		{
			var smallPlanet = Utils.FindScaled( Planetname );
			
            var atmo = smallPlanet.transform.FindChild("Atmosphere");
            var afg = atmo.GetComponent<AtmosphereFromGround>();
			
			afg.transform.localScale = Vector3.one * inputnum;
		}
		//HackHack
		public static PQSMod AddPQSMod( PQS mainSphere, Type ofType )
		{
			var newgob = new GameObject();
            var newComponent = (PQSMod)newgob.AddComponent(ofType);
			newgob.name = (""+ofType);
			newgob.transform.parent = mainSphere.gameObject.transform;
            newComponent.sphere = mainSphere;
			
			return newComponent;
		}
		
		//Reslah fix ported for multiuse
		public static void FixStar( string starName )
		{
			//Get defualt stuff
			GameObject Scenery = GameObject.Find("Scenery");
			Sun DefualtStar = Scenery.GetComponentInChildren<Sun>();
			
			//Create a new instance of "scenery star"
			GameObject newSceneryStar = new GameObject( starName + "_scenery" );
			newSceneryStar.transform.parent = Scenery.transform;
			
			//Create a new lense flare, and dump the existing suns data to it.
			LensFlare NewLenseFlare = newSceneryStar.AddComponent<LensFlare>();
			NewLenseFlare.color = GameObject.Find("SunLight").GetComponentInChildren<LensFlare>().color;
			NewLenseFlare.flare = GameObject.Find("SunLight").GetComponentInChildren<LensFlare>().flare;
			
			//Add the "light"
			Light newLight = newSceneryStar.AddComponent<Light>();
			
			//Create a new instance of "CustomStar"
			CustomStar newStar = newSceneryStar.AddComponent<CustomStar>();
			newStar.name = starName + "Sun";
			newStar.target = DefualtStar.target;
			newStar.brightnessCurve = DefualtStar.brightnessCurve;
			newStar.AU = DefualtStar.AU;
			newStar.sun = Utils.FindCB( starName );
			newStar.sunFlare = NewLenseFlare;
			newStar.localTime = DefualtStar.localTime;
			newStar.fadeStart = DefualtStar.fadeStart;
			newStar.fadeEnd = DefualtStar.fadeEnd;
			
			newLight.type = DefualtStar.light.type;
			newLight.intensity = DefualtStar.light.intensity;
			newLight.color = DefualtStar.light.color;
			
			newLight.transform.position = Utils.FindCB( starName ).transform.position;
			newLight.transform.parent = Utils.FindCB( starName ).transform.parent;
			
			newSceneryStar.transform.position = Utils.FindScaled( starName ).transform.position;
			newSceneryStar.transform.parent = Utils.FindScaled( starName ).transform;
			newSceneryStar.layer = Utils.FindScaled( starName ).layer;
			
			GameObject DetectorGOB;
			
			if( GameObject.FindObjectOfType( typeof( Detector ) ) == null ) //spawn a new detector
			{
				DetectorGOB = new GameObject( "Detector", typeof( Detector ) );
				GameObject.DontDestroyOnLoad( DetectorGOB );
			}
			else
			{
				DetectorGOB = (GameObject)GameObject.FindObjectOfType( typeof( Detector ) );
			}
			
			Detector StarFixer = (Detector)DetectorGOB.GetComponent( typeof(Detector) );
			StarFixer.AddStar( starName, newStar );
		}
		//Rings...
		public static GameObject AddRingToPlanet( GameObject ScaledPlanet, double IRadius, double ORadius, float angles, Texture2D Tex, Color rendercolour)
		{
			Vector3 StartVec = new Vector3( 1, 0, 0 );
			int RingSteps = 128;
			var vertices = new List<Vector3>();
			var Uvs = new List<Vector2>();
			var Tris = new List<int>();
			var Normals = new List<Vector3>();
			
			for( float i = 0.0f; i < 360.0f; i += (360.0f/RingSteps) )
			{
				var eVert = Quaternion.Euler( 0, i, 0 ) * StartVec;
			
				//Inner Radius
				vertices.Add( eVert*(float)IRadius);
				Normals.Add( -Vector3.right );
				Uvs.Add( new Vector2( 0, 0 ) );
			
				//Outer Radius
				vertices.Add( eVert * (float)ORadius );
				Normals.Add( -Vector3.right );
				Uvs.Add( new Vector2( 1 , 1 ) );
			}
			for( float i = 0.0f; i < 360.0f; i += (360.0f/RingSteps) )
			{
				var eVert = Quaternion.Euler( 0, i, 0 ) * StartVec;
			
				//Inner Radius
				vertices.Add( eVert*(float)IRadius);
				Normals.Add( -Vector3.right );
				Uvs.Add( new Vector2( 0, 0 ) );
			
				//Outer Radius
				vertices.Add( eVert * (float)ORadius );
				Normals.Add( -Vector3.right );
				Uvs.Add( new Vector2( 1 , 1 ) );
			}
			
			//Tri Wrapping
			int Wrapping = (RingSteps * 2);
			for( int i = 0; i < (RingSteps * 2); i+=2 )
			{
				Tris.Add( ( i ) % Wrapping );
				Tris.Add( ( i + 1 ) % Wrapping );
				Tris.Add( ( i + 2 ) % Wrapping );
				
				Tris.Add( ( i + 1 ) % Wrapping );
				Tris.Add( ( i + 3 ) % Wrapping );
				Tris.Add( ( i + 2 ) % Wrapping );
			}
			
			for( int i = 0; i < (RingSteps * 2); i+=2 )
			{
				Tris.Add( Wrapping + ( i + 2 ) % Wrapping );
				Tris.Add( Wrapping + ( i + 1 ) % Wrapping );
				Tris.Add( Wrapping + ( i ) % Wrapping );
				
				Tris.Add( Wrapping + ( i + 2 ) % Wrapping );
				Tris.Add( Wrapping + ( i + 3 ) % Wrapping );
				Tris.Add( Wrapping + ( i + 1 ) % Wrapping );
			}
			
			//Create GameObject
			GameObject RingObject = new GameObject( "PlanetaryRingObject" );
			RingObject.transform.parent = ScaledPlanet.transform;
			RingObject.transform.position = ScaledPlanet.transform.localPosition;
			RingObject.transform.localRotation = Quaternion.Euler(angles, 0, 0);
			
			RingObject.transform.localScale = ScaledPlanet.transform.localScale;
			RingObject.layer = ScaledPlanet.layer;
			
			//Create MeshFilter
			MeshFilter RingMesh = (MeshFilter)RingObject.AddComponent<MeshFilter>(  );
			
			//Set mesh
			RingMesh.mesh = new Mesh();
			RingMesh.mesh.vertices = vertices.ToArray();
			RingMesh.mesh.triangles = Tris.ToArray();
			RingMesh.mesh.uv = Uvs.ToArray();
			RingMesh.mesh.RecalculateNormals();
			RingMesh.mesh.RecalculateBounds();
			RingMesh.mesh.Optimize();
			RingMesh.sharedMesh = RingMesh.mesh;
			
			//Set texture
			MeshRenderer PlanetRenderer = (MeshRenderer)ScaledPlanet.GetComponentInChildren<MeshRenderer>();
			MeshRenderer RingRender = (MeshRenderer)RingObject.AddComponent<MeshRenderer>();
			RingRender.material = PlanetRenderer.material;
			RingRender.material.shader = Shader.Find("Transparent/Diffuse");
			RingRender.material.mainTexture = Tex;
			RingRender.material.color = rendercolour;
			
			//MaterialSetDirection:
			MaterialSetDirection MSD = ScaledPlanet.GetComponentInChildren<MaterialSetDirection>();
			MaterialSetDirection RingMsd = (MaterialSetDirection)RingObject.AddComponent<MaterialSetDirection>();
			//RingMsd.gameObject.transform.parent = RingObject.transform;
			RingMsd.target = MSD.target;
			RingMsd.Update();
			
			RingObject.AddComponent<AngleLocker>();
			
			return RingObject;
		}
	}
}

