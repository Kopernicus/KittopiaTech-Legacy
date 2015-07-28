using HighlightingSystem;
using Kopernicus.Configuration;
using Kopernicus.Configuration.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a Particle-Editor
        public class ParticlesEditor
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Ring
            public static ParticleLoader.PlanetaryParticle particles;

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

                // Render the Window
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, 330));

                // Ring-Selector
                if (PlanetUI.currentBody.scaledBody.GetComponents<ParticleLoader.PlanetaryParticle>().Length == 0)
                {
                    if (GUI.Button(new Rect(60, offset, 250, 20), "Add Particle-System"))
                    {
                        particles = ParticleLoader.PlanetaryParticle.CreateInstance(PlanetUI.currentBody.scaledBody);
                    }
                }
                else
                {
                    particles = PlanetUI.currentBody.scaledBody.GetComponent<ParticleLoader.PlanetaryParticle>();

                    GUI.Label(new Rect(20, offset, 178, 20), "Target");
                    if (GUI.Button(new Rect(200, offset, 170, 20), "Edit Target"))
                    {
                        FieldInfo key = particles.GetType().GetField("target");
                        CBBrowser.SetEditedObject(key, particles);
                    }
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "minEmission");
                    particles.minEmission = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + particles.minEmission));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "maxEmission");
                    particles.maxEmission = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + particles.maxEmission));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "lifespanMin");
                    particles.minEnergy = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + particles.minEnergy));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "lifespanMax");
                    particles.maxEnergy = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + particles.maxEnergy));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "sizeMin");
                    particles.emitter.minSize = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + particles.emitter.minSize));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "sizeMax");
                    particles.emitter.maxSize = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + particles.emitter.maxSize));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "speedScale");
                    particles.speedScale = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + particles.speedScale));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "rate");
                    particles.animator.sizeGrow = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + particles.animator.sizeGrow));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 200, 20), "randVelocity");
                    Vector3 value = particles.randomVelocity;
                    value.x = Single.Parse(GUI.TextField(new Rect(200, offset, 50, 20), "" + value.x));
                    value.y = Single.Parse(GUI.TextField(new Rect(260, offset, 50, 20), "" + value.y));
                    value.z = Single.Parse(GUI.TextField(new Rect(320, offset, 50, 20), "" + value.z));
                    particles.randomVelocity = value;
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "Texture");
                    if (GUI.Button(new Rect(200, offset, 80, 20), "Load"))
                    {
                        UIController.Instance.isFileBrowser = !UIController.Instance.isFileBrowser;
                        FileBrowser.location = "";
                    }

                    // Apply the new Texture
                    if (GUI.Button(new Rect(290, offset, 80, 20), "Apply"))
                    {
                        string path = FileBrowser.location.Replace(Path.Combine(Directory.GetCurrentDirectory(), "GameData") + Path.DirectorySeparatorChar, "");
                        Texture2D texture = Utility.LoadTexture(path, false, false, false);
                        texture.name = path.Replace("\\", "/");
                        particles.Renderer.material.mainTexture = texture;
                    }
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "Colors:");
                    offset += 25;

                    int buttonOffset = 20;
                    for (int i = 0; i < 5; i++)
                    {
                        if (GUI.Button(new Rect(buttonOffset, offset, 60, 20), "Color " + (i + 1)))
                        {
                            ColorPicker.index = i;
                            PropertyInfo prop = typeof(ParticleAnimator).GetProperty("colorAnimation");
                            ColorPicker.SetEditedObject(prop, particles.animator.colorAnimation[i], particles.animator);
                        }
                        buttonOffset += 65;
                    }
                    offset += 45;
                 
                    if (GUI.Button(new Rect(20, offset, 200, 20), "Delete Particles"))
                    {
                        MonoBehaviour.Destroy(particles.Renderer);
                        MonoBehaviour.Destroy(particles.animator);
                        MonoBehaviour.Destroy(particles.emitter);
                        MonoBehaviour.Destroy(particles);
                    }
                }

                GUI.EndScrollView();
            }
        }
    }
}
