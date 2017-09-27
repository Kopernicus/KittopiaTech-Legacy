/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;
using System.Reflection;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to edit a simplex
        /// </summary>
        [Position(420, 100, 320, 220)]
        public class EnumWindow : Window<Enum>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return $"KittopiaTech - {Localization.LOC_KITTOPIATECH_ENUMWINDOW}";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Scroll
                FieldInfo[] fields = Current.GetType().GetFields(BindingFlags.Static | BindingFlags.Public);
                BeginScrollView((Int32)position.height - 30, fields.Length * distance + distance * 3, 10);

                // Render possible values
                foreach (FieldInfo obj in fields)
                    Button(obj.Name, () => { Current = (Enum)obj.GetValue(null); }, width: 240);

                // Exit
                Button(Localization.LOC_KITTOPIATECH_EXIT, () => { UIController.Instance.DisableWindow(KittopiaWindows.Enum); }, width: 240);

                // End Scroll
                EndScrollView();
            }
        }
    }
}