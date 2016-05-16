/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the main Planet Window. Here the main components of a planet are edited
        /// </summary>
        [Position(20, 20, 420, 560)]
        public class PlanetWindow : Window<CelestialBody>
        {
            /// <summary>
            /// The conttoller for the Editors inside of this window
            /// </summary>
            protected Controller<KittopiaEditors> EditorController { get; set; }

            public PlanetWindow()
            {
                EditorController = new Controller<KittopiaEditors>();
                EditorController.Create(null, true); // TODO: Implement
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
                BeginScrollView(240, 370);

                // Current Body
                DependencyButton("Current body: " + Current.name, "No body selected!", () => { UIController.Instance.SetEditedObject(KittopiaWindows.Array, Current, b => Current = b); UIController.Instance.EnableWindow(KittopiaWindows.Array); }, () => Current != null);

                // Editors
                Button("Atmosphere Editor", () => EditorController.EnableWindow(KittopiaEditors.Atmosphere));
                Button("CelestialBody Editor", () => EditorController.EnableWindow(KittopiaEditors.CelestialBody));
                Button("PQS Editor", () => EditorController.EnableWindow(KittopiaEditors.Terrain)); // TODO: Set PQS editor to mode LIST?
                Button("Orbit Editor", () => EditorController.EnableWindow(KittopiaEditors.Orbit));
                Button("ScaledSpace Editor", () => EditorController.EnableWindow(KittopiaEditors.ScaledSpace));
                Button("Starlight Editor", () => EditorController.EnableWindow(KittopiaEditors.Starlight));
                Button("Ring Editor", () => EditorController.EnableWindow(KittopiaEditors.Ring));
                Button("Particles Editor", () => EditorController.EnableWindow(KittopiaEditors.Particles));

                // Space
                index++;

                // Special Stuff
                Button("Save Body", () => ConfigIO.SaveCelestial(Current));
                Button("[HACK] Instantiate Body", () => Utils.Instantiate(Utility.FindBody(PSystemManager.Instance.systemPrefab.rootBody, Current.transform.name), "Body" + new Random().Next(1000)));

                // Scroll
                EndScrollView();

                // Design Hack
                HorizontalLine(5f);

                // Render the EditorControler
                EditorController.RenderUI();
            }
        }
    }
}