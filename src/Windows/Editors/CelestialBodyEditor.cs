/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the editor for the properties of a single body
        /// </summary>
        public class CelestialBodyEditor : Editor<CelestialBody>
        {
            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Scroll
                BeginScrollView(250, Utils.GetScrollSize<CelestialBody>() + 10, 20);

                // Index
                index = 0;

                // Render the Object
                RenderObject(Current);

                // No callback needed, directly end scroll
                EndScrollView();
            }
        }
    }
}