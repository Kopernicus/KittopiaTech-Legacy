/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Linq;
using Kopernicus.UI.Enumerations;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to select objects
        /// </summary>
        [Position(420, 100, 320, 340)]
        public class SelectorWindow : Window<UnityEngine.Object>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return "KittopiaTech - Selector";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Scroll
                BeginScrollView(300, UnityEngine.Object.FindObjectsOfType(Current.GetType()).Length * distance + 70);

                // Selectors
                foreach (UnityEngine.Object o in UnityEngine.Object.FindObjectsOfType(Current.GetType()))
                {
                    Button(o.ToString(), () => Callback(o), width: 240);
                }

                // Exit
                index++;
                Button("Exit", () => UIController.Instance.DisableWindow(KittopiaWindows.Selector), width: 240);

                // End Scroll
                EndScrollView();
            }
        }
    }
}