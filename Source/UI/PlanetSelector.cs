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
        // Class that renders a Planet-Picker
        public class PlanetSelector
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Return an OnGUI()-Window.
            public static void Render()
            {
                if (PSystemManager.Instance.localBodies != null)
                {
                    // Render variables
                    int offset = 280;
                    int trimmedScrollSize = PSystemManager.Instance.localBodies.Count() * 30;

                    // Render the GUI
                    scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, trimmedScrollSize));
                    foreach (CelestialBody body in PSystemManager.Instance.localBodies)
                    {
                        if (GUI.Button(new Rect(20, offset, 200, 20), body.name))
                        {
                            PlanetUI.currentName = body.name;
                            PlanetUI.currentBody = body;
                            OrbitEditor.reference = "";
                        }
                        offset += 30;
                    }
                    GUI.EndScrollView();
                }
            }
        }
    }
}
