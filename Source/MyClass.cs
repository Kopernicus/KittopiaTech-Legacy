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
				GameObject ghost = new GameObject("Kittopiatech Main Plugin", typeof(PlanetToolsUiController));
	        	GameObject.DontDestroyOnLoad(ghost);
				
				GameObject AsteriodHandler = new GameObject("Kittopiatech Asteriod Handler", typeof(TerraformingAsteriodAddModule));
	        	GameObject.DontDestroyOnLoad(AsteriodHandler);
				
				DestroyImmediate( this );
			}
		}
	}
}

