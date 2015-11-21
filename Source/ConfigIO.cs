// This file could get a bit large....

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

namespace Kopernicus
{
    namespace UI
    {
        // Class to save a CelestialBody as a Kopernicus-configuration-file
        public class ConfigIO
        {
            // Types that need special manipulation
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

            // Call this on the Body you want to save
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
                    ConfigNode atmo = WriteObjectToConfigNode("Atmosphere", ref body, new AtmosphereLoader(planet));
                    WriteObjectToConfigNode("AtmosphereFromGround", ref atmo, new AtmosphereFromGroundLoader(planet) { afg = planet.afg }); // Haha
                }

                // ScaledVersion
                ScaledVersionLoader scaledLoader = new ScaledVersionLoader(planet);
                ConfigNode scaled = WriteObjectToConfigNode("ScaledVersion", ref body, scaledLoader);
                if (scaledLoader.type == BodyType.Star)
                {
                    WriteObjectToConfigNode("Material", ref scaled, new EmissiveMultiRampSunspotsLoader(planet.scaledBody.renderer.sharedMaterial));
                    WriteObjectToConfigNode("Light", ref scaled, new LightShifterLoader() { lsc = planet.scaledBody.GetComponentsInChildren<LightShifter>(true)[0] });
                    if (planet.scaledBody.GetComponentsInChildren<SunCoronas>().Length != 0)
                    {
                        ConfigNode coronas = scaled.AddNode("Coronas");
                        foreach (SunCoronas corona in planet.scaledBody.GetComponentsInChildren<SunCoronas>(true))
                            WriteObjectToConfigNode("Corona", ref coronas, new CoronaLoader(corona));
                    }
                }
                else if (scaledLoader.type == BodyType.Atmospheric)
                    WriteObjectToConfigNode("Material", ref scaled, new ScaledPlanetRimAerialLoader(planet.scaledBody.renderer.sharedMaterial));
                else
                    WriteObjectToConfigNode("Material", ref scaled, new ScaledPlanetSimpleLoader(planet.scaledBody.renderer.sharedMaterial));

                // Particles
                if (planet.scaledBody.GetComponent<PlanetParticleEmitter>())
                {
                    ParticleLoader loader = new ParticleLoader(planet);
                    typeof(ParticleLoader).GetField("particle", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(loader, planet.scaledBody.GetComponent<PlanetParticleEmitter>()); // Haha
                    WriteObjectToConfigNode("Particle", ref body, loader);
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
                    PQSLoader pqsLoader = new PQSLoader(planet.pqsController);
                    ConfigNode pqs = WriteObjectToConfigNode("PQS", ref body, pqsLoader);
                    WriteObjectToConfigNode("Material", ref pqs, pqsLoader.surfaceMaterial);
                    WriteObjectToConfigNode("FallbackMaterial", ref pqs, pqsLoader.fallbackMaterial);
                    WriteObjectToConfigNode("PhysicsMaterial", ref pqs, pqsLoader.physicsMaterial);

                    // Mods
                    IEnumerable<PQSMod> mods = planet.pqsController.GetComponentsInChildren<PQSMod>(true).Where(m => m.sphere == planet.pqsController);

                    // Get all loaded types
                    IEnumerable<Type> types = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetTypes());
                    if (mods.Count() != 0)
                    {
                        ConfigNode modsNode = pqs.AddNode("Mods");
                        foreach (PQSMod mod in mods)
                        {
                            Type loaderType = types.FirstOrDefault(t => t.Name == mod.GetType().Name.Replace("PQSMod_", "").Replace("PQS", ""));

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
                                        ((IParserEventSubscriber)classLoader).Apply(null);
                                        ConfigNode scatterNode = WriteObjectToConfigNode("Scatter", ref scatters, classLoader);
                                        Material material = null; ;
                                        if (classLoader.materialType == LandControl.LandClassScatterLoader.ScatterMaterialType.AerialCutout)
                                            material = new AerialTransCutoutLoader(scatter.material);
                                        else if (classLoader.materialType == LandControl.LandClassScatterLoader.ScatterMaterialType.BumpedDiffuse)
                                            material = new NormalBumpedLoader(scatter.material);
                                        else if (classLoader.materialType == LandControl.LandClassScatterLoader.ScatterMaterialType.CutoutDiffuse)
                                            material = new AlphaTestDiffuseLoader(scatter.material);
                                        else if (classLoader.materialType == LandControl.LandClassScatterLoader.ScatterMaterialType.Diffuse)
                                            material = new NormalDiffuseLoader(scatter.material);
                                        else if (classLoader.materialType == LandControl.LandClassScatterLoader.ScatterMaterialType.DiffuseDetail)
                                            material = new NormalDiffuseDetailLoader(scatter.material);
                                        else
                                            material = new DiffuseWrapLoader(scatter.material);
                                        WriteObjectToConfigNode("Material", ref scatterNode, material);
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
                        }
                    }
                }
                
