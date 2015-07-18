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

        public class NoiseModWrapper
        {
            private static PQSMod_VertexPlanet.NoiseModWrapper nWrapper;

            private static Vector2 scrollPosition;

            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(38564, rect, RenderWindow, title);
            }

            public static void SetEditedObject(PQSMod_VertexPlanet.NoiseModWrapper nWrap)
            {
                nWrapper = nWrap;
                UIController.Instance.isNoiseModWrapper = true;
            }

            public static void RenderWindow(int windowID)
            {
                int offset = 30;
                scrollPosition = GUI.BeginScrollView(new Rect(0, 30, 300, 250), scrollPosition, new Rect(0, 0, 400, 10000));

                foreach (FieldInfo key in nWrapper.GetType().GetFields())
                {
                    System.Object obj = (System.Object)nWrapper;
                    if (key.FieldType == typeof(double))
                    {
                        GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                        key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                        offset += 30;
                    }
                }
                offset += 30;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                {
                    UIController.Instance.isNoiseModWrapper = false;
                }

                GUI.EndScrollView();

                GUI.DragWindow();
            }

        }
    }
}
