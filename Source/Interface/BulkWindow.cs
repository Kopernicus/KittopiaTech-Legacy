/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Abstraction for window rendering types
        /// </summary>
        public abstract class BulkWindow<T> : Window<IEnumerable<T>>
        {
            /// <summary>
            /// The modes of the window
            /// </summary>
            public enum Mode
            {
                Selection,
                Editor
            }

            /// <summary>
            /// The values that are currently edited
            /// </summary>
            protected IEnumerable<T> Collection
            {
                get { return base.Current; }
                set { base.Current = value; }
            } 

            /// <summary>
            /// The current mode of the editor
            /// </summary>
            protected Mode mode { get; set; }

            /// <summary>
            /// The value that is currently edited
            /// </summary>
            protected new T Current { get; set; }

            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                if (mode == Mode.Selection)
                {
                    BeginScrollView((Int32)position.height - 30, Collection.Count() * 25 + 100, 10);
                    foreach (T obj in base.Current)
                        Button(Format(obj), () => { Current = obj; mode = Mode.Editor; }, width: position.width - 80);
                    RenderModifiers(id);
                    Button("Exit", Exit);
                    EndScrollView();
                }
                else
                {
                    BeginScrollView(200, Utils.GetScrollSize(Current.GetType()) + 100);
                    RenderEditor(id);
                    Button("Exit", () => { Current = default(T); mode = Mode.Selection; });
                    EndScrollView();
                }
            }

            /// <summary>
            /// Formats the specified object.
            /// </summary>
            protected virtual String Format(T obj)
            {
                return obj.ToString();
            }

            /// <summary>
            /// Closes the Window
            /// </summary>
            protected abstract void Exit();

            /// <summary>
            /// Renders Collection Modifiers
            /// </summary>
            protected abstract void RenderModifiers(Int32 id);

            /// <summary>
            /// Renders the Editor
            /// </summary>
            protected abstract void RenderEditor(Int32 id);
        }
    }
}