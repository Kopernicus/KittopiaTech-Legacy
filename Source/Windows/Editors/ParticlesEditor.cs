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
using Kopernicus.UI.Enumerations;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the editor for the particles.
        /// </summary>
        public class ParticleEditor : Editor<CelestialBody>
        {
            /// <summary>
            /// The currently edited particle system
            /// </summary>
            private PlanetParticleEmitter particle { get; set; }

            /// <summary>
            /// The index of the current system in all particles
            /// </summary>
            private new Int32 position { get; set; }

            /// <summary>
            /// All particles attached to the planet
            /// </summary>
            private List<GameObject> particles = null;

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Scroll
                BeginScrollView(250, Utils.GetScrollSize<PlanetParticleEmitter>() + 175, 20);

                // Get the list
                if (particles == null)
                {
                    particles = new List<GameObject>();
                    foreach (Transform t in from Transform t in Current.scaledBody.transform where t.name.EndsWith("Particles") select t)
                        particles.Add(t.gameObject);
                }

                // Index
                index = 0;

                // Menu
                Enabled(() => position > 0, () => Button("<<", () => position--, new Rect(20, index * distance + 10, 30, 20))); index--;
                Button("Add new Particles", () =>
                {
                    // Add a new Particle System
                    PlanetParticleEmitter.Create(Current.scaledBody).colorAnimation = new [] { Color.white, Color.white, Color.white, Color.white, Color.white };

                    // Get all particles
                    particles = new List<GameObject>();
                    foreach (Transform t in from Transform t in Current.scaledBody.transform where t.name.EndsWith("Particles") select t)
                        particles.Add(t.gameObject);
                    position = particles.Count - 1;
                }, new Rect(60, index * distance + 10, 250, 20)); index--;
                Enabled(() => position < particles.Count - 1, () => Button(">>", () => position++, new Rect(320, index * distance + 10, 30, 20))); index++;

                // Render the Particle Editor
                if (particles.Count <= 0) return;

                // Assign
                particle = particles[position].GetComponent<PlanetParticleEmitter>();

                // Loop through all the Fields
                RenderObject(particle);

                // Color Array
                Int32 buttonOffset = 20;
                for (Int32 i = 0; i < 5; i++)
                {
                    Int32 i1 = i;
                    Button("Color " + (i + 1), () => {
                        UIController.Instance.SetEditedObject(KittopiaWindows.Color, particle.colorAnimation[i1], c => particle.colorAnimation[i1] = c);
                        UIController.Instance.EnableWindow(KittopiaWindows.Color);
                    }, new Rect(buttonOffset, index*distance + 10, 60, 20));
                    buttonOffset += 65; index--;
                }
                index += 2;

                // Rebuild the Ring
                Button("Rebuild Particles", () =>
                {
                    typeof(PlanetParticleEmitter).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(particle, null);
                    particle.targetTransform = null;
                });

                // Delete Ring
                Button("Delete Particles", () =>
                {
                    UnityEngine.Object.Destroy(particles[position]);
                    particles = null;
                    position = 0;
                });

                // End Scroll
                EndScrollView();
            }

            /// <summary>
            /// Resets objects
            /// </summary>
            protected override void SetEditedObject()
            {
                particles = null;
            }
        }
    }
}