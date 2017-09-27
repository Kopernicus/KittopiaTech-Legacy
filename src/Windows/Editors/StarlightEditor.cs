/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kopernicus.Components;
using Kopernicus.MaterialWrapper;
using Kopernicus.UI.Enumerations;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the editor for the starlight.
        /// </summary>
        public class StarlightEditor : Editor<CelestialBody>
        {
            /// <summary>
            /// The light component
            /// </summary>
            public LightShifter lsc;

            /// <summary>
            /// The material of the sun surface
            /// </summary>
            public EmissiveMultiRampSunspots material;

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Scroll
                BeginScrollView(250, Utils.GetScrollSize<LightShifter>() + Utils.GetScrollSize<EmissiveMultiRampSunspots>() + distance * 4, 20);

                // Index
                index = 0;

                // Planet must be a star
                if (Current.scaledBody.GetComponentsInChildren<SunShaderController>(true).Length == 0)
                {
                    Label(Localization.LOC_KITTOPIATECH_STARLIGHTEDITOR_NOSTAR, new Rect(20, 310, 400, 20));
                    return;
                }

                // Get the light components
                if (lsc == null || material == null)
                {
                    lsc = Current.scaledBody.GetComponentsInChildren<LightShifter>(true).First();
                    material = new EmissiveMultiRampSunspots(Current.scaledBody.GetComponent<Renderer>().sharedMaterial);
                }

                // Render the objects
                RenderObject(material);
                RenderObject(lsc);

                // Space
                index++;

                // Apply
                Button(Localization.LOC_KITTOPIATECH_APPLY, () =>
                {
                    Current.scaledBody.GetComponent<Renderer>().sharedMaterial = material;
                    Current.scaledBody.GetComponentsInChildren<StarComponent>().First().SetAsActive();
                });

                // End Scroll
                EndScrollView();
            }

            /// <summary>
            /// Resets objects
            /// </summary>
            protected override void SetEditedObject()
            {
                lsc = null;
                material = null;
            }
        }
    }
}