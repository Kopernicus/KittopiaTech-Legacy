/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Abstraction for window rendering types
        /// </summary>
        public abstract class Editor<T> : Window<T>
        {
            /// <summary>
            /// The position of the window
            /// </summary>
            protected override Rect position => new Rect(20, 20, 420, 590);

            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return "";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                index = id;
            }
        }
    }
}