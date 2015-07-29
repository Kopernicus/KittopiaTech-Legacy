using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a PQS-Browser
        public class PQSBrowser
        {
            // The edited Material
            public static PQS pqs;
            public static FieldInfo field;
            public static PropertyInfo prop;
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
                prop = null;
                UIController.Instance.isPQSBrowser = true;
            }

            // Set edited object
            public static void SetEditedObject(PropertyInfo propertyInfo, object parentObj)
            {
                parent = parentObj;
                prop = propertyInfo;
                field = null;
                UIController.Instance.isPQSBrowser = true;
            }

            // GUI stuff
            private static Vector2 scrollPosition;

            // Fill the returned window
            public static void RenderWindow(int windowID)
            {
                // Render Stuff
                int offset = 40;

                // Get the available PQS-Spheres
                IEnumerable<PQS> spheres = PSystemManager.Instance.localBodies.SelectMany(b => b.GetComponentsInChildren<PQS>(true));

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 200, 300), scrollPosition, new Rect(0, 38, 180, (spheres.Count() * 25) + 80));

                // Render the Properties of the Shader
                foreach (PQS sphere in spheres)
                {
                    if (GUI.Button(new Rect(20, offset, 150, 20), "" + sphere.name))
                        pqs = sphere;
                    offset += 25;
                }
                offset += 20;

                // Apply the selected Sphere
                if (GUI.Button(new Rect(20, offset, 150, 20), "Apply"))
                {
                    if (field != null)
                        field.SetValue(parent, pqs);
                    else
                        prop.SetValue(parent, pqs, null);
                }
                offset += 25;

                // Close the window
                if (GUI.Button(new Rect(20, offset, 150, 20), "Exit"))
                {
                    UIController.Instance.isPQSBrowser = false;
                    pqs = null;
                }

                // Finish
                GUI.EndScrollView();
                GUI.DragWindow();
            }
        }
    }
}
