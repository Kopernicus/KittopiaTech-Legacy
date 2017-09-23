/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Abstraction for window rendering types
        /// </summary>
        public abstract partial class Window<T> : IWindow
        {
            /// <summary>
            /// The distance between two elements of the window
            /// </summary>
            public const Int32 distance = 25;
            
            /// <summary>
            /// The function that gets called when the object was edited
            /// </summary>
            protected Action<T> Callback { get; set; }

            /// <summary>
            /// The current position of the scrollbar
            /// </summary>
            protected Vector2 scrollPosition { get; set; }

            /// <summary>
            /// The current object
            /// </summary>
            protected Int32 index { get; set; }

            /// <summary>
            /// Whether we have an active error
            /// </summary>
            protected Boolean isError { get; set; }

            /// <summary>
            /// Caches the values in the parser fields
            /// </summary>
            protected Dictionary<Int32, String> parseCache { get; set; }

            /// <summary>
            /// Whether ther was a parsing error
            /// </summary>
            private Dictionary<Int32, Boolean> isParseError { get; set; }

            /// <summary>
            /// The depth of mapSO's
            /// </summary>
            private Int32 mapDepth { get; set; }

            /// <summary>
            /// The position of the window
            /// </summary>
            protected virtual Rect position
            {
                get { return UIController.Instance.RectCache[this]; }
                set { UIController.Instance.RectCache[this] = value; }
            }

            /// <summary>
            /// The value that is currently edited
            /// </summary>
            public T Current;

            /// <summary>
            /// Renders the Window
            /// </summary>
            void IWindow.Render(Int32 id)
            {
                index = 0;
                if (parseCache == null)
                    parseCache = new Dictionary<Int32, String>();
                if (isParseError == null)
                    isParseError = new Dictionary<Int32, Boolean>();
                Render(id);
                GUI.DragWindow();
            }

            /// <summary>
            /// Sets the currently edited object.
            /// </summary>
            void IWindow.SetEditedObject(Object value, Action<Object> callback)
            {
                Current = (T)value;
                mapDepth = 5;
                Callback = c => callback(c);
                SetEditedObject();
            }

            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            String IWindow.Title()
            {
                return Title();
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected abstract void Render(Int32 id);

            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected abstract String Title();

            /// <summary>
            /// Resets objects
            /// </summary>
            protected virtual void SetEditedObject() { }
        }
    }
}