using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;

//This file is getting a bit large...
//Todo: Comment areas for easier navigation.

namespace PFUtilityAddon
{
	public class RingSaveStorageHelper
	{
		public RingSaveStorageHelper(float ITilt, double IOuterRadius, double IInnerRadius, Color IColour, GameObject IgObj )
		{
			tilt = ITilt;
			OuterRadius = IOuterRadius;
			InnerRadius = IInnerRadius;
			Colour = IColour;
			gObj = IgObj;
		}
		public float tilt;
		public double OuterRadius;
		public double InnerRadius;
		public Color Colour;
		public GameObject gObj;
	}
	public class AdditionalSettingsHandler //Storage class
	{
		public AdditionalSettingsHandler(string name)
		{
			Name = name;
			
			foreach (string s in stockPlanets )
			{
				if( Name == s )
				{
					IsStock = true;
				}
			}
		}
		public bool IsStock;
		
		public bool HasAtmoFx;
		public bool AddAtmoFX;
		public bool ModScaledAtmoShader;
		public Color AtmoInvColour;
		
		public bool HasOceanFx;
		public bool AddOceanFx;
		public string OceanTemplate;
		
		public string Name;
		
		public bool AddRing;
		public List<RingSaveStorageHelper> Rings = new List<RingSaveStorageHelper>();
		
		string[] stockPlanets = {"Sun", "Moho", "Eve", "Gilly", "Kerbin", "Mun", "Minmus", "Duna", "Ike", "Dres", "Jool", "Laythe", "Tylo", "Vall", "Bop", "Pol", "Eeloo" };
	}
	
	//PlanetUI Class
	public class PlanetToolsUiController : MonoBehaviour
	{
		bool GuiEnabled = false;
		int selector;
		
		Dictionary< string, AdditionalSettingsHandler > PlanetarySettings = new Dictionary<string, AdditionalSettingsHandler>();
		
		public static Dictionary< string, BaseGuiWindow > NewWindows = new Dictionary<string, BaseGuiWindow>();
		
		public static PlanetToolsUiController uiController;
		
		Rect windowPosMain = new Rect( 20,20,420,500);
		Rect windowPosColourEdit = new Rect( 420,20,400,200);
		Rect windowPosLandclassEdit = new Rect( 420,400,400,400);
		
		//Constructor
		public PlanetToolsUiController()
		{
			uiController = this;
			
			ListPlanetsRecursive( PSystemManager.Instance.systemPrefab.rootBody );
			
			foreach ( PSystemBody psB in Templates )
			{
				if( !PlanetarySettings.ContainsKey( psB.celestialBody.name ) )
				{
					PlanetarySettings[ psB.celestialBody.name ] = new AdditionalSettingsHandler( psB.celestialBody.name );
				}		
						
				try
				{
					//Load globals...
					LoadData( psB.celestialBody.name, "Gamedata/KittopiaSpace/SaveLoad/"+psB.celestialBody.name+".cfg" );
				}
				catch( Exception e )
				{ 
					print( "failed to load: " +psB.celestialBody.name+ " Exception:" + e + "\n" ); 
				}
				
				LoadStarData( psB.celestialBody.name );
			}
			//Spawn per-save planet loader
			//GameObject persaveloader = new GameObject( "Per Save Loader", typeof( PerSave_Loader ) );
			//DontDestroyOnLoad( persaveloader );
			
			//Construct additional windows:
			NewWindows[ "PQSSelector" ] = new ScrollWindow( PQSSelector.ReturnPQSNames() , PQSSelector.GetButtons() , "PQS Selector", 1661269 );
		}
		public void LoadPerSavePlanets( string savegamename )
		{
			foreach ( PSystemBody psB in Templates )
			{
				string save_dir;
				string curSave = savegamename;
				
				if( curSave == null )
				{
					save_dir = "Gamedata/KittopiaSpace/CustomData.cfg";
				}
				else
				{
					save_dir = "Gamedata/KittopiaSpace/PlanetUI/"+curSave+".cfg";
				}
				//Load globals...
				LoadData( psB.celestialBody.name, save_dir );
			}
		}
		//OnGUI, leave this alone unless adding new window class.
		public void OnGUI()
		{
			if( GuiEnabled )
			{
				windowPosMain = GUI.Window( 1661266, windowPosMain, WindowFunction, "Planet Editor" );
			}
			if( isshowingColourEditor )
			{
				windowPosColourEdit = GUI.Window( 1661267, windowPosColourEdit, ColourWindowFunc, "Colour Generator" );
			}
			if( showLandClassmenu )
			{
				windowPosLandclassEdit = GUI.Window( 1661268, windowPosLandclassEdit, LandClassWindowFunc, "LandClass Modder" );
			}
			
			//Render custom windows:
			foreach( BaseGuiWindow window in NewWindows.Values )
			{
				window.RenderWindow();
			}
		}
		//Toggles the UI
		public void Update()
		{
			if( Input.GetKeyDown( KeyCode.P ) && Input.GetKey( KeyCode.LeftControl ) )
			{
				GuiEnabled = !GuiEnabled;
				//print( "CreatorPlanetEditor: Toggling GUI\n" ); //Dont need this anymore?
			}
		}
		
		Vector2 ScrollPosition;
		Vector2 ScrollPosition2;
		string TemplateName = "";
		Color windowOutput;
		float rVal,gVal,bVal,aVal;
		
		Texture2D ColourPickerBlankTexture;
		
		GUIStyle ColourPreviewstyle;    
			
