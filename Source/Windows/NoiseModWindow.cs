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
        /// This class renders a window to edit pqs lerp ranges
        /// </summary>
        [Position(420, 20, 420, 170)]
        public class NoiseModWindow : Window<PQSMod_VertexPlanet.NoiseModWrapper>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return "KittopiaTech - NoiseMod Editor";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Scroll
                BeginScrollView(200, Utils.GetScrollSize<PQSMod_VertexPlanet.NoiseModWrapper>() + 50);

                // Render the editor
                RenderObject(Current);

                // Exit
                index++;
                Callback?.Invoke(Current);
                Button("Exit", () => UIController.Instance.DisableWindow(KittopiaWindows.NoiseMod));

                // End scroll
                EndScrollView();
            }
        }
    }
}