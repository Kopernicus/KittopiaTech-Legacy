using System;
using UnityEngine;

namespace PFUtilityAddon
{
	public class AtmosphericReproccesser : PartModule
	{
		[KSPField(guiActive = true, guiName = "Progress")]
		public float Progress;
		
		bool isActive;
		
		public Color targetAtmoColour;
		
		[KSPField(isPersistant = true, guiName = "Red Gases", guiActiveEditor = true, guiActive = true)]
		[UI_FloatRange(minValue = 0f, maxValue = 255f, stepIncrement = 1f)]
		public float AtmoRedColour;
		
		[KSPField(isPersistant = true, guiName = "Green Gases", guiActiveEditor = true, guiActive = true)]
		[UI_FloatRange(minValue = 0f, maxValue = 255f, stepIncrement = 1f)]
		public float AtmoGreenColour;
		
		[KSPField(isPersistant = true, guiName = "Blue Gases", guiActiveEditor = true, guiActive = true)]
		[UI_FloatRange(minValue = 0f, maxValue = 255f, stepIncrement = 1f)]
		public float AtmoBlueColour;
		
		
		[KSPField(isPersistant = true, guiName = "Desired Pressure", guiActiveEditor = true, guiActive = true)]
		[UI_FloatRange(minValue = 0f, maxValue = 10f, stepIncrement = 0.1f)]
		public float targetAtmoPressure;
		
		
		public float targetAtmoHeight;
		AtmosphereFromGround targetAtmo;
		
		[KSPAction( "Begin Proccess" )]
		void StartAtmoReproccessing(KSPActionParam param)
		{
			Progress = 0;
			CheckAtmosphere();
			isActive = true;
		}
		
		[KSPEvent(guiActive = true, guiName = "Begin Proccess" )]
		void StartAtmoReproccessing()
		{
			Progress = 0;
			CheckAtmosphere();
			isActive = true;
		}
		
		public override void OnUpdate()
		{
			if( isActive && (vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.PRELAUNCH || vessel.situation == Vessel.Situations.SPLASHED) )
			{
				if( part.RequestResource( "AtmosphericGas", 1 ) < 1 )
				{
					
				}
				else
				{
					Progress += 0.0001f;
					float r, g, b;
					r = Mathf.Lerp( targetAtmo.waveLength.r, Math.Abs( targetAtmoColour.r - 1.0f ), Progress/100 );
					b = Mathf.Lerp( targetAtmo.waveLength.b, Math.Abs( targetAtmoColour.b - 1.0f ), Progress/100 );
					g = Mathf.Lerp( targetAtmo.waveLength.g, Math.Abs( targetAtmoColour.g - 1.0f ), Progress/100 );
					targetAtmo.waveLength = new Color( r, g, b );
					
					vessel.mainBody.staticPressureASL = Mathf.Lerp( (float)vessel.mainBody.staticPressureASL, targetAtmoPressure , Progress/100 );
					
					if( vessel.mainBody.staticPressureASL == targetAtmoPressure )
					{
						isActive = false;
					}
				}
			}
		}
		
		void CheckAtmosphere()
		{
			targetAtmoColour = new Color32( (byte)AtmoRedColour, (byte)AtmoGreenColour, (byte)AtmoBlueColour, 255);
			AtmosphereFromGround atmo = Utils.FindScaled( vessel.mainBody.name ).GetComponentInChildren<AtmosphereFromGround>();
			if( atmo == null )
			{
				PlanetUtils.AddAtmoFX( vessel.mainBody.name, 1, new Color( 1, 1, 1, 0 ), 0 );
				atmo = Utils.FindScaled( vessel.mainBody.name ).GetComponentInChildren<AtmosphereFromGround>();
			}
			
			targetAtmo = atmo;
		}
		
		
	}
}

