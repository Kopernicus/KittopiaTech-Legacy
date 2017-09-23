using System;
using System.Collections.Generic;
using Kopernicus.UI.Enumerations;
using UniLinq;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    namespace UI
    {
        public class Controller<T> : MonoBehaviour
        {
            // A list for all Windows
            public Dictionary<T, HashSet<IWindow>> Windows { get; set; }

            // The window states
            public Dictionary<T, Boolean> WindowStates { get; set; }

            // The cached positons
            public Dictionary<IWindow, Rect> RectCache { get; set; }

            // The callback used to do something when the window is rendered
            public Action<IWindow> Callback { get; set; }

            /// <summary>
            /// Whether only one <see cref="IWindow"/> can get rendered at the same time
            /// </summary>
            public Boolean singleObject { get; set; }
            
            /// <summary>
            /// Start up, and create needed fields
            /// </summary>
            public void Create(Action<IWindow> callback, Boolean singleObject)
            {
                // Create Dictionaries
                Windows = new Dictionary<T, HashSet<IWindow>>();
                WindowStates = new Dictionary<T, Boolean>();
                RectCache = new Dictionary<IWindow, Rect>();
                Callback = callback;
                this.singleObject = singleObject;
            }

            /// <summary>
            /// Renders the UI
            /// </summary>
            public void RenderUI()
            {
                // Loop through all the windows
                foreach (KeyValuePair<T, HashSet<IWindow>> window in Windows)
                {
                    if (!WindowStates[window.Key]) continue;
                    foreach (IWindow w in window.Value) Callback(w);
                }
            }

            /// <summary>
            /// Extracts the position of the Window
            /// </summary>
            public Rect GetRect(IWindow window)
            {
                if (RectCache.ContainsKey(window))
                    return RectCache[window];
                Position position = (window.GetType().GetCustomAttributes(typeof(Position), false) as Position[])?[0];
                RectCache[window] = position.Value;
                return position.Value;
            }

            /// <summary>
            /// Registers the window for the specified Task
            /// </summary>
            public void RegisterWindow<T_>(T window) where T_ : IWindow, new()
            {
                if (Windows.ContainsKey(window) && Windows[window] != null)
                    Windows[window].Add(new T_());
                else
                    Windows[window] = new HashSet<IWindow> { new T_() };
                WindowStates.Add(window, false);
            }

            /// <summary>
            /// Disables the window.
            /// </summary>
            public void DisableWindow(T window)
            {
                WindowStates[window] = false;
            }

            /// <summary>
            /// Enables the window.
            /// </summary>
            public void EnableWindow(T window)
            {
                if (singleObject)
                {
                    Dictionary<T, Boolean> states = new Dictionary<T, Boolean>();
                    foreach (T key in WindowStates.Keys) states[key] = false;
                    WindowStates = states;
                }
                WindowStates[window] = true;
            }

            /// <summary>
            /// Toggles the window.
            /// </summary>
            public void ToggleWindow(T window)
            {
                if (singleObject && !WindowStates[window])
                    foreach (T key in WindowStates.Keys) WindowStates[key] = false;
                WindowStates[window] = !WindowStates[window];
            }

            /// <summary>
            /// Sets the edited object for the window
            /// </summary>
            public void SetEditedObject<T_>(T window, T_ value)
            {
                foreach (IWindow w in Windows[window])
                    w.SetEditedObject(value, o => {});
            }

            /// <summary>
            /// Sets the edited object for the window
            /// </summary>
            public void SetEditedObject<T_>(T window, T_ value, Action<T_> callback)
            {
                foreach (IWindow w in Windows[window])
                    w.SetEditedObject(value, o => callback((T_)o));
            }
        }
    }
}