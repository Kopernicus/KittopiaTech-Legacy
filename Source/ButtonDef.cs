/*using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using System.IO;

namespace PFUtilityAddon
{
	public class PQSSelector
	{
		static public PQS ReturnedPQS;
		
		static public void UpdatePQSList( int i )
		{
			//Hacky...
			PlanetToolsUiController.NewWindows[ "PQSSelector" ] = null; //Destroy
			PlanetToolsUiController.NewWindows[ "PQSSelector" ] = new ScrollWindow( PQSSelector.ReturnPQSNames() , PQSSelector.GetButtons() , "PQS Selector", 1661269 ); //rebuild;
			
			PlanetToolsUiController.NewWindows[ "PQSSelector" ].ToggleWindow();
		}
		
		static public void Button_Select( int i )
		{
			List<PQS> norm_PqsList = new List<PQS>();
			foreach( PQS pqs in GameObject.FindObjectsOfType(typeof( PQS )) )
			{
				norm_PqsList.Add( pqs );
			}
			
			ReturnedPQS = norm_PqsList[i];
			
			Debug.Log( "DEBUG: Selected "+ReturnedPQS.name+"\n");
			
			PlanetToolsUiController.NewWindows[ "PQSSelector" ].ToggleWindow();
			
			//PlanetToolsUiController.NewWindows[ "PQSSelector" ].ToggleWindow();
		}
		static public string[] ReturnPQSNames()
		{
			List<PQS> norm_PqsList = new List<PQS>();
			foreach( PQS pqs in GameObject.FindObjectsOfType(typeof( PQS )) )
			{
				norm_PqsList.Add( pqs );
			}
			
			List<string> PQSNames = new List<string>();
			foreach( PQS pqs in norm_PqsList )
			{
				PQSNames.Add( pqs.name );
			}
			
			PQSNames.Add( "UPDATE PQS LIST" );
			PQSNames.Add( "Blank." );
			
			return PQSNames.ToArray();
		}
		static public ButtonDelegate[] GetButtons()
		{
			//Get PQS
			int numPQS = 0;
			foreach( PQS pqs in GameObject.FindObjectsOfType(typeof( PQS )) )
			{
				numPQS++;
			}
			ButtonDelegate[] delgates = new ButtonDelegate[numPQS + 2];
			
			//Populate lists
			int i;
			for( i = 0; i < numPQS; i++ )
			{
				delgates[i] = Button_Select;
			}
			//i++;
			delgates[i] = UpdatePQSList;
			
			return delgates;
		}
	}
	
	public class TextureBrowser
	{
		public static string PlanetName;
		public static MapSO ReturnedMapSo;
		
		static public void UpdateTextureList()
		{
			//Hacky...
			PlanetToolsUiController.NewWindows[ "TextureBrowser" ] = null; //Destroy
			PlanetToolsUiController.NewWindows[ "TextureBrowser" ] = new ScrollWindow( TextureBrowser.GetTextures( PlanetToolsUiController.uiController.TemplateName ) , TextureBrowser.GetButtons() , "Texture Browser", 1661270 ); //rebuild;
			
			PlanetToolsUiController.NewWindows[ "TextureBrowser" ].ToggleWindow();
		}
		
		static public string[] GetTextures( string IPlanetName )
		{
			try{
			PlanetName = IPlanetName;
			
			List<string> FileNames = new List<string>();
			
			DirectoryInfo directory = new DirectoryInfo( "GameData/KittopiaSpace/Textures/"+ PlanetName +"/PQS/" );
			var files = directory.GetFiles("*.png");
			foreach( FileInfo f in files )
			{
				FileNames.Add( f.Name );
			}
			FileNames.Add( "Filler" );
			
			return FileNames.ToArray();
			}catch{}
			return new string[]{};
		}
		
		static public void Button_Select( int i )
		{
			List<string> FileNames = new List<string>();
			
			DirectoryInfo directory = new DirectoryInfo( "GameInfo/KittopiaSpace/Textures/"+ PlanetName +"/PQS/" );
			var files = directory.GetFiles("*.png");
			foreach( FileInfo f in files )
			{
				FileNames.Add( f.Name );
			}
			FileNames.Add( "Filler" );
			
			Texture2D texture = Utils.LoadTexture( "GameInfo/KittopiaSpace/Textures/"+ PlanetName +"/PQS/" + FileNames[i] );
			
			ReturnedMapSo = (MapSO) ScriptableObject.CreateInstance(typeof (MapSO));
			ReturnedMapSo.CreateMap( MapSO.MapDepth.RGBA, texture );
		}
		
		static public ButtonDelegate[] GetButtons()
		{
			//Get PQS
			int numFiles = 0;
			
			List<string> FileNames = new List<string>();
			
			DirectoryInfo directory = new DirectoryInfo( "GameInfo/KittopiaSpace/Textures/"+ PlanetName +"/PQS/" );
			var files = directory.GetFiles("*.png");
			foreach( FileInfo f in files )
			{
				FileNames.Add( f.Name );
				numFiles++;
			}
			
			ButtonDelegate[] delgates = new ButtonDelegate[numFiles + 2];
			
			//Populate lists
			int i;
			for( i = 0; i < numFiles; i++ )
			{
				delgates[i] = Button_Select;
			}
			//i++;
			delgates[i] = Button_Select;
			
			return delgates;
		}
	}
}*/
