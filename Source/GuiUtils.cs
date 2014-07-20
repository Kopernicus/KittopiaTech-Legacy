
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;

namespace PFUtilityAddon
{
	//BASE window class
	public class BaseGuiWindow
	{
		public BaseGuiWindow( int i_id, string i_name )
		{
			name = i_name;
			id = i_id;
		}
		
		public bool enabled;
		protected string name;
		protected int id;
		
		Rect windowPos = new Rect( 420,20,400,200 );
		public void RenderWindow()
		{
			if( enabled )
			{
				windowPos = GUI.Window( id, windowPos, WindowFunction, name );
			}
		}
		
		public void ToggleWindow()
		{
			enabled = !enabled;
			
			Debug.Log( "DEBUG: Toggled "+name+"\n");
		}
		
		public virtual void WindowFunction( int WindowId )
		{
			GUI.DragWindow( new Rect( 420,20,400,200 ) );
		}
	}
	
	public delegate void ButtonDelegate( int i );
	
	//SCROLL window class
	public class ScrollWindow : BaseGuiWindow
	{
		Vector2 ScrollPosition;
		int NumButtons;
		string[] buttonOptions;
		ButtonDelegate[] ButtonFuncs;
		
		public ScrollWindow( string[] ButtonOptions_I, ButtonDelegate[] i_buttonFuncs, string name_i, int id_i ) : base( id_i, name_i )
		{
			buttonOptions = ButtonOptions_I;
			id = id_i;
			name = name_i;
			ButtonFuncs = i_buttonFuncs;
			NumButtons = buttonOptions.Length;
		}
		
		public override void WindowFunction( int WindowId )
		{	
			int offset = 30;
			ScrollPosition = GUI.BeginScrollView( new Rect( 0, 20, 360, 150 ), ScrollPosition, new Rect( 0,0, 360, NumButtons * offset) );
			
			for( int i = 0; i < NumButtons; i++ )
			{
				if( buttonOptions[i] != null )
				{
					if( GUI.Button( new Rect( 20, offset, 200, 20 ), ""+buttonOptions[i] ) )
					{
						ButtonFuncs[i]( i );
					}
				}
				offset += 30;
			}
			
			GUI.EndScrollView();
			
			GUI.DragWindow( new Rect( 0,0,400,200 ) );
		}
	}
	
	//HELP window class
	public class HelpWindow : BaseGuiWindow
	{
		public HelpWindow( int id_i, string name_i ) : base( id_i, name_i )
		{
			name = name_i;
			id = id_i;
		}
		
		string HelpText;
		public void SetString( string QueryString )
		{
			ConfigNode loadString;
			loadString = ConfigNode.Load( "Gamedata/KittopiaSpace/Help.cr_help" );
			if( loadString.HasNode( QueryString ) )
			{
				HelpText = loadString.GetNode( QueryString ).GetValue("HelpText");
			}
			else
			{
				HelpText = "No information found";
			}
		}
		
		public override void WindowFunction( int WindowId )
		{
			GUI.TextArea( new Rect( 10, 30, 390, 170 ) , HelpText );
			GUI.DragWindow(  );
		}
		
		string OldQueryString;
		public void CustomToggle( string QueryString )
		{
			if( QueryString == OldQueryString )
			{
				enabled = !enabled;
			}
			else
			{
				OldQueryString = QueryString;
				SetString(QueryString);
				enabled = true;
			}
		}
	}
}