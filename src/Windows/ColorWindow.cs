/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to edit colors
        /// </summary>
        [Position(420, 20, 400, 200)]
        public class ColorWindow : Window<Color>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return $"KittopiaTech - {Localization.LOC_KITTOPIATECH_COLORWINDOW}";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                GUIStyle colorStyle = new GUIStyle();
                Texture2D blankTexture = new Texture2D(1, 1) { wrapMode = TextureWrapMode.Repeat };

                // Render Sliders
                Single r_ = GUI.HorizontalSlider(new Rect(10, 30, 190, 20), Current.r, 0, 1);
                TextField(Current.r, r => Current.r = Current.r == r ? r_ : r, new Rect(220, 30, 100, 20), Current.r == r_);
                Single g_ = GUI.HorizontalSlider(new Rect(10, 60, 190, 20), Current.g, 0, 1);
                TextField(Current.g, g => Current.g = Current.g == g ? g_ : g, new Rect(220, 60, 100, 20), Current.g == g_);
                Single b_ = GUI.HorizontalSlider(new Rect(10, 90, 190, 20), Current.b, 0, 1);
                TextField(Current.b, b => Current.b = Current.b == b ? b_ : b, new Rect(220, 90, 100, 20), Current.b == b_);
                Single a_ = GUI.HorizontalSlider(new Rect(10, 120, 190, 20), Current.a, 0, 1);
                TextField(Current.a, a => Current.a = Current.a == a ? a_ : a, new Rect(220, 120, 100, 20), Current.a == a_);

                // Show the color
                GUI.color = Current;
                blankTexture.SetPixel(0, 0, Current);
                blankTexture.Apply();
                colorStyle.normal.background = blankTexture;
                GUI.Box(new Rect(210, 150, 240, 100), blankTexture, colorStyle);
                blankTexture.SetPixel(0, 0, new Color(Mathf.Abs(Current.r - 1.0f), Mathf.Abs(Current.g - 1.0f), Mathf.Abs(Current.b - 1.0f), 1.0f));
                blankTexture.Apply();
                colorStyle.normal.background = blankTexture;
                GUI.Box(new Rect(300, 150, 240, 100), blankTexture, colorStyle);
                GUI.color = Color.white;

                // Save
                Callback?.Invoke(Current);
                Button(Localization.LOC_KITTOPIATECH_EXIT, () => UIController.Instance.DisableWindow(KittopiaWindows.Color), new Rect(10, 150, 200, 50));
            }
        }
    }
}