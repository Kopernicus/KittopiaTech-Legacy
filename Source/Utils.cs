/*=======================================================================================================*\
 * This code is partitially by Kragrathea (GPL) and by the Kopernicus-Team (LGPL). Used with permission. *
\*=======================================================================================================*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using System.IO;
using Kopernicus.Configuration;


namespace Kopernicus
{
    namespace UI
    {
        // Collection of Utillity-functions
        public class Utils : Kopernicus.Utility
        {
            /// <summary>
            /// Returns the LocalSpace GameObject for a Body
            /// </summary>
            /// <param name="name">The name of the Body</param>
            /// <returns>The LocalSpace GameObject of the Body</returns>
            public static GameObject FindLocal(string name)
            {
                if (LocalSpace.transform.FindChild(name) != null)
                    return LocalSpace.transform.FindChild(name).gameObject;
                else
                    return null;
            }

            /// <summary>
            /// Returns the ScaledSpace GameObject for a Body
            /// </summary>
            /// <param name="name">The name of the Body</param>
            /// <returns>The ScaledSpace GameObject of the Body</returns>
            public static GameObject FindScaled(string name)
            {
                if (ScaledSpace.Instance.transform.FindChild(name) != null)
                    return ScaledSpace.Instance.transform.FindChild(name).gameObject;
                else
                    return null;
            }

            /// <summary>
            /// Returns the CelestialBody Component for a Body
            /// </summary>
            /// <param name="name">The name of the Body</param>
            /// <returns>The CelestialBody Component of the body</returns>
            public static CelestialBody FindCB(string name)
            {
                return PSystemManager.Instance.localBodies.Find(b => b.name == name);
            }

            /// <summary>
            /// Loads a Texture from GameDatabase or from the Game-Assets
            /// </summary>
            /// <param name="path">The GameData-relative path of the Texture</param>
            /// <returns>The loaded Texture</returns>
            public static Texture2D LoadTexture(string path)
            {
                Texture2DParser parser = new Texture2DParser();
                parser.SetFromString(path);
                return parser.value;
            }

            /// <summary>
            /// Updates the Atmosphere-Ramp in ScaledSpace for a body
            /// </summary>
            /// <param name="name">The name of the Body</param>
            /// <param name="texture">The new Atmosphere-Ramp</param>
            public static void UpdateAtmosphereRamp(string name, Texture2D texture)
            {
                GameObject scaledVersion = FindScaled(name);
                MeshRenderer meshRenderer = scaledVersion.GetComponentInChildren<MeshRenderer>();
                meshRenderer.material.SetTexture("_rimColorRamp", texture);
            }

            /// <summary>
            /// [HACK] Spawn a new body from the PSystem-Prefab
            /// </summary>
            /// <param name="template">The Template-Body</param>
            /// <param name="name">The new name of the body</param>
            public static void Instantiate(PSystemBody template, string name)
            {
                // Fix Templates
                if (template == null)
                {
                    ScreenMessages.PostScreenMessage("You can only copy Stock-Bodies!", 3f, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }

                // Spawn Message
                ScreenMessages.PostScreenMessage("Created new Planet " + name + ", based on " + template.name + "!", 5f, ScreenMessageStyle.UPPER_CENTER);
                ScreenMessages.PostScreenMessage("This tool is meant to be used by modders, it can break mods!", 5f, ScreenMessageStyle.UPPER_CENTER);

                // Clone the Template
                GameObject bodyObject = MonoBehaviour.Instantiate(template.gameObject) as GameObject;
                PSystemBody body = bodyObject.GetComponent<PSystemBody>();

                // Alter it's name and flight-Number
                body.name = name;
                body.celestialBody.bodyName = name;
                body.celestialBody.transform.name = name;
                body.celestialBody.bodyTransform.name = name;
                body.scaledVersion.name = name;
                if (body.pqsVersion != null)
                {
                    body.pqsVersion.name = name;
                    body.pqsVersion.gameObject.name = name;
                    body.pqsVersion.transform.name = name;
                    foreach (PQS p in body.pqsVersion.GetComponentsInChildren(typeof(PQS), true))
                        p.name = p.name.Replace(template.celestialBody.bodyName, name);
                }
                body.flightGlobalsIndex = PSystemManager.Instance.localBodies.Last().flightGlobalsIndex + 1;

                // Change it's Orbit
                body.orbitDriver.orbit = Orbit.CreateRandomOrbitAround(PSystemManager.Instance.localBodies.First(), 4000000000, 60000000000);
                body.orbitDriver.referenceBody = PSystemManager.Instance.localBodies.First();
                body.orbitDriver.orbit.referenceBody = body.orbitDriver.referenceBody;
                body.orbitRenderer.lowerCamVsSmaRatio = template.orbitRenderer.lowerCamVsSmaRatio;
                body.orbitRenderer.upperCamVsSmaRatio = template.orbitRenderer.upperCamVsSmaRatio;

                // Clear it's childs
                body.children = new List<PSystemBody>();

                // Hack^6 - Hack the PSystemManager to spawn this thing
                MethodInfo spawnBody = typeof(PSystemManager).GetMethod("SpawnBody", BindingFlags.NonPublic | BindingFlags.Instance);
                spawnBody.Invoke(PSystemManager.Instance, new object[] { PSystemManager.Instance.localBodies.First(), body });
                CelestialBody cBody = PSystemManager.Instance.localBodies.Last();

                // Add the body to FlightGlobals.Bodies
                FlightGlobals.fetch.bodies.Add(cBody);

                // Start the CelestialBody
                typeof(CelestialBody).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(cBody, null);
                
                // Start the OrbitDriver
                if (cBody.orbitDriver != null)
                    typeof(OrbitDriver).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(cBody.orbitDriver, null);

                // Fix and start the OrbitRenderer
                if (Resources.FindObjectsOfTypeAll<OrbitRenderer>().Where(r => r.name == cBody.name).Count() == 1)
                {
                    OrbitRenderer renderer = Resources.FindObjectsOfTypeAll<OrbitRenderer>().Where(r => r.name == cBody.name).First();
                    renderer.driver = cBody.orbitDriver;
                    renderer.celestialBody = cBody;
                    typeof(OrbitRenderer).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(renderer, null);
                }

                // Force the start of the PQS-Spheres
                foreach (PQS p in FindLocal(cBody.name).GetComponentsInChildren<PQS>(true))
                    p.ForceStart();

                // Fix the ScaledVersion
                if (cBody.scaledBody.GetComponents<ScaledSpaceFader>().Length == 1)
                    cBody.scaledBody.GetComponent<ScaledSpaceFader>().celestialBody = cBody;
                if (cBody.scaledBody.GetComponents<AtmosphereFromGround>().Length == 1)
                    cBody.scaledBody.GetComponent<AtmosphereFromGround>().planet = cBody;

                // Register the Template
                PlanetUI.templates.Add(cBody.transform.name, template.name);
            }

            private static Dictionary<string, string> kopernicusFields = new Dictionary<string,string>() 
            {
                {"longitudeOfAscendingNode", "LAN"},
            };

            /// <summary>
            /// Returns a field-name based on a Kopernicus-ParserTarget
            /// </summary>
            /// <param name="input">The Kopernicus Parser-Target</param>
            /// <returns></returns>
            public static string GetField(string input)
            {
                if (kopernicusFields.ContainsKey(input))
                    return kopernicusFields[input];
                else
                    return input;
            }

            // Generate the scaled space mesh using PQS (all results use scale of 1)
            public static void GeneratePQSMaps(CelestialBody body)
            {
                // Get the PQS and the ScaledSpace
                PQS pqs = FindLocal(body.name).GetComponentsInChildren<PQS>(true).First();
                GameObject scaledVersion = FindScaled(body.name);

                // Get the Textures from the PQS
                List<Texture2D> textures = pqs.CreateMaps(2048, pqs.mapMaxHeight, pqs.mapOcean, pqs.mapOceanHeight, pqs.mapOceanColor).ToList();
                textures.Add(BumpToNormalMap(textures[1], 9));

                // Remove Alpha
                Color32[] pixels = textures[0].GetPixels32();
                for (int i = 0; i < pixels.Count(); i++)
                    if (pixels[i] != pqs.mapOceanColor)
                        pixels[i].a = 255;
                textures[0].SetPixels32(pixels);
                textures[0].Apply();

                // Serialize them to disk
                string path = KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Textures/" + body.name + "/";
                Directory.CreateDirectory(path);
                File.WriteAllBytes(path + body.name + "_Color.png", textures[0].EncodeToPNG());
                File.WriteAllBytes(path + body.name + "_Height.png", textures[1].EncodeToPNG());
                File.WriteAllBytes(path + body.name + "_Normal.png", textures[2].EncodeToPNG());

                // Apply them to the ScaledVersion
                scaledVersion.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", textures[0]);
                scaledVersion.GetComponent<MeshRenderer>().material.SetTexture("_BumpMap", textures[2]);
            }
        }
    }
}