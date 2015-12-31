using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            public static PropertyInfo prop;
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
                prop = null;
                field = fieldInfo;
                UIController.Instance.isMaterial = true;
            }

            // Set edited object
            public static void SetEditedObject(Material mat, PropertyInfo property, object parentObj)
            {
                material = mat;
                parent = parentObj;
                field = null;
                prop = property;
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
                    IEnumerable<Type> types = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetTypes());
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
                            obj = Activator.CreateInstance(t, material);
                            break;
                        }
                    }
                }

                // Get the height of the scroll list
                object[] objects = Utils.GetInfos<PropertyInfo>(obj);
                int scrollSize = Utils.GetScrollSize(objects);

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, 400, 400), scrollPosition, new Rect(0, 38, 380, scrollSize + 75));

                // Render the Selection
                Utils.RenderSelection<PropertyInfo>(objects, ref obj, ref offset);
                offset += 20;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Apply"))
                {
                    if (field != null)
                        field.SetValue(parent, obj);
                    else
                        prop.SetValue(parent, obj, null);
                    if (parent.GetType() == typeof(PQS))
                        (parent as PQS).RebuildSphere(); // Why?!?!
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