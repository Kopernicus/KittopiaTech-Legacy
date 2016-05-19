/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.Configuration;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the orbital editor.
        /// </summary>
        public class AtmosphereEditor : Editor<CelestialBody>
        {
            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Scroll
                BeginScrollView(250, Utils.GetScrollSize<AtmosphereFromGround>() + 50, 20);

                // Index
                index = 0;

                // Check for existing AFG
                if (Current.afg == null)
                {
                    Button("Add Atmosphere to " + Current.name, () =>
                    {
                        // Create the atmosphere shell game object
                        GameObject scaledAtmosphere = new GameObject("Atmosphere");
                        scaledAtmosphere.transform.parent = Current.scaledBody.transform;
                        scaledAtmosphere.layer = Constants.GameLayers.ScaledSpaceAtmosphere;
                        MeshRenderer renderer = scaledAtmosphere.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = new MaterialWrapper.AtmosphereFromGround();
                        MeshFilter meshFilter = scaledAtmosphere.AddComponent<MeshFilter>();
                        meshFilter.sharedMesh = Templates.ReferenceGeosphere;
                        Current.afg = scaledAtmosphere.AddComponent<AtmosphereFromGround>();

                        // Register the AFG for updates
                        AFGInfo.StoreAFG(Current.afg);
                    });
                    return;
                }

                // Render AFG
                RenderObject(Current.afg);
                index++;

                // Updates
                Button("Update Atmosphere", () => { AtmosphereFromGroundLoader.CalculatedMembers(Current.afg); AFGInfo.PatchAFG(Current.afg); });

                // End Scroll
                EndScrollView();
            }
        }
    }
}