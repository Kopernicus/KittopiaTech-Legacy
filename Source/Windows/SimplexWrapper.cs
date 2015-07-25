using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {

        public class SimplexWrapper
        {
            // The Wrapper we're editing
            private static PQSMod_VertexPlanet.SimplexWrapper sWrapper;

            // GUI-Stuff
            private static Vector2 scrollPosition;

            // Return the Window
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(7742, rect, RenderWindow, title);
            }

            // Activate the Window
            public static void SetEditedObject (PQSMod_VertexPlanet.SimplexWrapper swrap)
            {
                sWrapper = swrap;
                UIController.Instance.isNoiseModWrapper = false;
                UIController.Instance.isSimplexWrapper = true;
            }

            // Render the Window
            public static void RenderWindow(int windowID)
            {
                // Render-Stuff
                int offset = 40;

                // Get the height of the scroll list
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3), typeof(PQSLandControl.LandClass[]), typeof(PQSMod_VertexPlanet.LandClass[]), typeof(MapSO), typeof(PQS), typeof(PQSMod_VertexPlanet.SimplexWrapper), typeof(PQSMod_VertexPlanet.NoiseModWrapper) };
                int scrollOffset = sWrapper.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;

                // Render the Scroll-Box
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 38, 380, scrollOffset + 50));

                // Loop through all the Fields
                foreach (FieldInfo key in sWrapper.GetType().GetFields())
                {
                    System.Object obj = (System.Object)sWrapper;
                    if (key.FieldType == typeof(double))
                    {
                        GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                        key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                        offset += 25;
                    }
                }

                offset += 20;

                // Exit
                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                {
                    UIController.Instance.isSimplexWrapper = false;
                }

                // Finish
                GUI.EndScrollView();
                GUI.DragWindow();
            }
        }
    }
}