                // Save the node
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Config/");
                ConfigNode save = new ConfigNode();
                save.AddNode(root);
                save.Save("GameData/KittopiaTech/Config/" + planet.name + ".cfg", "KittopiaTech - a Kopernicus Visual Editor");
            }

            // Formats a texture path
            public static string Format(UnityEngine.Object o)
            {
                string path = GameDatabase.Instance.ExistsTexture(o.name) || Utility.TextureExists(o.name) ? o.name : "BUILTIN/" + o.name;
                return path;
            }

            // Writes an Object to a Configuration Node, using it's parser targets
            public static ConfigNode WriteObjectToConfigNode(string name, ref ConfigNode node, object @object)
            {
                // Start
                ConfigNode config = node.AddNode(name);

                // Crawl it's member infos
                foreach (MemberInfo member in @object.GetType().GetMembers(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    // Get the parser target
                    ParserTarget[] targets = member.GetCustomAttributes(typeof(ParserTarget), false) as ParserTarget[];
                    if (targets.Length == 0)
                        continue;

                    // Get stuff
                    RequireConfigType[] types = (member.MemberType == MemberTypes.Field ? (member as FieldInfo).FieldType : (member as PropertyInfo).PropertyType).GetCustomAttributes(typeof(RequireConfigType), false) as RequireConfigType[];

                    // Write
                    object targetValue = null;
                    Type targetType = null;
                    try
                    {
                        if (member.MemberType == MemberTypes.Field)
                        {
                            targetValue = (member as FieldInfo).GetValue(@object);
                            targetType = (member as FieldInfo).FieldType;
                        }
                        else if ((member as PropertyInfo).CanRead)
                        {
                            targetType = (member as PropertyInfo).PropertyType;
                            targetValue = (member as PropertyInfo).GetValue(@object, null);
                        }
                    }
                    catch { }

                    if (targetValue == null)
                        continue;

                    // Type
                    ConfigType type = types.Length == 1 ? types[0].type : targetType.Name.StartsWith("MapSOParser_") || targetType == typeof(string) ? ConfigType.Value : ConfigType.Node;

                    // Convert
                    if (targetType != typeof(string) && (type == ConfigType.Value || targetType == typeof(FloatCurveParser)))
                    {
                        targetValue = targetType == typeof(PhysicsMaterialParser) ?
                            targetType.GetProperty("material").GetValue(targetValue, null) :
                            targetType == typeof(FloatCurveParser) ?
                                targetType.GetProperty("curve").GetValue(targetValue, null) :
                                targetType.GetField("value").GetValue(targetValue);
                        if (targetValue == null || targetType == typeof(LandControl.LandClassScatterLoader.StockMaterialParser))
                            continue;
                        if (targetValue.GetType().GetInterface("IEnumerable") != null && targetType != typeof(string))
                        {
                            string s = "";
                            foreach (object o in targetValue as IEnumerable)
                                s += o.ToString() + ",";
                            targetValue = s.Remove(s.Length - 1);
                        }
                    }

                    // Do ConfigNode stuff
                    if (writeableTypes.Contains(targetType))
                    {
                        if (targetValue is Vector2)
                            targetValue = ConfigNode.WriteVector((Vector2)targetValue);
                        if (targetValue is Vector3)
                            targetValue = ConfigNode.WriteVector((Vector3)targetValue);
                        if (targetValue is Vector3d)
                            targetValue = ConfigNode.WriteVector((Vector3d)targetValue);
                        if (targetValue is Vector4)
                            targetValue = ConfigNode.WriteVector((Vector4)targetValue);
                        if (targetValue is Quaternion)
                            targetValue = ConfigNode.WriteQuaternion((Quaternion)targetValue);
                        if (targetValue is QuaternionD)
                            targetValue = ConfigNode.WriteQuaternion((QuaternionD)targetValue);
                        if (targetValue is Color)
                            targetValue = ConfigNode.WriteColor((Color)targetValue);
                    }

                    // Texture
                    Type[] textureTypes = new Type[] { typeof(Mesh), typeof(Texture2D), typeof(Texture), typeof(MapSO), typeof(CBAttributeMapSO) };
                    if (textureTypes.Contains(targetValue.GetType()) || textureTypes.Contains(targetValue.GetType().BaseType))
                        targetValue = Format(targetValue as UnityEngine.Object);

                    // Write
                    if (type == ConfigType.Value && targetValue.GetType() != typeof(FloatCurve) && targetValue.GetType() != typeof(AnimationCurve))
                        config.AddValue(targets[0].fieldName, targetValue);
                    else if (targetValue.GetType() == typeof(FloatCurve))
                        (targetValue as FloatCurve).Save(config.AddNode(targets[0].fieldName));
                    else if (targetValue.GetType() == typeof(AnimationCurve))
                        new FloatCurve((targetValue as AnimationCurve).keys).Save(config.AddNode(targets[0].fieldName));
                }
                return config;
            }
        }
    }
}