/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Kopernicus.Configuration;
using Kopernicus.UI.Enumerations;
using Kopernicus.UI.Extensions;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Abstraction for window rendering types
        /// </summary>
        public abstract partial class Window<T>
        {
            /// <summary>
            /// Enables the UI element drawn in callback only if check passes
            /// </summary>
            protected void Enabled(Func<Boolean> check, Action callback)
            {
                Boolean e = isError;
                isError = !check();
                callback();
                isError = e;
            }

            /// <summary>
            /// Starts scrolling
            /// </summary>
            protected void BeginScrollView(Int32 viewHeight, Int32 maxHeight, [Optional]Int32? offset)
            {
                scrollPosition = GUI.BeginScrollView(new Rect(10, index * distance + 30 - (offset ?? 0), position.width - 20, viewHeight), scrollPosition, new Rect(0, 0, position.width - 40, maxHeight));
            }

            /// <summary>
            /// Stops scrolling
            /// </summary>
            protected void EndScrollView()
            {
                GUI.EndScrollView();
            }

            /// <summary>
            /// Renders a button
            /// </summary>
            protected void Button(String label, Action callback, [Optional]Rect? rect, [Optional]Single? width)
            {
                // Null checks
                if (callback == null)
                    return;

                // Draw the button
                GUI.enabled = !isError;
                if (GUI.Button(rect ?? new Rect(20, index * distance + 10, width ?? 200, 20), label))
                    callback();
                index++;

            }

            protected void Label(String label, [Optional]Rect? rect)
            {
                // Draw the label
                GUI.enabled = !isError;
                GUI.Label(rect ?? new Rect(20, index * distance + 10, 170, 20), label);
                index++;
            }

            /// <summary>
            /// Renders a Button that selects needed parts for the UI
            /// </summary>
            protected void DependencyButton(String label, String labelError, Action callback, Func<Boolean> check, [Optional]Rect? rect, [Optional]Rect? errorRect)
            {
                // Null checks
                if (callback == null || check == null)
                    return;

                // Someone clicked the button
                if (GUI.Button(rect ?? new Rect(20, index * distance + 10, 200, 20), check() ? label : labelError))
                {
                    // Run the callback
                    callback();
                    
                    // Check the result
                    if (check())
                        isError = false;
                }

                // Disable functionality
                if (!check())
                {
                    isError = true;

                    // Show a warning
                    GUIStyle redAlert = new GUIStyle(GUI.skin.label);
                    redAlert.fontStyle = FontStyle.Bold;
                    redAlert.normal.textColor = Color.red;
                    GUI.Label(errorRect ?? new Rect(240, index * distance + 10, 200, 20), "!", redAlert);
                }
                else
                {
                    isError = false;

                    // Everything is ok
                    GUIStyle greenAlert = new GUIStyle(GUI.skin.label);
                    greenAlert.fontStyle = FontStyle.Bold;
                    greenAlert.normal.textColor = Color.green;
                    GUI.Label(errorRect ?? new Rect(240, index * distance + 10, 200, 20), "✓", greenAlert);
                }

                // Index
                index++;

            }

            /// <summary>
            /// Draws a horizontal line
            /// </summary>
            protected void HorizontalLine(Single height, [Optional]Rect? rect)
            {
                GUI.enabled = !isError;
                GUI.HorizontalSlider(rect ?? new Rect(10, index * distance + 10, 400, height), 0.5f, 0, 1, GUI.skin.horizontalSlider, new GUIStyle());
                index++;
            }

            /// <summary>
            /// Renders a Textfield and converts it into the given type T
            /// </summary>
            protected void TextField<T>(T defaultValue, Action<T> callback, [Optional] Rect? rect, [Optional][DefaultValue("false")]Boolean overrideValue)
            {
                GUI.enabled = !isError;
                T result = default(T);
                if (typeof (T) == typeof (Double) || typeof (T) == typeof (Single))
                {
                    Double value = 0;
                    if (isParseError.ContainsKey(index) && isParseError[index]) GUI.backgroundColor = Color.red;
                    String cache = GUI.TextField(rect ?? new Rect(20, index*distance + 10, 178, 20), parseCache.ContainsKey(index) && !overrideValue ? parseCache[index] : defaultValue.ToString());
                    GUI.backgroundColor = Color.white;
                    if (Double.TryParse(cache, out value))
                    {
                        result = (T) Convert.ChangeType(value, typeof (T));
                        isParseError[index] = false;
                    }
                    else
                    {
                        result = defaultValue;
                        isParseError[index] = true;
                    }
                    parseCache[index] = cache;
                }
                if (typeof (T) == typeof (Int16) || typeof (T) == typeof (Int32) || typeof (T) == typeof (Int64))
                {
                    Int64 value = 0;
                    if (isParseError.ContainsKey(index) && isParseError[index]) GUI.backgroundColor = Color.red;
                    String cache = GUI.TextField(rect ?? new Rect(20, index * distance + 10, 178, 20), parseCache.ContainsKey(index) && !overrideValue ? parseCache[index] : defaultValue.ToString());
                    GUI.backgroundColor = Color.white;
                    if (Int64.TryParse(cache, out value))
                    {
                        result = (T)Convert.ChangeType(value, typeof(T));
                        isParseError[index] = false;
                    }
                    else
                    {
                        result = defaultValue;
                        isParseError[index] = true;
                    }
                    parseCache[index] = cache;
                }
                if (typeof (T) == typeof (UInt16) || typeof (T) == typeof (UInt32) || typeof (T) == typeof (UInt64))
                {
                    UInt64 value = 0;
                    if (isParseError.ContainsKey(index) && isParseError[index]) GUI.backgroundColor = Color.red;
                    String cache = GUI.TextField(rect ?? new Rect(20, index * distance + 10, 178, 20), parseCache.ContainsKey(index) && !overrideValue ? parseCache[index] : defaultValue.ToString());
                    GUI.backgroundColor = Color.white;
                    if (UInt64.TryParse(cache, out value))
                    {
                        result = (T)Convert.ChangeType(value, typeof(T));
                        isParseError[index] = false;
                    }
                    else
                    {
                        result = defaultValue;
                        isParseError[index] = true;
                    }
                    parseCache[index] = cache;
                }
                if (typeof (T) == typeof (Boolean))
                    result = (T) (Object) GUI.Toggle(rect ?? new Rect(20, index*distance + 10, 178, 20), (Boolean) (Object) defaultValue, "Bool");
                if (typeof (T) == typeof (String))
                    result = (T) (Object) GUI.TextField(rect ?? new Rect(20, index*distance + 10, 178, 20), defaultValue.ToString());

                // Return
                index++;
                callback(result);
            }

            /// <summary>
            /// Renders an editor for the object
            /// </summary>
            protected void RenderObject(Object @object)
            {
                // Null check
                if (@object == null)
                    return;

                // Get all parseable MemberInfos
                MemberInfo[] infos = @object.GetType().GetMembers()
                    .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                    .Where(m => !(m as FieldInfo)?.IsLiteral ?? true)
                    .Where(m => m is PropertyInfo ? (m as PropertyInfo).CanRead && (m as PropertyInfo).CanWrite : true)
                    .Where(m => Utils.supportedTypes.Contains(m.GetMemberType()))
                    .ToArray();

                // Loop through all fields and display them
                foreach (MemberInfo info in infos)
                {
                    // Get the type and the value of the member
                    Type FieldType = info.GetMemberType();
                    Object value = info.GetValue(@object);

                    if (FieldType == typeof(String))
                    {
                        Label(info.Name); index--;
                        TextField(value.ToString(), v => info.SetValue(@object, v), new Rect(200, index * distance + 10, 170, 20));
                    }
                    else if (FieldType == typeof(Boolean))
                    {
                        Label(info.Name); index--;
                        TextField((Boolean)value, v => info.SetValue(@object, v), new Rect(200, index * distance + 10, 170, 20));
                    }
                    else if (FieldType == typeof(Int32))
                    {
                        Label(info.Name); index--;
                        TextField((Int32)value, v => info.SetValue(@object, v), new Rect(200, index * distance + 10, 170, 20));
                    }
                    else if (FieldType == typeof(Single))
                    {
                        Label(info.Name); index--;
                        TextField((Single)value, v => info.SetValue(@object, v), new Rect(200, index * distance + 10, 170, 20));
                    }
                    else if (FieldType == typeof(Double))
                    {
                        Label(info.Name); index--;
                        TextField((Double)value, v => info.SetValue(@object, v), new Rect(200, index * distance + 10, 170, 20));
                    }
                    else if (FieldType == typeof(Color))
                    {
                        Label(info.Name); index--;
                        Button("Edit Color", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.Color, (Color) value, c => info.SetValue(@object, c));
                            UIController.Instance.EnableWindow(KittopiaWindows.Color);
                        }, new Rect(200, index * distance + 10, 170, 20));
                    }
                    else if (FieldType == typeof(Vector3))
                    {
                        Label(info.Name); index--;
                        Vector3 value_ = (Vector3) value;
                        TextField(value_.x, f => { value_.x = f; info.SetValue(@object, value_); }, new Rect(200, index * distance + 10, 50, 20)); index--;
                        TextField(value_.y, f => { value_.y = f; info.SetValue(@object, value_); }, new Rect(260, index * distance + 10, 50, 20)); index--;
                        TextField(value_.z, f => { value_.z = f; info.SetValue(@object, value_); }, new Rect(320, index * distance + 10, 50, 20));
                        
                    }
                    else if (FieldType == typeof(Vector3d))
                    {
                        Label(info.Name); index--;
                        Vector3d value_ = (Vector3d)value;
                        TextField(value_.x, f => { value_.x = f; info.SetValue(@object, value_); }, new Rect(200, index * distance + 10, 50, 20)); index--;
                        TextField(value_.y, f => { value_.y = f; info.SetValue(@object, value_); }, new Rect(260, index * distance + 10, 50, 20)); index--;
                        TextField(value_.z, f => { value_.z = f; info.SetValue(@object, value_); }, new Rect(320, index * distance + 10, 50, 20));
                        
                    }
                    else if (FieldType == typeof(Vector2))
                    {
                        Label(info.Name); index--;
                        Vector2 value_ = (Vector2)value;
                        TextField(value_.x, f => { value_.x = f; info.SetValue(@object, value_); }, new Rect(200, index * distance + 10, 50, 20)); index--;
                        TextField(value_.y, f => { value_.y = f; info.SetValue(@object, value_); }, new Rect(285, index * distance + 10, 50, 20));
                        
                    }
                    else if (FieldType == typeof(Vector2d))
                    {
                        Label(info.Name); index--;
                        Vector2d value_ = (Vector2d)value;
                        TextField(value_.x, f => { value_.x = f; info.SetValue(@object, value_); }, new Rect(200, index * distance + 10, 50, 20)); index--;
                        TextField(value_.y, f => { value_.y = f; info.SetValue(@object, value_); }, new Rect(285, index * distance + 10, 50, 20));
                        
                    }
                    else if (FieldType == typeof(CBAttributeMapSO))
                    {
                        // Load the MapSO
                        Label(info.Name); index--;
                        Button("Load CBMap", () =>
                        {
                            FileWindow.type = FieldType;
                            UIController.Instance.SetEditedObject(KittopiaWindows.Files, value == null ? "" : ConfigIO.Format(value as UnityEngine.Object), location =>
                            {
                                if (File.Exists(location))
                                {
                                    String path = location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, "");
                                    Texture2D texture = Utility.LoadTexture(path, false, false, false);
                                    texture.name = path.Replace("\\", "/");
                                    CBAttributeMapSO mapSO = ScriptableObject.CreateInstance<CBAttributeMapSO>();
                                    mapSO.exactSearch = false;
                                    mapSO.nonExactThreshold = 0.05f;
                                    mapSO.CreateMap(MapSO.MapDepth.RGB, texture);
                                    mapSO.Attributes = (value as CBAttributeMapSO).Attributes;
                                    mapSO.name = path.Replace("\\", "/");
                                    info.SetValue(@object, mapSO);
                                }
                                else
                                {
                                    info.SetValue(@object, Resources.FindObjectsOfTypeAll<CBAttributeMapSO>().Where(m => m.name == location));
                                }
                            });
                            UIController.Instance.EnableWindow(KittopiaWindows.Files);
                        }, new Rect(200, index * distance + 10, 170, 20));
                        

                        // Edit the Biome-Definitions
                        Button("Edit Biomes", () => { UIController.Instance.SetEditedObject(KittopiaWindows.Biome, (value as CBAttributeMapSO).Attributes, att => { (value as CBAttributeMapSO).Attributes = att; info.SetValue(@object, value); }); UIController.Instance.EnableWindow(KittopiaWindows.Biome); });
                        
                    }
                    else if (FieldType == typeof(Texture2D) || FieldType == typeof(Texture))
                    {
                        Label(info.Name); index--;
                        Button("Load Texture", () =>
                        {
                            FileWindow.type = FieldType;
                            UIController.Instance.SetEditedObject(KittopiaWindows.Files, value == null ? "" : ConfigIO.Format(value as UnityEngine.Object), location =>
                            {
                                if (File.Exists(location))
                                {
                                    String path = location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, "");
                                    Texture2D texture = Utility.LoadTexture(path, false, false, false);
                                    texture.name = path.Replace("\\", "/");
                                    info.SetValue(@object, texture);
                                }
                                else
                                {
                                    info.SetValue(@object, Resources.FindObjectsOfTypeAll<Texture>().Where(m => m.name == location));
                                }
                            });
                            UIController.Instance.EnableWindow(KittopiaWindows.Files);
                        }, new Rect(200, index * distance + 10, 170, 20));
                        
                    }
                    else if (FieldType == typeof(PQSLandControl.LandClass[]))
                    {
                        Button("Edit LandClasses", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.LandClass, (PQSLandControl.LandClass[]) value, lc => info.SetValue(@object, lc));
                            UIController.Instance.EnableWindow(KittopiaWindows.LandClass);
                        });
                    }
                    else if (FieldType == typeof(PQSMod_VertexPlanet.LandClass[]))
                    {
                        Button("Edit LandClasses", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.LandClass, (PQSMod_VertexPlanet.LandClass[])value, lc => info.SetValue(@object, lc));
                            UIController.Instance.EnableWindow(KittopiaWindows.LandClass);
                        });
                    }
                    else if (FieldType == typeof(PQSMod_HeightColorMap.LandClass[]))
                    {
                        Button("Edit LandClasses", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.LandClass, (PQSMod_HeightColorMap.LandClass[])value, lc => info.SetValue(@object, lc));
                            UIController.Instance.EnableWindow(KittopiaWindows.LandClass);
                        });
                    }
                    else if (FieldType == typeof(PQSLandControl.LerpRange))
                    {
                        Label(info.Name); index--;
                        Button("Edit LerpRange", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.LerpRange, (PQSLandControl.LerpRange)value, lc => info.SetValue(@object, lc));
                            UIController.Instance.EnableWindow(KittopiaWindows.LerpRange);
                        }, new Rect(200, index * distance + 10, 170, 20));
                    }
                    else if (FieldType == typeof(PQSMod_VertexPlanet.SimplexWrapper))
                    {
                        Label(info.Name); index--;
                        Button("Edit Simplex", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.Simplex, (PQSMod_VertexPlanet.SimplexWrapper)value, lc => info.SetValue(@object, lc));
                            UIController.Instance.EnableWindow(KittopiaWindows.Simplex);
                        }, new Rect(200, index * distance + 10, 170, 20));
                    }
                    else if (FieldType == typeof(PQSMod_VertexPlanet.NoiseModWrapper))
                    {
                        Label(info.Name); index--;
                        Button("Edit NoiseMod", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.NoiseMod, (PQSMod_VertexPlanet.NoiseModWrapper)value, lc => info.SetValue(@object, lc));
                            UIController.Instance.EnableWindow(KittopiaWindows.NoiseMod);
                        }, new Rect(200, index * distance + 10, 170, 20));
                    }
                    /*else if (FieldType == typeof(MapSO))
                    {
                        // Stuff
                        if (mapDepth == 5 && key.GetValue(obj) != null)
                            mapDepth = (int)(key.GetValue(obj) as MapSO).Depth;
                        else if (mapDepth == 5 && key.GetValue(obj) == null)
                            mapDepth = 0;

                        // Load the MapSO
                        GUI.Label(new Rect(20, index * distance + 10, 178, 20), "" + info.Name);
                        if (GUI.Button(new Rect(200, index * distance + 10, 80, 20), "Load"))
                        {
                            UIController.Instance.isFileBrowser = !UIController.Instance.isFileBrowser;
                            FileBrowser.type = key.FieldType;
                            FileBrowser.location = "";
                        }

                        // Apply the new MapSO
                        if (GUI.Button(new Rect(290, index * distance + 10, 80, 20), "Apply"))
                        {
                            if (!FileBrowser.builtin)
                            {
                                string path = FileBrowser.location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, "");
                                Texture2D texture = Utility.LoadTexture(path, false, false, false);
                                MapSO mapSO = ScriptableObject.CreateInstance<MapSO>();
                                mapSO.CreateMap((MapSO.MapDepth)mapDepth, texture);
                                mapSO.name = path.Replace("\\", "/");
                                key.SetValue(obj, mapSO);
                            }
                            else
                            {
                                key.SetValue(obj, FileBrowser.value);
                            }
                        }
                        
                        mapDepth = GUI.SelectionGrid(new Rect(20, index * distance + 10, 350, 20), mapDepth, new string[] { "Greyscale", "HeightAlpha", "RGB", "RGBA" }, 4);
                        
                    }*/
                    else if (FieldType == typeof(PQS))
                    {
                        Label(info.Name); index--;
                        Button("Edit Sphere", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.Selector, value as PQS ?? new PQS(), s => info.SetValue(@object, s));
                            UIController.Instance.EnableWindow(KittopiaWindows.Selector);
                        }, new Rect(200, index * distance + 10, 170, 20));
                        
                    }
                    else if (value is Material) // Kopernicus creates Wrappers for the Materials, so key.FieldType == typeof(Material) would return false. :/
                    {
                        Label(info.Name); index--;
                        Button("Edit Material", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.Material, value as Material, m => info.SetValue(@object, m));
                            UIController.Instance.EnableWindow(KittopiaWindows.Material);
                        }, new Rect(200, index * distance + 10, 170, 20));
                        
                    }
                    else if (FieldType == typeof(FloatCurve))
                    {
                        Label(info.Name); index--;
                        Button("Edit Curve", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.Curve, (FloatCurve)value, lc => info.SetValue(@object, lc));
                            UIController.Instance.EnableWindow(KittopiaWindows.Curve);
                        }, new Rect(200, index * distance + 10, 170, 20));

                    }
                    else if (FieldType == typeof(AnimationCurve))
                    {
                        Label(info.Name); index--;
                        Button("Edit Curve", () => {
                            UIController.Instance.SetEditedObject(KittopiaWindows.Curve, new FloatCurve(((AnimationCurve) value).keys), lc => info.SetValue(@object, lc.Curve));
                            UIController.Instance.EnableWindow(KittopiaWindows.Curve);
                        }, new Rect(200, index * distance + 10, 170, 20));

                    }
                    else if (FieldType == typeof(Mesh))
                    {
                        Label(info.Name); index--;
                        Button("Load Mesh", () =>
                        {
                            FileWindow.type = FieldType;
                            UIController.Instance.SetEditedObject(KittopiaWindows.Files, value == null ? "" : ConfigIO.Format(value as UnityEngine.Object), location =>
                            {
                                if (File.Exists(location))
                                {
                                    String path = location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, "");
                                    MeshParser parser = new MeshParser(value as Mesh);
                                    parser.SetFromString(path);
                                    parser.value.name = path.Replace("\\", "/");
                                    info.SetValue(@object, parser.value);
                                }
                                else
                                {
                                    info.SetValue(@object, Resources.FindObjectsOfTypeAll<Mesh>().Where(m => m.name == location));
                                }
                            });
                            UIController.Instance.EnableWindow(KittopiaWindows.Files);
                        }, new Rect(200, index * distance + 10, 170, 20));
                    }
                }
            }
        
        }
    }
}