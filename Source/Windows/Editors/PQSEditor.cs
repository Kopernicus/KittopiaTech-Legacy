/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Kopernicus.Components;
using Kopernicus.MaterialWrapper;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the editor for the planetary terrain.
        /// </summary>
        public class PQSEditor : Editor<CelestialBody>
        {
            /// <summary>
            /// The different stages for this editor
            /// </summary>
            public enum Modes
            {
                List,
                PQS,
                PQSMod,
                AddMod
            }

            /// <summary>
            /// The current mode
            /// </summary>
            private Modes _mode = Modes.List;

            /// <summary>
            /// The current PQS that is rendered
            /// </summary>
            private PQS _sphere;

            /// <summary>
            /// The current PQS mod that is rendered
            /// </summary>
            private PQSMod _mod;

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Check for PQS
                if (Current.pqsController == null)
                    return;

                // Mode List
                if (_mode == Modes.List)
                {
                    // Get the PQS-Spheres and their mods
                    IEnumerable<PQS> pqsList = Current.GetComponentsInChildren<PQS>(true);
                    IEnumerable<PQSMod> pqsModList = Current.GetComponentsInChildren<PQSMod>(true);

                    // Scroll
                    BeginScrollView(250, (pqsList.Count() + pqsModList.Count()) * 25 + 65, 20);

                    // Index
                    index = 0;

                    // Render
                    foreach (PQS pqs in pqsList)
                    {
                        Button(pqs.ToString(), () =>
                        {
                            _mode = Modes.PQS;
                            _sphere = pqs;
                            parseCache = new Dictionary<Int32, String>();
                        }, new Rect(20, index * distance + 10, 350, 20));
                    }
                    foreach (PQSMod mod in pqsModList)
                    {
                        Button(mod.ToString(), () =>
                        {
                            _mode = Modes.PQSMod;
                            _sphere = mod.sphere;
                            _mod = mod;
                            parseCache = new Dictionary<Int32, String>();
                        }, new Rect(20, index * distance + 10, 350, 20));
                    }
                    index++;
                    Button("Add PQSMod", () => _mode = Modes.AddMod);

                    // End Scroll
                    EndScrollView();
                }

                // Mode PQS
                if (_mode == Modes.PQS)
                {
                    // Scroll
                    BeginScrollView(250, Utils.GetScrollSize<PQS>() + 65, 20);

                    // Index
                    index = 0;

                    // Render the PQS
                    RenderObject(_sphere);

                    // If it is an ocean, create an Hazardous Ocean button
                    if (_sphere.surfaceMaterial is PQSOceanSurfaceQuad)
                        RenderObject(_sphere.gameObject.AddOrGetComponent<HazardousOcean>());
                    index++;

                    // Rebuild
                    Button("Rebuild Sphere", () => _sphere.RebuildSphere());

                    // End Scroll
                    EndScrollView();
                }

                // Mode PQSMod
                if (_mode == Modes.PQSMod)
                {
                    // Scroll
                    BeginScrollView(250, Utils.GetScrollSize(_mod.GetType()) + 115, 20);

                    // Index
                    index = 0;

                    // Render the PQS
                    RenderObject(_mod);
                    index++;

                    // Rebuild
                    Button("Rebuild Sphere", () => _sphere.RebuildSphere());

                    // Remove
                    Button("Remove PQSMod", () =>
                    {
                        _mod.sphere = null;
                        UnityEngine.Object.Destroy(_mod);
                        _mod = null;

                        // Hack
                        _sphere.SetupExternalRender();
                        _sphere.CloseExternalRender();

                        _mode = Modes.List;
                    });

                    // End Scroll
                    EndScrollView();
                }

                // Mode AddPQSMod
                if (_mode == Modes.AddMod)
                {
                    // Get all PQSMod types
                    List<Type> types = Injector.ModTypes.Where(t => t.IsSubclassOf(typeof (PQSMod))).ToList();

                    // Begin Scroll
                    BeginScrollView(250, types.Count * 25, 20);

                    // Index
                    index = 0;

                    // Render the possible types
                    foreach (Type t in types)
                        Button(t.FullName, () =>
                        {
                            // Hack^6
                            GameObject pqsModObject = new GameObject(t.Name);
                            pqsModObject.transform.parent = Current.pqsController.transform;
                            PQSMod mod = pqsModObject.AddComponent(t) as PQSMod;
                            mod.sphere = Current.pqsController;

                            if (t == typeof(PQSMod_VoronoiCraters))
                            {
                                CelestialBody mun = Utils.FindCB("Mun");
                                PQSMod_VoronoiCraters craters = mun.GetComponentsInChildren<PQSMod_VoronoiCraters>()[0];
                                PQSMod_VoronoiCraters nc = pqsModObject.GetComponentsInChildren<PQSMod_VoronoiCraters>()[0];
                                nc.craterColourRamp = craters.craterColourRamp;
                                nc.craterCurve = craters.craterCurve;
                                nc.jitterCurve = craters.jitterCurve;
                            }
                            else if (t == typeof(PQSMod_VertexPlanet))
                            {
                                PQSMod_VertexPlanet vp = mod as PQSMod_VertexPlanet;
                                vp.landClasses = new [] { new PQSMod_VertexPlanet.LandClass("Class", 0, 1, Color.black, Color.white, 0) };
                                vp.continental = new PQSMod_VertexPlanet.SimplexWrapper(0, 0, 0, 0);
                                vp.continentalRuggedness = new PQSMod_VertexPlanet.SimplexWrapper(0, 0, 0, 0);
                                vp.continentalSharpness = new PQSMod_VertexPlanet.NoiseModWrapper(0, 0, 0, 0);
                                vp.continentalSharpnessMap = new PQSMod_VertexPlanet.SimplexWrapper(0, 0, 0, 0);
                                vp.terrainType = new PQSMod_VertexPlanet.SimplexWrapper(0, 0, 0, 0);
                            }
                            else if (t == typeof (PQSMod_HeightColorMap))
                            {
                                (mod as PQSMod_HeightColorMap).landClasses = new [] { new PQSMod_HeightColorMap.LandClass("Class", 0, 1, Color.black, Color.white, 0) };
                            }
                            else if (t == typeof(PQSMod_HeightColorMap2))
                            {
                                (mod as PQSMod_HeightColorMap2).landClasses = new[] { new PQSMod_HeightColorMap2.LandClass("Class", 0, 1, Color.black, Color.white, 0) };
                            }
                            else if (t == typeof(PQSMod_HeightColorMapNoise))
                            {
                                (mod as PQSMod_HeightColorMapNoise).landClasses = new[] { new PQSMod_HeightColorMapNoise.LandClass("Class", 0, 1, Color.black, Color.white, 0) };
                            }
                            else if (t == typeof (PQSLandControl))
                            {
                                PQSLandControl lc = mod as PQSLandControl;
                                lc.altitudeSimplex = new Simplex();
                                lc.scatters = new PQSLandControl.LandClassScatter[0];
                                lc.landClasses = new [] { new PQSLandControl.LandClass() { altitudeRange = new PQSLandControl.LerpRange(),
                                                                                           coverageSimplex = new Simplex(),
                                                                                           longitudeRange = new PQSLandControl.LerpRange(),
                                                                                           latitudeDoubleRange = new PQSLandControl.LerpRange(),
                                                                                           latitudeRange = new PQSLandControl.LerpRange(),
                                                                                           scatter = new PQSLandControl.LandClassScatterAmount[0] } };
                                lc.latitudeSimplex = new Simplex();
                                lc.longitudeSimplex = new Simplex();
                            }
                            
                            // Edit the mod
                            _mod = mod;
                            _sphere = mod.sphere;
                            _mode = Modes.PQSMod;
                        }, new Rect(20, index * distance + 10, 350, 20));


                    // End Scroll
                    EndScrollView();
                }
            }

            /// <summary>
            /// Resets objects
            /// </summary>
            protected override void SetEditedObject()
            {
                _mod = null;
                _sphere = null;
                _mode = Modes.List;
            }
        }
    }
}