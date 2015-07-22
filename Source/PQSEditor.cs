using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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

            // PQSMod Stuff
            private static string pqsModSphereName = "";
            private static int currentMapDepth = 5;

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
                IEnumerable<PQS> pqsList = Utils.FindLocal(PlanetUI.currentBody.transform.name).GetComponentsInChildren<PQS>(true);
                IEnumerable<PQSMod> pqsModList = Utils.FindLocal(PlanetUI.currentBody.transform.name).GetComponentsInChildren<PQSMod>(true);

                // Render the scrollbar
                int scrollSize = ((pqsList.Count() + pqsModList.Count()) * 25) + 20;
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 25));

                // Display every PQS
                foreach (PQS pqs in pqsList)
                {
                    if (GUI.Button(new Rect(20, offset, 350, 20), "" + pqs))
                    {
                        // Select the PQS
                        currentPQS = pqs;

                        // Set the current Sphere-Name
                        if (pqsModSphereName == "" && currentPQS.parentSphere != null)
                            pqsModSphereName = currentPQS.parentSphere.name;
                        else
                            pqsModSphereName = "";

                        // Set the Mode
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
                        // Select the PQSMod
                        currentPQSMod = pqsMod;
                        
                        // Set the current Sphere-Name
                        if (pqsModSphereName == "" && currentPQSMod.sphere != null)
                            pqsModSphereName = currentPQSMod.sphere.name;
                        else
                            pqsModSphereName = "";

                        // Set the Mode
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
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3), typeof(PQSLandControl.LandClass[]), typeof(PQSMod_VertexPlanet.LandClass[]), typeof(PQS), typeof(PQSMod_VertexPlanet.SimplexWrapper), typeof(PQSMod_VertexPlanet.NoiseModWrapper) };
                int scrollOffset = currentPQSMod.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;
                scrollOffset += currentPQSMod.GetType().GetFields().Where(f => f.FieldType == typeof(MapSO)).Count() * 75;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollOffset + 115));

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
                                LandClassModifier.SetEditedObject((PQSLandControl.LandClass[])key.GetValue(obj), key, obj);
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "Mod Land Classes"))
                                LandClassModifier.SetEditedObject((PQSMod_VertexPlanet.LandClass[])key.GetValue(obj), key, obj);
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(PQSMod_HeightColorMap.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, offset, 178, 20), "Mod Land Classes"))
                                LandClassModifier.SetEditedObject((PQSMod_HeightColorMap.LandClass[])key.GetValue(obj), key, obj);
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
                        else if (key.FieldType == typeof(MapSO))
                        {
                            // Stuff
                            if (currentMapDepth == 5 && key.GetValue(obj) != null)
                                currentMapDepth = (int)(key.GetValue(obj) as MapSO).Depth;
                            else if (currentMapDepth == 5 && key.GetValue(obj) == null)
                                currentMapDepth = 0;

                            // Load the MapSO
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name + ":");
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Load"))
                            {
                                UIController.Instance.isFileBrowser = !UIController.Instance.isFileBrowser;
                                FileBrowser.location = "";
                            }
                            offset += 25;
                            currentMapDepth = GUI.SelectionGrid(new Rect(20, offset, 350, 20), currentMapDepth, new string[] { "Greyscale", "HeightAlpha", "RGB", "RGBA" }, 4);
                            offset += 25;
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Apply"))
                            {
                                string path = FileBrowser.location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, "");
                                Texture2D texture = Utility.LoadTexture(path, false, false, false);
                                MapSO mapSO = ScriptableObject.CreateInstance<MapSO>();
                                mapSO.CreateMap((MapSO.MapDepth)currentMapDepth, texture);
                                key.SetValue(obj, mapSO);
                            }
                            offset += 25;
                        }
                    }
                    catch { }
                }
                offset += 25;

                // PQS Variable Selector.
                GUI.Label(new Rect(20, offset, 178, 20), "ParentSphere");
                pqsModSphereName = GUI.TextField(new Rect(200, offset, 170, 20), pqsModSphereName);
                offset += 25;

                // Apply the PQS-Changes
                if (GUI.Button(new Rect(200, offset, 80, 20), "Apply"))
                {
                    // Get the new PQS and reparent the Mod
                    PQS pqs = Utils.FindLocal(pqsModSphereName.Replace("Ocean", "")).GetComponentsInChildren<PQS>(true).FirstOrDefault(s => s.name == pqsModSphereName);
                    currentPQSMod.sphere = pqs;
                    currentPQSMod.transform.parent = pqs.transform;
                }
                offset += 25;

                // Rebuild the PQS-Sphere
                if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild"))
                    currentPQSMod.RebuildSphere();
                offset += 25;

                // Remove the PQSMod
                if (GUI.Button(new Rect(20, offset, 200, 20), "Remove PQSMod"))
                {
                    currentPQSMod.sphere = null;
                    MonoBehaviour.Destroy(currentPQSMod);
                    currentPQSMod = null;
                    
                    // Hack
                    PlanetUI.currentBody.pqsController.SetupExternalRender();
                    PlanetUI.currentBody.pqsController.CloseExternalRender();

                    mode = Modes.List;
                }

                // Finish
                GUI.EndScrollView();
            }

            // GUI for modifing a PQS-Sphere
            private static void PQS()
            {
                // Render Stuff
                int offset = 280;

                // Create the height of the Scroll-List
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3), typeof(PQSLandControl.LandClass[]), typeof(PQSMod_VertexPlanet.LandClass[]), typeof(MapSO), typeof(PQS) };
                int scrollOffset = currentPQS.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollOffset + 65));

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
                        else if (key.GetValue(obj) is Material) // Kopernicus creates Wrappers for the Materials, so key.FieldType == typeof(Material) would return false. :/
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Edit Material"))
                                MaterialEditor.SetEditedObject(key.GetValue(obj) as Material, key, obj);
                            offset += 25;
                        }
                    }
                    catch { }
                }

                // PQS Variable Selector.
                GUI.Label(new Rect(20, offset, 178, 20), "ParentSphere");
                pqsModSphereName = GUI.TextField(new Rect(200, offset, 170, 20), pqsModSphereName);
                offset += 25;

                // Apply the PQS-Changes
                if (GUI.Button(new Rect(200, offset, 80, 20), "Apply"))
                {
                    // Get the new PQS and reparent the Mod
                    PQS pqs = Utils.FindLocal(pqsModSphereName.Replace("Ocean", "")).GetComponentsInChildren<PQS>(true).FirstOrDefault(s => s.name == pqsModSphereName);
                    currentPQS.parentSphere = pqs;
                    currentPQS.transform.parent = pqs.transform;
                }
                offset += 25;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild"))
                {
                    currentPQS.RebuildSphere();
                }

                GUI.EndScrollView();
            }

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

                        if (type.Name == "PQSMod_VoronoiCraters")
                        {
                            CelestialBody mun = Utils.FindCB("Mun");
                            PQSMod_VoronoiCraters craters = mun.GetComponentsInChildren<PQSMod_VoronoiCraters>()[0];

                            PQSMod_VoronoiCraters nc = pqsModObject.GetComponentsInChildren<PQSMod_VoronoiCraters>()[0];
                            nc.craterColourRamp = craters.craterColourRamp;
                            nc.craterCurve = craters.craterCurve;
                            nc.jitterCurve = craters.jitterCurve;
                        }

                        // Revert to the List
                        mode = Modes.List;
                    }
                    offset += 25;
                }

                // Finish
                GUI.EndScrollView();
            }
        }
    }
}