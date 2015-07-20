
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

