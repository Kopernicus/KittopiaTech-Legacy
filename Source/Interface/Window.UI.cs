/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Reflection;
using System.Runtime.InteropServices;
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
            /// Starts scrolling
            /// </summary>
            protected void BeginScrollView(Int32 viewHeight, Int32 maxHeight)
            {
                scrollPosition = GUI.BeginScrollView(new Rect(10, 30, position.width - 20, viewHeight), scrollPosition, new Rect(0, 0, position.width - 40, maxHeight));
            }

            /// <summary>
            /// Stops scrolling
            /// </summary>
            protected void EndScrollView()
            {
                GUI.EndScrollView(true);
            }

            /// <summary>
            /// Renders a button
            /// </summary>
            protected void Button(String label, Action callback, [Optional]Rect? rect)
            {
                // Null checks
                if (callback == null)
                    return;

                // Draw the button
                GUI.enabled = !isError;
                if (GUI.Button(rect ?? new Rect(20, index * distance + 10, 200, 20), label))
                    callback();
                index++;
            }

            /// <summary>
            /// Renders a Button that selects needed parts for the UI
            /// </summary>
            protected void DependencyButton(String label, String label_error, Action callback, Func<Boolean> check, [Optional]Rect? rect, [Optional]Rect? errorRect)
            {
                // Null checks
                if (callback == null || check == null)
                    return;

                // Someone clicked the button
                if (GUI.Button(rect ?? new Rect(20, index * distance + 10, 200, 20), check() ? label : label_error))
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
                    redAlert.font.material.color = Color.red;
                    GUI.Label(errorRect ?? new Rect(240, index * distance + 10, 200, 20), "!", redAlert);
                }
                else
                {
                    isError = false;

                    // Everything is ok
                    GUIStyle greenAlert = new GUIStyle(GUI.skin.label);
                    greenAlert.fontStyle = FontStyle.Bold;
                    greenAlert.font.material.color = Color.green;
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
                GUI.HorizontalSlider(rect ?? new Rect(10, index * distance + 10, 400, height), 0.5f, 0, 1, GUI.skin.horizontalSlider, new GUIStyle());
            }

            /// <summary>
            /// Renders a Textfield and converts it into the given type T
            /// </summary>
            protected void TextField<T>(T defaultValue, Action<T> callback, [Optional]Rect? rect)
            {
                T result = default(T);
                try
                {
                    if (typeof (T) == typeof (Double) || typeof (T) == typeof (Single))
                    {
                        Double value = 0;
                        String cache = GUI.TextField(rect ?? new Rect(20, index * distance + 10, 178, 20), parseCache.Count >= index ? parseCache[index] : defaultValue.ToString());
                        if (Double.TryParse(cache, out value))
                            result = (T) (Object) value;
                        else
                        {
                            parseCache[index] = cache;
                            result = defaultValue;
                        }
                    }
                    if (typeof (T) == typeof (Int16) || typeof (T) == typeof (Int32) || typeof (T) == typeof (Int64))
                    {
                        Int64 value = 0;
                        String cache = GUI.TextField(rect ?? new Rect(20, index * distance + 10, 178, 20), parseCache.Count >= index ? parseCache[index] : defaultValue.ToString());
                        if (Int64.TryParse(cache, out value))
                            result = (T) (Object) value;
                        else
                        {
                            parseCache[index] = cache;
                            result = defaultValue;
                        }
                    }
                    if (typeof (T) == typeof (UInt16) || typeof (T) == typeof (UInt32) || typeof (T) == typeof (UInt64))
                    {
                        UInt64 value = 0;
                        String cache = GUI.TextField(rect ?? new Rect(20, index * distance + 10, 178, 20), parseCache.Count >= index ? parseCache[index] : defaultValue.ToString());
                        if (UInt64.TryParse(cache, out value))
                            result = (T) (Object) value;
                        else
                        {
                            parseCache[index] = cache;
                            result = defaultValue;
                        }
                    }
                    if (typeof (T) == typeof (Boolean))
                        result = (T) (Object) GUI.Toggle(rect ?? new Rect(20, index*distance + 10, 178, 20), (Boolean) (Object) defaultValue, "Bool");
                    if (typeof (T) == typeof (String))
                        result = (T)(Object)GUI.TextField(rect ?? new Rect(20, index * distance + 10, 178, 20), defaultValue.ToString());

                    // Return
                    index++;
                    callback(result);
                }
                catch
                {
                    index++;
                    callback(default(T));
                }
            }
        }
    }
}