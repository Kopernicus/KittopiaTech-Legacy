using System;
using UnityEngine;

namespace PFUtilityAddon
{
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, true)]
	public class MyClass : MonoBehaviour
	{
		public MyClass ()
		{
			
		}
		public void Update()
		{
			if( Utils.FindCB( "Kerbin" ) != null && Utils.FindCB( "Eeloo" ) != null ) //Now is the time
			{
				GameObject ghost = new GameObject("PlanetToolsUiController", typeof(PlanetToolsUiController));
	        	GameObject.DontDestroyOnLoad(ghost);
				
				DestroyImmediate( this );
			}
		}
	}
}

