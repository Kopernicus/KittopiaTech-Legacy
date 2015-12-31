using Kopernicus.Configuration;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Kopernicus.Components;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a Particle-Editor
        public class ParticlesEditor
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Particle
            public static PlanetParticleEmitter particle;

            private static List<GameObject> particles = null;
            private static int index = 0;

            // Return an OnGUI()-Window.
            public static void Render()
            {
                // Render variables
                int offset = 280;

                // If we have no Body selected, abort
                if (PlanetUI.currentName == "")
                {
                    GUI.Label(new Rect(20, 310, 400, 20), "No Planet selected!");
                    return;
                }

                // Get the list
                if (particles == null)
                {
                    particles = new List<GameObject>();
                    foreach (Transform t in PlanetUI.currentBody.scaledBody.transform)
                        if (t.name.Contains("Particles")) particles.Add(t.gameObject);
                }

                // Render the Window
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, 480));

                // Particle-Selector
                if (index > 0)
                {
                    if (GUI.Button(new Rect(20, offset, 30, 20), "<<"))
                        index--;
                }
                if (GUI.Button(new Rect(60, offset, 250, 20), "Add new Particles"))
                {
                    // Add a new Particle
                    particle = PlanetParticleEmitter.Create(PlanetUI.currentBody.scaledBody);

                    // Get all particles
                    particles = new List<GameObject>();
                    foreach (Transform t in PlanetUI.currentBody.scaledBody.transform)
                        if (t.name.Contains("Particles")) particles.Add(t.gameObject);
                    index = particles.Count - 1;
                }
                if (index < particles.Count - 1)
                {
                    if (GUI.Button(new Rect(320, offset, 30, 20), ">>"))
                        index++;
                }
                offset += 35;
                if (particles.Count > 0)
                {
                    // Assign
                    particle = particles[index].GetComponent<PlanetParticleEmitter>();

                    // Loop through all the Fields
                    object[] objects = Utils.GetInfos<FieldInfo>(particle);
                    object obj = particle as System.Object;
                    Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                    offset += 20;

                    if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild Particles"))
                    {
                        typeof(PlanetParticleEmitter).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(particle, null);
                        particle.targetTransform = null;
                    }
                    offset += 25;
                    if (GUI.Button(new Rect(20, offset, 200, 20), "Delete particles on: " + PlanetUI.currentName))
                    {
                        UnityEngine.Object.Destroy(particles[index]);

                        // Refresh the Ring-List
                        particles = null;
                        index = 0;
                    }
                }
                GUI.EndScrollView();
            }
        }
    }
}