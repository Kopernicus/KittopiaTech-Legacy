
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using System.Reflection;
using Kopernicus.UI;

namespace PFUtilityAddon
{
	//Helper class!
	public class CustomStar : Sun
	{
		public bool Enabled;
		public Light MYLight;
		
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
		//Atmosphere instantiation
		public static void AddAtmoFX( string Planetname, float inputnum, Color waveColour, float radiusAddition )
		{
			GameObject Jool = Utils.FindScaled( "Jool" );
			GameObject PlanetAtmoAdd = Utils.FindScaled( Planetname );
			
			GameObject Atmo = Jool.GetComponentInChildren<AtmosphereFromGround>().gameObject;
			GameObject NewAtmo = (GameObject)GameObject.Instantiate( Atmo );
			NewAtmo.transform.position = PlanetAtmoAdd.transform.position;
			NewAtmo.transform.parent = PlanetAtmoAdd.transform;
			NewAtmo.name = "Atmosphere";
			
			var body = Utils.FindCB( Planetname );
			
            var afg = NewAtmo.GetComponent<AtmosphereFromGround>();
			
			afg.planet = body;
			
            afg.waveLength = waveColour;

			afg.DEBUG_alwaysUpdateAll = true;
			afg.doScale = false;
			
			//afg.transform.localScale = Vector3.one;// * ((float)(body.Radius + body.maxAtmosphereAltitude) / (float)body.Radius);
			
			//afg.transform.localScale = Vector3.one * (float)((body.Radius + body.maxAtmosphereAltitude) / 600000);
			
			
			// Add the material light direction behavior 
			//MaterialSetDirection materialLightDirection = PlanetAtmoAdd.AddComponent<MaterialSetDirection>(); 
			//materialLightDirection.valueName            = "_localLightDirection"; 


			// Create the atmosphere shell game object 
			//GameObject scaledAtmosphere       = new GameObject( "atmosphere" ); 
			//scaledAtmosphere.transform.position = PlanetAtmoAdd.transform.position;
			//scaledAtmosphere.transform.parent = PlanetAtmoAdd.transform; 
			//scaledAtmosphere.layer            = 9; 
			
			//MeshRenderer renderer             = scaledAtmosphere.AddComponent<MeshRenderer>(); 
			//renderer.material                 = new Material( Shader.Find( "AtmosphereFromGround" ) ); 
			//renderer.material.shader = Shader.Find( "AtmosphereFromGround" );
			
			//MeshFilter meshFilter = scaledAtmosphere.AddComponent<MeshFilter>(); 
			//meshFilter.sharedMesh = Jool.GetComponent<MeshFilter>().sharedMesh;
			//AtmosphereFromGround afg = scaledAtmosphere.AddComponent<AtmosphereFromGround>();
			//afg.planet = Utils.FindCB( Planetname );
			//afg.waveLength = waveColour;
			
			//afg.
		}
		
		//Recalculate Atmosphere
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

            if (ofType.Name == "PQSMod_VoronoiCraters")
            {
                var mun = Utils.FindCB("Mun");
                var craters = mun.GetComponentsInChildren<PQSMod_VoronoiCraters>()[0];
                
                PQSMod_VoronoiCraters nc = newComponent.GetComponentsInChildren<PQSMod_VoronoiCraters>()[0];
                nc.craterColourRamp = craters.craterColourRamp;
                nc.craterCurve = craters.craterCurve;
                nc.jitterCurve = craters.jitterCurve;
            }
            newgob.name = (""+ofType);
            newgob.transform.parent = mainSphere.gameObject.transform;
            newComponent.sphere = mainSphere;
            
			return newComponent;
		}
		
		//Reslah fix ported for multiuse
		public static CustomStar FixStar( string starName )
		{
			//Get defualt stuff
			GameObject Scenery = GameObject.Find("Scenery");
			Sun DefualtStar = Scenery.GetComponentInChildren<Sun>();
			
			//Create a new instance of "scenery star"
			GameObject newSceneryStar = new GameObject( starName + "_scenery" );
			newSceneryStar.name = starName + "_scenery";
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
			newLight.enabled = false;
			
			newLight.transform.position = Utils.FindCB( starName ).transform.position;
			newLight.transform.parent = Utils.FindCB( starName ).transform.parent;
			
			newStar.MYLight = newLight;
			
			newSceneryStar.transform.position = Utils.FindScaled( starName ).transform.position;
			newSceneryStar.transform.parent = Utils.FindScaled( starName ).transform;
			newSceneryStar.layer = Utils.FindScaled( starName ).layer;
			
			//Sun Shader:
			//SunShaderController Hack_SSS = Utils.FindScaled(starName).GetComponentInChildren<SunShaderController>();
			try
			{
				RecursiveFixAtmo( Utils.FindCB( starName ), starName );
				
				GameObject DetectorGOB;
				
				if( GameObject.FindObjectOfType<StarDetector>( ) == null ) //spawn a new detector
				{
					DetectorGOB = new GameObject( "StarDetector" );
					DetectorGOB.AddComponent<StarDetector>();
					GameObject.DontDestroyOnLoad( DetectorGOB );
				}
				else
				{
					DetectorGOB = GameObject.FindObjectOfType< StarDetector >().gameObject;
				}
				
				StarDetector StarFixer = DetectorGOB.GetComponent<StarDetector>( );
				StarFixer.AddStar( starName, newStar );
				
				newStar.Enabled = false;
				newStar.SunlightEnabled( false );
				
			}catch(Exception e){ Debug.Log( "PLANETUI: Exeption thrown in StarFix: "+e+"\n" );}
			//If all else fails, at least return the star
			return newStar;
		}
		
