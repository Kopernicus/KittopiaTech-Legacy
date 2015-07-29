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
                object[] objects = Utils.GetInfos<FieldInfo>(currentPQSMod);
                int scrollSize = Utils.GetScrollSize(objects);

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 115));

                // Render the Selection
                object obj = currentPQSMod as System.Object;
                Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset, ref currentMapDepth);
                offset += 20;

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
                object[] objects = Utils.GetInfos<FieldInfo>(currentPQS);
                int scrollSize = Utils.GetScrollSize(objects);

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 65));

                // Render the Selection
                object obj = currentPQS as System.Object;
                Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                offset += 20;

                // Rebuild the Sphere
                if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild"))
                    currentPQS.RebuildSphere();

                // Finish
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