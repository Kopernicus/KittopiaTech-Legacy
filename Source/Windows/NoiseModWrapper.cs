using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a window to edit NoiseModWrappers
        public class NoiseModWrapper
        {
            // The Wrapper we're editing
            private static PQSMod_VertexPlanet.NoiseModWrapper nWrapper;

            // GUI-Stuff
            private static Vector2 scrollPosition;

            // Return the Window
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(54799, rect, RenderWindow, title);
            }

            // Activate the Window
            public static void SetEditedObject(PQSMod_VertexPlanet.NoiseModWrapper nWrap)
            {
                nWrapper = nWrap;
                UIController.Instance.isSimplexWrapper = false;
                UIController.Instance.isNoiseModWrapper = true;
            }

            // Window-Function
            public static void RenderWindow(int windowID)
            {
                // Render-Stuff
                int offset = 40;

                // Get the height of the scroll list
                object[] objects = Utils.GetInfos<FieldInfo>(nWrapper);
                int scrollSize = Utils.GetScrollSize(objects);

                // Render the Scroll-Box
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 38, 380, scrollSize + 50));

                // Render the Selection
                object obj = nWrapper as System.Object;
                Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                offset += 20;

                // Exit
                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                    UIController.Instance.isNoiseModWrapper = false;

                // Finish
                GUI.EndScrollView();
                GUI.DragWindow();
            }

        }
    }
}
