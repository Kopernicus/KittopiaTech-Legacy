using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

            // FieldInfo to apply additions / removals
            private static FieldInfo fieldInfo;

            private static PropertyInfo propertyInfo;
            private static object modObj;

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

            #region SetEditedObject

            // Set edited object
            public static void SetEditedObject(PQSMod_VertexPlanet.LandClass[] landClassArray, FieldInfo field, object modObject)
            {
                mode = Mode.VertexPlanet;
                state = State.Select;
                vertexPlanetClasses = landClassArray;
                fieldInfo = field;
                propertyInfo = null;
                modObj = modObject;
                UIController.Instance.isLandClass = true;
            }

            // Set edited object
            public static void SetEditedObject(PQSLandControl.LandClass[] landClassArray, FieldInfo field, object modObject)
            {
                mode = Mode.LandControl;
                state = State.Select;
                landControlClasses = landClassArray;
                fieldInfo = field;
                propertyInfo = null;
                modObj = modObject;
                UIController.Instance.isLandClass = true;
            }

            // Set edited object
            public static void SetEditedObject(PQSMod_HeightColorMap.LandClass[] landClassArray, FieldInfo field, object modObject)
            {
                mode = Mode.HeightColorMap;
                state = State.Select;
                heightColorClasses = landClassArray;
                fieldInfo = field;
                propertyInfo = null;
                modObj = modObject;
                UIController.Instance.isLandClass = true;
            }

            // Set edited object
            public static void SetEditedObject(PQSMod_VertexPlanet.LandClass[] landClassArray, PropertyInfo prop, object modObject)
            {
                mode = Mode.VertexPlanet;
                state = State.Select;
                vertexPlanetClasses = landClassArray;
                fieldInfo = null;
                propertyInfo = prop;
                modObj = modObject;
                UIController.Instance.isLandClass = true;
            }

            // Set edited object
            public static void SetEditedObject(PQSLandControl.LandClass[] landClassArray, PropertyInfo prop, object modObject)
            {
                mode = Mode.LandControl;
                state = State.Select;
                landControlClasses = landClassArray;
                fieldInfo = null;
                propertyInfo = prop;
                modObj = modObject;
                UIController.Instance.isLandClass = true;
            }

            // Set edited object
            public static void SetEditedObject(PQSMod_HeightColorMap.LandClass[] landClassArray, PropertyInfo prop, object modObject)
            {
                mode = Mode.HeightColorMap;
                state = State.Select;
                heightColorClasses = landClassArray;
                fieldInfo = null;
                propertyInfo = prop;
                modObj = modObject;
                UIController.Instance.isLandClass = true;
            }

            #endregion SetEditedObject

            // GUI stuff
            private static Vector2 scrollPosition;

            // Current Object
            private static object obj;

            public static void RenderWindow(int windowID)
            {
                // Render Stuff
                int offset = 40;

                // If we have to select a LandClass
                if (state == State.Select)
                {
                    // PQSLandControl
                    if (mode == Mode.LandControl)
                    {
                        // Get the size of the Scrollbar
                        int scrollSize = landControlClasses.Count() * 25;

                        // Render the Scrollbar
                        scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 38, 380, scrollSize + 70));

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
                        scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 70));

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
                        scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 70));

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

                    // Add a landclass
                    if (GUI.Button(new Rect(20, offset, 200, 20), "Add LandClass"))
                    {
                        if (mode == Mode.VertexPlanet)
                        {
                            PQSMod_VertexPlanet.LandClass landClass = new PQSMod_VertexPlanet.LandClass("LandClass", 0, 0, new Color(), new Color(), 0);
                            landClass.colorNoiseMap = new PQSMod_VertexPlanet.SimplexWrapper(0, 0, 0, 0);
                            List<PQSMod_VertexPlanet.LandClass> classes = new List<PQSMod_VertexPlanet.LandClass>(vertexPlanetClasses);
                            classes.Add(landClass);
                            vertexPlanetClasses = classes.ToArray();
                            if (fieldInfo != null)
                                fieldInfo.SetValue(modObj, vertexPlanetClasses);
                            else
                                propertyInfo.SetValue(modObj, vertexPlanetClasses, null);
                        }

                        if (mode == Mode.HeightColorMap)
                        {
                            PQSMod_HeightColorMap.LandClass landClass = new PQSMod_HeightColorMap.LandClass("LandClass", 0, 0, new Color(), new Color(), 0);
                            List<PQSMod_HeightColorMap.LandClass> classes = new List<PQSMod_HeightColorMap.LandClass>(heightColorClasses);
                            classes.Add(landClass);
                            heightColorClasses = classes.ToArray();
                            if (fieldInfo != null)
                                fieldInfo.SetValue(modObj, heightColorClasses);
                            else
                                propertyInfo.SetValue(modObj, heightColorClasses, null);
                        }

                        if (mode == Mode.LandControl)
                        {
                            PQSLandControl.LandClass landClass = new PQSLandControl.LandClass();
                            PQSLandControl.LerpRange range;

                            // Initialize default parameters
                            landClass.altDelta = 1;
                            landClass.color = new Color(0, 0, 0, 0);
                            landClass.coverageFrequency = 1;
                            landClass.coverageOctaves = 1;
                            landClass.coveragePersistance = 1;
                            landClass.coverageSeed = 1;
                            landClass.landClassName = "Base";
                            landClass.latDelta = 1;
                            landClass.lonDelta = 1;
                            landClass.noiseColor = new Color(0, 0, 0, 0);
                            landClass.noiseFrequency = 1;
                            landClass.noiseOctaves = 1;
                            landClass.noisePersistance = 1;
                            landClass.noiseSeed = 1;

                            range = new PQSLandControl.LerpRange();
                            range.endEnd = 1;
                            range.endStart = 1;
                            range.startEnd = 0;
                            range.startStart = 0;
                            landClass.altitudeRange = range;

                            range = new PQSLandControl.LerpRange();
                            range.endEnd = 1;
                            range.endStart = 1;
                            range.startEnd = 0;
                            range.startStart = 0;
                            landClass.latitudeRange = range;

                            range = new PQSLandControl.LerpRange();
                            range.endEnd = 1;
                            range.endStart = 1;
                            range.startEnd = 0;
                            range.startStart = 0;
                            landClass.latitudeDoubleRange = range;

                            range = new PQSLandControl.LerpRange();
                            range.endEnd = 2;
                            range.endStart = 2;
                            range.startEnd = -1;
                            range.startStart = -1;
                            landClass.longitudeRange = range;

                            List<PQSLandControl.LandClass> classes = new List<PQSLandControl.LandClass>(landControlClasses);
                            classes.Add(landClass);
                            landControlClasses = classes.ToArray();
                            if (fieldInfo != null)
                                fieldInfo.SetValue(modObj, landControlClasses);
                            else
                                propertyInfo.SetValue(modObj, landControlClasses, null);
                        }
                    }
                    offset += 25;
                }

                // If we modify a LandClass
                if (state == State.Modify)
                {
                    // Reset Offset, just to be sure
                    offset = 35;

                    // Create the height of the Scroll-List
                    object[] objects = Utils.GetInfos<FieldInfo>(obj);
                    int scrollSize = Utils.GetScrollSize(objects);

                    // Render the Scrollbar
                    scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 50));

                    // Render the Selection
                    Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                    offset += 20;

                    // Remove a landclass
                    if (GUI.Button(new Rect(20, offset, 200, 20), "Remove LandClass"))
                    {
                        if (mode == Mode.VertexPlanet)
                        {
                            List<PQSMod_VertexPlanet.LandClass> classes = vertexPlanetClasses.ToList();
                            classes.Remove(obj as PQSMod_VertexPlanet.LandClass);
                            vertexPlanetClasses = classes.ToArray();
                            if (fieldInfo != null)
                                fieldInfo.SetValue(modObj, vertexPlanetClasses);
                            else
                                propertyInfo.SetValue(modObj, vertexPlanetClasses, null);
                        }

                        if (mode == Mode.HeightColorMap)
                        {
                            List<PQSMod_HeightColorMap.LandClass> classes = heightColorClasses.ToList();
                            classes.Remove(obj as PQSMod_HeightColorMap.LandClass);
                            heightColorClasses = classes.ToArray();
                            if (fieldInfo != null)
                                fieldInfo.SetValue(modObj, heightColorClasses);
                            else
                                propertyInfo.SetValue(modObj, heightColorClasses, null);
                        }

                        if (mode == Mode.LandControl)
                        {
                            List<PQSLandControl.LandClass> classes = landControlClasses.ToList();
                            classes.Remove(obj as PQSLandControl.LandClass);
                            landControlClasses = classes.ToArray();
                            if (fieldInfo != null)
                                fieldInfo.SetValue(modObj, landControlClasses);
                            else
                                propertyInfo.SetValue(modObj, landControlClasses, null);
                        }
                        obj = null;
                        state = State.Select;
                    }
                    offset += 25;
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