/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using Kopernicus.Configuration;
using Kopernicus.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.MaterialWrapper;
using Kopernicus.UI.Extensions;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Class to save a CelestialBody as a Kopernicus-configuration-file
        /// </summary>
        public class ConfigIO
        {
            /// <summary>
            /// Types that need special manipulation
            /// </summary>
            public static readonly Type[] writeableTypes = new Type[]
            {
                typeof(ColorParser),
                typeof(Vector2Parser),
                typeof(Vector3DParser),
                typeof(Vector3Parser),
                typeof(Vector4Parser),
                typeof(QuaternionParser),
                typeof(QuaternionDParser)
            };

            /// <summary>
            /// Saves the celestial body into a Kopernicus formatted ConfigNode.
            /// </summary>
            public static void SaveCelestial(CelestialBody planet)
            {
                // Create the main node
                ConfigNode root = new ConfigNode("@Kopernicus:NEEDS[!Kopernicus]");
                ConfigNode body = root.AddNode("Body");

                // Create the body
                body.AddValue("name", planet.name);

                // Properties
                ConfigNode properties = WriteObjectToConfigNode("Properties", ref body, new PropertiesLoader(planet));
                if (planet.BiomeMap != null)
                {
                    ConfigNode biomes = properties.AddNode("Biomes");
                    foreach (CBAttributeMapSO.MapAttribute biome in planet.BiomeMap.Attributes)
                        WriteObjectToConfigNode("Biome", ref biomes, new BiomeLoader(biome));
                }
                if (planet.scienceValues != null)
                    WriteObjectToConfigNode("ScienceValues", ref properties, new ScienceValuesLoader(planet.scienceValues));

                // Orbit
                if (planet.orbitDriver)
                    WriteObjectToConfigNode("Orbit", ref body, new OrbitLoader(planet) { referenceBody = planet.orbit.referenceBody.name }); // Haha

                // Atmosphere
                if (planet.atmosphere)
                {
                    try
                    {
                        ConfigNode atmo = WriteObjectToConfigNode("Atmosphere", ref body, new AtmosphereLoader(planet));
                        WriteObjectToConfigNode("AtmosphereFromGround", ref atmo, new AtmosphereFromGroundLoader(planet) { afg = planet.afg }); // Haha
                    }
                    catch { }
                }

                // ScaledVersion
                ScaledVersionLoader scaledLoader = new ScaledVersionLoader(planet);
                ConfigNode scaled = WriteObjectToConfigNode("ScaledVersion", ref body, scaledLoader);
                if (scaledLoader.type == BodyType.Star)
                {
                    WriteObjectToConfigNode("Material", ref scaled, new EmissiveMultiRampSunspotsLoader(planet.scaledBody.GetComponent<Renderer>().sharedMaterial));   
                    WriteObjectToConfigNode("Light", ref scaled, new LightShifterLoader() { lsc = planet.scaledBody.GetComponentsInChildren<LightShifter>(true)[0] });
                    if (planet.scaledBody.GetComponentsInChildren<SunCoronas>().Length != 0)
                    {
                        ConfigNode coronas = scaled.AddNode("Coronas");
                        foreach (SunCoronas corona in planet.scaledBody.GetComponentsInChildren<SunCoronas>(true))
                            WriteObjectToConfigNode("Corona", ref coronas, new CoronaLoader(corona));
                    }
                }
                else if (scaledLoader.type == BodyType.Atmospheric)
                    WriteObjectToConfigNode("Material", ref scaled, new ScaledPlanetRimAerialLoader(planet.scaledBody.GetComponent<Renderer>().sharedMaterial));
                else
                    WriteObjectToConfigNode("Material", ref scaled, new ScaledPlanetSimpleLoader(planet.scaledBody.GetComponent<Renderer>().sharedMaterial));

                // Particles
                ConfigNode particles = body.AddNode("Particles");
                foreach (PlanetParticleEmitter e in planet.scaledBody.GetComponentsInChildren<PlanetParticleEmitter>(true))
                {
                    ParticleLoader loader = new ParticleLoader(planet, e.gameObject);
                    WriteObjectToConfigNode("Particle", ref particles, loader);
                }

                // Rings
                if (planet.scaledBody.GetComponentsInChildren<Ring>(true).Length != 0)
                {
                    ConfigNode rings = body.AddNode("Rings");
                    foreach (Ring ring in planet.scaledBody.GetComponentsInChildren<Ring>(true))
                        WriteObjectToConfigNode("Ring", ref rings, new RingLoader(ring));
                }

                // PQS
                if (planet.pqsController)
                    WritePQSToConfigNode(planet.pqsController, ref body, false);

                // Ocean
                if (planet.ocean)
                    WritePQSToConfigNode(planet.pqsController.ChildSpheres[0], ref body, true);

                // Save the node
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Config/");
                ConfigNode save = new ConfigNode();
                save.AddNode(root);
                save.Save("GameData/KittopiaTech/Config/" + planet.name + ".cfg", "KittopiaTech - a Kopernicus Visual Editor");
            }

            /// <summary>
            /// Formats a texture path
            /// </summary>
            public static String Format(UnityEngine.Object o)
            {
                return GameDatabase.Instance.ExistsTexture(o.name) || Utility.TextureExists(o.name) ? o.name : "BUILTIN/" + o.name;
            }

            /// <summary>
            /// Writes an Object to a Configuration Node, using it's parser targets
            /// </summary>
            /// <returns></returns>
            public static ConfigNode WriteObjectToConfigNode<T>(string name, ref ConfigNode node, T target)
            {
                // Start
                ConfigNode config = node.AddNode(name);

                // Crawl it's member infos
                foreach (MemberInfo member in target.GetType().GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    // Get the parser target
                    ParserTarget[] targets = member.GetCustomAttributes(typeof(ParserTarget), false) as ParserTarget[];
                    if (targets.Length == 0)
                        continue;

                    // Get stuff
                    RequireConfigType[] types = (member.MemberType == MemberTypes.Field ? (member as FieldInfo).FieldType : (member as PropertyInfo).PropertyType).GetCustomAttributes(typeof(RequireConfigType), false) as RequireConfigType[];

                    // Write
                    if ((member.Name == "mapOcean" || member.Name == "oceanColor" || member.Name == "oceanHeight" || member.Name == "density") && typeof(T) == typeof(OceanLoader)) continue;
                    object value = member.GetValue(target);
                    Type targetType = member.GetMemberType();

                    if (value == null)
                        continue;

                    // Type
                    ConfigType type = types.Length == 1 ? types[0].type : targetType.Name.StartsWith("MapSOParser_") || targetType == typeof(string) ? ConfigType.Value : ConfigType.Node;

                    // Convert
                    if (targetType != typeof(string) && (type == ConfigType.Value || targetType == typeof(FloatCurveParser)))
                    {
                        value = targetType == typeof(PhysicsMaterialParser) ?
                            targetType.GetProperty("material").GetValue(value, null) :
                            targetType == typeof(FloatCurveParser) ?
                                targetType.GetProperty("curve").GetValue(value, null) :
                                targetType.GetField("value").GetValue(value);
                        if (value == null || targetType == typeof(LandControl.LandClassScatterLoader.StockMaterialParser))
                            continue;
                        if (value.GetType().GetInterface("IEnumerable") != null && targetType != typeof(string))
                            value = String.Join(targetType == typeof (StringCollectionParser) ? "," : " ", (value as IEnumerable).Cast<System.Object>().Select(o => o.ToString()).ToArray());
                    }

                    // Do ConfigNode stuff
                    if (writeableTypes.Contains(targetType))
                    {
                        if (value is Vector2)
                            value = ConfigNode.WriteVector((Vector2)value);
                        if (value is Vector3)
                            value = ConfigNode.WriteVector((Vector3)value);
                        if (value is Vector3d)
                            value = ConfigNode.WriteVector((Vector3d)value);
                        if (value is Vector4)
                            value = ConfigNode.WriteVector((Vector4)value);
                        if (value is Quaternion)
                            value = ConfigNode.WriteQuaternion((Quaternion)value);
                        if (value is QuaternionD)
                            value = ConfigNode.WriteQuaternion((QuaternionD)value);
                        if (value is Color)
                            value = ConfigNode.WriteColor((Color)value);
                    }

                    // Texture
                    Type[] textureTypes = new Type[] { typeof(Mesh), typeof(Texture2D), typeof(Texture), typeof(MapSO), typeof(CBAttributeMapSO) };
                    if (textureTypes.Contains(value.GetType()) || textureTypes.Contains(value.GetType().BaseType))
                        value = Format(value as UnityEngine.Object);

                    // Write
                    if (type == ConfigType.Value && value.GetType() != typeof(FloatCurve) && value.GetType() != typeof(AnimationCurve))
                        config.AddValue(targets[0].fieldName, value);
                    else if (value.GetType() == typeof(FloatCurve))
                        (value as FloatCurve).Save(config.AddNode(targets[0].fieldName));
                    else if (value.GetType() == typeof(AnimationCurve))
                        new FloatCurve((value as AnimationCurve).keys).Save(config.AddNode(targets[0].fieldName));
                }
                return config;
            }

            /// <summary>
            /// Writes a PQS to a new config node
            /// </summary>
            public static void WritePQSToConfigNode(PQS pqsVersion, ref ConfigNode body, bool ocean)
            {
                ConfigNode pqs = null;
                if (!ocean)
                {
                    PQSLoader pqsLoader = new PQSLoader(pqsVersion);
                    pqs = WriteObjectToConfigNode("PQS", ref body, pqsLoader);
                    WriteObjectToConfigNode("Material", ref pqs, pqsLoader.surfaceMaterial);
                    WriteObjectToConfigNode("FallbackMaterial", ref pqs, pqsLoader.fallbackMaterial);
                    if (pqsLoader.physicsMaterial.material != null)
                        WriteObjectToConfigNode("PhysicsMaterial", ref pqs, pqsLoader.physicsMaterial);
                }
                else
                {
                    CelestialBody cb = pqsVersion.parentSphere.GetComponentInParent<CelestialBody>();
                    OceanLoader oceanLoader = new OceanLoader(pqsVersion);
                    pqs = WriteObjectToConfigNode("Ocean", ref body, oceanLoader);
                    pqs.AddValue("ocean", pqsVersion.parentSphere.mapOcean && cb.ocean);
                    pqs.AddValue("oceanColor", pqsVersion.parentSphere.mapOceanColor);
                    pqs.AddValue("oceanHeight", pqsVersion.parentSphere.mapOceanHeight);
                    pqs.AddValue("density", cb.oceanDensity);
                    WriteObjectToConfigNode("Material", ref pqs, oceanLoader.surfaceMaterial);
                    WriteObjectToConfigNode("FallbackMaterial", ref pqs, oceanLoader.fallbackMaterial);
                    WriteObjectToConfigNode("Fog", ref pqs, new FogLoader(Part.GetComponentUpwards<CelestialBody>(pqsVersion.gameObject)));
                    if (pqsVersion.gameObject.GetComponent<HazardousOcean>() != null)
                        pqsVersion.gameObject.GetComponent<HazardousOcean>().heatCurve.Save(pqs.AddNode("HazardousOcean"));
                }

                // Mods
                IEnumerable<PQSMod> mods = pqsVersion.GetComponentsInChildren<PQSMod>(true).Where(m => ocean ? true : m.sphere == pqsVersion);

                // Get all loaded types
                IEnumerable<Type> types = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetTypes());
                if (mods.Count() != 0)
                {
                    ConfigNode modsNode = pqs.AddNode("Mods");
                    foreach (PQSMod Mod in mods)
                    {
                        // Fix PQSMods
                        PQSMod mod = Mod;

                        Type loaderType = types.FirstOrDefault(t => t.Name == Mod.GetType().Name.Replace("PQSMod_", "").Replace("PQS", ""));

                        // No loader
                        if (loaderType == null)
                            continue;

                        // Create the loader
                        object loader = Activator.CreateInstance(loaderType);

                        // Reflection, because C# being silly... :/
                        PropertyInfo info = loaderType.GetProperty("mod");
                        if (info == null) continue;
                        info.SetValue(loader, mod, null);

                        // Load
                        ConfigNode modNode = WriteObjectToConfigNode(loaderType.Name, ref modsNode, loader);
                        modNode.AddValue("name", mod.name);
                        IEnumerable<PQSMod> existingMods = pqsVersion.GetComponentsInChildren<PQSMod>(true).Where(m => m.GetType() == mod.GetType() && m.sphere == pqsVersion && m.name == mod.name);
                        modNode.AddValue("index", existingMods.ToList().IndexOf(mod));

                        // Submods
                        if (mod is PQSMod_HeightColorMap)
                        {
                            PQSMod_HeightColorMap hcm = mod as PQSMod_HeightColorMap;
                            if (hcm.landClasses != null)
                            {
                                ConfigNode landClasses = modNode.AddNode("LandClasses");
                                foreach (PQSMod_HeightColorMap.LandClass landClass in hcm.landClasses)
                                    WriteObjectToConfigNode("Class", ref landClasses, new HeightColorMap.LandClassLoader(landClass));
                            }
                        }
                        if (mod is PQSMod_HeightColorMap2)
                        {
                            PQSMod_HeightColorMap2 hcm2 = mod as PQSMod_HeightColorMap2;
                            if (hcm2.landClasses != null)
                            {
                                ConfigNode landClasses = modNode.AddNode("LandClasses");
                                foreach (PQSMod_HeightColorMap2.LandClass landClass in hcm2.landClasses)
                                    WriteObjectToConfigNode("Class", ref landClasses, new HeightColorMap2.LandClassLoader2(landClass));
                            }
                        }
                        if (mod is PQSMod_HeightColorMapNoise)
                        {
                            PQSMod_HeightColorMapNoise hcmNoise = mod as PQSMod_HeightColorMapNoise;
                            if (hcmNoise.landClasses != null)
                            {
                                ConfigNode landClasses = modNode.AddNode("LandClasses");
                                foreach (PQSMod_HeightColorMapNoise.LandClass landClass in hcmNoise.landClasses)
                                    WriteObjectToConfigNode("Class", ref landClasses, new HeightColorMapNoise.LandClassLoaderNoise(landClass));
                            }
                        }
                        if (mod is PQSLandControl)
                        {
                            PQSLandControl lc = mod as PQSLandControl;
                            if (lc.altitudeSimplex != null)
                                WriteObjectToConfigNode("altitudeSimplex", ref modNode, new LandControl.SimplexLoader(lc.altitudeSimplex));
                            if (lc.latitudeSimplex != null)
                                WriteObjectToConfigNode("latitudeSimplex", ref modNode, new LandControl.SimplexLoader(lc.latitudeSimplex));
                            if (lc.longitudeSimplex != null)
                                WriteObjectToConfigNode("longitudeSimplex", ref modNode, new LandControl.SimplexLoader(lc.longitudeSimplex));
                            if (lc.landClasses != null)
                            {
                                ConfigNode landClasses = modNode.AddNode("landClasses");
                                foreach (PQSLandControl.LandClass landClass in lc.landClasses)
                                {
                                    ConfigNode lcNode = WriteObjectToConfigNode("Class", ref landClasses, new LandControl.LandClassLoader(landClass));
                                    WriteObjectToConfigNode("altitudeRange", ref lcNode, new LandControl.LerpRangeLoader(landClass.altitudeRange));
                                    WriteObjectToConfigNode("coverageSimplex", ref lcNode, new LandControl.SimplexLoader(landClass.coverageSimplex));
                                    WriteObjectToConfigNode("latitudeDoubleRange", ref lcNode, new LandControl.LerpRangeLoader(landClass.latitudeDoubleRange));
                                    WriteObjectToConfigNode("latitudeRange", ref lcNode, new LandControl.LerpRangeLoader(landClass.latitudeRange));
                                    WriteObjectToConfigNode("longitudeRange", ref lcNode, new LandControl.LerpRangeLoader(landClass.longitudeRange));
                                    WriteObjectToConfigNode("noiseSimplex", ref lcNode, new LandControl.SimplexLoader(landClass.noiseSimplex));
                                    if (landClass.scatter != null)
                                    {
                                        ConfigNode amount = lcNode.AddNode("scatters");
                                        foreach (PQSLandControl.LandClassScatterAmount scatterAmount in landClass.scatter)
                                            WriteObjectToConfigNode("Scatter", ref amount, new LandControl.LandClassScatterAmountLoader(scatterAmount));
                                    }
                                }
                            }
                            if (lc.scatters != null)
                            {
                                ConfigNode scatters = modNode.AddNode("scatters");
                                foreach (PQSLandControl.LandClassScatter scatter in lc.scatters)
                                {
                                    LandControl.LandClassScatterLoader classLoader = new LandControl.LandClassScatterLoader(scatter);
                                    if (scatter.material.shader == new NormalDiffuse().shader)
                                        classLoader.customMaterial = new NormalDiffuseLoader(scatter.material);
                                    else if (scatter.material.shader == new NormalBumped().shader)
                                        classLoader.customMaterial = new NormalBumpedLoader(scatter.material);
                                    else if (scatter.material.shader == new NormalDiffuseDetail().shader)
                                        classLoader.customMaterial = new NormalDiffuseDetailLoader(scatter.material);
                                    else if (scatter.material.shader == new DiffuseWrapLoader().shader)
                                        classLoader.customMaterial = new DiffuseWrapLoader(scatter.material);
                                    else if (scatter.material.shader == new AlphaTestDiffuse().shader)
                                        classLoader.customMaterial = new AlphaTestDiffuseLoader(scatter.material);
                                    else if (scatter.material.shader == new AerialTransCutout().shader)
                                        classLoader.customMaterial = new AerialTransCutoutLoader(scatter.material);
                                    ConfigNode scatterNode = WriteObjectToConfigNode("Scatter", ref scatters, classLoader);
                                    WriteObjectToConfigNode("Material", ref scatterNode, classLoader.customMaterial);
                                    scatterNode.AddNode("Experiment");
                                }
                            }
                        }
                        if (mod is PQSMod_VertexPlanet)
                        {
                            PQSMod_VertexPlanet vp = mod as PQSMod_VertexPlanet;
                            WriteObjectToConfigNode("ContinentalSimplex", ref modNode, new VertexPlanet.SimplexWrapper(vp.continental));
                            WriteObjectToConfigNode("RuggednessSimplex", ref modNode, new VertexPlanet.SimplexWrapper(vp.continentalRuggedness));
                            WriteObjectToConfigNode("SharpnessNoise", ref modNode, new VertexPlanet.NoiseModWrapper(vp.continentalSharpness));
                            WriteObjectToConfigNode("SharpnessSimplexMap", ref modNode, new VertexPlanet.SimplexWrapper(vp.continentalSharpnessMap));
                            WriteObjectToConfigNode("TerrainTypeSimplex", ref modNode, new VertexPlanet.SimplexWrapper(vp.terrainType));
                            if (vp.landClasses != null)
                            {
                                ConfigNode landClasses = modNode.AddNode("LandClasses");
                                foreach (PQSMod_VertexPlanet.LandClass landClass in vp.landClasses)
                                {
                                    ConfigNode classNode = WriteObjectToConfigNode("Class", ref landClasses, new VertexPlanet.LandClassLoader(landClass));
                                    WriteObjectToConfigNode("SimplexNoiseMap", ref classNode, new VertexPlanet.SimplexWrapper(landClass.colorNoiseMap));
                                }
                            }
                        }
                        if (mod is PQSMod_OceanFX)
                        {
                            ConfigNode watermain = modNode.AddNode((loader as OceanFX).watermain);
                            foreach (ConfigNode.Value value in watermain.values)
                                value.value = Format(Resources.FindObjectsOfTypeAll<Texture2D>().First(o => o.name == value.value));
                        }
                    }
                }
            }
        }
    }
}