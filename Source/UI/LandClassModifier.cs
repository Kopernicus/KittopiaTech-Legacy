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
            private static PQSMod_VertexPlanet.LandClass vertexPlanetClass;

            // LandControl
            private static PQSLandControl.LandClass[] landControlClasses;
            private static PQSLandControl.LandClass landControlClass;

            // HeightColorMap
            private static PQSMod_HeightColorMap.LandClass[] heightColorClasses;
            private static PQSMod_HeightColorMap.LandClass heightColorClass;

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
                return GUI.Window(1, rect, RenderWindow, title);
            }

            // Set edited object
            public static void SetEditedObject (PQSMod_VertexPlanet.LandClass[] landClassArray)
            {
                mode = Mode.VertexPlanet;
                state = State.Select;
                vertexPlanetClasses = landClassArray;
            }

            // GUI stuff
            private static Vector2 scrollPosition;

            public static void RenderWindow(int windowID)
            {
                int offset = 30;
                scrollPosition = GUI.BeginScrollView(new Rect(0, 30, 300, 250), scrollPosition, new Rect(0, 0, 400, 10000));

                if (mode == Mode.LandControl) //PQSLandControl
                {
                    if (state == State.Select)
                    {
                        foreach (PQSLandControl.LandClass landClass in landControlClasses)
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "" + landClass.landClassName))
                            {
                                landControlClass = landClass;
                                state = State.Modify;
                            }
                            offset += 30;
                        }
                    }
                    if (state == State.Modify)
                    {
                        foreach (FieldInfo key in landControlClass.GetType().GetFields())
                        {
                            try
                            {
                                System.Object obj = (System.Object)landControlClass;
                                if (key.FieldType == typeof(string))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj)));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(bool))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 200, 20), (bool)key.GetValue(obj), "Bool"));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(int))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, Int32.Parse(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(float))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, Single.Parse(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(double))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(Color))
                                {
                                    GUI.Label(new Rect(20, offset, 100, 20), "" + key.Name);
                                    if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                                    {
                                        Color getColour;
                                        getColour = (Color)key.GetValue(obj);
                                        ColorPicker.SetEditedObject(key, getColour, obj);
                                        UIController.Instance.isColor = true;
                                    }
                                    //key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
                                    offset += 30;
                                }
                            }
                            catch { }
                        }
                        offset += 30;
                    }
                }
                else if (mode == Mode.VertexPlanet)
                {
                    if (state == State.Select)
                    {
                        foreach (PQSMod_VertexPlanet.LandClass landClass in vertexPlanetClasses)
                        {
                            if (GUI.Button(new Rect(20, offset, 200, 20), "" + landClass.name))
                            {
                                vertexPlanetClass = landClass;
                                state = State.Modify;
                            }
                            offset += 30;
                        }
                    }
                    if (state == State.Modify)
                    {
                        foreach (FieldInfo key in vertexPlanetClass.GetType().GetFields())
                        {
                            try
                            {
                                System.Object obj = (System.Object)vertexPlanetClass;
                                if (key.FieldType == typeof(string))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj)));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(bool))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 200, 20), (bool)key.GetValue(obj), "Bool"));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(int))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, Int32.Parse(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(float))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, Single.Parse(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(double))
                                {
                                    GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);
                                    key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 200, 20), "" + key.GetValue(obj))));
                                    offset += 30;
                                }
                                else if (key.FieldType == typeof(Color))
                                {
                                    GUI.Label(new Rect(20, offset, 100, 20), "" + key.Name);
                                    if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                                    {
                                        Color getColour;
                                        getColour = (Color)key.GetValue(obj);
                                        ColorPicker.SetEditedObject(key, getColour, obj);
                                        UIController.Instance.isColor = true;
                                    }
                                    //key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
                                    offset += 30;
                                }
                            }
                            catch { }
                        }
                        offset += 30;
                    }
                }

                offset += 30;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                {
                    UIController.Instance.isLandClass = false;
                    state = State.Select;
                }
                GUI.EndScrollView();

                GUI.DragWindow();
            }
        }
    }
}
