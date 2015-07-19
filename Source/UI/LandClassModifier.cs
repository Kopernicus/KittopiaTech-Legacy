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
        // Class that renders a LandClassModder
        public class LandClassModifier
        {
            // VertexPlanet
            private static PQSMod_VertexPlanet.LandClass[] vertexPlanetClasses;

            // LandControl
            private static PQSLandControl.LandClass[] landControlClasses;

            // HeightColorMap
            private static PQSMod_HeightColorMap.LandClass[] heightColorClasses;

            // Mode
            public enum Mode : int
            {
                VertexPlanet,
                LandControl,
                HeightColorMap
            }

            // State
            public enum State : int
            {
                Select,
                Modify
            }

            // Store them
            private static Mode mode;
            private static State state;

            // Return an OnGUI()-Window.
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(86636, rect, RenderWindow, title);
            }

            // Set edited object
            public static void SetEditedObject (PQSMod_VertexPlanet.LandClass[] landClassArray)
            {
                mode = Mode.VertexPlanet;
                state = State.Select;
                vertexPlanetClasses = landClassArray;
                UIController.Instance.isLandClass = true;
                
            }

            // Set edited object
            public static void SetEditedObject(PQSLandControl.LandClass[] landClassArray)
            {
                mode = Mode.LandControl;
                state = State.Select;
                landControlClasses = landClassArray;
                UIController.Instance.isLandClass = true;
            }
            public static void SetEditedObject(PQSMod_HeightColorMap.LandClass[] landClassArray)
            {
                mode = Mode.HeightColorMap;
                state = State.Select;
                heightColorClasses = landClassArray;
                UIController.Instance.isLandClass = true;
            }

            // GUI stuff
            private static Vector2 scrollPosition;

            // Current Object
            private static object obj;

            public static void RenderWindow(int windowID)
            {
                // Render Stuff
                int offset = 40;

                if (state == State.Select)
                {
                    // PQSLandControl
                    if (mode == Mode.LandControl)
                    {
                        // Get the size of the Scrollbar
                        int scrollSize = landControlClasses.Count() * 25;

                        // Render the Scrollbar
                        scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 38, 380, scrollSize + 50));

                        // Render the LandClasses
                        foreach (PQSLandControl.LandClass landClass in landControlClasses)
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "" + landClass.landClassName))
                            {
                                obj = landClass;
                                state = State.Modify;
                            }
                            offset += 25;
                        }
                    }

                    // PQSMod_VertexPlanet
                    if (mode == Mode.VertexPlanet)
                    {
                        // Get the size of the Scrollbar
                        int scrollSize = vertexPlanetClasses.Count() * 25;

                        // Render the Scrollbar
                        scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 50));

                        // Render the LandClasses
                        foreach (PQSMod_VertexPlanet.LandClass landClass in vertexPlanetClasses)
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "" + landClass.name))
                            {
                                obj = landClass;
                                state = State.Modify;
                            }
                            offset += 25;
                        }
                    }

                    // PQSMod_HeightColorMap
                    if (mode == Mode.HeightColorMap)
                    {
                        // Get the size of the Scrollbar
                        int scrollSize = heightColorClasses.Count() * 25;

                        // Render the Scrollbar
                        scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 50));

                        // Render the LandClasses
                        foreach (PQSMod_HeightColorMap.LandClass landClass in heightColorClasses)
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "" + landClass.name))
                            {
                                obj = landClass;
                                state = State.Modify;
                            }
                            offset += 25;
                        }
                    }
                    offset += 20;
                }


                if (state == State.Modify)
                {
                    // Reset Offset, just to be sure
                    offset = 35;

                    // Modify the Scroll-Size
                    Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3), typeof(PQSLandControl.LandClass[]), typeof(PQSMod_VertexPlanet.LandClass[]), typeof(MapSO), typeof(PQS), typeof(PQSMod_VertexPlanet.SimplexWrapper), typeof(PQSMod_VertexPlanet.NoiseModWrapper), typeof(PQSLandControl.LerpRange) };
                    int scrollSize = obj.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;

                    // Render the Scrollbar
                    scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 50));

                    // Render the Fields
                    foreach (FieldInfo key in obj.GetType().GetFields())
                    {
                        try
                        {
                            if (key.FieldType == typeof(string))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                key.SetValue(obj, GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj)));
                                offset += 25;
                            }
                            else if (key.FieldType == typeof(bool))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 170, 20), (bool)key.GetValue(obj), "Bool"));
                                offset += 25;
                            }
                            else if (key.FieldType == typeof(int))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                key.SetValue(obj, Int32.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                                offset += 25;
                            }
                            else if (key.FieldType == typeof(float))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                key.SetValue(obj, Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                                offset += 25;
                            }
                            else if (key.FieldType == typeof(double))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                                offset += 25;
                            }
                            else if (key.FieldType == typeof(Color))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                    ColorPicker.SetEditedObject(key, (Color)key.GetValue(obj), obj);

                                offset += 25;
                            }
                            else if (key.FieldType == typeof(PQSMod_VertexPlanet.SimplexWrapper))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                    SimplexWrapper.SetEditedObject((PQSMod_VertexPlanet.SimplexWrapper)key.GetValue(obj));
                                offset += 25;
                            }
                            else if (key.FieldType == typeof(PQSLandControl.LerpRange))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                    LerpRange.SetEditedObject((PQSLandControl.LerpRange)key.GetValue(obj));

                                offset += 25;
                            }
                        }
                        catch { }
                    }
                    offset += 20;
                }

                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                {
                    if (state == State.Modify)
                        state = State.Select;
                    else
                        UIController.Instance.isLandClass = false;
                }

                // Exit
                GUI.EndScrollView();
                GUI.DragWindow();
            }
        }
    }
}