		//Colour Picker
		void ColourWindowFunc( int windowID )
		{
			if( ColourPreviewstyle == null )
			{
				ColourPreviewstyle = new GUIStyle();
			}
			if( ColourPickerBlankTexture == null )
			{
				ColourPickerBlankTexture = new Texture2D( 1, 1 );
				ColourPickerBlankTexture.wrapMode = TextureWrapMode.Repeat;
			}
			rVal = GUI.HorizontalSlider( new Rect( 10, 30, 190, 20 ), rVal, 0, 1 );
			rVal = StrToFloat(GUI.TextField( new Rect( 200, 30, 100, 20 ), ""+rVal));
			
			gVal = GUI.HorizontalSlider( new Rect( 10, 60, 190, 20 ), gVal, 0, 1 );
			gVal = StrToFloat(GUI.TextField( new Rect( 200, 60, 100, 20 ), ""+gVal));
			
			bVal = GUI.HorizontalSlider( new Rect( 10, 90, 190, 20 ), bVal, 0, 1 );
			bVal= StrToFloat(GUI.TextField( new Rect( 200, 90, 100, 20 ), ""+bVal));
			
			aVal = GUI.HorizontalSlider( new Rect( 10, 120, 190, 20 ), aVal, 0, 1 );
			aVal= StrToFloat(GUI.TextField( new Rect( 200, 120, 100, 20 ), ""+aVal));
			
			GUI.color = new Color( rVal, gVal, bVal, aVal );
			
			ColourPickerBlankTexture.SetPixel( 0, 0,  new Color( rVal, gVal, bVal, aVal ) );
			ColourPickerBlankTexture.Apply();
			
			ColourPreviewstyle.normal.background = ColourPickerBlankTexture;
			
			GUI.Box(  new Rect( 210, 150, 240, 100 ) , ColourPickerBlankTexture, ColourPreviewstyle );
			
			
			ColourPickerBlankTexture.SetPixel( 0, 0,  new Color( (float)Math.Abs( rVal - 1.0 ), (float)Math.Abs( gVal - 1.0 ), (float)Math.Abs( bVal - 1.0 ), 1.0f ) );
			ColourPickerBlankTexture.Apply();
			
			ColourPreviewstyle.normal.background = ColourPickerBlankTexture;
			
			GUI.Box(  new Rect( 300, 150, 240, 100 ) , ColourPickerBlankTexture, ColourPreviewstyle );
			//GUI.
			GUI.color = Color.white;
			
			if( GUI.Button( new Rect( 10, 150, 200, 50), "Save to buffer and exit" ) )
			{
				windowOutput = new Color( rVal, gVal, bVal, aVal );
				isshowingColourEditor = false;
			}
			
			GUI.DragWindow();
		}
		//LandClass Modder.
		PQSMod_VertexPlanet.LandClass[] VertexLandclassestoMod;
		PQSMod_VertexPlanet.LandClass VertexLandclasstoMod;
		PQSLandControl.LandClass[] LandclassestoMod;
		int landmodder_state = 0;
		int landmodder_mode = 0;
		PQSLandControl.LandClass landclasstoMod;
		Vector2 scrollposition3;
		void LandClassWindowFunc( int windowID )
		{
			int yoffset = 30;
			scrollposition3 = GUI.BeginScrollView( new Rect( 0, 30, 300, 250 ), scrollposition3,new Rect( 0,0,400,10000));
			if( landmodder_mode == 0 ) //PQSLandControl
			{
				if( landmodder_state == 0 )
				{
					foreach( PQSLandControl.LandClass lc_Mod in LandclassestoMod)
					{
						if(GUI.Button( new Rect( 20, yoffset, 200, 20 ), ""+lc_Mod.landClassName ))
						{
							landclasstoMod = lc_Mod;
							landmodder_state = 1;
						}
						yoffset += 30;
					}
				}
				if( landmodder_state == 1 )
				{
					foreach( FieldInfo key in landclasstoMod.GetType().GetFields() )
					{
					try{
					System.Object obj = (System.Object)landclasstoMod;
					if( key.GetValue(obj).GetType() == typeof( string ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) );
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( bool ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, GUI.Toggle( new Rect( 200 , yoffset, 200, 20 ), (bool)key.GetValue(obj), "Bool" ));
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( int ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, (int)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( float ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( double ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( Color ))
					{
						GUI.Label( new Rect( 20 , yoffset, 100, 20), ""+key.Name );
						if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
						{
							Color getColour;
							getColour = (Color)key.GetValue(obj);
							rVal = getColour.r;
							gVal = getColour.g;
							bVal = getColour.b;
							aVal = getColour.a;
								
							isshowingColourEditor = true;
						}
						if( GUI.Button( new Rect( 200 , yoffset, 50, 20), "Save" ) )
						{
							key.SetValue( obj, windowOutput );
						}
						//key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
						yoffset += 30;
					}
					}catch{}
				}
				yoffset += 30;
				}
			}
			else
			{
				if( landmodder_state == 0 )
				{
					foreach( PQSMod_VertexPlanet.LandClass lc_Mod in VertexLandclassestoMod)
					{
						if(GUI.Button( new Rect( 20, yoffset, 200, 20 ), ""+lc_Mod.name ))
						{
							VertexLandclasstoMod = lc_Mod;
							landmodder_state = 1;
						}
						yoffset += 30;
					}
				}
				if( landmodder_state == 1 )
				{
					foreach( FieldInfo key in VertexLandclasstoMod.GetType().GetFields() )
					{
					try{
					System.Object obj = (System.Object)VertexLandclasstoMod;
					if( key.GetValue(obj).GetType() == typeof( string ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) );
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( bool ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, GUI.Toggle( new Rect( 200 , yoffset, 200, 20 ), (bool)key.GetValue(obj), "Bool" ));
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( int ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, (int)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( float ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( double ))
					{
						GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
						key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
						yoffset += 30;
					}
					else if( key.GetValue(obj).GetType() == typeof( Color ))
					{
						GUI.Label( new Rect( 20 , yoffset, 100, 20), ""+key.Name );
						if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
						{
							Color getColour;
							getColour = (Color)key.GetValue(obj);
							rVal = getColour.r;
							gVal = getColour.g;
							bVal = getColour.b;
							aVal = getColour.a;
								
							isshowingColourEditor = true;
						}
						if( GUI.Button( new Rect( 200 , yoffset, 50, 20), "Save" ) )
						{
							key.SetValue( obj, windowOutput );
						}
						//key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
						yoffset += 30;
					}
					}catch{}
				}
				yoffset += 30;
				}
			}
			
			yoffset += 30;
			
			if( GUI.Button( new Rect( 20, yoffset, 200, 20 ), "Exit" ) )
			{
				showLandClassmenu = false;
				landmodder_state = 0;
			}
			GUI.EndScrollView();
			
			GUI.DragWindow();
		}
		bool isshowingColourEditor = false;
		
		//Main UI
		void WindowFunction( int windowID )
		{
			ScrollPosition = GUI.BeginScrollView( new Rect( 0, 20, 400, 240 ), ScrollPosition,new Rect( 0,0,390,500));
			
			if( GUI.Button( new Rect( 20, 30, 200, 20 ), "Atmo tools" ) )
			{
				selector = 1;
			}
			if(GUI.Button( new Rect( 20, 60, 200, 20 ), "CB Editor" ))
			{
				selector = 4;
			}
			if(GUI.Button( new Rect( 20, 90, 200, 20 ), "PQS Editor" ))
			{
				selector = 3;
				pqsModderStage = 0;
			}
			if( GUI.Button( new Rect( 20, 120, 200, 20 ), "Planet Selection" ) )
			{
				//Templates.Clear();
				//ListPlanetsRecursive( PSystemManager.Instance.systemPrefab.rootBody );
				selector = 2;
			}
			GUI.Label( new Rect( 20, 150, 200, 20 ), "Template: " + TemplateName );
			if( GUI.Button( new Rect( 20, 180, 200, 20 ), "Orbit Editor" ) )
			{
				if( TemplateName != null )
				{
					Lookupbody = Utils.FindCB( TemplateName ).orbitDriver.referenceBody.name;
				}
				selector = 6;
			}
			
			if(GUI.Button( new Rect( 20, 210, 200, 20 ), "UI Options" ))
			{
				//Todo: Options menu, export/import settings, for example.
				
			}
			
			if( GUI.Button( new Rect( 20, 240, 200, 20 ), "ScaledSpace updater" ) )
			{
				//Update scaledspace with as little lag as possible... (Nope, will lag like crazy.)
				//TODO: Save scaled textures for easier previews on terraformed planets (Game load)
				//Note: This might detract from original scope.
				Texture2D PlanetColours;
				Texture2D[] textures;
				GameObject localSpace = Utils.FindLocal( TemplateName );
				GameObject scaledSpace = Utils.FindScaled( TemplateName );
				
				PQS pqsGrabtex = localSpace.GetComponentInChildren<PQS>();
				textures = pqsGrabtex.CreateMaps( 2048, 2000, pqsGrabtex.mapOcean, pqsGrabtex.mapOceanHeight, pqsGrabtex.mapOceanColor );
				PlanetColours = textures[0];
				
				MeshRenderer planettextures = scaledSpace.GetComponentInChildren<MeshRenderer>();
				planettextures.material.SetTexture("_MainTex",PlanetColours);
				
				RegenerateModel( pqsGrabtex, scaledSpace.GetComponentInChildren<MeshFilter>() );
			}
			if( GUI.Button( new Rect( 20, 270, 200, 20 ), "Ocean Tools" ) )
			{
				OceanToolsUiSelector = 0;
				selector = 5;
			}
			if( GUI.Button( new Rect( 20, 300, 200, 20 ), "Save data" ) )
			{
				if( TemplateName != null )
				{
					SaveData();
				}
			}
			if( GUI.Button( new Rect( 20, 330, 200, 20 ), "Load data" ) )
			{
				if( TemplateName != null )
				{
					LoadData( TemplateName, "Gamedata/KittopiaSpace/SaveLoad/"+TemplateName+".cfg" );
				}
			}
			
			if( GUI.Button( new Rect( 20, 360, 200, 20 ), "Add Starfix To: "+TemplateName ) )
			{
				PlanetUtils.FixStar( TemplateName );
				SaveStarData( TemplateName );
			}
			
			if( GUI.Button( new Rect( 20, 390, 200, 20 ), "Ring tools" ) )
			{
				selector = 7;
			}
			
			//if( GUI.Button( new Rect( 20, 420, 200, 20 ), "HACK: Instantiate " + TemplateName ) )
			//{
			//	//Hack
			//	PSystemBody NewPlanet;
			//	foreach( PSystemBody body in Templates )
			//	{
			//		if( body.celestialBody.name == TemplateName )
			//		{
			//			NewPlanet = (PSystemBody)Instantiate( body );
			//			NewPlanet.name = "HackHack";
			//			NewPlanet.celestialBody.bodyName = "HackHack";
			//			NewPlanet.children.Clear();
			//			NewPlanet.enabled = false;
			//		}
			//	}
			//	
			//	Templates.Clear();
			//	ListPlanetsRecursive( PSystemManager.Instance.systemPrefab.rootBody );
			//}
			
			GUI.EndScrollView();
			
			switch( selector )
			{
			case 1: AFGEditorFunc(); break;
			case 2: TemplateSelector(); break;
			case 3: PQSModderPT1(); break;
			case 4: CBModifier(); break;
			case 5: OceanToolsUI(); break;
			case 6: OrbitEditorUI(); break;
			case 7: RingEditorFunc(); break;
			default: break;
			}
			
			GUI.DragWindow();
		}
		string RadiusAddNumber = "0";
		float RadiusAddNumberOld;
		
		private void AFGEditorFunc()
		{			
			int yoffset = 280;
			
			if( TemplateName == "" )
			{
				GUI.Label( new Rect( 20 , yoffset, 200, 20), "NO TEMPLATE AVALIABLE" );
				return;
			}
			
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 0, 260, 400, 250 ), ScrollPosition2 ,new Rect( 0,220,400,290));
			
			AtmosphereFromGround AtmoToMod = Utils.FindScaled(TemplateName).GetComponentInChildren<AtmosphereFromGround>();
			
			if( AtmoToMod == null )
			{
				if( GUI.Button( new Rect( 20 , yoffset, 200, 20), "Add atmosphere to: " + TemplateName ) )
				{
					PlanetUtils.AddAtmoFX( TemplateName, 1, new Color( 1.0f, 0.5f, 0.2f, 0.0f ), 0 );
					PlanetarySettings[ TemplateName ].AddAtmoFX = true;
				}
				return;
			}
			else
			{
				//Atmo Settings
				GUI.Label( new Rect( 20, yoffset, 200, 20 ), "Radius Addition" );
				yoffset+=30;
				RadiusAddNumber = GUI.TextField( new Rect( 20, yoffset, 200, 20 ), RadiusAddNumber );
				if( GUI.Button( new Rect( 210, yoffset, 100, 20 ),"Update") )
				{
					PlanetUtils.RecalculateAtmo( TemplateName, (float)System.Convert.ToSingle(RadiusAddNumber) );
				}
				yoffset+=30;
				GUI.Label( new Rect( 20, yoffset, 200, 20 ), "Transform Scale" );
				yoffset+=30;
				AtmoToMod.gameObject.transform.localScale = Vector3.one * StrToFloat(GUI.TextField( new Rect( 20, yoffset, 200, 20 ), ""+AtmoToMod.gameObject.transform.localScale.x ));
				yoffset+=30;
				GUI.Label( new Rect( 20, yoffset, 200, 20 ), "Atmo Scale Var" );
				yoffset+=30;
				GUI.TextField( new Rect( 20, yoffset, 200, 20 ), ""+AtmoToMod.scale );
				
				RadiusAddNumberOld = StrToFloat( RadiusAddNumber );
				
				yoffset+=30;
				
				GUI.Label( new Rect( 20 , yoffset, 100, 20), "WaveColour" );
				if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
				{
					Color getColour;
					getColour = AtmoToMod.waveLength;
					rVal = getColour.r;
					gVal = getColour.g;
					bVal = getColour.b;
					aVal = getColour.a;
						
					isshowingColourEditor = true;
				}
				if( GUI.Button( new Rect( 200 , yoffset, 50, 20), "Save" ) )
				{
					AtmoToMod.waveLength = windowOutput;
				}
				
				yoffset+=30;
				
				if( GUI.Button( new Rect( 20 , yoffset, 300, 20), "Update Scaledspace atmo shader" ) )
				{
					//Load rim texture...
					string RimTex = "Gamedata/KittopiaSpace/Textures/"+TemplateName+"_rimtex.png";
					if( !Utils.FileExists( RimTex ) )
					{
						RimTex = "Gamedata/KittopiaSpace/Textures/default/blank_rim_text.png";
						if( !Utils.FileExists( RimTex ) )
						{
							GUI.EndScrollView();
							return;
						}
						
						//Update atmo shader texture
						Utils.LoadScaledPlanetAtmoShader( TemplateName, Utils.LoadTexture( RimTex, false ) );
						PlanetarySettings[ TemplateName ].ModScaledAtmoShader = true;
					}
				}
				
				GUI.EndScrollView();
			}
		}
		
		List<PSystemBody> Templates = new List<PSystemBody>();
		public void ListPlanetsRecursive(PSystemBody body)
		{
			Templates.Add( body );
			foreach (PSystemBody current in body.children)
			{
				ListPlanetsRecursive(current);
			}
		}
		private void TemplateSelector()
		{
			int yoffset = 280;
			int trimmedScrollSize = ( Templates.Count() + 1 )*30;
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 0, 250, 400, 250 ), ScrollPosition2 ,new Rect( 0,250,400,trimmedScrollSize));
			foreach( PSystemBody body in Templates )
			{
				if( GUI.Button( new Rect( 20, yoffset, 200, 20 ), body.celestialBody.name ) )
				{
					TemplateName = body.celestialBody.name;
					if( !PlanetarySettings.ContainsKey( TemplateName ) )
					{
						PlanetarySettings[ TemplateName ] = new AdditionalSettingsHandler( TemplateName );
					}
				}
				yoffset += 30;
			}
			GUI.EndScrollView();
		}
		private float StrToFloat( string input )
		{
			return (float)System.Convert.ToSingle( input );
		}
		int pqsModderStage = 0;
		PQSMod pqsmodtoMod;
		PQS pqstoMod;
		//PQS Modder PT1
		private void PQSModderPT1()
		{
			//Todo: swap with switch?
			if( pqsModderStage == 1 )
			{
				PQSModderPT2();
				return;
			}
			if( pqsModderStage == 2 )
			{
				PQSModderPT3();
				return;
			}
			if( pqsModderStage == 3 )
			{
				PQSAdderFunc();
				return;
			}
			
			
			int yoffset = 280;
			if( TemplateName == "" )
			{
				GUI.Label( new Rect( 20 , yoffset, 200, 20), "NO TEMPLATE AVALIABLE" );
				return;
			}
			List<PQS> norm_PqsList = new List<PQS>();
			foreach( PQS pqs in Utils.FindLocal(TemplateName).GetComponentsInChildren(typeof( PQS )) )
			{
				norm_PqsList.Add( pqs );
			}
			
			List<PQSMod> PqsList = new List<PQSMod>();
			foreach( PQSMod pqs in Utils.FindLocal(TemplateName).GetComponentsInChildren(typeof( PQSMod )) )
			{
				PqsList.Add( pqs );
			}
			int trimmedScrollSize = ((PqsList.Count() + norm_PqsList.Count() )*30) + 90;
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 0, 250, 380, 250 ), ScrollPosition2 ,new Rect( 0,250,380,trimmedScrollSize));
			foreach (PQS pqs in norm_PqsList)
			{
				if( GUI.Button( new Rect( 20, yoffset, 400, 20 ), ""+pqs ) )
				{
					//TemplateName = body.celestialBody.name;
					pqstoMod = pqs;
					pqsModderStage = 2;
				}
				yoffset += 30;
			}
			foreach (PQSMod pqs in PqsList)
			{
				if( GUI.Button( new Rect( 20, yoffset, 400, 20 ), ""+pqs ) )
				{
					//TemplateName = body.celestialBody.name;
					pqsmodtoMod = pqs;
					pqsModderStage = 1;
				}
				yoffset += 30;
			}
			yoffset += 30;
			if( GUI.Button( new Rect( 20, yoffset, 400, 20 ), "Add new PQSMod" ) )
			{
				pqsModderStage = 3;
			}
			GUI.EndScrollView();
		}
		bool showLandClassmenu;
		//PQS Modder PT2
		private void PQSModderPT2()
		{
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 20, 280, 380, 250 ), ScrollPosition2 ,new Rect( 20,280,380,10000) );
			
			int yoffset = 280;
			foreach( FieldInfo key in pqsmodtoMod.GetType().GetFields() )
			{
				try{
				System.Object obj = (System.Object)pqsmodtoMod;
				if( key.GetValue(obj).GetType() == typeof( string ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) );
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( bool ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, GUI.Toggle( new Rect( 200 , yoffset, 200, 20 ), (bool)key.GetValue(obj), "Bool" ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( int ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, (int)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( float ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( double ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( Color ))
				{
					GUI.Label( new Rect( 20 , yoffset, 100, 20), ""+key.Name );
					if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
					{
						Color getColour;
						getColour = (Color)key.GetValue(obj);
						rVal = getColour.r;
						gVal = getColour.g;
						bVal = getColour.b;
						aVal = getColour.a;
							
						isshowingColourEditor = true;
					}
					if( GUI.Button( new Rect( 200 , yoffset, 50, 20), "Save" ) )
					{
						key.SetValue( obj, windowOutput );
					}
					//key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( Vector3 ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					try
					{
						string vecAsString = key.GetValue(obj).ToString();
						vecAsString.Replace( "(" , "" );
						vecAsString.Replace( ")" , "" );
							
						vecAsString = GUI.TextField( new Rect( 200 , yoffset, 200, 20), vecAsString );
						Vector3 blah;
						blah = ConfigNode.ParseVector3( vecAsString );
						key.SetValue( obj, blah );
					}
					catch{}
					yoffset += 30;
				}
				else if(key.GetValue(obj).GetType() == typeof( PQSLandControl.LandClass[] ))
				{
					if( GUI.Button(new Rect( 20, yoffset, 200, 20), "Mod Land Classes") )
					{
						LandclassestoMod = (PQSLandControl.LandClass[])key.GetValue(obj);
						landmodder_mode = 0;
						landmodder_state = 0;
						showLandClassmenu = true;
					}
					yoffset += 30;
				}
				else if(key.GetValue(obj).GetType() == typeof( PQSMod_VertexPlanet.LandClass[] ))
				{
					if( GUI.Button(new Rect( 20, yoffset, 200, 20), "Mod Land Classes") )
					{
						VertexLandclassestoMod = (PQSMod_VertexPlanet.LandClass[])key.GetValue(obj);
						landmodder_mode = 1;
						landmodder_state = 0;
						showLandClassmenu = true;
					}
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType () == typeof( PQS ) )
				{
					//PQS Variable Selector.
					GUI.Label( new Rect( 20 , yoffset, 100, 20), ""+key.Name );
					if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
					{
						NewWindows[ "PQSSelector" ].ToggleWindow();
					}
					if( GUI.Button( new Rect( 200 , yoffset, 80, 20), "Save Edit" ) )
					{
						key.SetValue( obj, PQSSelector.ReturnedPQS );
					}
						
					yoffset += 30;
				}
				}catch{}
			}
			yoffset += 30;
			
			//PQS Variable Selector.
			GUI.Label( new Rect( 20 , yoffset, 100, 20), "ParentSphere" );
			if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
			{
				NewWindows[ "PQSSelector" ].ToggleWindow();
			}
			if( GUI.Button( new Rect( 200 , yoffset, 80, 20), "Save Edit" ) )
			{
				try
				{
					pqsmodtoMod.gameObject.transform.parent = PQSSelector.ReturnedPQS.gameObject.transform;
				}
				catch{}
			}
				
			yoffset += 30;
			
			if( GUI.Button( new Rect( 20 , yoffset, 200, 20), "Rebuild" ))
			{
				pqsmodtoMod.RebuildSphere();
			}
			
			GUI.EndScrollView();
		}
		
		//PQS Modder PT3
		private void PQSModderPT3()
		{
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 20, 250, 380, 250 ), ScrollPosition2 ,new Rect( 20,250,380,10000) );
			
			int yoffset = 280;
			foreach( FieldInfo key in pqstoMod.GetType().GetFields() )
			{
				try{
				System.Object obj = (System.Object)pqstoMod;
				if( key.GetValue(obj).GetType() == typeof( string ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) );
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( bool ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, GUI.Toggle( new Rect( 200 , yoffset, 200, 20 ), (bool)key.GetValue(obj), "Bool" ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( int ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, (int)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( float ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( double ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( Color ))
				{
					GUI.Label( new Rect( 20 , yoffset, 100, 20), ""+key.Name );
					if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
					{
						Color getColour;
						getColour = (Color)key.GetValue(obj);
						rVal = getColour.r;
						gVal = getColour.g;
						bVal = getColour.b;
						aVal = getColour.a;
							
						isshowingColourEditor = true;
					}
					if( GUI.Button( new Rect( 200 , yoffset, 50, 20), "Save" ) )
					{
						key.SetValue( obj, windowOutput );
					}
					//key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( Vector3 ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					try
					{
						Vector3 blah;
						blah = ConfigNode.ParseVector3( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj).ToString()) );
						key.SetValue( obj, blah );
					}
					catch{}
					yoffset += 30;
				}
				else if(key.GetValue(obj).GetType() == typeof( PQSLandControl.LandClass[] ))
				{
					if( GUI.Button(new Rect( 20, yoffset, 200, 20), "Mod Land Classes") )
					{
						LandclassestoMod = (PQSLandControl.LandClass[])key.GetValue(obj);
						landmodder_mode = 0;
						landmodder_state = 0;
						showLandClassmenu = true;
					}
					yoffset += 30;
				}
				else if(key.GetValue(obj).GetType() == typeof( PQSMod_VertexPlanet.LandClass[] ))
				{
					if( GUI.Button(new Rect( 20, yoffset, 200, 20), "Mod Land Classes") )
					{
						VertexLandclassestoMod = (PQSMod_VertexPlanet.LandClass[])key.GetValue(obj);
						landmodder_mode = 1;
						landmodder_state = 0;
						showLandClassmenu = true;
					}
					yoffset += 30;
				}
				}catch{}
			}
			yoffset += 30;
			if( GUI.Button( new Rect( 20 , yoffset, 200, 20), "Rebuild" ))
			{
				pqstoMod.RebuildSphere();
			}
			
			GUI.EndScrollView();
		}
		
		//PQS Adder
		void PQSAdderFunc()
		{
			//Urrgg... hacky at best :/
			Type[] types = Assembly.GetAssembly(typeof(PQSMod)).GetTypes();
			
			int scrollbaroffsetter = 30;
			foreach (Type type in types)
			{
			    if(type.IsSubclassOf(typeof(PQSMod)))
			    {
					scrollbaroffsetter += 30;
				}
			}
			
			int yoffset = 280;
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 20, 250, 380, 250 ), ScrollPosition2 ,new Rect( 20,250,380,scrollbaroffsetter) );
			
			//Still hacky, Im not proud.			
			foreach (Type type in types)
			{
			    if(type.IsSubclassOf(typeof(PQSMod)))
			    {
					if( GUI.Button( new Rect( 20, yoffset, 200, 20 ), ""+type.Name ) )
					{
						//Hack^6
						PQS mainSphere = Utils.FindLocal(TemplateName).GetComponentInChildren<PQS>();
						PlanetUtils.AddPQSMod( mainSphere, type );
						
						pqsModderStage = 0;
					}
					yoffset += 30;
				}
			}
			
			GUI.EndScrollView();
		}
		
		//Celestail Body Variable Editor
		private void CBModifier()
		{
			int yoffset = 280;
			
			if( TemplateName == "" )
			{
				GUI.Label( new Rect( 20 , yoffset, 200, 20), "NO TEMPLATE AVALIABLE" );
				return;
			}
			
			CelestialBody cbBody;
			cbBody = Utils.FindCB( TemplateName );
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 0, 250, 400, 250 ), ScrollPosition2 ,new Rect( 0,250,400,10000) );
			
			foreach( FieldInfo key in cbBody.GetType().GetFields() )
			{
				try{
				System.Object obj = (System.Object)cbBody;
				if( key.GetValue(obj).GetType() == typeof( string ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) );
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( bool ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, GUI.Toggle( new Rect( 200 , yoffset, 200, 20 ), (bool)key.GetValue(obj), "Bool" ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( int ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, (int)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( float ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( double ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( Color ))
				{
					GUI.Label( new Rect( 20 , yoffset, 100, 20), ""+key.Name );
					if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
					{
						Color getColour;
						getColour = (Color)key.GetValue(obj);
						rVal = getColour.r;
						gVal = getColour.g;
						bVal = getColour.b;
						aVal = getColour.a;
							
						isshowingColourEditor = true;
					}
					if( GUI.Button( new Rect( 200 , yoffset, 50, 20), "Save" ) )
					{
						key.SetValue( obj, windowOutput );
					}
					//key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
					yoffset += 30;
				}
				else if( key.GetValue(obj).GetType() == typeof( Vector3 ))
				{
					GUI.Label( new Rect( 20 , yoffset, 200, 20), ""+key.Name );
					try
					{
						Vector3 blah;
						blah = ConfigNode.ParseVector3( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj).ToString()) );
						key.SetValue( obj, blah );
					}
					catch{}
					yoffset += 30;
				}
				}catch{}
			}
			yoffset += 30;
			if( GUI.Button( new Rect( 20 , yoffset, 200, 20), "Update" ))
			{
				cbBody.CBUpdate();
			}
			
			GUI.EndScrollView();
		}
		
		//Orbit Editor GUI
		string Lookupbody;
		private void OrbitEditorUI()
		{
			int yoffset = 280;
			
			if( TemplateName == "" )
			{
				GUI.Label( new Rect( 20 , yoffset, 200, 20), "NO TEMPLATE AVALIABLE" );
				return;
			}
			
			CelestialBody cbBody;
			cbBody = Utils.FindCB( TemplateName );
			Orbit orbittoMod = cbBody.orbitDriver.orbit;
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 0, 250, 400, 250 ), ScrollPosition2 ,new Rect( 0,250,400,2000) );
			
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "Inclination" );
			orbittoMod.inclination = StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+orbittoMod.inclination ) );
				yoffset += 30;
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "Eccentricity" );
			orbittoMod.eccentricity = StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+orbittoMod.eccentricity ) );
				yoffset += 30;
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "Semi-Major Axis" );
			orbittoMod.semiMajorAxis = StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+orbittoMod.semiMajorAxis ) );
				yoffset += 30;
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "LAN" );
			orbittoMod.LAN = StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+orbittoMod.LAN ) );
				yoffset += 30;
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "Argument Of Periapsis" );
			orbittoMod.argumentOfPeriapsis = StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+orbittoMod.argumentOfPeriapsis ) );
				yoffset += 30;
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "Mean Anomaly At Epoch" );
			orbittoMod.meanAnomalyAtEpoch = StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+orbittoMod.meanAnomalyAtEpoch ) );
				yoffset += 30;
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "Epoch" );
			orbittoMod.epoch = StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+orbittoMod.epoch ) );
				yoffset += 30;
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "Reference Body" );
			
			Lookupbody = GUI.TextField( new Rect( 200 , yoffset, 200, 20), Lookupbody );
			
			yoffset += 30;
			
			if( GUI.Button( new Rect( 200 , yoffset, 200, 20), "Update Reference Body" ) )
			{
				if( Utils.FindCB(Lookupbody) != null )
				{
					orbittoMod.referenceBody = Utils.FindCB(Lookupbody);
				}
			}
				
			yoffset += 30;
			
			GUI.Label( new Rect( 20 , yoffset, 200, 20), "Orbit Colour:" );
			//colour editor
			if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
			{
				Color getColour;
				getColour = cbBody.orbitDriver.Renderer.orbitColor;
				rVal = getColour.r;
				gVal = getColour.g;
				bVal = getColour.b;
				aVal = getColour.a;
					
				isshowingColourEditor = true;
			}
			if( GUI.Button( new Rect( 200 , yoffset, 50, 20), "Save" ) )
			{
				cbBody.orbitDriver.orbitColor = windowOutput;
			}
			
			yoffset += 30;
			
			if( GUI.Button( new Rect( 20 , yoffset, 200, 20), "Update Orbit" ) )
			{
				cbBody.orbitDriver.UpdateOrbit();
			}
			
			GUI.EndScrollView();
		}
		
		//Scaled Space Updater
		private void RegenerateModel(PQS bodyPQS, MeshFilter meshfilter_input)
		{
			var originalVert = meshfilter_input.mesh.vertices[0];
	        var originalHeight = (float)bodyPQS.GetSurfaceHeight(originalVert);
	        var scale = originalHeight / originalVert.magnitude;
				
			bodyPQS.isBuildingMaps = true;
	        var newVerts = new Vector3[meshfilter_input.mesh.vertices.Count()];
	        for (int i = 0; i < meshfilter_input.mesh.vertices.Count(); i++)
	        {
	            var vertex = meshfilter_input.mesh.vertices[i];
	            var rootrad = (float)Math.Sqrt(vertex.x * vertex.x +
	                            vertex.y * vertex.y +
	                            vertex.z * vertex.z);
	            var radius = (float)bodyPQS.GetSurfaceHeight(vertex)/scale;
	            //radius = 1000;
	            newVerts[i] = vertex * (radius / rootrad);
	        }
	        bodyPQS.isBuildingMaps = false;
			
			meshfilter_input.mesh.vertices = newVerts;
			
			meshfilter_input.mesh.RecalculateNormals();
            Utils.RecalculateTangents(meshfilter_input.mesh);
		}
		
		//Ocean Tools
		int OceanToolsUiSelector;
		private void OceanToolsUI()
		{
			switch( OceanToolsUiSelector )
			{
			case 1:
				OceanToolsUI_AddOcean();
				return;
			case 2:
			//OceanToolsUI_AddOcean();
				return;
				
			case 3:
			//OceanToolsUI_ExportOcean();
				return;
			default:
				break;
			}
			
			int yoffset = 280;
			if( GUI.Button( new Rect( 20, yoffset, 400, 20 ), "Add ocean" ))
			{
				OceanToolsUiSelector = 1;
			}
			yoffset += 30;
			if( GUI.Button( new Rect( 20, yoffset, 400, 20 ), "Import ocean Texture for" + TemplateName ))
			{
				OceanToolsUI_ImportOcean();
			}
			//yoffset += 30;
			//if( GUI.Button( new Rect( 20, yoffset, 400, 20 ), "Export Ocean Textures (Temp)" ))
			//{
			//	OceanToolsUiSelector = 3;
			//}
			yoffset += 30;
			
		}
		
		private void OceanToolsUI_AddOcean()
		{
			int yoffset = 280;
			
			if( TemplateName == "" )
			{
				GUI.Label( new Rect( 20 , yoffset, 200, 20), "NO TEMPLATE AVALIABLE" );
				return;
			}
			
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 0, 250, 400, 250 ), ScrollPosition2 ,new Rect( 0,250,400,10000) );
			GUI.Label( new Rect( 20, yoffset, 400, 250 ), "Add ocean of type:" );
			yoffset += 30;
			//Get a list of "oceans"
			foreach( PSystemBody body in Templates )
			{
				foreach( PQS pqs in Utils.FindLocal(body.celestialBody.name).GetComponentsInChildren(typeof( PQS )) )
				{
					if( pqs.gameObject.name == (string)(body.celestialBody.name + "Ocean") )
					{
						if( GUI.Button( new Rect( 20, yoffset, 200, 20 ) , pqs.gameObject.name ) )
						{
							PQS LaytheOceanClone = (PQS)Instantiate(pqs);
							LaytheOceanClone.name = TemplateName + "Ocean";
							LaytheOceanClone.parentSphere = Utils.FindCB( TemplateName ).pqsController;
							LaytheOceanClone.transform.position = Utils.FindCB( TemplateName ).pqsController.transform.position;
							LaytheOceanClone.transform.parent = Utils.FindCB( TemplateName ).pqsController.transform;
							LaytheOceanClone.surfaceMaterial.mainTexture = pqs.surfaceMaterial.mainTexture;
							LaytheOceanClone.fallbackMaterial.mainTexture = pqs.fallbackMaterial.mainTexture;
							
							LaytheOceanClone.radius = Utils.FindCB( TemplateName ).Radius;
							//print ( " " + LaytheOceanClone.radius + "\n" );
							PQS OceanPQS = LaytheOceanClone.GetComponent<PQS>();
							OceanPQS.radius = Utils.FindCB( TemplateName ).Radius;
							//print ( " " + OceanPQS.radius + "\n" );
							OceanPQS.parentSphere = Utils.FindCB( TemplateName ).pqsController;
							
							OceanPQS.surfaceMaterial.mainTexture = pqs.surfaceMaterial.mainTexture;
							OceanPQS.fallbackMaterial.mainTexture = pqs.fallbackMaterial.mainTexture;
							
							OceanPQS.RebuildSphere();
							
							PQSMod_OceanFX LaytheOceanClone2 = (PQSMod_OceanFX)(OceanPQS.gameObject.GetComponentInChildren<PQSMod_OceanFX>() );
							LaytheOceanClone2.sphere = LaytheOceanClone;
							LaytheOceanClone2.waterMat = pqs.surfaceMaterial;
							LaytheOceanClone2.OnSetup();
							LaytheOceanClone2.OnUpdateFinished();
							LaytheOceanClone2.RebuildSphere();
							
							LaytheOceanClone.RebuildSphere();
							
							PlanetarySettings[ TemplateName ].OceanTemplate = pqs.gameObject.name;
							PlanetarySettings[ TemplateName ].AddOceanFx = true;
							PlanetarySettings[ TemplateName ].HasOceanFx = true;
						}
						yoffset += 30;
					}
				}
			}
			
			GUI.EndScrollView();
		}
		
		private void OceanToolsUI_ImportOcean()
		{
			//Grab ocean Gobj
			foreach( PSystemBody body in Templates )
			{
				foreach( PQS pqs in Utils.FindLocal(body.celestialBody.name).GetComponentsInChildren(typeof( PQS )) )
				{
					if( pqs.gameObject.name == TemplateName + "Ocean")
					{
						Texture2D tex = Utils.LoadTexture( "Gamedata/KittopiaSpace/Textures/"+TemplateName+"_Ocean.png" );
						pqs.surfaceMaterial.mainTexture = tex;
						pqs.fallbackMaterial.mainTexture = tex;
						pqs.RebuildSphere();
					}
				}
			}
			
		}
		
		//Ring tool vars:
		double OuterRadius;
		double InnerRadius;
		float Tilt = 0;
		Color RingColour;
		private void RingEditorFunc()
		{
			int yoffset = 280;
			
			if( TemplateName == "" )
			{
				GUI.Label( new Rect( 20 , yoffset, 200, 20), "NO TEMPLATE AVALIABLE" );
				return;
			}
			
			ScrollPosition2 = GUI.BeginScrollView( new Rect( 20, 260, 380, 250 ), ScrollPosition2 ,new Rect( 20,260,380,600) );
			
			GUI.Label( new Rect( 20, yoffset, 200, 20 ), "Inner Radius:" );
			yoffset+=30;
			InnerRadius = (double)System.Convert.ToDouble(GUI.TextField( new Rect( 20, yoffset, 200, 20 ), ""+InnerRadius ));
			
			yoffset+=60;
			GUI.Label( new Rect( 20, yoffset, 200, 20 ), "Outer Radius:" );
			yoffset+=30;
			OuterRadius = (double)System.Convert.ToDouble(GUI.TextField( new Rect( 20, yoffset, 200, 20 ), ""+OuterRadius ));
			
			yoffset+=60;
			GUI.Label( new Rect( 20, yoffset, 200, 20 ), "Inclination:" );
			yoffset+=30;
			Tilt = (float)System.Convert.ToSingle(GUI.TextField( new Rect( 20, yoffset, 200, 20 ), ""+ Tilt ));
			
			yoffset+=60;
			GUI.Label( new Rect( 20, yoffset, 200, 20 ), "Colour:" );
			if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
			{
				Color getColour = new Color( 1, 1, 1 );
				rVal = getColour.r;
				gVal = getColour.g;
				bVal = getColour.b;
				aVal = getColour.a;
					
				isshowingColourEditor = true;
			}
			if( GUI.Button( new Rect( 200 , yoffset, 50, 20), "Save" ) )
			{
				RingColour = windowOutput;
			}
			
			yoffset+=60;
			
			GameObject Ring = null;
			
			if( GUI.Button( new Rect( 20 , yoffset, 200, 20), "Generate ring for: " + TemplateName ) )
			{
				string PlanetRingTexName = "Gamedata/KittopiaSpace/Textures/" + TemplateName + "_ring.png";
				if ( Utils.FileExists( PlanetRingTexName ) )
				{
					Ring = PlanetUtils.AddRingToPlanet( Utils.FindScaled( TemplateName ), InnerRadius, OuterRadius, Tilt, Utils.LoadTexture( PlanetRingTexName, false ), RingColour );
				}
				else
				{
					PlanetRingTexName = "Gamedata/KittopiaSpace/Textures/Default/ring.png";
					if ( Utils.FileExists( PlanetRingTexName ) )
					{
						Ring = PlanetUtils.AddRingToPlanet( Utils.FindScaled( TemplateName ), InnerRadius, OuterRadius, Tilt, Utils.LoadTexture( PlanetRingTexName, false ), RingColour );
					}
					else
					{
						print( "PlanetUI: Critical failure: Default ring texture not found!\n" );
						return;
					}
				}
				PlanetarySettings[ TemplateName ].AddRing = true;
				PlanetarySettings[ TemplateName ].Rings.Add( new RingSaveStorageHelper( Tilt, OuterRadius, InnerRadius, RingColour, Ring ) );
			}
			yoffset+=30;
			if( GUI.Button( new Rect( 20 , yoffset, 200, 20), "Delete rings on: " + TemplateName ) )
			{
				GameObject ScaledPlanet = Utils.FindScaled( TemplateName );
				GameObject RingGobj = ScaledPlanet.transform.FindChild( "PlanetaryRingObject" ).gameObject;
				if( RingGobj != null )
				{
					Destroy( RingGobj );
					//todo: delete ring storage class
					
					PlanetarySettings[ TemplateName ].Rings.RemoveAt( 0 );
				}
			}
			
			GUI.EndScrollView();
		}
		
		public ConfigNode cfgNodes;
		
		//Data Saver
		private void SaveData()
		{
			string save_dir;
			string curSave = HighLogic.SaveFolder;
			
			//if( curSave == null )
			//{
				save_dir = "Gamedata/KittopiaSpace/SaveLoad/"+TemplateName+".cfg";
			//}
			//else
			//{
			//	save_dir = "Gamedata/KittopiaSpace/PlanetUI/"+curSave+".cfg";
			//}
			
			//Ok, here goes...
			cfgNodes = ConfigNode.Load( save_dir );
			if( cfgNodes == null )
			{
				cfgNodes = new ConfigNode();
				cfgNodes.Save( save_dir, TemplateName );
			}
			
			//Save to file
			if( cfgNodes.HasNode( "Planet"+TemplateName ) )
			{
				cfgNodes.RemoveNode( "Planet"+TemplateName );
			}
			ConfigNode planet_rootnode = cfgNodes.AddNode( "Planet"+TemplateName );
			
			ConfigNode additionTools_root = planet_rootnode.AddNode( "AdditionalData" );
			additionTools_root.AddValue( "Stock", PlanetarySettings[ TemplateName ].IsStock );
			additionTools_root.AddValue( "AddAtmoFx", PlanetarySettings[ TemplateName ].AddAtmoFX );
			try
			{
				AtmosphereFromGround AtmoToMod = Utils.FindScaled(TemplateName).GetComponentInChildren<AtmosphereFromGround>();
				additionTools_root.AddValue( "AtmoWaveColour", AtmoToMod.waveLength );
			}
			catch{}
			//additionTools_root.AddValue( "AtmoWaveColour", PlanetarySettings[ TemplateName ].AtmoInvColour );
			additionTools_root.AddValue( "AddOceanFx", PlanetarySettings[ TemplateName ].AddOceanFx );
			if( PlanetarySettings[ TemplateName ].AddOceanFx == true )
			{
				additionTools_root.AddValue( "OceanTemplate", PlanetarySettings[ TemplateName ].OceanTemplate );
			}
			
			additionTools_root.AddValue( "ModScaledAtmoShader" , PlanetarySettings[ TemplateName ].ModScaledAtmoShader );
			
			additionTools_root.AddValue( "AddRings", PlanetarySettings[ TemplateName ].AddRing );
			if( PlanetarySettings[ TemplateName ].AddRing == true )
			{
				ConfigNode Rings_root = additionTools_root.AddNode( "Rings" );
				
				foreach( RingSaveStorageHelper ring in PlanetarySettings[ TemplateName ].Rings )
				{
					ConfigNode RingNode = Rings_root.AddNode( "Ring" );
					RingNode.AddValue( "Tilt", ring.tilt );
					RingNode.AddValue( "OuterRadius", ring.OuterRadius );
					RingNode.AddValue( "InnerRadius", ring.InnerRadius );
					RingNode.AddValue( "Colour", ring.Colour );
				}
			}
			
			CelestialBody cbBody;
			cbBody = Utils.FindCB( TemplateName );
			
			if( cbBody.GetOrbitDriver() != null )
			{
				ConfigNode Orbit_Node = planet_rootnode.AddNode( "Orbit" );
				Orbit_Node.AddValue( "semiMajorAxis", cbBody.orbitDriver.orbit.semiMajorAxis );
				Orbit_Node.AddValue( "eccentricity", cbBody.orbitDriver.orbit.eccentricity );
				Orbit_Node.AddValue( "inclination", cbBody.orbitDriver.orbit.inclination );
				Orbit_Node.AddValue( "meanAnomalyAtEpoch", cbBody.orbitDriver.orbit.meanAnomalyAtEpoch );
				Orbit_Node.AddValue( "epoch", cbBody.orbitDriver.orbit.epoch );
				Orbit_Node.AddValue( "argumentOfPeriapsis", cbBody.orbitDriver.orbit.argumentOfPeriapsis );
				Orbit_Node.AddValue( "LAN", cbBody.orbitDriver.orbit.LAN );
				
				Orbit_Node.AddValue( "orbitColor", cbBody.orbitDriver.orbitColor );
			}
			
			ConfigNode CelestialBody_Node = planet_rootnode.AddNode( "CelestialBody" );
			
			//CB Dumps
			foreach( FieldInfo key in cbBody.GetType().GetFields() )
			{
				try{
				System.Object obj = (System.Object)cbBody;
				if( key.GetValue( obj ).GetType() == typeof( string ) 
					|| key.GetValue( obj ).GetType() == typeof( double ) 
					|| key.GetValue( obj ).GetType() == typeof( int ) 
					|| key.GetValue( obj ).GetType() == typeof( float )
					|| key.GetValue( obj ).GetType() == typeof( bool )
					|| key.GetValue( obj ).GetType() == typeof( Color )
					|| key.GetValue( obj ).GetType() == typeof( Vector3 ))
				{
					CelestialBody_Node.AddValue( key.Name, key.GetValue( obj ) );
				}
				}catch{}
			}
			ConfigNode PQSRoot = planet_rootnode.AddNode( "PQS" );
			//PQS Dumps
			foreach( PQSMod pqs in Utils.FindLocal(TemplateName).GetComponentsInChildren(typeof( PQSMod )) )
			{
				ConfigNode savePQS = PQSRoot.AddNode( ""+pqs.GetType() );
				foreach( FieldInfo key in pqs.GetType().GetFields() )
				{
					try{
					System.Object obj = (System.Object)pqs;
					if( key.GetValue( obj ).GetType() == typeof( string ) 
						|| key.GetValue( obj ).GetType() == typeof( double ) 
						|| key.GetValue( obj ).GetType() == typeof( int ) 
						|| key.GetValue( obj ).GetType() == typeof( float )
						|| key.GetValue( obj ).GetType() == typeof( bool )
						|| key.GetValue( obj ).GetType() == typeof( Color )
						|| key.GetValue( obj ).GetType() == typeof( Vector3 )
						|| key.GetValue( obj ).GetType() == typeof( PQS ))
					{
						savePQS.AddValue( key.Name, key.GetValue( obj ) );
					}
						
						if( key.GetValue( obj ).GetType() == typeof( PQSLandControl.LandClass[] ) ) //Landclasses
						{
							ConfigNode landclasses_root = savePQS.AddNode( "Landclass[]" );
							foreach( PQSLandControl.LandClass lc in (PQSLandControl.LandClass[])key.GetValue( obj ) )
							{
								ConfigNode landclass_root = landclasses_root.AddNode( "Landclass" );
								foreach( FieldInfo key2 in lc.GetType().GetFields() )
								{
									try{
										System.Object obj2 = (System.Object)lc;
										if( key2.GetValue( obj2 ).GetType() == typeof( string ) 
											|| key2.GetValue( obj2 ).GetType() == typeof( double ) 
											|| key2.GetValue( obj2 ).GetType() == typeof( int ) 
											|| key2.GetValue( obj2 ).GetType() == typeof( float )
											|| key2.GetValue( obj2 ).GetType() == typeof( bool )
											|| key2.GetValue( obj2 ).GetType() == typeof( Color ) )
										{
											landclass_root.AddValue( key2.Name, key2.GetValue( obj2 ) );
										}
									}catch{}
								}
							}
						}
						if( key.GetValue( obj ).GetType() == typeof( PQSMod_VertexPlanet.LandClass[] ) )
						{
							ConfigNode landclasses_root = savePQS.AddNode( "VPLandclass[]" );
							foreach( PQSMod_VertexPlanet.LandClass lc in (PQSMod_VertexPlanet.LandClass[])key.GetValue( obj ) )
							{
								ConfigNode landclass = landclasses_root.AddNode( "Landclass" );
								foreach( FieldInfo key2 in lc.GetType().GetFields() )
								{
									try{
										System.Object obj2 = (System.Object)lc;
										if( key2.GetValue( obj2 ).GetType() == typeof( string ) 
											|| key2.GetValue( obj2 ).GetType() == typeof( double ) 
											|| key2.GetValue( obj2 ).GetType() == typeof( int ) 
											|| key2.GetValue( obj2 ).GetType() == typeof( float )
											|| key2.GetValue( obj2 ).GetType() == typeof( bool )
											|| key2.GetValue( obj2 ).GetType() == typeof( Color ) )
										{
											landclass.AddValue( key2.Name, key2.GetValue( obj2 ) );
										}
									}catch{}
								}
							}
						}
					}catch{}
				}
			}
			cfgNodes.Save( save_dir, "CustomData" );
		}
		
		//Data Loader
		public void LoadData( string PlanetName, string path )
		{	
			if( !Utils.FileExists(path) )
			{
				print("PlanetUI: No data loaded for " +PlanetName+ "\n" );
				return;
			}
			cfgNodes = ConfigNode.Load( path );
			if( cfgNodes == null )
			{
				print("PlanetUI: No data loaded for " +PlanetName+ "\n" );
				return;
			}
			
			if( cfgNodes.HasNode( "Planet"+PlanetName ) )
			{
				GameObject localPlanet = Utils.FindLocal( PlanetName );
				ConfigNode planet_rootnode = cfgNodes.GetNode( "Planet"+PlanetName );
			
				ConfigNode additionalsettings_Rootnode = planet_rootnode.GetNode( "AdditionalData" );
				string tempColourString;
				
				bool ShouldGenerateNewAtmo = bool.Parse(additionalsettings_Rootnode.GetValue( "AddAtmoFx" ));
				bool ShouldGenerateNewOcean = bool.Parse(additionalsettings_Rootnode.GetValue( "AddOceanFx" ));
				
				if( PlanetarySettings.ContainsKey( PlanetName ) )
				{
					if( additionalsettings_Rootnode.HasValue( "AtmoWaveColour" ) )
					{
						tempColourString = additionalsettings_Rootnode.GetValue( "AtmoWaveColour" );
						tempColourString = tempColourString.Replace( "RGBA(" , "" );
						tempColourString = tempColourString.Replace( ")" , "" );
						Color newAtmoWaveColour = ConfigNode.ParseColor( tempColourString );
						
						if( PlanetarySettings[ PlanetName ].HasAtmoFx == false && ShouldGenerateNewAtmo == true )
						{
							PlanetUtils.AddAtmoFX( PlanetName, 1, newAtmoWaveColour, 0 );
							PlanetarySettings[ PlanetName ].HasAtmoFx = true;
						}
						else
						{
							try
							{	
								AtmosphereFromGround AtmoToMod = Utils.FindScaled(PlanetName).GetComponentInChildren<AtmosphereFromGround>();
								AtmoToMod.waveLength = newAtmoWaveColour;
							}
							catch{}
						}
					}
					//Oceans
					if( PlanetarySettings[ PlanetName ].HasOceanFx == false && ShouldGenerateNewOcean == true )
					{
						//Stuff
						string oceanTemplateName = additionalsettings_Rootnode.GetValue( "OceanTemplate" );
						foreach( PSystemBody body in Templates )
						{
							foreach( PQS pqs in Utils.FindLocal(body.celestialBody.name).GetComponentsInChildren(typeof( PQS )) )
							{
								if( pqs.gameObject.name == oceanTemplateName )
								{
									PQS LaytheOceanClone = (PQS)Instantiate(pqs);
									LaytheOceanClone.name = TemplateName + "Ocean";
									LaytheOceanClone.parentSphere = Utils.FindCB( PlanetName ).pqsController;
									LaytheOceanClone.transform.position = Utils.FindCB( PlanetName ).pqsController.transform.position;
									LaytheOceanClone.transform.parent = Utils.FindCB( PlanetName ).pqsController.transform;
									LaytheOceanClone.surfaceMaterial = pqs.surfaceMaterial;
									LaytheOceanClone.fallbackMaterial = pqs.fallbackMaterial;
									
									LaytheOceanClone.radius = Utils.FindCB( PlanetName ).Radius;
									//print ( " " + LaytheOceanClone.radius + "\n" );
									PQS OceanPQS = LaytheOceanClone.GetComponent<PQS>();
									OceanPQS.radius = Utils.FindCB( PlanetName ).Radius;
									//print ( " " + OceanPQS.radius + "\n" );
									OceanPQS.parentSphere = Utils.FindCB( PlanetName ).pqsController;
									
									OceanPQS.surfaceMaterial = pqs.surfaceMaterial;
									OceanPQS.fallbackMaterial = pqs.fallbackMaterial;
									
									OceanPQS.RebuildSphere();
									
									PQSMod_OceanFX LaytheOceanClone2 = (PQSMod_OceanFX)(OceanPQS.gameObject.GetComponentInChildren<PQSMod_OceanFX>() );
									LaytheOceanClone2.sphere = LaytheOceanClone;
									LaytheOceanClone2.waterMat = pqs.surfaceMaterial;
									LaytheOceanClone2.OnSetup();
									LaytheOceanClone2.OnUpdateFinished();
									LaytheOceanClone2.RebuildSphere();
									
									LaytheOceanClone.RebuildSphere();
									
									PlanetarySettings[ PlanetName ].HasOceanFx = true;
								}
							}
						}
					}
					
					//Rings
					if( additionalsettings_Rootnode.HasNode( "Rings" ) )
					{
						ConfigNode RingBaseNode = additionalsettings_Rootnode.GetNode( "Rings" );
						
						print("PlanetUI: Found rings on:" +PlanetName+ "\n" );
						
						foreach( ConfigNode ringNode in RingBaseNode.nodes )
						{
							float tilt;
							double outerradius,innerradius;
							Color ringcolour;
							
							tilt = (float)Convert.ToSingle(ringNode.GetValue( "Tilt" ));
							outerradius = Convert.ToDouble(ringNode.GetValue( "OuterRadius" ));
							innerradius = Convert.ToDouble(ringNode.GetValue( "InnerRadius" ));
							
							tempColourString = ringNode.GetValue( "Colour" );
							tempColourString = tempColourString.Replace( "RGBA(" , "" );
							tempColourString = tempColourString.Replace( ")" , "" );
							ringcolour = ConfigNode.ParseColor( tempColourString );
							
							string PlanetRingTexName = "Gamedata/KittopiaSpace/Textures/" + PlanetName + "_ring.png";
							if ( Utils.FileExists( PlanetRingTexName ) )
							{
								PlanetUtils.AddRingToPlanet( Utils.FindScaled( PlanetName ), innerradius, outerradius, tilt, Utils.LoadTexture( PlanetRingTexName, false ), ringcolour );
							}
							else
							{
								PlanetRingTexName = "Gamedata/KittopiaSpace/Textures/Default/ring.png";
								if( Utils.FileExists( PlanetRingTexName ) )
								{
									PlanetUtils.AddRingToPlanet( Utils.FindScaled( PlanetName ), innerradius, outerradius, tilt, Utils.LoadTexture( PlanetRingTexName, false ), ringcolour );
								}
								else
								{
									print( "PlanetUI: Critical failure: Default ring texture not found!\n" );
								}
							}
							
							print("PlanetUI: Created a ring for:" +PlanetName+ "\n" );
						}
					}
					
					//Scaled Atmo
					if( additionalsettings_Rootnode.HasValue( "ModScaledAtmoShader" ) )
					{
						//Load rim texture...
						string RimTex = "Gamedata/KittopiaSpace/Textures/"+PlanetName+".png";
						if( !Utils.FileExists( RimTex ) )
						{
							RimTex = "Gamedata/KittopiaSpace/Textures/default/blank_rim_text.png";
							if( Utils.FileExists( RimTex ) )
							{
								//Update atmo shader texture
								Utils.LoadScaledPlanetAtmoShader( PlanetName, Utils.LoadTexture( RimTex, false ) );
								PlanetarySettings[ PlanetName ].ModScaledAtmoShader = true;	
							}
						}
					}
				}
				
				print("PlanetUI: Loaded ADDITIONALDATA of " +PlanetName+ "\n" );
				
				CelestialBody cbBody;
				cbBody = Utils.FindCB( PlanetName );
				
				if( cbBody.GetOrbitDriver() != null )
				{
					ConfigNode Orbit_Node = planet_rootnode.GetNode( "Orbit" );
					cbBody.orbitDriver.orbit.semiMajorAxis = Convert.ToDouble( Orbit_Node.GetValue( "semiMajorAxis" ) );
					cbBody.orbitDriver.orbit.eccentricity = Convert.ToDouble(Orbit_Node.GetValue( "eccentricity" ));
					cbBody.orbitDriver.orbit.inclination = Convert.ToDouble(Orbit_Node.GetValue( "inclination" ));
					cbBody.orbitDriver.orbit.meanAnomalyAtEpoch = Convert.ToDouble(Orbit_Node.GetValue( "meanAnomalyAtEpoch" ));
					cbBody.orbitDriver.orbit.epoch = Convert.ToDouble(Orbit_Node.GetValue( "epoch" ));
					cbBody.orbitDriver.orbit.argumentOfPeriapsis = Convert.ToDouble(Orbit_Node.GetValue( "argumentOfPeriapsis" ));
					cbBody.orbitDriver.orbit.LAN = Convert.ToDouble(Orbit_Node.GetValue( "LAN" ));
					
					tempColourString = Orbit_Node.GetValue( "orbitColor" );
					tempColourString = tempColourString.Replace( "RGBA(" , "" );
					tempColourString = tempColourString.Replace( ")" , "" );
					Color orbitColor = ConfigNode.ParseColor( tempColourString );
					
					cbBody.orbitDriver.orbitColor = orbitColor;
					cbBody.orbitDriver.UpdateOrbit();
				}
				
				print("PlanetUI: Loaded ORBIT of " +PlanetName+ "\n" );
				
				ConfigNode cb_rootnode = planet_rootnode.GetNode( "CelestialBody" );
				
				//Load CB related stuff
				System.Object cbobj = cbBody;
				foreach( FieldInfo key in cbobj.GetType().GetFields() )
				{
					if( cb_rootnode.HasValue( key.Name ) )
					{
						string val = cb_rootnode.GetValue( key.Name );
						System.Object castedval = val;
						Type t = key.GetValue( cbobj ).GetType();
						
						if ( t == typeof(UnityEngine.Vector3) )
						{
							val = val.Replace( "(" , "" );
							val = val.Replace( ")" , "" );
							
							key.SetValue( cbobj, ConfigNode.ParseVector3( val ) );
						}
						else if ( t == typeof(UnityEngine.Color) )
						{
							val = val.Replace( "RGBA(" , "" );
							val = val.Replace( ")" , "" );
							key.SetValue( cbobj, ConfigNode.ParseColor( val ) );
						}
						else
						{
							key.SetValue( cbobj, Convert.ChangeType( castedval, t ) );
						}
					}
					
					CelestialBody CBody = (CelestialBody)cbobj;
					CBody.CBUpdate();
				}
				print("PlanetUI: Loaded CB of " +PlanetName+ "\n" );
				
				//Load PQS related stuff
				ConfigNode pqs_rootnode = planet_rootnode.GetNode( "PQS" );
				foreach( ConfigNode node in pqs_rootnode.nodes )
				{
					var componentTypeStr = node.name;
	                var componentType = Type.GetType(componentTypeStr + ", Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
	                if (componentType == null)
	                {
	                    print("Cant find PQSMod type:" + componentTypeStr + "\n");
	                    continue;
	                }

					print( node.name + "\n" );
					if( localPlanet.GetComponentInChildren(componentType) == null )
					{
						PQS mainsphere = localPlanet.GetComponentInChildren<PQS>();
						if( mainsphere == null )
						{
							print ( "Cannot add PQSMod to " + PlanetName + "... Is it a gas giant?\n" );
							continue;
						}
						
						PlanetUtils.AddPQSMod( mainsphere , componentType);
					}
					var component =	localPlanet.GetComponentInChildren(componentType);
					
					System.Object obj = component;
					foreach( FieldInfo key in obj.GetType().GetFields() )
					{
						try
						{
							if( node.HasValue( key.Name ) )
							{
								if ( key.GetValue( obj ).GetType() == typeof(PQS) )
								{
									//key.SetValue( cbobj, ConfigNode.ParseColor( val ) );
									print ( "PQS not compatible at this point." );
									continue;
								}
								
								if( key.GetValue( obj ).GetType() == typeof( PQSLandControl.LandClass[] ) )
								{
									if( node.HasNode( "Landclass[]" ) )
									{
										//print( "LANDCLASS \n" );
										LoadLandControl( node.GetNode("Landclass[]") , obj , key );
									}
									else
									{
										print( "PlanetUI: Failed to load LANDCLASS\n" );
									}
								}
								else if( node.HasNode( "VPLandclass[]" ) && key.GetValue( obj ).GetType() == typeof( PQSMod_VertexPlanet.LandClass[] ) )
								{
									LoadVPLandControl( node.GetNode("VPLandclass[]") , obj , key );
								}
								
							
								//print( "PlanetUI: Attempting: " + key.Name + " of type " + key.GetValue( obj ).GetType() + "\n" );
								string val = node.GetValue( key.Name );
								System.Object castedval = val;
								Type t = key.GetValue( obj ).GetType();
								
								//print( "PlanetUI: " + component + " " + key.Name + " = ("+t+") " + castedval + "\n" );
								
								if ( t == typeof(UnityEngine.Vector3) )
								{
									val = val.Replace( "(" , "" );
									val = val.Replace( ")" , "" );
									
									key.SetValue( obj, ConfigNode.ParseVector3( val ) );
								}
								else if ( t == typeof(UnityEngine.Color) )
								{
									val = val.Replace( "RGBA(" , "" );
									val = val.Replace( ")" , "" );
									key.SetValue( obj, ConfigNode.ParseColor( val ) );
								}
								else
								{
									key.SetValue( obj, Convert.ChangeType( castedval, t ) );
								}
							}
						}
						catch( Exception e )
						{
							print ( "PlanetUI: Failed to load: "+obj+", Exeption: " + e );
							continue;
						}
					}
					//Utils.FindLocal( PlanetName ).GetComponentInChildren<PQS>().RebuildSphere();
				}
				print("PlanetUI: Loaded PQS of " +PlanetName+ "\n" );
			}
			else
			{
				print("PlanetUI: No data loaded for " +PlanetName+ "\n" );
			}
		}
		void LoadLandControl( ConfigNode pqsNode, System.Object obj, FieldInfo key )
		{
			//int count = pqsNode.CountNodes;
			int countincrement = 0;
			foreach( PQSLandControl.LandClass lc in (PQSLandControl.LandClass[])key.GetValue( obj ) )
			{
				foreach( FieldInfo key2 in lc.GetType().GetFields() )
				{
					if( pqsNode.nodes[countincrement] == null )
					{
						continue;
					}
					if( pqsNode.nodes[countincrement].HasValue( key2.Name ) )
					{
						System.Object obj2 = (System.Object)lc;
						
						string val = pqsNode.nodes[countincrement].GetValue( key2.Name );
						System.Object castedval = val;
						Type t = key2.GetValue( obj2 ).GetType();
						
						//print( "("+t+") " + castedval + "\n" );
						
						if( t != typeof(UnityEngine.Color) )
						{
							key2.SetValue( obj2, Convert.ChangeType( castedval, t ) );
						}
						else
						{
							val = val.Replace( "RGBA(" , "" );
							val = val.Replace( ")" , "" );
							key2.SetValue( obj2, ConfigNode.ParseColor( val ) );
						}
					}
				}
				countincrement++;
			}
		}
		void LoadVPLandControl( ConfigNode pqsNode, System.Object obj, FieldInfo key )
		{
			//int count = pqsNode.CountNodes;
			int countincrement = 0;
			foreach( PQSMod_VertexPlanet.LandClass lc in (PQSMod_VertexPlanet.LandClass[])key.GetValue( obj ) )
			{
				foreach( FieldInfo key2 in lc.GetType().GetFields() )
				{
					if( pqsNode.nodes[countincrement] == null )
					{
						continue;
					}
					if( pqsNode.nodes[countincrement].HasValue( key2.Name ) )
					{
						System.Object obj2 = (System.Object)lc;
						
						string val = pqsNode.nodes[countincrement].GetValue( key2.Name );
						System.Object castedval = val;
						Type t = key2.GetValue( obj2 ).GetType();
						
						//print( "("+t+") " + castedval + "\n" );
						
						if( t != typeof(UnityEngine.Color) )
						{
							key2.SetValue( obj2, Convert.ChangeType( castedval, t ) );
						}
						else
						{
							val = val.Replace( "RGBA(" , "" );
							val = val.Replace( ")" , "" );
							key2.SetValue( obj2, ConfigNode.ParseColor( val ) );
						}
					}
				}
				countincrement++;
			}
		}
		
		
		public ConfigNode cfgNodes2;
		
		//Data Saver
		private void SaveStarData( string starName )
		{
			string save_dir;
			save_dir = "Gamedata/KittopiaSpace/StarFix.cfg";
			
			cfgNodes2 = ConfigNode.Load( save_dir );
			if( cfgNodes2 == null )
			{
				cfgNodes2 = new ConfigNode();
				cfgNodes2.Save( save_dir, "StarFix" );
			}
			
			//Save to file
			if( cfgNodes2.HasNode( starName ) )
			{
				cfgNodes2.RemoveNode( starName );
			}
			
			cfgNodes2.AddNode( starName );
			
			cfgNodes2.Save( save_dir, "StarFix" );
		}
		private void LoadStarData( string starName )
		{
			string save_dir;
			save_dir = "Gamedata/KittopiaSpace/StarFix.cfg";
			
			cfgNodes2 = ConfigNode.Load( save_dir );
			if( cfgNodes2 == null )
			{
				cfgNodes2 = new ConfigNode();
				cfgNodes2.Save( save_dir, "StarFix" );
			}

			if( cfgNodes2.HasNode( starName ) )
			{
				PlanetUtils.FixStar( starName );
			}
		}
	}
}

