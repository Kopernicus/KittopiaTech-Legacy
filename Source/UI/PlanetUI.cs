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
        // Class that renders a LandClassModder
        public class PlanetUI
        {
            // Window-Modes
            public enum Modes : int
            {
                AtmosphereSFX,
                CelestialBody,
                PQS,
                PlanetSelector,
                Orbit,
                Ocean,
                StarFix,
                Rings,
                Particles,
                GroundScatter
            }

            // Current window mode
            public static Modes mode;

            // Curently edited body
            public static CelestialBody curentBody;
            public static string currentName = "";

            // Return an OnGUI()-Window.
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(1, rect, RenderWindow, title);
            }

            // Scroll positions
            private static Vector2 scrollPositionTop;

            // ScaledMesh Export
            private static bool exportScaled;

            // Create the GUI-Elements
            public static void RenderWindow(int windowID)
            {
                scrollPositionTop = GUI.BeginScrollView(new Rect(10, 30, 400, 240), scrollPositionTop, new Rect(0, 0, 380, 410), false, true);

                // Render the Navigation-Menu
                if (GUI.Button(new Rect(20, 10, 200, 20), "Atmosphere SFX tools"))
                    mode = Modes.AtmosphereSFX;

                if (GUI.Button(new Rect(20, 40, 200, 20), "CB Editor"))
                    mode = Modes.CelestialBody;

                if (GUI.Button(new Rect(20, 70, 200, 20), "PQS Editor"))
                    mode = Modes.PQS;

                if (GUI.Button(new Rect(20, 100, 200, 20), "Planet Selection"))
                    mode = Modes.PlanetSelector;

                GUI.Label(new Rect(20, 130, 200, 20), "Planet Selected: " + currentName);

                if (GUI.Button(new Rect(20, 160, 200, 20), "Orbit Editor"))
                    mode = Modes.Orbit;

                exportScaled = GUI.Toggle(new Rect(240, 190, 300, 20), exportScaled, "Export?");
                if (GUI.Button(new Rect(20, 190, 200, 20), "ScaledSpace updater"))
                {
                    Utility.UpdateScaledMesh(curentBody.scaledBody, curentBody.pqsController, curentBody, Body.ScaledSpaceCacheDirectory, "", exportScaled, false);
                }

                if (GUI.Button(new Rect(20, 220, 200, 20), "Ocean Tools"))
                    mode = Modes.Ocean;

                if (GUI.Button(new Rect(20, 250, 200, 20), "Modify Starlight data"))
                    mode = Modes.StarFix;

                if (GUI.Button(new Rect(20, 280, 200, 20), "Ring tools"))
                    mode = Modes.Rings;

                if (GUI.Button(new Rect(20, 310, 200, 20), "HACK: Instantiate " + currentName))
                    ScreenMessages.PostScreenMessage("Instantiation Tools deactivated!", 5f, ScreenMessageStyle.UPPER_CENTER);

                if (GUI.Button(new Rect(20, 340, 200, 20), "Planetary Particles"))
                    mode = Modes.Particles;

                if (GUI.Button(new Rect(20, 370, 200, 20), "Ground Scatter Editor"))
                    mode = Modes.GroundScatter;

                GUI.EndScrollView();

                // Design Hack
                GUI.HorizontalSlider(new Rect(10, 280, 400, 5), 0.5f, 0, 1, GUI.skin.horizontalSlider, new GUIStyle());

                GUI.DragWindow();
            }
        }
    }
}
