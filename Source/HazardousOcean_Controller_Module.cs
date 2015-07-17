
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;
using Kopernicus.UI;

namespace PFUtilityAddon
{
	//Lava Planet Config Class
	public class HazardOceanPlanet
	{
		public HazardOceanPlanet( string IName, double maxDist, float HeatRate )
		{
			Name = IName;
			CBody = Utils.FindCB(IName);
			
			MaxDistToStartHeating = maxDist;
			HeatingRate = HeatRate;
		}
		
		public string Name;
		public CelestialBody CBody;
		
		public double MaxDistToStartHeating;
		public float HeatingRate;
	}
	
	//Lava Ocean Detector
	public class LavaDetector : MonoBehaviour
	{
		Dictionary< string, HazardOceanPlanet > Planets = new Dictionary< string, HazardOceanPlanet >();
		
		//Add to list.
		public void AddLavaPlanet( string Name, double maxDist, float HeatRate )
		{
			Planets.Add( Name, new HazardOceanPlanet( Name, maxDist, HeatRate ) );
		}
		
		//The main functionality
		void Update()
		{
			if( Planets.Count > 0 )
			{
				//Overheat vessels at sea level on lava planets:
				try //This is probbaly not needed...
				{
					foreach( Vessel SpaceShip in FlightGlobals.Vessels ) //Iterate over all vessels 
					{
						Vector3 CraftPosition = SpaceShip.GetTransform().position; //Grab positions.
						if( SpaceShip != null ) //If I exist...
						{
							foreach( HazardOceanPlanet Planet in Planets.Values )
							{
								double distanceToPlanet;
								distanceToPlanet = FlightGlobals.getAltitudeAtPos( CraftPosition, Planet.CBody ); //Get distance.
								
								if( distanceToPlanet <= Planet.MaxDistToStartHeating ) //If within heating distance...
								{
									foreach (Part part in SpaceShip.parts ) //...Loop through all parts...
									{
										part.temperature += Planet.HeatingRate; //...And heat them up!
										
										//if( part.temperature > 100 ) //HackHack: Seems they dont explode like this by defualt. Do it manualy
										//{
										//	//BOOM!
										//	part.explode();
										//}
									}
								}
							}
						}
					}
				}
				catch{} //...But do it anyway, just in case.
			}
		}
	}
}