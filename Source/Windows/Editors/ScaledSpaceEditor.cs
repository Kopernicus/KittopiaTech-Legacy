/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Threading;
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
            // Whether maps should be left transparent (oceans)
            private Boolean transparentMaps = true;

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Scroll
                BeginScrollView(250, Utils.GetScrollSize<MeshFilter>() + Utils.GetScrollSize<MeshRenderer>() + Utils.GetScrollSize<ScaledSpaceFader>() + 180, 20);

                // Index
                index = 0;

                // Render Objects
                RenderObject(Current.GetComponent<ScaledSpaceFader>());
                RenderObject(Current.GetComponent<MeshRenderer>());
                RenderObject(Current.GetComponent<MeshFilter>());

                // Map Data
                CelestialBody body = Utils.FindCB(Current.name);
                if (body.pqsController != null)
                {
                    Label("mapFilesize");
                    index--;
                    TextField(body.pqsController.mapFilesize, v => body.pqsController.mapFilesize = v, new Rect(200, index * distance + 10, 170, 20));
                    Label("mapMaxHeight");
                    index--;
                    TextField(body.pqsController.mapMaxHeight, v => body.pqsController.mapMaxHeight = v, new Rect(200, index * distance + 10, 170, 20));
                    Label("normalStrength");
                    index--;
                    TextField(UIController.NormalStrength, v => UIController.NormalStrength = v, new Rect(200, index * distance + 10, 170, 20));
                    Label("transparentMaps");
                    index--;
                    TextField(transparentMaps, v => transparentMaps = v, new Rect(200, index * distance + 10, 170, 20));
                }

                // Update Orbit
                index++;
                Button(Localization.LOC_KITTOPIATECH_SCALEDEDITOR_UPDATEMESH, () => Utils.GenerateScaledSpace(Utils.FindCB(Current.name), Current.GetComponent<MeshFilter>().sharedMesh));
                Enabled(() => body.pqsController != null, () => Button(Localization.LOC_KITTOPIATECH_SCALEDEDITOR_UPDATETEX, () => UIController.Instance.StartCoroutine(Utils.GeneratePQSMaps(Utils.FindCB(Current.name), transparentMaps))));

                // End Scroll
                EndScrollView();
            }
        }
    }
}