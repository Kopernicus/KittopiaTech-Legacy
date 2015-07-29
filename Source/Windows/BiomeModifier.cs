using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a BiomeModder
        public class BiomeModifier
        {
            // Biomes
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

                    // Render the Biomes
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

                    // Create the height of the Scroll-List
                    object[] objects = Utils.GetInfos<FieldInfo>(obj);
                    int scrollSize = Utils.GetScrollSize(objects);

                    // Render the Scrollbar
                    scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 200), scrollPosition, new Rect(0, 28, 380, scrollSize + 50));

                    // Render the Selection
                    Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                    offset += 20;

                    // Remove a Biome
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
