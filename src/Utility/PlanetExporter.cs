/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kopernicus.Components;
using Kopernicus.Configuration;
using Kopernicus.Configuration.ModLoader;
using Kopernicus.MaterialWrapper;
using Kopernicus.UI.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// Class to save a CelestialBody as a Kopernicus-configuration-file
        /// </summary>
        public class PlanetExporter
        {
            /// <summary>
            /// Types that need special manipulation
            /// </summary>
            public static readonly Type[] writeableTypes = {
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

                // Properties.Biomes
                if (planet.BiomeMap != null)
                {
                    ConfigNode biomes = properties.AddNode("Biomes");
                    foreach (CBAttributeMapSO.MapAttribute biome in planet.BiomeMap.Attributes)
                        WriteObjectToConfigNode("Biome", ref biomes, new BiomeLoader(biome));
                }

                // Properties.ScienceValues
                if (planet.scienceValues != null)
                {
                    WriteObjectToConfigNode("ScienceValues", ref properties, new ScienceValuesLoader(planet.scienceValues));
                }

                // Orbit
                if (planet.orbitDriver)
                    WriteObjectToConfigNode("Orbit", ref body, new OrbitLoader(planet));

                // Atmosphere
                if (planet.atmosphere)
                {
                    ConfigNode atmo = WriteObjectToConfigNode("Atmosphere", ref body, new AtmosphereLoader(planet));

                    // Atmosphere.AtmosphereFromGround
                    if (planet.afg != null)
                    {
                        WriteObjectToConfigNode("AtmosphereFromGround", ref atmo, new AtmosphereFromGroundLoader(planet));
                    }
                }

                // ScaledVersion
                ScaledVersionLoader scaledLoader = new ScaledVersionLoader(planet);
                ConfigNode scaled = WriteObjectToConfigNode("ScaledVersion", ref body, scaledLoader);

                // ScaledVersion.Material
                Material material = planet.scaledBody.GetComponent<Renderer>().sharedMaterial;
                if (scaledLoader.type == BodyType.Star)
                {
                    WriteObjectToConfigNode("Material", ref scaled, new EmissiveMultiRampSunspotsLoader(material));

                    // ScaledVersion.Light
                    WriteObjectToConfigNode("Light", ref scaled, new LightShifterLoader(planet));

                    // ScaledVersion.Coronas
                    if (planet.scaledBody.GetComponentsInChildren<SunCoronas>().Length != 0)
                    {
                        ConfigNode coronas = scaled.AddNode("Coronas");
                        foreach (SunCoronas corona in planet.scaledBody.GetComponentsInChildren<SunCoronas>(true))
                            WriteObjectToConfigNode("Corona", ref coronas, new CoronaLoader(corona));
                    }
                }
                else if (scaledLoader.type == BodyType.Atmospheric)
                {
                    WriteObjectToConfigNode("Material", ref scaled, new ScaledPlanetRimAerialLoader());
                }
                else
                {
                    WriteObjectToConfigNode("Material", ref scaled, new ScaledPlanetSimpleLoader(material));
                }

                // Particles
                ConfigNode particles = body.AddNode("Particles");
                foreach (PlanetParticleEmitter e in planet.scaledBody.GetComponentsInChildren<PlanetParticleEmitter>(true))
                {
                    ParticleLoader loader = new ParticleLoader(e);
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
                {
                    WritePQSToConfigNode(planet.pqsController, ref body, false);
                }

                // Ocean
                if (planet.ocean)
                {
                    WritePQSToConfigNode(planet.pqsController.ChildSpheres[0], ref body, true);
                }

                // Save the node
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Config/");
                ConfigNode save = new ConfigNode();
                save.AddNode(root);
                save.Save("GameData/KittopiaTech/Config/" + planet.name + ".cfg", "KittopiaTech - a Kopernicus Visual Editor");
            }

            /// <summary>
            /// Formats a texture path
            /// </summary>
            public static String Format(Object o)
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
                    // Get the first parser target
                    ParserTarget[] targets = member.GetCustomAttributes(typeof(ParserTarget), false) as ParserTarget[];
                    if (targets.Length == 0)
                        continue;

                    // Is this a node or a value?
                    RequireConfigType[] configTypes = (member.MemberType == MemberTypes.Field ? (member as FieldInfo).FieldType : (member as PropertyInfo).PropertyType).GetCustomAttributes(typeof(RequireConfigType), false) as RequireConfigType[];

                    // Write
                    System.Object memberValue = member.GetValue(target);
                    Type memberType = member.GetMemberType();

                    if (memberValue == null)
                        continue;

                    // Type
                    ConfigType configType = configTypes.Length == 1 ? configTypes[0].Type : memberType.Name.StartsWith("MapSOParser_") || memberType == typeof(string) ? ConfigType.Value : ConfigType.Node;

                    // Convert
                    if (memberType != typeof(string) && (configType == ConfigType.Value || memberType == typeof(FloatCurveParser)))
                    {
                        memberValue = memberType == typeof(PhysicsMaterialParser) ?
                            memberType.GetProperty("material").GetValue(memberValue, null) :
                            memberType == typeof(FloatCurveParser) ?
                                memberType.GetProperty("curve").GetValue(memberValue, null) :
                                memberType.GetField("value").GetValue(memberValue);
                        if (memberValue == null || memberType == typeof(StockMaterialParser))
                        {
                            continue;
                        }
                        if (memberValue.GetType().GetInterface("IEnumerable") != null && memberType != typeof(string))
                        {
                            memberValue = String.Join(memberType == typeof(StringCollectionParser) ? "," : " ", (memberValue as IEnumerable).Cast<System.Object>().Select(o => o.ToString()).ToArray());
                        }
                    }

                    // Format Unity types
                    if (writeableTypes.Contains(memberType))
                    {
                        if (memberValue is Vector2)
                            memberValue = ConfigNode.WriteVector((Vector2)memberValue);
                        if (memberValue is Vector3)
                            memberValue = ConfigNode.WriteVector((Vector3)memberValue);
                        if (memberValue is Vector3d)
                            memberValue = ConfigNode.WriteVector((Vector3d)memberValue);
                        if (memberValue is Vector4)
                            memberValue = ConfigNode.WriteVector((Vector4)memberValue);
                        if (memberValue is Quaternion)
                            memberValue = ConfigNode.WriteQuaternion((Quaternion)memberValue);
                        if (memberValue is QuaternionD)
                            memberValue = ConfigNode.WriteQuaternion((QuaternionD)memberValue);
                        if (memberValue is Color)
                            memberValue = ConfigNode.WriteColor((Color)memberValue);
                    }

                    // Texture
                    Type[] textureTypes = { typeof(Mesh), typeof(Texture2D), typeof(Texture), typeof(MapSO), typeof(CBAttributeMapSO) };
                    if (textureTypes.Contains(memberValue.GetType()) || textureTypes.Contains(memberValue.GetType().BaseType))
                        memberValue = Format(memberValue as Object);

                    // Write
                    if (configType == ConfigType.Value && memberValue.GetType() != typeof(FloatCurve) && memberValue.GetType() != typeof(AnimationCurve))
                    {
                        config.AddValue(targets[0].FieldName, memberValue);
                    }
                    else if (memberValue.GetType() == typeof(FloatCurve))
                    {
                        (memberValue as FloatCurve).Save(config.AddNode(targets[0].FieldName));
                    }
                    else if (memberValue is AnimationCurve)
                    {
                        new FloatCurve((memberValue as AnimationCurve).keys).Save(config.AddNode(targets[0].FieldName));
                    }
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
                    CelestialBody cb = pqsVersion.parentSphere.GetComponentInParent<CelestialBody>();
                    PQSLoader pqsLoader = new PQSLoader(cb);
                    pqs = WriteObjectToConfigNode("PQS", ref body, pqsLoader);
                    WriteObjectToConfigNode("Material", ref pqs, pqsLoader.surfaceMaterial);
                    WriteObjectToConfigNode("FallbackMaterial", ref pqs, pqsLoader.fallbackMaterial);
                    if (pqsLoader.physicsMaterial.Value != null)
                        WriteObjectToConfigNode("PhysicsMaterial", ref pqs, pqsLoader.physicsMaterial);
                }
                else
                {
                    CelestialBody cb = pqsVersion.parentSphere.GetComponentInParent<CelestialBody>();
                    OceanLoader oceanLoader = new OceanLoader(cb);
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
                IEnumerable<PQSMod> mods = pqsVersion.GetComponentsInChildren<PQSMod>(true).Where(m => (ocean || m.sphere == pqsVersion) && !(m is PQSCity) && !(m is PQSCity2));

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
                        IEnumerable<PQSMod> existingMods = pqsVersion.GetComponentsInChildren<PQSMod>(true).Where(m => m.GetType() == mod.GetType() && m.sphere == pqsVersion && m.name == mod.name);
                        modNode.AddValue("index", existingMods.ToList().IndexOf(mod));

                        // Submods
                        PQSMod_HeightColorMap hcm = mod as PQSMod_HeightColorMap;
                        if (hcm?.landClasses != null)
                        {
                            ConfigNode landClasses = modNode.AddNode("LandClasses");
                            foreach (PQSMod_HeightColorMap.LandClass landClass in hcm.landClasses)
                                WriteObjectToConfigNode("Class", ref landClasses, new HeightColorMap.LandClassLoader(landClass));
                        }
                        PQSMod_HeightColorMap2 hcm2 = mod as PQSMod_HeightColorMap2;
                        if (hcm2?.landClasses != null)
                        {
                            ConfigNode landClasses = modNode.AddNode("LandClasses");
                            foreach (PQSMod_HeightColorMap2.LandClass landClass in hcm2.landClasses)
                                WriteObjectToConfigNode("Class", ref landClasses, new HeightColorMap2.LandClassLoader(landClass));
                        }
                        PQSMod_HeightColorMapNoise hcmNoise = mod as PQSMod_HeightColorMapNoise;
                        if (hcmNoise?.landClasses != null)
                        {
                            ConfigNode landClasses = modNode.AddNode("LandClasses");
                            foreach (PQSMod_HeightColorMapNoise.LandClass landClass in hcmNoise.landClasses)
                                WriteObjectToConfigNode("Class", ref landClasses, new HeightColorMapNoise.LandClassLoader(landClass));
                        }
                        if (mod is PQSLandControl)
                        {
                            PQSLandControl lc = mod as PQSLandControl;
                            if (lc.altitudeSimplex != null)
                            {
                                KopernicusSimplexWrapper lcaltsimpwrap = new KopernicusSimplexWrapper(lc.altitudeBlend, lc.altitudeOctaves, lc.altitudePersistance, lc.altitudeFrequency);
                                lcaltsimpwrap.seed = lc.altitudeSeed;
                                WriteObjectToConfigNode("altitudeSimplex", ref modNode, new VertexPlanet.SimplexLoader(lcaltsimpwrap));
                            }
                            if (lc.latitudeSimplex != null)
                            {
                                KopernicusSimplexWrapper lclatsimpwrap = new KopernicusSimplexWrapper(lc.latitudeBlend, lc.latitudeOctaves, lc.latitudePersistance, lc.latitudeFrequency);
                                lclatsimpwrap.seed = lc.latitudeSeed;
                                WriteObjectToConfigNode("latitudeSimplex", ref modNode, new VertexPlanet.SimplexLoader(lclatsimpwrap));
                            }
                            if (lc.longitudeSimplex != null)
                            {
                                KopernicusSimplexWrapper lclongsimpwrap = new KopernicusSimplexWrapper(lc.longitudeBlend, lc.longitudeOctaves, lc.longitudePersistance, lc.longitudeFrequency);
                                lclongsimpwrap.seed = lc.longitudeSeed;
                                WriteObjectToConfigNode("longitudeSimplex", ref modNode, new VertexPlanet.SimplexLoader(lclongsimpwrap));
                            }
                            if (lc.landClasses != null)
                            {
                                ConfigNode landClasses = modNode.AddNode("landClasses");
                                foreach (PQSLandControl.LandClass landClass in lc.landClasses)
                                {
                                    ConfigNode lcNode = WriteObjectToConfigNode("Class", ref landClasses, new LandControl.LandClassLoader(landClass));
                                    WriteObjectToConfigNode("altitudeRange", ref lcNode, new LandControl.LerpRangeLoader(landClass.altitudeRange));
                                    KopernicusSimplexWrapper lccovsimpwrap = new KopernicusSimplexWrapper(landClass.coverageBlend, landClass.coverageOctaves, landClass.coveragePersistance, landClass.coverageFrequency);
                                    lccovsimpwrap.seed = landClass.coverageSeed;
                                    WriteObjectToConfigNode("coverageSimplex", ref lcNode, new VertexPlanet.SimplexLoader(lccovsimpwrap));
                                    WriteObjectToConfigNode("latitudeDoubleRange", ref lcNode, new LandControl.LerpRangeLoader(landClass.latitudeDoubleRange));
                                    WriteObjectToConfigNode("latitudeRange", ref lcNode, new LandControl.LerpRangeLoader(landClass.latitudeRange));
                                    WriteObjectToConfigNode("longitudeRange", ref lcNode, new LandControl.LerpRangeLoader(landClass.longitudeRange));
                                    KopernicusSimplexWrapper lcnoisesimpwrap = new KopernicusSimplexWrapper(landClass.noiseBlend, landClass.noiseOctaves, landClass.noisePersistance, landClass.noiseFrequency);
                                    lcnoisesimpwrap.seed = landClass.noiseSeed;
                                    WriteObjectToConfigNode("noiseSimplex", ref lcNode, new VertexPlanet.SimplexLoader(lcnoisesimpwrap));
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
                            WriteObjectToConfigNode("ContinentalSimplex", ref modNode, new PQSMod_VertexPlanet.SimplexWrapper(vp.continental));
                            WriteObjectToConfigNode("RuggednessSimplex", ref modNode, new PQSMod_VertexPlanet.SimplexWrapper(vp.continentalRuggedness));
                            WriteObjectToConfigNode("SharpnessNoise", ref modNode, new PQSMod_VertexPlanet.NoiseModWrapper(vp.continentalSharpness));
                            WriteObjectToConfigNode("SharpnessSimplexMap", ref modNode, new PQSMod_VertexPlanet.SimplexWrapper(vp.continentalSharpnessMap));
                            WriteObjectToConfigNode("TerrainTypeSimplex", ref modNode, new PQSMod_VertexPlanet.SimplexWrapper(vp.terrainType));
                            if (vp.landClasses != null)
                            {
                                ConfigNode landClasses = modNode.AddNode("LandClasses");
                                foreach (PQSMod_VertexPlanet.LandClass landClass in vp.landClasses)
                                {
                                    ConfigNode classNode = WriteObjectToConfigNode("Class", ref landClasses, new VertexPlanet.LandClassLoader(landClass));
                                    WriteObjectToConfigNode("SimplexNoiseMap", ref classNode, new PQSMod_VertexPlanet.SimplexWrapper(landClass.colorNoiseMap));
                                }
                            }
                        }
                        if (!(mod is PQSMod_OceanFX)) continue;
                        List<Texture2DParser> wm = (loader as OceanFX).watermain;
                        ConfigNode watermain = modNode.AddNode("Watermain");
                        foreach (Texture2DParser texture in wm)
                            watermain.AddValue("waterTex-" + wm.ToList().IndexOf(texture), texture.Value.name);
                    }
                }
            }
        }
    }
}