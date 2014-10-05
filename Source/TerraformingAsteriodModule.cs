using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;

namespace PFUtilityAddon
{
	public class TerraformingAsteriodAddModule : MonoBehaviour
	{
		public TerraformingAsteriodAddModule()
		{
		}
		
		public void Start()
		{
			//GameEvents.onVesselCreate.Add( AsteriodModifier );
		}
		
		public void AsteriodModifier( Vessel asteriod )
		{
			if( asteriod.vesselType == VesselType.SpaceObject && asteriod.loaded == false )
			{
				try
				{
					TerraformingAsteriodImpactor impactor = asteriod.gameObject.AddComponent<TerraformingAsteriodImpactor>();
					//Comets are rarer than asteriods:
					if( UnityEngine.Random.Range( 1, 2 ) == 1 ) //1 in 40 asteriods
					{
						impactor.Init( asteriod, 1 );
						asteriod.vesselName = asteriod.vesselName.Replace( "Ast." , "Comet" );
					}
					else
					{
						impactor.Init( asteriod, 0 );
					}
				}
				catch
				{
					asteriod.Die();
				}
			}
		}
	}
	
	public class TerraformingAsteriodImpactor : MonoBehaviour
	{
		ParticleEmitter MainEmitter;
		ParticleAnimator ParticleAnim;
		ParticleRenderer ParticleRender;
		
		public void Init( Vessel v, int type )
		{
			attachedTo = v;
			AsteriodType = type;
			
			if( AsteriodType == 1 )//Comet
			{
				try
				{
					MainEmitter = (ParticleEmitter)v.rootPart.gameObject.AddComponent("MeshParticleEmitter");
					ParticleRender = v.rootPart.gameObject.AddComponent<ParticleRenderer>();
					ParticleAnim = v.rootPart.gameObject.AddComponent<ParticleAnimator>();
					
					ParticleRender.material.mainTexture = Utils.LoadTexture( "GameData/KittopiaSpace/Textures/Default/Glow.png" );
					
					MainEmitter.maxSize = 20;
					MainEmitter.minSize = 10;
					MainEmitter.maxEnergy = 5;
					MainEmitter.minEnergy = 2.5f;
					
					MainEmitter.useWorldSpace = false;
					
					MainEmitter.rndVelocity = new Vector3( 50, 50, 50 );
					MainEmitter.emit = true;
				}
				catch{}
			}
			
			CanImpact = false;
		}
		
		int AsteriodType;
		public Vessel attachedTo;
		
		PQS mainPQS;
		bool CanImpact;
		Vector3 curPos;
		
		public void Update()
		{
			mainPQS = attachedTo.mainBody.gameObject.GetComponent<PQS>();
			curPos = attachedTo.transform.position;
			
			if( attachedTo.heightFromSurface < 100 && attachedTo.verticalSpeed > 100 ) //Allow planet impact if within range...
			{
				CanImpact = true;
				attachedTo.rootPart.explode();
			}
			
			if( AsteriodType == 1 )
			{
				//Comet stuff
				
			}
		}
		
		public void OnDestroy()
		{
			if( CanImpact ) //Deform terrain
			{
				mainPQS = attachedTo.mainBody.gameObject.GetComponent<PQS>(); //Just in case...
				
				print( "PlanetUI: Created a nice explosion..." );
				
				
				PQSMod_MapDecal crater = mainPQS.gameObject.AddComponent<PQSMod_MapDecal>();
				crater.position = curPos;
				crater.removeScatter = true;
				crater.radius = UnityEngine.Random.Range( 3000, 6000 );
				crater.heightMapDeformity = -UnityEngine.Random.Range( 3000, 6000 );
				crater.heightMap = new MapSO();
				crater.heightMap.CreateMap( MapSO.MapDepth.Greyscale, Utils.LoadTexture( "GameData/KittopiaSpace/Textures/Default/Crater.png" ) );
				crater.sphere = mainPQS;
				
				crater.name = "AsteriodCrater";
				crater.RebuildSphere();
			}
		}
	}
}

