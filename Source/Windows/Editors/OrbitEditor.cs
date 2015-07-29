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
        // Class that renders a Orbit-Editor
        public class OrbitEditor
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Orbital stuff
            public static string reference = "";

            // Return an OnGUI()-Window.
            public static void Render()
            {
                // Render variables
                int offset = 280;

                // If we have no Body selected, abort
                if (PlanetUI.currentName == "")
                {
                    GUI.Label(new Rect(20, 310, 400, 20), "No Planet selected!");
                    return;
                }

                // Create the height of the Scroll-List
                object[] objects = Utils.GetInfos<FieldInfo>(PlanetUI.currentBody.orbitDriver.orbit);
                int scrollSize = Utils.GetScrollSize(objects);

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 80));

                // Render the Selection
                object obj = PlanetUI.currentBody.orbitDriver.orbit as System.Object;
                Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                offset += 20;

                // Update the Orbit
                if (GUI.Button(new Rect(20, offset, 178, 20), "Update Orbit"))
                    PlanetUI.currentBody.orbitDriver.UpdateOrbit();

                offset += 25;

                // Hide the Orbit
                if (GUI.Button(new Rect(20, offset, 178, 20), "Deactivate orbit renderer"))
                    PlanetUI.currentBody.orbitDriver.Renderer.drawMode = OrbitRenderer.DrawMode.OFF;

                // Finish
                GUI.EndScrollView();
            }
        }
    }
}