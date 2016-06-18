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
using Kopernicus.Configuration;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.MaterialWrapper;
using UnityEngine;
using Kopernicus.UI.Enumerations;

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
                {
                    Button("Add PQS", () =>
                    {
                        // Create a new PQS
                        GameObject controllerRoot = new GameObject(Current.name);
                        controllerRoot.transform.parent = Current.transform;
                        PQS pqsVersion = controllerRoot.AddComponent<PQS>();

                        // I am at this time unable to determine some of the magic parameters which cause the PQS to work... (Or just lazy but who cares :P)
                        PSystemBody Laythe = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                        Utility.CopyObjectFields(Laythe.pqsVersion, pqsVersion);
                        pqsVersion.surfaceMaterial = Laythe.pqsVersion.surfaceMaterial;

                        // Create the fallback material (always the same shader)
                        pqsVersion.fallbackMaterial = new PQSProjectionFallbackLoader();
                        pqsVersion.fallbackMaterial.name = Guid.NewGuid().ToString();

                        // Create the celestial body transform
                        GameObject mod = new GameObject("_CelestialBody");
                        mod.transform.parent = controllerRoot.transform;
                        PQSMod_CelestialBodyTransform transform = mod.AddComponent<PQSMod_CelestialBodyTransform>();
                        transform.sphere = pqsVersion;
                        transform.forceActivate = false;
                        transform.deactivateAltitude = 115000;
                        transform.forceRebuildOnTargetChange = false;
                        transform.planetFade = new PQSMod_CelestialBodyTransform.AltitudeFade();
                        transform.planetFade.fadeFloatName = "_PlanetOpacity";
                        transform.planetFade.fadeStart = 100000.0f;
                        transform.planetFade.fadeEnd = 110000.0f;
                        transform.planetFade.valueStart = 0.0f;
                        transform.planetFade.valueEnd = 1.0f;
                        transform.planetFade.secondaryRenderers = new List<GameObject>();
                        transform.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
                        transform.requirements = PQS.ModiferRequirements.Default;
                        transform.modEnabled = true;
                        transform.order = 10;

                        // Create the material direction
                        mod = new GameObject("_Material_SunLight");
                        mod.transform.parent = controllerRoot.gameObject.transform;
                        PQSMod_MaterialSetDirection lightDirection = mod.AddComponent<PQSMod_MaterialSetDirection>();
                        lightDirection.sphere = pqsVersion;
                        lightDirection.valueName = "_sunLightDirection";
                        lightDirection.requirements = PQS.ModiferRequirements.Default;
                        lightDirection.modEnabled = true;
                        lightDirection.order = 100;

                        // Create the UV planet relative position
                        mod = new GameObject("_Material_SurfaceQuads");
                        mod.transform.parent = controllerRoot.transform;
                        PQSMod_UVPlanetRelativePosition uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                        uvs.sphere = pqsVersion;
                        uvs.requirements = PQS.ModiferRequirements.Default;
                        uvs.modEnabled = true;
                        uvs.order = 999999;

                        // Crete the quad mesh colliders
                        mod = new GameObject("QuadMeshColliders");
                        mod.transform.parent = controllerRoot.gameObject.transform;
                        PQSMod_QuadMeshColliders collider = mod.AddComponent<PQSMod_QuadMeshColliders>();
                        collider.sphere = pqsVersion;
                        collider.maxLevelOffset = 0;
                        collider.physicsMaterial = new PhysicMaterial();
                        collider.physicsMaterial.name = "Ground";
                        collider.physicsMaterial.dynamicFriction = 0.6f;
                        collider.physicsMaterial.staticFriction = 0.8f;
                        collider.physicsMaterial.bounciness = 0.0f;
                        collider.physicsMaterial.frictionCombine = PhysicMaterialCombine.Maximum;
                        collider.physicsMaterial.bounceCombine = PhysicMaterialCombine.Average;
                        collider.requirements = PQS.ModiferRequirements.Default;
                        collider.modEnabled = true;
                        collider.order = 100;

                        // Assing the new PQS
                        Current.pqsController = pqsVersion;
                        pqsVersion.transform.position = Current.transform.position;
                        pqsVersion.transform.localPosition = Vector3.zero;

                        // Set mode
                        _mode = Modes.List;
                    }, new Rect(20, index*distance + 10, 350, 20));
                    return;
                }

                // Mode List
                if (_mode == Modes.List)
                {
                    // Get the PQS-Spheres and their mods
                    IEnumerable<PQS> pqsList = Current.GetComponentsInChildren<PQS>(true);
                    IEnumerable<PQSMod> pqsModList = Current.GetComponentsInChildren<PQSMod>(true);

                    // Scroll
                    BeginScrollView(250, (pqsList.Count() + pqsModList.Count()) * 25 + 115, 20);

                    // Index
                    index = 0;

                    // Render
                    foreach (PQS pqs in pqsList)
                    {
                        Button(pqs.ToString(), () =>
                        {
                            _mode = Modes.PQS;
                            _sphere = pqs;
                        }, new Rect(20, index * distance + 10, 350, 20));
                    }
                    foreach (PQSMod mod in pqsModList)
                    {
                        Button(mod.ToString(), () =>
                        {
                            _mode = Modes.PQSMod;
                            _sphere = mod.sphere;
                            _mod = mod;
                        }, new Rect(20, index * distance + 10, 350, 20));
                    }
                    index++;
                    Button("Add PQSMod", () => _mode = Modes.AddMod, new Rect(20, index * distance + 10, 350, 20));
                    if (Current.pqsController.ChildSpheres.All(s => s.name != Current.pqsController.name + "Ocean"))
                    {
                        Button("Add Ocean", () =>
                        {
                            // Generate the PQS object
                            GameObject gameObject = new GameObject("Ocean");
                            gameObject.layer = Constants.GameLayers.LocalSpace;
                            PQS ocean = gameObject.AddComponent<PQS>();

                            // Setup materials
                            PSystemBody Body = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Laythe");
                            foreach (PQS oc in Body.pqsVersion.GetComponentsInChildren<PQS>(true))
                            {
                                if (oc.name != "LaytheOcean") continue;

                                // Copying Laythes Ocean-properties
                                Utility.CopyObjectFields<PQS>(oc, ocean);
                            }

                            // Load our new Material into the PQS
                            ocean.surfaceMaterial = new PQSOceanSurfaceQuadLoader(ocean.surfaceMaterial);
                            ocean.surfaceMaterial.name = Guid.NewGuid().ToString();

                            // Load fallback material into the PQS            
                            ocean.fallbackMaterial = new PQSOceanSurfaceQuadFallbackLoader(ocean.fallbackMaterial);
                            ocean.fallbackMaterial.name = Guid.NewGuid().ToString();

                            // Create the UV planet relative position
                            GameObject mod = new GameObject("_Material_SurfaceQuads");
                            mod.transform.parent = gameObject.transform;
                            PQSMod_UVPlanetRelativePosition uvs = mod.AddComponent<PQSMod_UVPlanetRelativePosition>();
                            uvs.sphere = ocean;
                            uvs.requirements = PQS.ModiferRequirements.Default;
                            uvs.modEnabled = true;
                            uvs.order = 999999;

                            // Create the AerialPerspective Material
                            AerialPerspectiveMaterial mat = new AerialPerspectiveMaterial();
                            mat.Create(ocean);

                            // Create the OceanFX
                            OceanFX oceanFX = new OceanFX();
                            oceanFX.Create(ocean);

                            // Apply the Ocean
                            ocean.transform.parent = Current.pqsController.transform;

                            // Add the ocean PQS to the secondary renders of the CelestialBody Transform
                            PQSMod_CelestialBodyTransform transform = Current.pqsController.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).FirstOrDefault(mod_ => mod_.transform.parent == Current.pqsController.transform);
                            transform.planetFade.secondaryRenderers.Add(ocean.gameObject);
                            typeof(PQS).GetField("_childSpheres", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(Current.pqsController, null);

                            // Names!
                            ocean.name = Current.pqsController.name + "Ocean";
                            ocean.gameObject.name = Current.pqsController.name + "Ocean";
                            ocean.transform.name = Current.pqsController.name + "Ocean";

                            // Set up the ocean PQS
                            ocean.parentSphere = Current.pqsController;
                            ocean.transform.position = Current.pqsController.transform.position;
                            ocean.transform.localPosition = Vector3.zero;
                            ocean.radius = Current.Radius;
                        }, new Rect(20, index * distance + 10, 350, 20));
                    }
                    else
                    {
                        Button("Remove Ocean", () =>
                        {
                            // Find atmosphere the ocean PQS
                            PQS ocean = Current.pqsController.GetComponentsInChildren<PQS>(true).First(pqs => pqs != Current.pqsController);
                            PQSMod_CelestialBodyTransform cbt = Current.pqsController.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true).First();

                            // Destroy the ocean PQS (this could be bad - destroying the secondary fades...)
                            cbt.planetFade.secondaryRenderers.Remove(ocean.gameObject);
                            typeof(PQS).GetField("_childSpheres", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(Current.pqsController, null);
                            cbt.secondaryFades = new PQSMod_CelestialBodyTransform.AltitudeFade[0];
                            ocean.transform.parent = null;
                            UnityEngine.Object.Destroy(ocean);
                        }, new Rect(20, index * distance + 10, 350, 20));
                    }

                    // End Scroll
                    EndScrollView();
                }

                // Mode PQS
                if (_mode == Modes.PQS)
                {
                    // Scroll
                    BeginScrollView(250, Utils.GetScrollSize<PQS>() + Utils.GetScrollSize<HazardousOcean>() + 65, 20);

                    // Index
                    index = 0;

                    // Render the PQS
                    RenderObject(_sphere);

                    // If it is an ocean, create an Hazardous Ocean button
                    if (PQSOceanSurfaceQuad.UsesSameShader(_sphere.surfaceMaterial))
                    {
                        Label("hazardousOcean"); index--;
                        if (_sphere.GetComponent<HazardousOcean>() != null)
                        {
                            Button("Edit", () =>
                            {
                                UIController.Instance.SetEditedObject(KittopiaWindows.Curve, _sphere.GetComponent<HazardousOcean>().heatCurve ?? new FloatCurve(), c => _sphere.GetComponent<HazardousOcean>().heatCurve = c);
                                UIController.Instance.EnableWindow(KittopiaWindows.Curve);
                            }, new Rect(200, index*distance + 10, 75, 20)); index--;
                            Button("Remove", () => UnityEngine.Object.DestroyImmediate(_sphere.GetComponent<HazardousOcean>()), new Rect(285, index*distance + 10, 75, 20));
                        }
                        else
                        {
                            Button("Add Hazardous Ocean", () => _sphere.gameObject.AddComponent<HazardousOcean>(), new Rect(200, index * distance + 10, 170, 20));
                        }
                    }
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
                                PQS mun = Utility.FindBody(Injector.StockSystemPrefab.rootBody, "Mun").pqsVersion;
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