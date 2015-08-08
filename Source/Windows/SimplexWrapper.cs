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
            public static void SetEditedObject(PQSMod_VertexPlanet.SimplexWrapper swrap)
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
                object[] objects = Utils.GetInfos<FieldInfo>(sWrapper);
                int scrollSize = Utils.GetScrollSize(objects);

                // Render the Scroll-Box
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 38, 380, scrollSize + 50));

                // Render the Selection
                object obj = sWrapper as System.Object;
                Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                offset += 20;

                // Exit
                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                    UIController.Instance.isSimplexWrapper = false;

                // Finish
                GUI.EndScrollView();
                GUI.DragWindow();
            }
        }
    }
}