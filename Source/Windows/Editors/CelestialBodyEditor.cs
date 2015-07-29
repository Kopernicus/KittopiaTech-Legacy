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
        // Class that renders a CelestialBody-Editor
        public class CelestialBodyEditor
        {
            // GUI stuff
            private static Vector2 scrollPosition;

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
                object[] objects = Utils.GetInfos<FieldInfo>(PlanetUI.currentBody);
                int scrollSize = Utils.GetScrollSize(objects);

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 50));

                // Render the Selection
                object obj = PlanetUI.currentBody as System.Object;
                Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                offset += 20;

                // Update the CelestialBody
                if (GUI.Button(new Rect(20, offset, 200, 20), "Update"))
                    PlanetUI.currentBody.CBUpdate();

                // Finish
                GUI.EndScrollView();
            }
        }
    }
}
