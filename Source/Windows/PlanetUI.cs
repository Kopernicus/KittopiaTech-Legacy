using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a LandClassModder
        public class PlanetUI
        {
            // Window-Modes
            public enum Modes : int
            {
                AtmosphereEditor,
                CelestialBodyEditor,
                PQSEditor,
                PlanetSelector,
                OrbitEditor,
                Ocean,
                StarlightEditor,
                RingEditor,
                ParticlesEditor,
                GroundScatter,
                None
            }

            // Current window mode
            public static Modes mode = Modes.None;

            // Curently edited body
            public static CelestialBody currentBody;

            public static string currentName = "";

            // Return an OnGUI()-Window.
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(954225, rect, RenderWindow, title);
            }

            // Scroll positions
            private static Vector2 scrollPositionTop;

            // ScaledMesh Export
            private static bool exportScaled;

            private static bool exportMaps;

            // Planet-Creation Stuff
            public static Dictionary<string, string> templates = new Dictionary<string, string>();

            // Create the GUI-Elements
            public static void RenderWindow(int windowID)
            {
                scrollPositionTop = GUI.BeginScrollView(new Rect(10, 30, 400, 240), scrollPositionTop, new Rect(0, 0, 380, 370), false, true);

                // Render the Navigation-Menu
                if (GUI.Button(new Rect(20, 10, 200, 20), "Atmosphere Editor"))
                    mode = Modes.AtmosphereEditor;

                if (GUI.Button(new Rect(20, 40, 200, 20), "CB Editor"))
                    mode = Modes.CelestialBodyEditor;

                if (GUI.Button(new Rect(20, 70, 200, 20), "PQS Editor"))
                {
                    mode = Modes.PQSEditor;
                    PQSEditor.mode = PQSEditor.Modes.List;
                }

                if (GUI.Button(new Rect(20, 100, 200, 20), "Planet Selection"))
                    mode = Modes.PlanetSelector;

                GUI.Label(new Rect(20, 130, 200, 20), "Planet Selected: " + currentName);

                if (GUI.Button(new Rect(20, 160, 200, 20), "Orbit Editor"))
                    mode = Modes.OrbitEditor;

                exportScaled = GUI.Toggle(new Rect(240, 178, 300, 20), exportScaled, "Export Mesh?");
                exportMaps = GUI.Toggle(new Rect(240, 202, 300, 20), exportMaps, "Export Textures?");
                if (GUI.Button(new Rect(20, 190, 200, 20), "ScaledSpace updater"))
                {
                    // Stuff
                    GameObject scaled = Utils.FindScaled(currentName);
                    PQS pqs = Utils.FindLocal(currentName).GetComponentsInChildren<PQS>(true).First();

                    // Generate the ScaledSpace-Mesh
                    ScaledPlanetMesh mesh = ScaledPlanetMesh.Generate(pqs, scaled.GetComponentInChildren<MeshFilter>().mesh);
                    mesh.ApplyToScaledSpace(scaled);

                    // Export the Mesh
                    if (exportScaled)
                        Utils.SerializeMesh(Utils.FindScaled(currentName).GetComponentInChildren<MeshFilter>().sharedMesh, KSPUtil.ApplicationRootPath + Body.ScaledSpaceCacheDirectory + "/" + currentName + ".bin");
                    if (exportMaps)
                        UIController.Instance.StartCoroutine(Utils.GeneratePQSMaps(currentBody));
                }

                if (GUI.Button(new Rect(20, 220, 200, 20), "Save Body"))
                    ConfigIO.SaveCelestial(currentBody);

                //if (GUI.Button(new Rect(20, 250, 200, 20), "Ocean Tools"))
                //    mode = Modes.Ocean;

                if (GUI.Button(new Rect(20, 250, 200, 20), "Starlight Editor"))
                    mode = Modes.StarlightEditor;

                if (GUI.Button(new Rect(20, 280, 200, 20), "Ring Editor"))
                    mode = Modes.RingEditor;

                if (GUI.Button(new Rect(20, 310, 200, 20), "HACK: Instantiate " + currentName))
                    Utils.Instantiate(Utils.FindBody(PSystemManager.Instance.systemPrefab.rootBody, currentName), "Test" + new System.Random().Next(1000));

                if (GUI.Button(new Rect(20, 340, 200, 20), "Particles Editor"))
                    mode = Modes.ParticlesEditor;

                //if (GUI.Button(new Rect(20, 400, 200, 20), "Ground Scatter Editor"))
                //    mode = Modes.GroundScatter;

                GUI.EndScrollView();

                // Design Hack
                GUI.HorizontalSlider(new Rect(10, 280, 400, 5), 0.5f, 0, 1, GUI.skin.horizontalSlider, new GUIStyle());

                // Use the current mode to render the editor
                if (mode != Modes.None)
                {
                    //try
                    //{
                    Type modeType = Assembly.GetAssembly(typeof(PlanetUI)).GetTypes().Where(t => t.Name == mode.ToString().Replace("Modes.", "")).FirstOrDefault();
                    MethodInfo method = modeType.GetMethod("Render", BindingFlags.Public | BindingFlags.Static);
                    method.Invoke(null, null);
                    //}
                    //catch
                    //{
                    //    Debug.LogWarning("[KittopiaTech]: RenderClass \"" + mode.ToString().Replace("Modes.", "") + "\" not available!");
                    //    mode = Modes.None;
                    //}
                }

                // Finish the GUI-Drawing
                GUI.DragWindow();
            }
        }
    }
}