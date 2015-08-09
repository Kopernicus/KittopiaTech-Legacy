using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a CurveWindow
        public class CurveWindow
        {
            // The curve we're editing
            private static FloatCurve curve;
            private static List<Keyframe> frames = new List<Keyframe>();

            // FieldInfo to apply changes
            private static FieldInfo fieldInfo;
            private static PropertyInfo propertyInfo;
            private static object parent;

            // FloatCurve or AnimationCurve?
            private static bool isAnim = false;

            // Return an OnGUI()-Window.
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(86636, rect, RenderWindow, title);
            }

            // Set edited object
            public static void SetEditedObject(FloatCurve floatCurve, FieldInfo field, object curveObject, bool anim)
            {
                curve = floatCurve;
                frames = curve.Curve.keys.ToList();
                fieldInfo = field;
                propertyInfo = null;
                parent = curveObject;
                isAnim = anim;
                UIController.Instance.isCurveWindow = true;
            }

            // Set edited object
            public static void SetEditedObject(FloatCurve floatCurve, PropertyInfo prop, object curveObject, bool anim)
            {
                curve = floatCurve;
                fieldInfo = null;
                propertyInfo = prop;
                parent = curveObject;
                isAnim = anim;
                UIController.Instance.isCurveWindow = true;
            }

            // GUI stuff
            private static Vector2 scrollPosition;

            public static void RenderWindow(int windowID)
            {
                // Render Stuff
                int offset = 40;

                // Create the height of the Scroll-List
                int scrollSize = frames.Count * 25;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 80));

                // Render the Selection
                for (int i = 0; i < frames.Count; i++)
                {
                    // Edit
                    float time = Single.Parse(GUI.TextField(new Rect(20, offset, 60, 20), "" + frames[i].time));
                    float value = Single.Parse(GUI.TextField(new Rect(90, offset, 60, 20), "" + frames[i].value));
                    float inTangent = Single.Parse(GUI.TextField(new Rect(160, offset, 60, 20), "" + frames[i].inTangent));
                    float outTangent = Single.Parse(GUI.TextField(new Rect(230, offset, 60, 20), "" + frames[i].outTangent));

                    frames[i] = new Keyframe(time, value, inTangent, outTangent);
                    
                    // Add
                    if (GUI.Button(new Rect(300, offset, 30, 20), "+"))
                    {
                        frames.Insert(i + 1, new Keyframe());
                        break;
                    }

                    // Remove
                    if (GUI.Button(new Rect(340, offset, 30, 20), "X"))
                    {
                        frames.RemoveAt(i);
                        break;
                    }

                    // offset
                    offset += 25;
                }
                offset += 20;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Apply"))
                {
                    curve.Curve.keys = frames.ToArray();
                    if (fieldInfo != null)
                        fieldInfo.SetValue(parent, (!isAnim ? (object)curve : curve.Curve));
                    else
                        propertyInfo.SetValue(parent, (!isAnim ? (object)curve : curve.Curve), null);
                }
                offset += 25;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                {
                    frames = null;
                    UIController.Instance.isCurveWindow = false;
                }

                // Exit
                GUI.EndScrollView();
                GUI.DragWindow();

            }
        }
    }
}