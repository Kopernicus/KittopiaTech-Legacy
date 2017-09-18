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
        [Position(420, 20, 420, 260)]
        public class LerpRangeWindow : Window<PQSLandControl.LerpRange>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return $"KittopiaTech - {Localization.LOC_KITTOPIATECH_LERPRANGEWINDOW}";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                index++;
                RenderObject(Current);

                // Exit
                index++;
                Callback?.Invoke(Current);
                Button(Localization.LOC_KITTOPIATECH_EXIT, () => UIController.Instance.DisableWindow(KittopiaWindows.LerpRange));
            }
        }
    }
}