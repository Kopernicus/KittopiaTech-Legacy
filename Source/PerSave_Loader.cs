using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PFUtilityAddon
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)] 
	public class PerSave_Loader : MonoBehaviour
	{
		string OldSaveGame;
		public PerSave_Loader()
		{
			//Stuff will happen
		}
		public void Awake()
        {
			string SavedGame = HighLogic.SaveFolder;
			if( OldSaveGame == SavedGame )
			{
				print( "PLANETUI: Dont do this more than once. \n" );
				return;
			}
			print( "PLANETUI: Im up already "+SavedGame+"!\n" );
			
			OldSaveGame = SavedGame;
			
			//PlanetToolsUiController.uiController.LoadPerSavePlanets( SavedGame );
			
			
		}
	}
}

