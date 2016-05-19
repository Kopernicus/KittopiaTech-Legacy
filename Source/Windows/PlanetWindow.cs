/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;
using UnityEngine;
using Random = System.Random;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the main Planet Window. Here the main components of a planet are edited
        /// </summary>
        [Position(20, 20, 420, 590)]
        public class PlanetWindow : Window<CelestialBody>
        {
            /// <summary>
            /// The conttoller for the Editors inside of this window
            /// </summary>
            protected Controller<KittopiaEditors> EditorController { get; set; }

            public PlanetWindow()
            {
                EditorController = new Controller<KittopiaEditors>();
                EditorController.Create(window => window.Render(index), true); // TODO: Implement

                // Register Editors
                EditorController.RegisterWindow<AtmosphereEditor>(KittopiaEditors.Atmosphere);
                EditorController.RegisterWindow<CelestialBodyEditor>(KittopiaEditors.CelestialBody);
                EditorController.RegisterWindow<OrbitEditor>(KittopiaEditors.Orbit);
                EditorController.RegisterWindow<ParticleEditor>(KittopiaEditors.Particles);
                EditorController.RegisterWindow<RingEditor>(KittopiaEditors.Ring);
                EditorController.RegisterWindow<ScaledSpaceEditor>(KittopiaEditors.ScaledSpace);
            }

            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return "KittopiaTech - a Kopernicus Visual Editor";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Scroll
                BeginScrollView(240, 345);

                // Current Body
                DependencyButton("Current body: " + Current?.name, "No body selected!", () => { UIController.Instance.SetEditedObject(KittopiaWindows.Selector, Current ?? new CelestialBody(), b => Current = b); UIController.Instance.EnableWindow(KittopiaWindows.Selector); }, () => Current != null);
                index++;

                // Editors
                Button("Atmosphere Editor", () => { EditorController.SetEditedObject(KittopiaEditors.Atmosphere, Current); EditorController.EnableWindow(KittopiaEditors.Atmosphere); });
                Button("CelestialBody Editor", () => { EditorController.SetEditedObject(KittopiaEditors.CelestialBody, Current); EditorController.EnableWindow(KittopiaEditors.CelestialBody); });
                Button("PQS Editor", () => EditorController.EnableWindow(KittopiaEditors.Terrain)); // TODO: Set PQS editor to mode LIST?
                Button("Orbit Editor", () => { EditorController.SetEditedObject(KittopiaEditors.Orbit, Current.orbitDriver); EditorController.EnableWindow(KittopiaEditors.Orbit); });
                Button("ScaledSpace Editor", () => { EditorController.SetEditedObject(KittopiaEditors.ScaledSpace, Current.scaledBody); EditorController.EnableWindow(KittopiaEditors.ScaledSpace); });
                Button("Starlight Editor", () => EditorController.EnableWindow(KittopiaEditors.Starlight));
                Button("Ring Editor", () => { EditorController.SetEditedObject(KittopiaEditors.Ring, Current); EditorController.EnableWindow(KittopiaEditors.Ring); });
                Button("Particles Editor", () => { EditorController.SetEditedObject(KittopiaEditors.Particles, Current); EditorController.EnableWindow(KittopiaEditors.Particles); });

                // Space
                index++;

                // Special Stuff
                Button("Save Body", () => ConfigIO.SaveCelestial(Current));
                Button("[HACK] Instantiate Body", () => Utils.Instantiate(Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, Current.transform.name), "Body" + new Random().Next(1000)));

                // Scroll
                EndScrollView();

                // Index
                index = 230 / 25 + 2;

                // Design Hack
                Boolean e = isError;
                isError = false;
                HorizontalLine(8f);
                isError = e;

                // Render the EditorControler
                EditorController.RenderUI();
            }
        }
    }
}