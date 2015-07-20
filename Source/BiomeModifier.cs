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
        // Class that renders a BiomeModder
        public class BiomeModifier
        {
            // VertexPlanet
            private static CBAttributeMapSO.MapAttribute[] biomes;

            // FieldInfo to apply additions / removals
            private static FieldInfo fieldInfo;
            private static object modObj;

            // State
            public enum State : int
            {
                Select,
                Modify
            }

            // Store them
            private static State state;

            // Return an OnGUI()-Window.
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(964673, rect, RenderWindow, title);
            }

            // Set edited object
            public static void SetEditedObject(CBAttributeMapSO.MapAttribute[] biomeArray, FieldInfo field, object modObject)
            {
                state = State.Select;
                biomes = biomeArray;
                fieldInfo = field;
                modObj = modObject;
                UIController.Instance.isBiome = true;
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
                    // Get the size of the Scrollbar
                    int scrollSize = biomes.Count() * 25;

                    // Render the Scrollbar
                    scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 80));

                    // Render the LandClasses
                    foreach (CBAttributeMapSO.MapAttribute biome in biomes)
                    {
                        if (GUI.Button(new Rect(20, offset, 200, 20), "" + biome.name))
                        {
                            obj = biome;
                            state = State.Modify;
                        }
                        offset += 25;
                    }

                    offset += 20;

                    // Add a biome
                    if (GUI.Button(new Rect(20, offset, 200, 20), "Add Biome"))
                    {
                        CBAttributeMapSO.MapAttribute biome = new CBAttributeMapSO.MapAttribute();
                        biome.name = "Biome";
                        List<CBAttributeMapSO.MapAttribute> Biomes = new List<CBAttributeMapSO.MapAttribute>(biomes);
                        Biomes.Add(biome);
                        biomes = Biomes.ToArray();
                        fieldInfo.SetValue(modObj, biomes);
                    }
                    offset += 25;
                }


                if (state == State.Modify)
                {
                    // Reset Offset, just to be sure
                    offset = 35;

                    // Modify the Scroll-Size
                    Type[] supportedTypes = new Type[] { typeof(string), typeof(float), typeof(Color) };
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
                            else if (key.FieldType == typeof(float))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                key.SetValue(obj, Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                                offset += 25;
                            }
                            else if (key.FieldType == typeof(Color))
                            {
                                GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                                if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                    ColorPicker.SetEditedObject(key, (Color)key.GetValue(obj), obj);

                                offset += 25;
                            }
                        }
                        catch { }
                    }
                    offset += 20;

                    // Remove a landclass
                    if (GUI.Button(new Rect(20, offset, 200, 20), "Remove Biome"))
                    {
                        List<CBAttributeMapSO.MapAttribute> Biomes = biomes.ToList();
                        Biomes.Remove(obj as CBAttributeMapSO.MapAttribute);
                        biomes = Biomes.ToArray();
                        fieldInfo.SetValue(modObj, biomes);
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
                        UIController.Instance.isBiome = false;
                }

                // Exit
                GUI.EndScrollView();
                GUI.DragWindow();
            }
        }
    }
}
