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
        // Class that renders a Color Picker
        public class ColorPicker
        {
            private static Color color;
            private static FieldInfo field;
            private static object fieldObject;
            public static int index;

            // Return an OnGUI()-Window.
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(1, rect, RenderWindow, title);
            }

            // Set edited object
            public static void SetEditedObject (FieldInfo fieldInfo, Color colorObject, object obj)
            {
                field = fieldInfo;
                color = colorObject;
                fieldObject = obj;
            }

            public static void RenderWindow(int windowID)
            {
                GUIStyle colorStyle = new GUIStyle();
                Texture2D blankTexture = new Texture2D(1, 1);
                blankTexture.wrapMode = TextureWrapMode.Repeat;

                color.r = GUI.HorizontalSlider(new Rect(10, 30, 190, 20), color.r, 0, 1);
                color.r = Single.Parse(GUI.TextField(new Rect(200, 30, 100, 20), "" + color.r));

                color.g = GUI.HorizontalSlider(new Rect(10, 60, 190, 20), color.g, 0, 1);
                color.g = Single.Parse(GUI.TextField(new Rect(200, 60, 100, 20), "" + color.g));

                color.b = GUI.HorizontalSlider(new Rect(10, 90, 190, 20), color.b, 0, 1);
                color.b = Single.Parse(GUI.TextField(new Rect(200, 90, 100, 20), "" + color.b));

                color.a = GUI.HorizontalSlider(new Rect(10, 120, 190, 20), color.a, 0, 1);
                color.a = Single.Parse(GUI.TextField(new Rect(200, 120, 100, 20), "" + color.a));

                GUI.color = color;

                blankTexture.SetPixel(0, 0, color);
                blankTexture.Apply();

                colorStyle.normal.background = blankTexture;

                GUI.Box(new Rect(210, 150, 240, 100), blankTexture, colorStyle);

                blankTexture.SetPixel(0, 0, new Color((float)Math.Abs(color.r - 1.0), (float)Math.Abs(color.g - 1.0), (float)Math.Abs(color.b - 1.0), 1.0f));
                blankTexture.Apply();

                colorStyle.normal.background = blankTexture;

                GUI.Box(new Rect(300, 150, 240, 100), blankTexture, colorStyle);
                //GUI.
                GUI.color = Color.white;

                if (GUI.Button(new Rect(10, 150, 200, 50), "Save"))
                {
                    //Hacky mcHack
                    if (field.GetValue(fieldObject) is Array)
                    {
                        Color[] colors = field.GetValue(fieldObject) as Color[];
                        colors[index] = color;
                        field.SetValue(fieldObject, colors);
                    }
                    else
                    {
                        field.SetValue(fieldObject, color);
                    }
                    UIController.Instance.isColor = false;
                }

                GUI.DragWindow();
            }
        }
    }
}
