/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to edit a float curve
        /// </summary>
        [Position(420, 400, 420, 250)]
        public class CurveWindow : Window<FloatCurve>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return "KittopiaTech - Curve Editor";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Scroll
                BeginScrollView(200, Current.Curve.keys.Length * 25 + 80);

                // Add default frames
                if (Current.Curve.keys.Length == 0)
                {
                    Current.Add(0, 0);
                    Current.Add(1, 0);
                }

                // Edit the frames
                for (Int32 i = 0; i < Current.Curve.keys.Length; i++)
                {
                    // Get the value
                    Keyframe frame = Current.Curve.keys[i];

                    // Edit
                    TextField(frame.time, t => frame.time = t, new Rect(20, index * distance + 10, 60, 20)); index--;
                    TextField(frame.value, t => frame.value = t, new Rect(90, index * distance + 10, 60, 20)); index--;
                    TextField(frame.inTangent, t => frame.inTangent = t, new Rect(160, index * distance + 10, 60, 20)); index--;
                    TextField(frame.outTangent, t => frame.outTangent = t, new Rect(230, index * distance + 10, 60, 20)); index--;

                    // Add
                    Button("+", () => { List<Keyframe> frames = Current.Curve.keys.ToList(); frames.Insert(i + 1, new Keyframe());
                                          Current.Curve.keys = frames.ToArray();
                    }, new Rect(300, index*distance + 10, 30, 20)); index--;

                    // Remove
                    Button("-", () => {
                        List<Keyframe> frames = Current.Curve.keys.ToList(); frames.RemoveAt(i);
                        Current.Curve.keys = frames.ToArray();
                    }, new Rect(340, index * distance + 10, 30, 20));
                }

                // Exit
                Callback?.Invoke(Current);
                Button("Exit", () => UIController.Instance.DisableWindow(KittopiaWindows.Curve));

                // End Scroll
                EndScrollView();
            }
        }
    }
}