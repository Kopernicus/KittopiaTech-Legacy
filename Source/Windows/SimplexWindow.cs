/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to edit a simplex
        /// </summary>
        [Position(420, 20, 420, 190)]
        public class SimplexWindow : Window<Simplex>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return "KittopiaTech - Simplex Editor";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Scroll
                BeginScrollView(200, Utils.GetScrollSize<Simplex>() + 50);

                // Render the editor
                RenderObject(Current);

                // Exit
                Button("Exit", () => UIController.Instance.DisableWindow(KittopiaWindows.Simplex));

                // End scroll
                EndScrollView();
            }
        }
    }
}