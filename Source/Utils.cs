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
                // Spawn Message
                ScreenMessages.PostScreenMessage("Creating new Planet " + name + ", based on " + template.name + "!", 5f, ScreenMessageStyle.UPPER_CENTER);
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
                //body.orbitRenderer.driver = body.orbitDriver;
                body.orbitRenderer.orbitColor = body.orbitDriver.orbitColor = Color.red;
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
                
                // Start the OrbitRenderer
                foreach (OrbitRenderer r in Resources.FindObjectsOfTypeAll<OrbitRenderer>())
                    Debug.Log(r + " - " + r.celestialBody);

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
            }

            /*===============================================*\
             * Kopernicus Code! Modified to work at runtime! *
            \*===============================================*/
            public static void UpdateScaledMesh(GameObject scaledVersion, PQS pqs, CelestialBody body, string path, string cacheFile, bool exportBin, bool useSpherical, bool exportMaps)
            {
                const double rJool = 6000000.0;
                const float rScaled = 1000.0f;

                // Compute scale between Jool and this body
                float scale = (float)(body.Radius / rJool);
                scaledVersion.transform.localScale = new Vector3(scale, scale, scale);

                Mesh scaledMesh;
                // Get the Paths for ScaledSpace
                string CacheDirectory = KSPUtil.ApplicationRootPath + path;
                string CacheFile = CacheDirectory + "/" + body.name + ".bin";

                // Write the Mesh
                Directory.CreateDirectory(CacheDirectory);
                Logger.Active.Log("[KittopiaTech]: Generating scaled space mesh: " + body.name);
                scaledMesh = Utils.ComputeScaledSpaceMesh(body, useSpherical ? null : pqs);
                Utility.RecalculateTangents(scaledMesh);
                scaledVersion.GetComponent<MeshFilter>().sharedMesh = scaledMesh;
                if (exportBin)
                    Utility.SerializeMesh(scaledMesh, CacheFile);
                if (exportMaps)
                {
                    Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Textures/" + pqs.name + "/");
                    Texture2D[] textures = pqs.CreateMaps(2048, pqs.mapMaxHeight, pqs.mapOcean, pqs.mapOceanHeight, pqs.mapOceanColor);
                    byte[] raw = textures[0].EncodeToPNG();
                    File.WriteAllBytes(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Textures/" + pqs.name + "/" + pqs.name + "_color.png", raw);
                    raw = textures[1].EncodeToPNG();
                    File.WriteAllBytes(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Textures/" + pqs.name + "/" + pqs.name + "_height.png", raw);
                    raw = BumpToNormalMap(textures[1], 9f).EncodeToPNG();
                    File.WriteAllBytes(KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Textures/" + pqs.name + "/" + pqs.name + "_normal.png", raw);
                }

                // Apply mesh to the body
                SphereCollider collider = scaledVersion.GetComponent<SphereCollider>();
                if (collider != null) collider.radius = rScaled;
                if (pqs != null && scaledVersion.gameObject != null && scaledVersion.gameObject.transform != null)
                {
                    scaledVersion.gameObject.transform.localScale = Vector3.one * (float)(pqs.radius / rJool);
                }
            }

            // Generate the scaled space mesh using PQS (all results use scale of 1)
            public static Mesh ComputeScaledSpaceMesh(CelestialBody body, PQS pqs)
            {
                // We need to get the body for Jool (to steal it's mesh)
                const double rScaledJool = 1000.0f;
                double rMetersToScaledUnits = (float)(rScaledJool / body.Radius);

                // Generate a duplicate of the Jool mesh
                Mesh mesh = Utility.DuplicateMesh(Utility.ReferenceGeosphere());

                // If this body has a PQS, we can create a more detailed object
                if (pqs != null)
                {
                    // first we enable all maps
                    OnDemand.OnDemandStorage.EnableBody(body.bodyName);

                    // Balcklisted Mods
                    Type[] blacklist = new Type[] { typeof(PQSMod_MapDecal), typeof(OnDemand.PQSMod_OnDemandHandler) };

                    // Deactivate blacklisted Mods
                    foreach (PQSMod mod in pqs.GetComponentsInChildren<PQSMod>(true).Where(m => blacklist.Contains(m.GetType())))
                        mod.modEnabled = false;

                    // Find the PQS mods
                    IEnumerable<PQSMod> mods = pqs.GetComponentsInChildren<PQSMod>(true).Where(m => m.modEnabled);

                    // If we were able to find PQS mods
                    if (mods.Count() > 0)
                    {
                        // Generate the PQS modifications
                        Vector3[] vertices = mesh.vertices;
                        for (int i = 0; i < mesh.vertexCount; i++)
                        {
                            // Get the UV coordinate of this vertex
                            Vector2 uv = mesh.uv[i];

                            // Since this is a geosphere, normalizing the vertex gives the direction from center center
                            Vector3 direction = vertices[i];
                            direction.Normalize();

                            // Build the vertex data object for the PQS mods
                            PQS.VertexBuildData vertex = new PQS.VertexBuildData();
                            vertex.directionFromCenter = direction;
                            vertex.vertHeight = body.Radius;
                            vertex.u = uv.x;
                            vertex.v = uv.y;

                            // Build from the PQS
                            foreach (PQSMod mod in mods)
                            {
                                mod.OnVertexBuild(vertex); // Why in heaven are there mods who modify height in OnVertexBuild() rather than OnVertexBuildHeight()?!?!
                                mod.OnVertexBuildHeight(vertex);
                            }

                            // Check for sea level
                            if (body.ocean && vertex.vertHeight < body.Radius)
                                vertex.vertHeight = body.Radius;

                            // Adjust the displacement
                            vertices[i] = direction * (float)(vertex.vertHeight * rMetersToScaledUnits);
                        }
                        mesh.vertices = vertices;
                        mesh.RecalculateNormals();
                        mesh.RecalculateBounds();
                    }
                }

                // Return the generated scaled space mesh
                return mesh;
            }
        }
    }
}