		public static void RecursiveFixAtmo( CelestialBody Planet, string SunName )
		{
			if( Planet.name != SunName )
			{
				//Scaledspace
				//Utils.FindScaled( input.celestialBody.name ).layer = 1024;
				//Stuff
				MaterialSetDirection msd = Utils.FindScaled( Planet.name ).GetComponentInChildren<MaterialSetDirection>();
				if( msd != null)
				{
					msd.target = GameObject.Find( SunName+"Sun" ).transform;
				}
				
				var atmo = Utils.FindScaled( Planet.name ).transform.FindChild( "Atmosphere" );
				if( atmo != null )
				{
           			AtmosphereFromGround afg = atmo.GetComponent<AtmosphereFromGround>();
					afg.sunLight = GameObject.Find( SunName+"Sun" );
				}
				
				var atmoL = Utils.FindLocal(Planet.name).transform.FindChild( "Atmosphere" );
				if (atmoL != null)
				{
					AtmosphereFromGround afgL = atmo.GetComponent<AtmosphereFromGround>();
					afgL.sunLight = GameObject.Find(SunName + "Sun");
				}

			}
			foreach ( CelestialBody child in Planet.orbitingBodies )
			{
				RecursiveFixAtmo( child, SunName );
			}
		}
		
		//Rings...
		public static GameObject AddRingToPlanet( GameObject ScaledPlanet, double IRadius, double ORadius, float angles, Texture2D Tex, Color rendercolour, bool lockrot = false, bool Unlit = false )
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
			if( Unlit )
			{
				RingRender.material.shader = Shader.Find("Unlit/Transparent");
			}
			else{
			RingRender.material.shader = Shader.Find("Transparent/Diffuse");
			}
			RingRender.material.mainTexture = Tex;
			RingRender.material.color = rendercolour;
			
			//MaterialSetDirection:
			//MaterialSetDirection MSD = ScaledPlanet.GetComponentInChildren<MaterialSetDirection>();
			//MaterialSetDirection RingMsd = (MaterialSetDirection)RingObject.AddComponent<MaterialSetDirection>();
			//RingMsd.gameObject.transform.parent = RingObject.transform;
			//RingMsd.target = MSD.target;
			//RingMsd.Update();
			
			RingObject.AddComponent<ReScaler>();
			
			if( lockrot )
			{
				Quaternion m_rotAngleLock = RingObject.transform.localRotation;
				AngleLocker m_ringAngleLock = (AngleLocker)RingObject.AddComponent<AngleLocker>();
				m_ringAngleLock.RotationLock = m_rotAngleLock;
			}
			GameObject.DontDestroyOnLoad(RingObject);
			return RingObject;
		}
		
		/*Hazardous Oceans.
		public static void AddHazardOceanModule( string Planet, double maxDist, float HeatRate )
		{
			GameObject DetectorGOB;
			
			if( GameObject.FindObjectOfType( typeof( LavaDetector ) ) == null ) //spawn a new detector
			{
				DetectorGOB = new GameObject( "LavaDetector", typeof( LavaDetector ) );
				GameObject.DontDestroyOnLoad( DetectorGOB );
			}
			else
			{
				DetectorGOB = (GameObject)GameObject.FindObjectOfType( typeof( LavaDetector ) );
			}
			
			LavaDetector LavaModule = (LavaDetector)DetectorGOB.GetComponent( typeof(LavaDetector) );
			LavaModule.AddLavaPlanet( Planet, maxDist, HeatRate );
		}*/
		
		//ParticleEmitterTest
		public static void AddParticleEmitter( string Planet, string Target, float speed, float ratemin, float ratemax, float lifemin, float lifemax, float sizemin, float sizemax, float growrate, Color[] ColourArray , Vector3 ParticleRandVelocity )
		{
			GameObject scaledPlanet = Utils.FindScaled( Planet );
			Particle_controller_module planetParticles = scaledPlanet.GetComponent< Particle_controller_module >();
			if( planetParticles == null )
			{
				planetParticles = scaledPlanet.AddComponent<Particle_controller_module>();
				planetParticles.Init( Planet, Target, speed, ratemin, ratemax, lifemin, lifemax, sizemin, sizemax, growrate , ColourArray, ParticleRandVelocity );
			}
			else
			{
				planetParticles.Modify( Planet, Target, speed, ratemin, ratemax, lifemin, lifemax, sizemin, sizemax, growrate , ColourArray, ParticleRandVelocity );
			}
		}
	}
}

