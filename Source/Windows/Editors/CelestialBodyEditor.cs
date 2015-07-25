using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a CelestialBody-Editor
        public class CelestialBodyEditor
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Return an OnGUI()-Window.
            public static void Render()
            {
                // Render variables
                int offset = 280;

                // If we have no Body selected, abort
                if (PlanetUI.currentName == "")
                {
                    GUI.Label(new Rect(20, 310, 400, 20), "No Planet selected!");
                    return;
                }

                // Create the height of the Scroll-List
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3) };
                int scrollOffset = PlanetUI.currentBody.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;
                scrollOffset += PlanetUI.currentBody.GetType().GetFields().Where(f => f.FieldType == typeof(CBAttributeMapSO)).Count() * 75;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollOffset + 50));

                // Loop through all fields and display them
                foreach (FieldInfo key in PlanetUI.currentBody.GetType().GetFields())
                {
                    try
                    {
                        object obj = PlanetUI.currentBody;
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
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);

                            Vector3 value = (Vector3)key.GetValue(obj);

                            value.x = Single.Parse(GUI.TextField(new Rect(200, offset, 50, 20), "" + value.x));
                            value.y = Single.Parse(GUI.TextField(new Rect(260, offset, 50, 20), "" + value.y));
                            value.z = Single.Parse(GUI.TextField(new Rect(320, offset, 50, 20), "" + value.z));

                            key.SetValue(obj, value);

                            offset += 25;
                        }
                        else if (key.FieldType == typeof(CBAttributeMapSO))
                        {
                            // Load the MapSO
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name + ":");
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Load"))
                            {
                                UIController.Instance.isFileBrowser = !UIController.Instance.isFileBrowser;
                                FileBrowser.location = "";
                            }
                            offset += 25;

                            // Edit the Biome-Definitions
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Mod Biomes"))
                            {
                                FieldInfo attributes = typeof(CBAttributeMapSO).GetField("Attributes");
                                object att = attributes.GetValue(key.GetValue(obj));
                                BiomeModifier.SetEditedObject(att as CBAttributeMapSO.MapAttribute[], attributes, key.GetValue(obj));
                            }    
                            offset += 25;

                            // Apply the new MapSO
                            if (GUI.Button(new Rect(200, offset, 170, 20), "Apply"))
                            {
                                string path = FileBrowser.location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, "");
                                Texture2D texture = Utility.LoadTexture(path, false, false, false);
                                texture.name = path.Replace("\\", "/");
                                CBAttributeMapSO mapSO = ScriptableObject.CreateInstance<CBAttributeMapSO>();
                                mapSO.exactSearch = false;
                                mapSO.nonExactThreshold = 0.05f;
                                mapSO.CreateMap(MapSO.MapDepth.RGB, texture);
                                mapSO.Attributes = (key.GetValue(obj) as CBAttributeMapSO).Attributes;
                                key.SetValue(obj, mapSO);
                            }
                            offset += 25;
                        }
                    }
                    catch { }
                }
                offset += 20;
                if (GUI.Button(new Rect(20, offset, 200, 20), "Update"))
                    PlanetUI.currentBody.CBUpdate();

                GUI.EndScrollView();
            }
        }
    }
}
