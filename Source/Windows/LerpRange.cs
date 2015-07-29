using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        public class LerpRange
        {
            //What we're editing
            private static PQSLandControl.LerpRange lRange;

            //Return the Window
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(4356, rect, RenderWindow, title);
            }

            // Activate the Window
            public static void SetEditedObject(PQSLandControl.LerpRange lrange)
            {
                lRange = lrange;
                UIController.Instance.isLerpRange = true;
            }

            //Render the Window
            public static void RenderWindow(int windowID)
            {
                // Render-Stuff
                int offset = 40;

                // Loop through all the Fields
                object[] objects = Utils.GetInfos<FieldInfo>(lRange);
                object obj = lRange as System.Object;
                Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                offset += 20;

                // Exit
                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                    UIController.Instance.isLerpRange = false;

                // Finish
                GUI.DragWindow();
            }


        }
    }
}
