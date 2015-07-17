using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a PQS-Editor
        public class PQSEditor
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Modes
            public enum Modes : int
            {
                List,
                PQS,
                PQSMod,
                AddMod
            }
            public static Modes mode = Modes.List;

            // Current PQS Components
            public static PQS currentPQS;
            public static PQSMod currentPQSMod;

            // Return an OnGUI()-Window.
            public static void Render()
            {
                // If we have no Body selected, abort
                if (PlanetUI.currentName == "")
                {
                    GUI.Label(new Rect(20, 310, 400, 20), "No Planet selected!");
                    return;
                }

                // If the body has no PQS, abort
                if (PlanetUI.currentBody.pqsController == null)
                {
                    GUI.Label(new Rect(20, 310, 400, 20), "No PQS found! Is the body a GasPlanet?");
                    return;
                }

                // Render an UI based on the current Mode
                try
                {
                    MethodInfo method = typeof(PQSEditor).GetMethod(mode.ToString().Replace("Modes.", ""), BindingFlags.Public | BindingFlags.Static);
                    method.Invoke(null, null);
                }
                catch
                {
                    Debug.LogWarning("[KittopiaTech]: RenderMethod \"" + mode.ToString().Replace("Modes.", "") + "\" not available!");
                    mode = Modes.List;
                }
            }

            public static void List()
            {
                // Show a list of all PQS mods applied to a body.
                int offset = 280;

                // Get the PQS-Spheres and their mods
                IEnumerable<PQS> pqsList = Utils.FindLocal(PlanetUI.currentName).GetComponentsInChildren<PQS>(true);
                IEnumerable<PQSMod> pqsModList = Utils.FindLocal(PlanetUI.currentName).GetComponentsInChildren<PQSMod>(true);

                // Render the scrollbar
                int scrollSize = ((pqsList.Count() + pqsModList.Count()) * 25) + 90;
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 25));

                // Display every PQS
                foreach (PQS pqs in pqsList)
                {
                    if (GUI.Button(new Rect(20, offset, 350, 20), "" + pqs))
                    {
                        currentPQS = pqs;
                        mode = Modes.PQS;
                    }
                    offset += 25;
                }
                offset += 10;

                // Display every PQSMod
                foreach (PQSMod pqsMod in pqsModList)
                {
                    if (GUI.Button(new Rect(20, offset, 350, 20), pqsMod.name + " (" + pqsMod.GetType().Name + ")"))
                    {
                        currentPQSMod = pqsMod;
                        mode = Modes.PQSMod;
                    } 
                    offset += 25;
                }
                offset += 10;

                // Display the "Add Mod" button
                if (GUI.Button(new Rect(20, offset, 350, 20), "Add new PQSMod"))
                    mode = Modes.AddMod;

                // Finish
                GUI.EndScrollView();
            }
            /*
            bool showLandClassmenu;

            //PQS Modder PT2
            //GUI for modifying a PQSMod.
            private void PQSModderPT2()
            {
                ScrollPosition2 = GUI.BeginScrollView(new Rect(20, 280, 380, 250), ScrollPosition2, new Rect(20, 280, 380, 10000));

                int offset = 280;
                foreach (FieldInfo key in pqsmodtoMod.GetType().GetFields())
                {
                    try
                    {
                        System.Object obj = (System.Object)pqsmodtoMod;
                        if (key.FieldType == typeof(string))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj)));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 200, 20), (bool)key.GetValue(obj), "Bool"));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(int))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (int)StrToFloat(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, StrToFloat(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (double)StrToFloat(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, offset, 100, 20), "" + key.Name);
                            if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                            {
                                Color getColour;
                                getColour = (Color)key.GetValue(obj);
                                rVal = getColour.r;
                                gVal = getColour.g;
                                bVal = getColour.b;
                                aVal = getColour.a;

                                objToEdit = obj;
                                KeyToEdit = key;

                                isshowingColourEditor = true;
                            }
                            //key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , offset, 200, 20), ""+key.GetValue(obj) ) ));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            offset += 30;

                            Vector3 blah = (Vector3)key.GetValue(obj);

                            blah.x = Convert.ToSingle(GUI.TextField(new Rect(20, offset, 50, 20), "" + blah.x));
                            blah.y = Convert.ToSingle(GUI.TextField(new Rect(80, offset, 50, 20), "" + blah.y));
                            blah.z = Convert.ToSingle(GUI.TextField(new Rect(140, offset, 50, 20), "" + blah.z));

                            key.SetValue(obj, blah);

                            offset += 30;
                        }
                        else if (key.FieldType == typeof(PQSLandControl.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "Mod Land Classes"))
                            {
                                LandclassestoMod = (PQSLandControl.LandClass[])key.GetValue(obj);
                                landmodder_mode = 0;
                                landmodder_state = 0;
                                showLandClassmenu = true;
                            }
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "Mod Land Classes"))
                            {
                                VertexLandclassestoMod = (PQSMod_VertexPlanet.LandClass[])key.GetValue(obj);
                                landmodder_mode = 1;
                                landmodder_state = 0;
                                showLandClassmenu = true;
                            }
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(MapSO))
                        {
                            GUI.Label(new Rect(20, offset, 100, 20), "" + key.Name + ":");
                            //					if( GUI.Button( new Rect( 150 , offset, 50, 20), "Edit" ) )
                            //					{
                            //						//TextureBrowser browser = NewWindows[ "TextureBrowser" ] as TextureBrowser;
                            //						TextureBrowser.UpdateTextureList();
                            //					}
                            //					if( GUI.Button( new Rect( 200 , offset, 80, 20), "Save Edit" ) )
                            //					{
                            //						key.SetValue( obj, TextureBrowser.ReturnedMapSo );
                            //					}
                            offset += 30;
                            if (GUI.Button(new Rect(20, offset, 400, 20), "Load texture from: Textures/" + TemplateName + "/PQS/" + pqsmodtoMod.name))
                            {
                                if (Utils.FileExists("GameInfo/KittopiaSpace/Textures/" + TemplateName + "/PQS/" + pqsmodtoMod.name))
                                {
                                    Texture2D texture = Utils.LoadTexture("GameInfo/KittopiaSpace/Textures/" + TemplateName + "/PQS/" + pqsmodtoMod.name);

                                    MapSO ReturnedMapSo = (MapSO)ScriptableObject.CreateInstance(typeof(MapSO));
                                    ReturnedMapSo.CreateMap(MapSO.MapDepth.RGBA, texture);

                                    key.SetValue(obj, ReturnedMapSo);
                                    print("Pushed MAPSO to object!\n");
                                }
                                else
                                {
                                    print("Failed to load: GameInfo/KittopiaSpace/Textures/" + TemplateName + "/PQS/" + pqsmodtoMod.name + ".png\n");
                                }
                            }

                            offset += 30;
                        }
                        else if (key.GetValue(obj).GetType() == typeof(PQS))
                        {
                            //PQS Variable Selector.
                            GUI.Label(new Rect(20, offset, 100, 20), "" + key.Name);
                            if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                            {
                                NewWindows["PQSSelector"].ToggleWindow();
                            }
                            if (GUI.Button(new Rect(200, offset, 80, 20), "Save Edit"))
                            {
                                key.SetValue(obj, PQSSelector.ReturnedPQS);
                            }

                            offset += 30;
                        }
                    }
                    catch { }
                }
                offset += 30;

                //PQS Variable Selector.
                GUI.Label(new Rect(20, offset, 100, 20), "ParentSphere");
                if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                {
                    NewWindows["PQSSelector"].ToggleWindow();
                }
                if (GUI.Button(new Rect(200, offset, 80, 20), "Save Edit"))
                {
                    try
                    {
                        pqsmodtoMod.gameObject.transform.parent = PQSSelector.ReturnedPQS.gameObject.transform;
                    }
                    catch { }
                }

                offset += 30;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild"))
                {
                    pqsmodtoMod.RebuildSphere();
                }

                GUI.EndScrollView();
            }

            //PQS Modder PT3
            //GUI for modifying a PQS class.
            private void PQSModderPT3()
            {
                ScrollPosition2 = GUI.BeginScrollView(new Rect(20, 280, 380, 250), ScrollPosition2, new Rect(20, 280, 380, 10000));

                int offset = 280;
                foreach (FieldInfo key in pqstoMod.GetType().GetFields())
                {
                    try
                    {
                        System.Object obj = (System.Object)pqstoMod;
                        if (key.FieldType == typeof(string))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj)));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 200, 20), (bool)key.GetValue(obj), "Bool"));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(int))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (int)StrToFloat(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, StrToFloat(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (double)StrToFloat(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, offset, 100, 20), "" + key.Name);
                            if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                            {
                                Color getColour;
                                getColour = (Color)key.GetValue(obj);
                                rVal = getColour.r;
                                gVal = getColour.g;
                                bVal = getColour.b;
                                aVal = getColour.a;

                                objToEdit = obj;
                                KeyToEdit = key;

                                isshowingColourEditor = true;
                            }
                            //key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , offset, 200, 20), ""+key.GetValue(obj) ) ));
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                            offset += 30;

                            Vector3 blah = (Vector3)key.GetValue(obj);

                            blah.x = Convert.ToSingle(GUI.TextField(new Rect(20, offset, 50, 20), "" + blah.x));
                            blah.y = Convert.ToSingle(GUI.TextField(new Rect(80, offset, 50, 20), "" + blah.y));
                            blah.z = Convert.ToSingle(GUI.TextField(new Rect(140, offset, 50, 20), "" + blah.z));

                            key.SetValue(obj, blah);

                            offset += 30;
                        }
                        else if (key.FieldType == typeof(PQSLandControl.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "Mod Land Classes"))
                            {
                                LandclassestoMod = (PQSLandControl.LandClass[])key.GetValue(obj);
                                landmodder_mode = 0;
                                landmodder_state = 0;
                                showLandClassmenu = true;
                            }
                            offset += 30;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "Mod Land Classes"))
                            {
                                VertexLandclassestoMod = (PQSMod_VertexPlanet.LandClass[])key.GetValue(obj);
                                landmodder_mode = 1;
                                landmodder_state = 0;
                                showLandClassmenu = true;
                            }
                            offset += 30;
                        }
                    }
                    catch { }
                }
                offset += 30;
                if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild"))
                {
                    pqstoMod.RebuildSphere();
                }

                GUI.EndScrollView();
            }

            //PQS Adder
            //GUI for adding more PQSMods to a body.
            void PQSAdderFunc()
            {
                //Urrgg... hacky at best :/
                Type[] types = Assembly.GetAssembly(typeof(PQSMod)).GetTypes();

                int scrollbaroffsetter = 30;
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(PQSMod)))
                    {
                        scrollbaroffsetter += 30;
                    }
                }

                int offset = 280;
                ScrollPosition2 = GUI.BeginScrollView(new Rect(20, 280, 380, 250), ScrollPosition2, new Rect(20, 280, 380, scrollbaroffsetter));

                //Still hacky, Im not proud.			
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(PQSMod)))
                    {
                        if (GUI.Button(new Rect(20, offset, 200, 20), "" + type.Name))
                        {
                            //Hack^6
                            PQS mainSphere = Utils.FindLocal(TemplateName).GetComponentInChildren<PQS>();
                            PlanetUtils.AddPQSMod(mainSphere, type);

                            pqsModderStage = 0;
                        } if (GUI.Button(new Rect(220, offset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("" + type.Name); }
                        offset += 30;
                    }
                }

                GUI.EndScrollView();
            }*/
        }
    }
}