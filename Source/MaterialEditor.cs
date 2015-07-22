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
        // Class that renders a Material Editor
        public class MaterialEditor
        {
            // The edited Material
            public static Material material;
            public static FieldInfo field;
            public static object parent;

            // Return an OnGUI()-Window.
            public static Rect Render(Rect rect, string title)
            {
                return GUI.Window(86636, rect, RenderWindow, title);
            }

            // Set edited object
            public static void SetEditedObject(Material mat, FieldInfo fieldInfo, object parentObj)
            {
                material = mat;
                parent = parentObj;
                field = fieldInfo;
                UIController.Instance.isMaterial = true;
                
            }

            // GUI stuff
            private static Vector2 scrollPosition;

            // Current Object
            private static object obj;

            public static void RenderWindow(int windowID)
            {
                // Render Stuff
                int offset = 40;

                // Get the Kopernicus-Parser Type
                Type type = null;
                if (obj == null)
                {
                    IEnumerable<Type> types = Assembly.GetAssembly(typeof(Injector)).GetTypes();
                    IEnumerable<Type> materials = types.Where(t => t.BaseType == typeof(Material));
                    foreach (Type t in materials)
                    {
                        // Big Reflection, because of internal class
                        Type singleton = types.First(t2 => t2.FullName == t.FullName + "+Properties");
                        string shaderName = (singleton.GetProperty("shader", BindingFlags.Static | BindingFlags.Public).GetValue(null, null) as Shader).name;

                        // Compare things and break
                        if (shaderName == material.shader.name)
                        {
                            type = t;
                            obj = Activator.CreateInstance(t, new object[] { material });
                            break;
                        }
                    }
                }

                // Get the size of the Scrollbar
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3), typeof(Texture2D) };
                Func<PropertyInfo, bool> predicate = p => supportedTypes.Contains(p.PropertyType) && p.CanRead && p.CanWrite;
                int scrollSize = obj.GetType().GetProperties().Where(predicate).Count() * 25;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 400), scrollPosition, new Rect(0, 38, 380, scrollSize + 75));

                // Render the Properties of the Shader
                foreach (PropertyInfo key in obj.GetType().GetProperties().Where(predicate))
                {
                    try
                    {
                        if (key.PropertyType == typeof(string))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.GetSetMethod().Invoke(obj, new object[] { GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj, null)) });
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 170, 20), (bool)key.GetValue(obj, null), "Bool"), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(int))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Int32.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj, null))), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(float))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj, null))), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(double))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj, null))), null);
                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                ColorPicker.SetEditedObject(key, (Color)key.GetValue(obj, null), obj);

                            offset += 25;
                        }
                        else if (key.PropertyType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);

                            Vector3 value = (Vector3)key.GetValue(obj, null);

                            value.x = Single.Parse(GUI.TextField(new Rect(200, offset, 50, 20), "" + value.x));
                            value.y = Single.Parse(GUI.TextField(new Rect(260, offset, 50, 20), "" + value.y));
                            value.z = Single.Parse(GUI.TextField(new Rect(320, offset, 50, 20), "" + value.z));

                            key.SetValue(obj, value, null);

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
                                key.SetValue(obj, texture, null);
                            }
                            offset += 25;
                        }
                    }
                    catch { }
                }
                offset += 20;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Apply"))
                {
                    field.SetValue(parent, obj);
                    if (parent.GetType() == typeof(PQS))
                    {
                        (parent as PQS).RebuildSphere(); // Why?!?!
                    }
                }
                offset += 25;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Exit"))
                {
                    UIController.Instance.isMaterial = false;
                    obj = null;
                }

                // Exit
                GUI.EndScrollView();
                GUI.DragWindow();
            }
        }
    }
}
