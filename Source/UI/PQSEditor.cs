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
                    MethodInfo method = typeof(PQSEditor).GetMethod(mode.ToString().Replace("Modes.", ""), BindingFlags.NonPublic | BindingFlags.Static);
                    method.Invoke(null, null);
                }
                catch
                {
                    Debug.LogWarning("[KittopiaTech]: RenderMethod \"" + mode.ToString().Replace("Modes.", "") + "\" not available!");
                    mode = Modes.List;
                }
            }

            // Show a list of all PQS mods applied to a body.
            private static void List()
            {
                // Render Stuff
                int offset = 280;

                // Get the PQS-Spheres and their mods
                IEnumerable<PQS> pqsList = Utils.FindLocal(PlanetUI.currentName).GetComponentsInChildren<PQS>(true);
                IEnumerable<PQSMod> pqsModList = Utils.FindLocal(PlanetUI.currentName).GetComponentsInChildren<PQSMod>(true);

                // Render the scrollbar
                int scrollSize = ((pqsList.Count() + pqsModList.Count()) * 25) + 20;
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

            // GUI for modifying a PQSMod.
            private static void PQSMod()
            {
                // Render Stuff
                int offset = 280;

                // Create the height of the Scroll-List
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3), typeof(PQSLandControl.LandClass[]), typeof(PQSMod_VertexPlanet.LandClass[]), typeof(MapSO), typeof(PQS), typeof(PQSMod_VertexPlanet.SimplexWrapper), typeof(PQSMod_VertexPlanet.NoiseModWrapper) };
                int scrollOffset = currentPQSMod.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollOffset + 40));

                // Display the Fields of the PQSMod
                foreach (FieldInfo key in currentPQSMod.GetType().GetFields())
                {
                    try
                    {
                        System.Object obj = (System.Object)currentPQSMod;
                        if (key.FieldType == typeof(string))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj)));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 170, 20), (bool)key.GetValue(obj), "Bool"));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(int))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Int32.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                ColorPicker.SetEditedObject(key, (Color)key.GetValue(obj), obj);
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);

                            Vector3 value = (Vector3)key.GetValue(obj);

                            value.x = Single.Parse(GUI.TextField(new Rect(200, offset, 50, 20), "" + value.x));
                            value.y = Single.Parse(GUI.TextField(new Rect(260, offset, 50, 20), "" + value.y));
                            value.z = Single.Parse(GUI.TextField(new Rect(320, offset, 50, 20), "" + value.z));

                            key.SetValue(obj, value);

                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSLandControl.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "Mod Land Classes"))
                                LandClassModifier.SetEditedObject((PQSLandControl.LandClass[])key.GetValue(obj));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "Mod Land Classes"))
                                LandClassModifier.SetEditedObject((PQSMod_VertexPlanet.LandClass[])key.GetValue(obj));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_HeightColorMap.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "Mod Land Classes"))
                                LandClassModifier.SetEditedObject((PQSMod_HeightColorMap.LandClass[])key.GetValue(obj));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.SimplexWrapper))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Edit Simplex Wrapper"))
                                SimplexWrapper.SetEditedObject((PQSMod_VertexPlanet.SimplexWrapper)key.GetValue(obj));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.NoiseModWrapper))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Edit NoiseMod Wrapper"))
                                NoiseModWrapper.SetEditedObject((PQSMod_VertexPlanet.NoiseModWrapper)key.GetValue(obj));
                            offset += 25;
                        }
                            /*
                        else if (key.FieldType == typeof(MapSO))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name + ":");
                            offset += 30;
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Load texture"))
                            {
                                FileBrowser browser = new FileBrowser(new Rect(420, 400, 400, 400), "Load MapSO", onFileBrowserSelected);
                            }
                            offset += 25;
                        }
                             * */
                    }
                    catch { }
                }
                offset += 25;

                /*
                //PQS Variable Selector.
                GUI.Label(new Rect(20, offset, 178, 20), "ParentSphere");
                if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                {
                    UIController.Instance.isPQS = true;
                }
                if (GUI.Button(new Rect(200, offset, 80, 20), "Save Edit"))
                {
                    currentPQSMod.transform.parent = PQSSelector.sphere.transform;
                }

                offset += 25;
                 * */

                if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild"))
                {
                    currentPQSMod.RebuildSphere();
                }

                GUI.EndScrollView();
            }
            
            // PQS Adder
            // GUI for adding more PQSMods to a body.
            private static void AddMod()
            {
                // Get all PQSMod-Types
                List<Type> types = Assembly.GetAssembly(typeof(PQSMod)).GetTypes().Where(type => type.IsSubclassOf(typeof(PQSMod))).ToList();
                types.AddRange(AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetTypes()).Where(type => type.IsSubclassOf(typeof(PQSMod))));

                // Create the Height for the Scrollbar
                int scrollOffset = (types.Count() + 1) * 25;

                // Render-Stuff
                int offset = 280;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollOffset));

                // Go through all Types and display them		
                foreach (Type type in types)
                {
                    if (GUI.Button(new Rect(20, offset, 350, 20), "" + type.Name))
                    {
                        // Hack^6
                        GameObject pqsModObject = new GameObject(type.Name);
                        pqsModObject.transform.parent = PlanetUI.currentBody.pqsController.transform;
                        PQSMod mod = pqsModObject.AddComponent(type) as PQSMod;
                        mod.sphere = PlanetUI.currentBody.pqsController;

                        // Revert to the List
                        mode = Modes.List;
                    } 
                    offset += 25;
                }

                // Finish
                GUI.EndScrollView();
            }

            private static void PQS()
            {
                // Render Stuff
                int offset = 280;

                // Create the height of the Scroll-List
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3), typeof(PQSLandControl.LandClass[]), typeof(PQSMod_VertexPlanet.LandClass[]), typeof(MapSO), typeof(PQS) };
                int scrollOffset = currentPQS.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollOffset + 40));

                // Display the Fields of the PQSMod
                foreach (FieldInfo key in currentPQS.GetType().GetFields())
                {
                    try
                    {
                        System.Object obj = (System.Object)currentPQS;
                        if (key.FieldType == typeof(string))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj)));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 170, 20), (bool)key.GetValue(obj), "Bool"));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(int))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Int32.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                ColorPicker.SetEditedObject(key, (Color)key.GetValue(obj), obj);
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);

                            Vector3 value = (Vector3)key.GetValue(obj);

                            value.x = Single.Parse(GUI.TextField(new Rect(200, offset, 50, 20), "" + value.x));
                            value.y = Single.Parse(GUI.TextField(new Rect(260, offset, 50, 20), "" + value.y));
                            value.z = Single.Parse(GUI.TextField(new Rect(320, offset, 50, 20), "" + value.z));

                            key.SetValue(obj, value);

                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSLandControl.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "Mod Land Classes"))
                                LandClassModifier.SetEditedObject((PQSLandControl.LandClass[])key.GetValue(obj));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "Mod Land Classes"))
                                LandClassModifier.SetEditedObject((PQSMod_VertexPlanet.LandClass[])key.GetValue(obj));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_HeightColorMap.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "Mod Land Classes"))
                                LandClassModifier.SetEditedObject((PQSMod_HeightColorMap.LandClass[])key.GetValue(obj));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.SimplexWrapper))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "" + key.Name))
                                SimplexWrapper.SetEditedObject((PQSMod_VertexPlanet.SimplexWrapper)key.GetValue(obj));

                            offset += 25;
                        }
                    }
                    catch { }
                }
                /*
                // PQS Variable Selector.
                GUI.Label(new Rect(20, offset, 178, 20), "Parent Sphere");
                if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                {
                                
                }
                if (GUI.Button(new Rect(200, offset, 80, 20), "Save Edit"))
                {
                                
                }
                 * */
                offset += 25;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild"))
                {
                    currentPQS.RebuildSphere();
                }

                GUI.EndScrollView();
            }
        }
    }
}