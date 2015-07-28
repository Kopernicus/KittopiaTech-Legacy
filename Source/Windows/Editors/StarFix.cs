using Kopernicus.Configuration;
using Kopernicus.MaterialWrapper;
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
        // Class that renders a Orbit-Editor
        public class StarFix
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Light
            public static LightShifterComponent lsc = null;
            public static EmissiveMultiRampSunspots material = null;

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

                // If we aren't a star, abort
                if (PlanetUI.currentBody.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length == 0)
                {
                    GUI.Label(new Rect(20, 310, 400, 20), "Selected Planet is not a Star!");
                    return;
                }

                if (lsc == null)
                {
                    material = new EmissiveMultiRampSunspots(PlanetUI.currentBody.scaledBody.renderer.sharedMaterial);
                    lsc = PlanetUI.currentBody.GetComponentsInChildren<LightShifterComponent>(true).First();
                }

                // Get the size of the Scrollbar
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3), typeof(Texture2D) };
                Func<PropertyInfo, bool> predicate = p => supportedTypes.Contains(p.PropertyType) && p.CanRead && p.CanWrite;
                int scrollSize = material.GetType().GetProperties().Where(predicate).Count() * 25;
                scrollSize += lsc.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 60));

                // Render the Properties of the Shader
                foreach (PropertyInfo key in material.GetType().GetProperties().Where(predicate))
                {
                    try
                    {
                        if (key.PropertyType == typeof(string))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(material, GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(material, null)), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(material, GUI.Toggle(new Rect(200, offset, 170, 20), (bool)key.GetValue(material, null), "Bool"), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(int))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(material, Int32.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(material, null))), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(float))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(material, Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(material, null))), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(double))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(material, Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(material, null))), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                ColorPicker.SetEditedObject(key, (Color)key.GetValue(material, null), material);

                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);

                            Vector3 value = (Vector3)key.GetValue(material, null);

                            value.x = Single.Parse(GUI.TextField(new Rect(200, offset, 50, 20), "" + value.x));
                            value.y = Single.Parse(GUI.TextField(new Rect(260, offset, 50, 20), "" + value.y));
                            value.z = Single.Parse(GUI.TextField(new Rect(320, offset, 50, 20), "" + value.z));

                            key.SetValue(material, value, null);

                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(Texture2D))
                        {
                            // Load the Texture
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name + ":");
                            if (GUI.Button(new Rect(200, offset, 80, 20), "Load"))
                            {
                                UIController.Instance.isFileBrowser = !UIController.Instance.isFileBrowser;
                                FileBrowser.location = "";
                            }

                            // Apply the new Texture
                            if (GUI.Button(new Rect(290, offset, 80, 20), "Apply"))
                            {
                                string path = FileBrowser.location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, "");
                                Texture2D texture = Utility.LoadTexture(path, false, false, false);
                                texture.name = path.Replace("\\", "/");
                                key.SetValue(material, texture, null);
                            }
                            offset += 25;
                        }
                    }
                    catch { }
                }

                // Render the LightShifterComponent
                foreach (FieldInfo key in lsc.GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)))
                {
                    try
                    {
                        if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(lsc, GUI.Toggle(new Rect(200, offset, 170, 20), (bool)key.GetValue(lsc), "Bool"));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(lsc, Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(lsc))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(lsc, Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(lsc))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                ColorPicker.SetEditedObject(key, (Color)key.GetValue(lsc), lsc);
                            offset += 25;
                        }
                    }
                    catch { }
                }
                offset += 20;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Apply"))
                {
                    PlanetUI.currentBody.scaledBody.renderer.sharedMaterial = material;
                    PlanetUI.currentBody.scaledBody.GetComponentsInChildren<StarComponent>().First().SetAsActive();
                }

                GUI.EndScrollView();
            }
        }
    }
}