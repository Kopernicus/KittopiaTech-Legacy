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
                EditorController.RegisterWindow<PQSEditor>(KittopiaEditors.Terrain);
                EditorController.RegisterWindow<RingEditor>(KittopiaEditors.Ring);
                EditorController.RegisterWindow<ScaledSpaceEditor>(KittopiaEditors.ScaledSpace);
                EditorController.RegisterWindow<StarlightEditor>(KittopiaEditors.Starlight);
            }

            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return $"KittopiaTech - {Localization.LOC_KITTOPIATECH_PLANETWINDOW}";
            }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Scroll
                BeginScrollView(240, 345);

                // Current Body
                DependencyButton(Localization.LOC_KITTOPIATECH_PLANETWINDOW_CURRENT + ": " + Current?.displayName.Replace("^N", ""), Localization.LOC_KITTOPIATECH_PLANETWINDOW_NOCURRENT, () => { UIController.Instance.SetEditedObject(KittopiaWindows.Selector, Current ?? new CelestialBody(), b => Current = b); UIController.Instance.EnableWindow(KittopiaWindows.Selector); }, () => Current != null);
                index++;

                // Editors
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_AFG_EDITOR, () => { EditorController.SetEditedObject(KittopiaEditors.Atmosphere, Current); EditorController.EnableWindow(KittopiaEditors.Atmosphere); });
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_CB_EDITOR, () => { EditorController.SetEditedObject(KittopiaEditors.CelestialBody, Current); EditorController.EnableWindow(KittopiaEditors.CelestialBody); });
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_PQS_EDITOR, () => { EditorController.SetEditedObject(KittopiaEditors.Terrain, Current); EditorController.EnableWindow(KittopiaEditors.Terrain); });
                Enabled(() => Current?.orbitDriver != null, () => { Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_ORBIT_EDITOR, () => { EditorController.SetEditedObject(KittopiaEditors.Orbit, Current.orbitDriver); EditorController.EnableWindow(KittopiaEditors.Orbit); }); });
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_SCALED_EDITOR, () => { EditorController.SetEditedObject(KittopiaEditors.ScaledSpace, Current.scaledBody); EditorController.EnableWindow(KittopiaEditors.ScaledSpace); });
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_LIGHT_EDITOR, () => {EditorController.SetEditedObject(KittopiaEditors.Starlight, Current); EditorController.EnableWindow(KittopiaEditors.Starlight); });
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_RING_EDITOR, () => { EditorController.SetEditedObject(KittopiaEditors.Ring, Current); EditorController.EnableWindow(KittopiaEditors.Ring); });
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_PARTICLES_EDITOR, () => { EditorController.SetEditedObject(KittopiaEditors.Particles, Current); EditorController.EnableWindow(KittopiaEditors.Particles); });

                // Space
                index++;

                // Special Stuff
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_SAVE, () => PlanetExporter.SaveCelestial(Current));
                Button(Localization.LOC_KITTOPIATECH_PLANETWINDOW_INSTANTIATE, () => Utils.Instantiate(Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, Current.transform.name), "Body" + new Random().Next(1000)));

                // Scroll
                EndScrollView();

                // Index
                index = 230 / distance + 2;

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