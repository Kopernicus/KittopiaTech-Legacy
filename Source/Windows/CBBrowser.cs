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
        // Class that renders a Celestial Body Browser
        public class CBBrowser
        {
            // The edited Material
            public static CelestialBody body;
            public static FieldInfo field;
            public static object parent;

            // Return an OnGUI()-Window.
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(936399, rect, RenderWindow, title);
            }

            // Set edited object
            public static void SetEditedObject(FieldInfo fieldInfo, object parentObj)
            {
                parent = parentObj;
                field = fieldInfo;
                UIController.Instance.isCBBrowser = true;
            }

            // GUI stuff
            private static Vector2 scrollPosition;

            public static void RenderWindow(int windowID)
            {
                // Render Stuff
                int offset = 40;

                // Get the available PQS-Spheres
                IEnumerable<CelestialBody> spheres = PSystemManager.Instance.localBodies;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 200, 300), scrollPosition, new Rect(0, 38, 180, (spheres.Count() * 25) + 80));

                // Render the Properties of the Shader
                foreach (CelestialBody sphere in spheres)
                {
                    if (GUI.Button(new Rect(20, offset, 150, 20), "" + sphere.name))
                        body = sphere;
                    offset += 25;
                }
                offset += 20;

                if (GUI.Button(new Rect(20, offset, 150, 20), "Apply"))
                {
                    field.SetValue(parent, body.transform.name);
                }
                offset += 25;

                if (GUI.Button(new Rect(20, offset, 150, 20), "Exit"))
                {
                    UIController.Instance.isCBBrowser = false;
                    body = null;
                }

                // Exit
                GUI.EndScrollView();
                GUI.DragWindow();
            }
        }
    }
}
