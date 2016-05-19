/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the editor for the scaled space of the body.
        /// </summary>
        public class ScaledSpaceEditor : Editor<GameObject>
        {
            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Scroll
                BeginScrollView(250, Utils.GetScrollSize<MeshFilter>() + Utils.GetScrollSize<MeshRenderer>() + Utils.GetScrollSize<ScaledSpaceFader>() + 75, 20);

                // Index
                index = 0;

                // Render Objects
                RenderObject(Current.GetComponent<ScaledSpaceFader>());
                RenderObject(Current.GetComponent<MeshRenderer>());
                RenderObject(Current.GetComponent<MeshFilter>());

                // Update Orbit
                index++;
                Button("Update Mesh", () => Utils.GenerateScaledSpace(Utils.FindCB(Current.name), Current.GetComponent<MeshFilter>().sharedMesh));
                Button("Update Textures", () => { Action<CelestialBody> generate = Utils.GeneratePQSMaps; generate.BeginInvoke(Utils.FindCB(Current.name), ar => generate.EndInvoke(ar), null); });

                // End Scroll
                EndScrollView();
            }
        }
    }
}