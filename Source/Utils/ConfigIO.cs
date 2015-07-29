// This file could get a bit large....

using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class to save a CelestialBody as a Kopernicus-configuration-file
        public class ConfigIO
        {
            // Call this on the Body you want to save
            public static void SaveCelestial(CelestialBody body)
            {
                // The conifg-node where we save the body
                ConfigNode root = new ConfigNode("@Kopernicus:NEEDS[Kopernicus]:FINAL");
                ConfigNode bodyNode = null;

                #region Body
                // Are we instanced?
                bool isCopy = false;

                // Determine if the body is already there, or if it is new.
                PSystemBody pBody = Utils.FindBody(PSystemManager.Instance.systemPrefab.rootBody, body.transform.name);
                if (pBody == null)
                {
                    bodyNode = new ConfigNode("Body");
                    pBody = Utils.FindBody(PSystemManager.Instance.systemPrefab.rootBody, PlanetUI.templates[body.transform.name]);
                    isCopy = true;
                }
                else
                {
                    bodyNode = new ConfigNode("@Body[" + body.transform.name + "]");
                }

                // Name-changes
                if (body.bodyName != pBody.name)
                {
                    if (isCopy)
                        bodyNode.AddValue("name", body.bodyName);
                    else
                        if (pBody.name == "Kerbin")
                            bodyNode.AddValue("cbNameLater", body.bodyName);
                        else
                            bodyNode.AddValue("@name", body.bodyName);
                }

                // If we're instanced, we need a Template definition
                if (isCopy)
                {
                    ConfigNode template = new ConfigNode("Template");
                    template.AddValue("name", pBody.name);
                    bodyNode.AddNode(template);
                }
                #endregion

                #region Orbit
                // Orbit
                ConfigNode oldOrbit = Utils.SearchNode("Orbit", body.transform.name);
                ConfigNode orbit = (oldOrbit == null) ? new ConfigNode("Orbit") : new ConfigNode("@Orbit");

                // Discover members tagged with parser attributes
                foreach (MemberInfo member in typeof(OrbitLoader).GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    // Is this member a parser target?
                    ParserTarget[] attributes = member.GetCustomAttributes((typeof(ParserTarget)), true) as ParserTarget[];
                    if (attributes.Length > 0)
                    {
                        // Get the Parser Target
                        ParserTarget target = attributes.First();
                        string value = "";
                        string preset = "";

                        // Get matching Fields / Properties
                        IEnumerable<FieldInfo> fields = body.orbit.GetType().GetFields().Where(f => f.Name == Utils.GetField(target.fieldName));
                        IEnumerable<PropertyInfo> properties = body.orbit.GetType().GetProperties().Where(p => p.Name == Utils.GetField(target.fieldName));

                        // Get the current and the previous value
                        if (fields.Count() == 1)
                        {
                            value = Format(fields.First().GetValue(body.orbit));
                            preset = Format(fields.First().GetValue(pBody.orbitDriver.orbit));
                        }
                        else if (properties.Count() == 1)
                        {
                            value = Format(properties.First().GetValue(body.orbit, null));
                            preset = Format(properties.First().GetValue(pBody.orbitDriver.orbit, null));
                        }
                        else if (target.fieldName == "color")
                        {
                            value = Format(body.orbitDriver.orbitColor);
                            preset = Format(pBody.orbitRenderer.orbitColor);
                        }
                        else
                        {
                            continue;
                        }

                        // Check if the value has changed and how to flag it
                        if (value != preset)
                        {
                            if (oldOrbit == null)
                                orbit.AddValue(target.fieldName, value);
                            else if (oldOrbit != null && oldOrbit.HasValue(target.fieldName))
                                orbit.AddValue("@" + target.fieldName, value);
                            else
                                orbit.AddValue(target.fieldName, value);
                        }
                    }
                }
                #endregion

                #region Properties
                //Parse the Properties from CelestialBody
                ConfigNode oldProp = Utils.SearchNode("Properties", body.transform.name);
                ConfigNode prop = (oldProp == null) ? new ConfigNode("Properties") : new ConfigNode("@Properties");

                // Discover members tagged with parser attributes
                foreach (MemberInfo member in typeof(Properties).GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    // Is this member a parser target?
                    ParserTarget[] attributes = member.GetCustomAttributes((typeof(ParserTarget)), true) as ParserTarget[];
                    if (attributes.Length > 0)
                    {
                        // Get the Parser Target
                        ParserTarget target = attributes.First();
                        string value = "";
                        string preset = "";

                        // Get matching Fields / Properties
                        IEnumerable<FieldInfo> fields = body.GetType().GetFields().Where(f => f.Name == Utils.GetField(target.fieldName));
                        IEnumerable<PropertyInfo> properties = body.GetType().GetProperties().Where(p => p.Name == Utils.GetField(target.fieldName));

                        // Get the current and the previous value
                        if (fields.Count() == 1)
                        {
                            value = Format(fields.First().GetValue(body));
                            preset = Format(fields.First().GetValue(pBody.celestialBody));
                        }
                        else if (properties.Count() == 1)
                        {
                            value = Format(properties.First().GetValue(body, null));
                            preset = Format(properties.First().GetValue(pBody.celestialBody, null));
                        }
                        else if (target.fieldName == "color")
                        {
                            value = Format(body);
                            preset = Format(pBody.celestialBody);
                        }
                        else
                        {
                            continue;
                        }

                        // Check if the value has changed and how to flag it
                        if (value != preset)
                        {
                            if (oldProp == null)
                                prop.AddValue(target.fieldName, value);
                            else if (oldProp != null && oldProp.HasValue(target.fieldName))
                                prop.AddValue("@" + target.fieldName, value);
                            else
                                prop.AddValue(target.fieldName, value);
                        }
                    }
                }
                #endregion

                #region PQS
                #region Start
                //Start getting the PQS stuff
                ConfigNode oldPQS = Utils.SearchNode("PQS", body.transform.name);
                ConfigNode pqs = (oldPQS == null) ? new ConfigNode("PQS") : new ConfigNode("@PQS");
                if (body.pqsController != null)
                {
                    // Store the values
                    string[] values = new string[7];
                    string[] presets = new string[7];
                    string[] parsed = new string[7];

                    // minLevel
                    values[0] = Format(body.pqsController.minLevel);
                    presets[0] = Format(pBody.pqsVersion.minLevel);
                    parsed[0] = "minLevel";

                    // maxLevel
                    values[1] = Format(body.pqsController.maxLevel);
                    presets[1] = Format(pBody.pqsVersion.maxLevel);
                    parsed[1] = "maxLevel";

                    // minDetailDistance
                    values[2] = Format(body.pqsController.minDetailDistance);
                    presets[2] = Format(pBody.pqsVersion.minDetailDistance);
                    parsed[2] = "minDetailDistance";

                    // maxQuadLenghtsPerFrame
                    values[3] = Format(body.pqsController.maxQuadLenghtsPerFrame);
                    presets[3] = Format(pBody.pqsVersion.maxQuadLenghtsPerFrame);
                    parsed[3] = "maxQuadLenghtsPerFrame";

                    // pqsFadeStart
                    values[4] = Format(body.pqsController.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)[0].planetFade.fadeStart);
                    presets[4] = Format(pBody.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)[0].planetFade.fadeStart);
                    parsed[4] = "pqsFadeStart";

                    // pqsFadeStart
                    values[5] = Format(body.pqsController.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)[0].planetFade.fadeEnd);
                    presets[5] = Format(pBody.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)[0].planetFade.fadeEnd);
                    parsed[5] = "pqsFadeEnd";

                    // pqsFadeStart
                    values[6] = Format(body.pqsController.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)[0].deactivateAltitude);
                    presets[6] = Format(pBody.pqsVersion.GetComponentsInChildren<PQSMod_CelestialBodyTransform>(true)[0].deactivateAltitude);
                    parsed[6] = "deactivateAltitude";

                    // Create the config
                    for (int i = 0; i < values.Count(); i++)
                    {
                        if (values[i] != presets[i])
                        {
                            if (oldPQS == null)
                                pqs.AddValue(parsed[i], values[i]);
                            else if (oldPQS != null && oldPQS.HasValue(parsed[i]))
                                pqs.AddValue("@" + parsed[i], values[i]);
                            else
                                pqs.AddValue(parsed[i], values[i]);
                        }
                    }
                }
                #endregion

                #region Material
                // Materials
                ConfigNode oldMat = null;
                ConfigNode mat = new ConfigNode();
                if (body.pqsController != null)
                {
                    if (oldPQS == null)
                        mat = new ConfigNode("Material");
                    else if (oldPQS != null && oldPQS.HasNode("Material"))
                        mat = new ConfigNode("@Material");
                    else
                        mat = new ConfigNode("Material");

                    if (mat.name.StartsWith("@"))
                        oldMat = Utils.SearchNode("Material", body.transform.name, oldPQS);

                    // Get the Parser-Type of the Material
                    object obj = null;
                    object pObj = null;
                    if (body.pqsController.surfaceMaterial != null)
                    {
                        IEnumerable<Type> types = Assembly.GetAssembly(typeof(Injector)).GetTypes();
                        IEnumerable<Type> materials = types.Where(t => t.BaseType == typeof(Material));
                        foreach (Type t in materials)
                        {
                            // Big Reflection, because of internal class
                            Type singleton = types.First(t2 => t2.FullName == t.FullName + "+Properties");
                            string shaderName = (singleton.GetProperty("shader", BindingFlags.Static | BindingFlags.Public).GetValue(null, null) as Shader).name;

                            // Compare things and break
                            if (shaderName == body.pqsController.surfaceMaterial.shader.name)
                            {
                                Type loader = types.First(l => l.Name == t.Name + "Loader");
                                obj = Activator.CreateInstance(loader, new object[] { body.pqsController.surfaceMaterial });
                                pObj = Activator.CreateInstance(loader, new object[] { pBody.pqsVersion.surfaceMaterial });
                                break;
                            }
                        }
                    }

                    // Discover members tagged with parser attributes
                    foreach (MemberInfo member in obj.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                    {
                        // Is this member a parser target?
                        ParserTarget[] attributes = member.GetCustomAttributes((typeof(ParserTarget)), true) as ParserTarget[];
                        if (attributes.Length > 0)
                        {
                            // Get the Parser Target
                            ParserTarget target = attributes.First();
                            string value = "";
                            string preset = "";

                            // Get matching Fields / Properties
                            IEnumerable<PropertyInfo> properties = obj.GetType().GetProperties().Where(p => p.Name == target.fieldName);

                            // Get the current and the previous value
                            if (properties.Count() == 1)
                            {
                                value = Format(properties.First().GetValue(obj, null));
                                preset = Format(properties.First().GetValue(pObj, null));
                            }
                            else
                            {
                                continue;
                            }

                            // Check if the value has changed and how to flag it
                            if (value != preset)
                            {
                                if (oldMat == null)
                                    mat.AddValue(target.fieldName, value);
                                else if (oldMat != null && oldMat.HasValue(target.fieldName))
                                    mat.AddValue("@" + target.fieldName, value);
                                else
                                    mat.AddValue(target.fieldName, value);
                            }
                        }
                    }
                }
                #endregion
                #endregion

                #region Biomes
                // Parse the biomes, do that manually, reflection would be a bit silly for three values :P
                ConfigNode oldBiomes = Utils.SearchNode("Biomes", body.transform.name);
                ConfigNode biomes = oldBiomes == null ? new ConfigNode("Biomes") : new ConfigNode("@Biomes");

                // Loop through the Biomes
                if (body.BiomeMap != null)
                {
                    // Add the biome-map definition (Builtin maps not allowed here)
                    if (Utils.TextureExists(body.BiomeMap.MapName))
                    {
                        if (oldBiomes != null)
                            if (Utils.SearchNode("Properties", body.transform.name).HasValue("biomeMap"))
                                prop.AddValue("@biomeMap", body.BiomeMap.MapName);
                            else
                                prop.AddValue("biomeMap", body.BiomeMap.MapName);
                        else
                            prop.AddValue("biomeMap", body.BiomeMap.MapName);
                    }

                    // Add the Biomes
                    foreach (CBAttributeMapSO.MapAttribute biome in body.BiomeMap.Attributes)
                    {
                        Func<ConfigNode, bool> p = n => n.HasValue("name") && n.GetValue("name") == biome.name;

                        if (oldBiomes == null || (oldBiomes != null && oldBiomes.GetNodes().Where(n => n.HasValue("name") && n.GetValue("name") == biome.name).Count() == 0))
                        {
                            ConfigNode biomeNode = new ConfigNode("Biome");
                            biomeNode.AddValue("name", biome.name);
                            biomeNode.AddValue("value", biome.value);
                            biomeNode.AddValue("color", Format(biome.mapColor));
                            biomes.AddNode(biomeNode);
                        }
                        else if (oldBiomes != null && oldBiomes.GetNodes().Where(p).Count() > 0)
                        {
                            // Stuff
                            bool isSame = false;
                            ConfigNode oldBiome = oldBiomes.GetNodes().Where(p).First();

                            // Create the node, and check changes
                            ConfigNode biomeNode = new ConfigNode("@Biome[" + biome.name + "]");
                            biomeNode.AddValue("@name", biome.name); isSame = biome.name == oldBiome.GetValue("name");
                            biomeNode.AddValue("@value", biome.value); isSame = biome.value.ToString() == oldBiome.GetValue("value");
                            biomeNode.AddValue("@color", Format(biome.mapColor)); isSame = biome.value.ToString() == oldBiome.GetValue("value");

                            // If nothing changed, do nothing


                            biomes.AddNode(biomeNode);
                        }
                    }

                    // Post process them, to delete the deleted biomes
                    if (oldBiomes != null)
                        foreach (ConfigNode n in oldBiomes.nodes)
                            if (!body.BiomeMap.Attributes.Select(b => b.name).Contains(n.GetValue("name")))
                                biomes.AddNode("!Biome[" + n.GetValue("name") + "]");
                }
                #endregion

                #region Rings
                // Parse the rings
                ConfigNode oldRings = Utils.SearchNode("Rings", body.transform.name);
                ConfigNode rings = oldRings == null ? new ConfigNode("Rings") : new ConfigNode("@Rings");

                // Nuke the rings
                if (oldRings != null)
                    rings.AddNode("!Ring, *");

                // Add the Biomes
                foreach (Transform t in body.scaledBody.transform)
                {
                    if (t.name == "PlanetaryRingObject")
                    {
                        Ring ring = Rings.RebuildRing(t.gameObject, true);

                        // Save stuff
                        ConfigNode node = new ConfigNode("Ring");
                        node.AddValue("angle", ring.angle);
                        node.AddValue("outerRadius", ring.outerRadius);
                        node.AddValue("innerRadius", ring.innerRadius);
                        node.AddValue("steps", ring.steps);
                        node.AddValue("texture", ring.texture.name);
                        node.AddValue("color", Format(ring.color));
                        node.AddValue("lockRotation", ring.lockRotation);
                        node.AddValue("unlit", ring.unlit);
                        rings.AddNode(node);
                    }
                }
                #endregion

                #region Particles
                // Parse the rings
                ConfigNode oldParticles = Utils.SearchNode("Particle", body.transform.name);
                ConfigNode particles = oldParticles == null ? new ConfigNode("Particle") : new ConfigNode("@Particle");

                // Add the Particles
                if (body.scaledBody.GetComponents<ParticleLoader.PlanetaryParticle>().Length == 1)
                {
                    ParticleLoader.PlanetaryParticle particle = body.scaledBody.GetComponent<ParticleLoader.PlanetaryParticle>();

                    if (oldParticles != null)
                    {
                        if (oldParticles.GetValue("target") != particle.target)
                            particles.AddValue("@target", particle.target);

                        if (oldParticles.GetValue("texture") != particle.Renderer.material.mainTexture.name)
                            particles.AddValue("@texture", particle.Renderer.material.mainTexture.name);

                        if (oldParticles.GetValue("minEmission") != particle.minEmission.ToString())
                            particles.AddValue("@minEmission", particle.minEmission);

                        if (oldParticles.GetValue("maxEmission") != particle.maxEmission.ToString())
                            particles.AddValue("@maxEmission", particle.maxEmission);

                        if (oldParticles.GetValue("lifespanMin") != particle.minEnergy.ToString())
                            particles.AddValue("@lifespanMin", particle.minEnergy);

                        if (oldParticles.GetValue("lifespanMax") != particle.maxEnergy.ToString())
                            particles.AddValue("@lifespanMax", particle.maxEnergy);

                        if (oldParticles.GetValue("sizeMin") != particle.emitter.minSize.ToString())
                            particles.AddValue("@sizeMin", particle.emitter.minSize);

                        if (oldParticles.GetValue("sizeMax") != particle.emitter.maxSize.ToString())
                            particles.AddValue("@sizeMax", particle.emitter.maxSize);

                        if (oldParticles.GetValue("speedScale") != particle.speedScale.ToString())
                            particles.AddValue("@speedScale", particle.speedScale);

                        if (oldParticles.GetValue("rate") != particle.animator.sizeGrow.ToString())
                            particles.AddValue("@rate", particle.animator.sizeGrow);

                        if (oldParticles.GetValue("randVelocity") != Format(particle.randomVelocity))
                            particles.AddValue("@randVelocity", Format(particle.randomVelocity));

                        ConfigNode colors = new ConfigNode("@Colors");
                        for (int i = 1; i < 6; i++)
                        {
                            if (oldParticles.GetNode("Colors").GetValue("color" + i) != Format(particle.animator.colorAnimation[i - 1]))
                                colors.AddValue("@color" + i, Format(particle.animator.colorAnimation[i - 1]));
                        }
                        if (colors.values.Count > 0)
                            particles.AddNode(colors);
                    }
                    else
                    {
                        particles.AddValue("target", particle.target);
                        particles.AddValue("texture", particle.Renderer.material.mainTexture.name);
                        particles.AddValue("minEmission", particle.minEmission);
                        particles.AddValue("maxEmission", particle.maxEmission);
                        particles.AddValue("lifespanMin", particle.minEnergy);
                        particles.AddValue("lifespanMax", particle.maxEnergy);
                        particles.AddValue("sizeMin", particle.emitter.minSize);
                        particles.AddValue("sizeMax", particle.emitter.maxSize);
                        particles.AddValue("speedScale", particle.speedScale);
                        particles.AddValue("rate", particle.animator.sizeGrow);
                        particles.AddValue("randVelocity", Format(particle.randomVelocity));
                        ConfigNode colors = new ConfigNode("Colors");
                        for (int i = 1; i < 6; i++)
                            colors.AddValue("color" + i, Format(particle.animator.colorAnimation[i - 1]));
                        particles.AddNode(colors);
                    }
                }
                #endregion

                // If the node has values, add it to the root
                if (orbit.values.Count > 0)
                    bodyNode.AddNode(orbit);
                if (biomes.nodes.Count > 0)
                    prop.AddNode(biomes);
                if (prop.values.Count > 0)
                    bodyNode.AddNode(prop);
                if (mat.values.Count > 0)
                    pqs.AddNode(mat);
                if (pqs.values.Count > 0)
                    bodyNode.AddNode(pqs);
                if (rings.nodes.Count > 0)
                    bodyNode.AddNode(rings);
                if (particles.values.Count > 0 || particles.nodes.Count > 0)
                    bodyNode.AddNode(particles);

                // Glue the nodes together
                root.AddNode(bodyNode);

                // Save the node
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Config/");
                ConfigNode save = new ConfigNode();
                save.AddNode(root);
                save.Save("GameData/KittopiaTech/Config/" + body.name + ".cfg", "KittopiaTech - a Kopernicus Visual Editor");
            }

            // Formats the Output of the object.ToString() function (=> removes the Type-Definition and some other things)
            private static string Format(object input)
            {
                if (input == null)
                    return null;
                if (input.GetType() == typeof(CelestialBody))
                    return (input as CelestialBody).transform.name;
                if (input.GetType() == typeof(Color))
                    return input.ToString().Replace("RGBA(", "").Replace(")", "");
                if (input.GetType() == typeof(Vector3))
                    return input.ToString().Replace("(", "").Replace(")", "");

                return input.ToString().Replace("(" + input.GetType().FullName + ")", "").Trim();
            }
        }
    }
}
