
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

