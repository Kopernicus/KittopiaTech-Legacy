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
        /// This class represents the orbital editor.
        /// </summary>
        public class OrbitEditor : Editor<OrbitDriver>
        {
            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Scroll
                BeginScrollView(250, Utils.GetScrollSize<OrbitDriver>() + Utils.GetScrollSize<Orbit>() + Utils.GetScrollSize<OrbitRenderer>(), 20);

                // Index
                index = 0;

                // Render Objects
                RenderObject(Current.orbit);
                RenderObject(Current);
                RenderObject(Current.Renderer);

                // Update Orbit
                index++;
                Button(Localization.LOC_KITTOPIATECH_ORBITEDITOR_UPDATE, () => Current.UpdateOrbit());

                // End Scroll
                EndScrollView();
            }
        }
    }
}