using Kopernicus.Configuration;
using Kopernicus.MaterialWrapper;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a Orbit-Editor
        public class StarFix
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Light
            public static LightShifterComponent lsc = null;
            public static EmissiveMultiRampSunspots material = null;

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

                // If we aren't a star, abort
                if (PlanetUI.currentBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length == 0)
                {
                    GUI.Label(new Rect(20, 310, 400, 20), "Selected Planet is not a Star!");
                    return;
                }

                if (lsc == null)
                {
                    material = new EmissiveMultiRampSunspots(PlanetUI.currentBody.scaledBody.renderer.sharedMaterial);
                    lsc = PlanetUI.currentBody.GetComponentsInChildren<LightShifterComponent>(true).First();
                }

                // Create the height of the Scroll-List
                object[] matObjects = Utils.GetInfos<PropertyInfo>(material);
                int scrollSize = Utils.GetScrollSize(matObjects);
                object[] lscObjects = Utils.GetInfos<FieldInfo>(lsc);
                scrollSize += Utils.GetScrollSize(lscObjects);

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 60));

                // Render the Properties of the Shader and the Fields of the LSC
                object matObj = material as System.Object;
                Utils.RenderSelection<PropertyInfo>(matObjects, ref matObj, ref offset);
                object lscObj = lsc as System.Object;
                Utils.RenderSelection<FieldInfo>(lscObjects, ref lscObj, ref offset);
                offset += 20;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Apply"))
                {
                    PlanetUI.currentBody.scaledBody.renderer.sharedMaterial = material;
                    PlanetUI.currentBody.scaledBody.GetComponentsInChildren<StarComponent>().First().SetAsActive();
                }

                GUI.EndScrollView();
            }
        }
    }
}