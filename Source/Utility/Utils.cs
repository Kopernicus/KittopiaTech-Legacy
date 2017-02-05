using Kopernicus.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Kopernicus.UI.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Kopernicus
{
    namespace UI
    {
        // Collection of Utillity-functions
        public class Utils
        {
            public static void GenerateScaledSpace(CelestialBody body, Mesh meshinput)
            {
                PQS bodyPQS = body.pqsController;
                Single joolScaledRad = 1000f;
                Single joolRad = 6000000f;
                Single scale = (float) bodyPQS.radius/joolScaledRad;

                Vector3[] vertices = new Vector3[meshinput.vertices.Count()];

                // One could use pqs.radiusMin and pqs.radiusMax to determine minimum and maximum height.
                // But to be safe, the height limit values will be determined manually.
                Single radiusMin = 0;
                Single radiusMax = 0;

                bodyPQS.isBuildingMaps = true;
                for (Int32 i = 0; i < meshinput.vertices.Count(); i++)
                {
                    Vector3 vertex = meshinput.vertices[i];
                    Single rootrad = (float) Math.Sqrt(vertex.x*vertex.x +
                                                       vertex.y*vertex.y +
                                                       vertex.z*vertex.z);
                    Single localRadius = (float) bodyPQS.GetSurfaceHeight(vertex)/scale;
                    vertices[i] = vertex*(localRadius/rootrad);

                    if (i == 0)
                    {
                        radiusMin = radiusMax = localRadius;
                    }
                    else
                    {
                        if (radiusMin > localRadius) radiusMin = localRadius;
                        if (radiusMax < localRadius) radiusMax = localRadius;
                    }
                }
                bodyPQS.isBuildingMaps = false;

                // Adjust the mesh so the maximum radius has 1000 unit in scaled space.
                // (so the planets will fit in the science archive list)
                Single r = radiusMax/1000;
                for (Int32 i = 0; i < vertices.Count(); i++)
                {
                    vertices[i] /= r;
                }

                // Use the lowest radius as collision radius.
                Single radius = radiusMin/r;

                // Calculate the local scale.
                Vector3 localScale = Vector3.one*((float) bodyPQS.radius/joolRad)*r;

                // Apply the mesh to ScaledSpace
                MeshFilter meshfilter = body.scaledBody.GetComponent<MeshFilter>();
                SphereCollider collider = body.scaledBody.GetComponent<SphereCollider>();
                meshfilter.sharedMesh.vertices = vertices;
                meshfilter.sharedMesh.RecalculateNormals();
                Utility.RecalculateTangents(meshfilter.sharedMesh);
                collider.radius = radius;
                body.scaledBody.transform.localScale = localScale;

                // Serialize
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + Body.ScaledSpaceCacheDirectory);
                Utility.SerializeMesh(meshfilter.sharedMesh, KSPUtil.ApplicationRootPath + Body.ScaledSpaceCacheDirectory + "/" + body.name + ".bin");
            }

            /// <summary>
            /// Returns the CelestialBody Component for a Body
            /// </summary>
            public static CelestialBody FindCB(string name)
            {
                return PSystemManager.Instance.localBodies.Find(b => b.name == name);
            }

            /// <summary>
            /// [HACK] Spawn a new body from the PSystem-Prefab
            /// </summary>
            public static void Instantiate(PSystemBody template, string name)
            {
                // Fix Templates
                if (template == null)
                {
                    ScreenMessages.PostScreenMessage("You need a valid Template!", 3f, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }

                // Spawn Message
                ScreenMessages.PostScreenMessage("Created new Planet " + name + ", based on " + template.name + "!", 5f, ScreenMessageStyle.UPPER_CENTER);
                ScreenMessages.PostScreenMessage("This tool is meant to be used by modders, it can break mods!", 5f, ScreenMessageStyle.UPPER_CENTER);

                // Clone the Template
                GameObject bodyObject = UnityEngine.Object.Instantiate(template.gameObject);
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
                    foreach (PQS p in body.pqsVersion.GetComponentsInChildren(typeof (PQS), true))
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

                // Add it to the System-Prefab
                body.transform.parent = PSystemManager.Instance.systemPrefab.transform;
                PSystemManager.Instance.systemPrefab.rootBody.children.Add(body);

                // Hack^6 - Hack the PSystemManager to spawn this thing
                MethodInfo spawnBody = typeof (PSystemManager).GetMethod("SpawnBody", BindingFlags.NonPublic | BindingFlags.Instance);
                spawnBody.Invoke(PSystemManager.Instance, new object[] {PSystemManager.Instance.localBodies.First(), body});
                CelestialBody cBody = PSystemManager.Instance.localBodies.Last();

                // Add the body to FlightGlobals.Bodies
                FlightGlobals.fetch.bodies.Add(cBody);

                // Start the CelestialBody
                typeof (CelestialBody).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(cBody, null);

                // Start the OrbitDriver
                if (cBody.orbitDriver != null)
                    typeof (OrbitDriver).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(cBody.orbitDriver, null);

                // Fix and start the OrbitRenderer
                if (Resources.FindObjectsOfTypeAll<OrbitRenderer>().Count(r => r.name == cBody.name) == 1)
                {
                    OrbitRenderer renderer = Resources.FindObjectsOfTypeAll<OrbitRenderer>().First(r => r.name == cBody.name);
                    renderer.driver = cBody.orbitDriver;
                    renderer.celestialBody = cBody;
                    typeof (OrbitRenderer).GetMethod("Start", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(renderer, null);
                }

                // Force the start of the PQS-Spheres
                foreach (PQS p in cBody.GetComponentsInChildren<PQS>(true))
                    p.ForceStart();

                // Fix the ScaledVersion
                if (cBody.scaledBody.GetComponents<ScaledSpaceFader>().Length == 1)
                    cBody.scaledBody.GetComponent<ScaledSpaceFader>().celestialBody = cBody;
                if (cBody.scaledBody.GetComponents<AtmosphereFromGround>().Length == 1)
                    cBody.scaledBody.GetComponent<AtmosphereFromGround>().planet = cBody;
            }

            /// <summary>
            /// Generate the scaled space Textures using PQS in a Coroutine
            /// </summary>
            public static IEnumerator GeneratePQSMaps(CelestialBody body, Boolean transparentMaps)
            {
                // Get time
                DateTime now = DateTime.Now;

                // Get PQS
                PQS pqs = body.pqsController;
                pqs.isBuildingMaps = true;
                pqs.isFakeBuild = true;

                // Get the mods
                Action<PQS.VertexBuildData> modOnVertexBuildHeight = (Action<PQS.VertexBuildData>)Delegate.CreateDelegate(
                    typeof(Action<PQS.VertexBuildData>), 
                    pqs, 
                    typeof (PQS).GetMethod("Mod_OnVertexBuildHeight", BindingFlags.Instance | BindingFlags.NonPublic));
                Action<PQS.VertexBuildData> modOnVertexBuild = (Action<PQS.VertexBuildData>)Delegate.CreateDelegate(
                    typeof(Action<PQS.VertexBuildData>), 
                    pqs, 
                    typeof(PQS).GetMethod("Mod_OnVertexBuild", BindingFlags.Instance | BindingFlags.NonPublic));
                PQSMod[] mods = pqs.GetComponentsInChildren<PQSMod>().Where(m => m.sphere == pqs && m.modEnabled).ToArray();

                // Create the Textures
                Texture2D colorMap = new Texture2D(pqs.mapFilesize, pqs.mapFilesize / 2, TextureFormat.ARGB32, true);
                Texture2D heightMap = new Texture2D(pqs.mapFilesize, pqs.mapFilesize / 2, TextureFormat.RGB24, true);

                // Arrays
                Color[] colorMapValues = new Color[pqs.mapFilesize*(pqs.mapFilesize/2)];
                Color[] heightMapValues = new Color[pqs.mapFilesize*(pqs.mapFilesize/2)];

                // Stuff
                ScreenMessage message = ScreenMessages.PostScreenMessage("Generating Planet-Maps", Single.MaxValue, ScreenMessageStyle.UPPER_CENTER);

                // Wait a some time
                yield return null;

                // Loop through the pixels
                for (int y = 0; y < (pqs.mapFilesize/2); y++)
                {
                    for (int x = 0; x < pqs.mapFilesize; x++)
                    {
                        // Update Message
                        Double percent = ((double) ((y*pqs.mapFilesize) + x)/((pqs.mapFilesize/2)*pqs.mapFilesize))*100;
                        while (CanvasUpdateRegistry.IsRebuildingLayout()) Thread.Sleep(10);
                        message.textInstance.text.text = "Generating Planet-Maps: " + percent.ToString("0.00") + "%";

                        // Create a VertexBuildData
                        PQS.VertexBuildData data = new PQS.VertexBuildData
                        {
                            directionFromCenter = (QuaternionD.AngleAxis((360d/pqs.mapFilesize)*x, Vector3d.up)*QuaternionD.AngleAxis(90d - (180d/(pqs.mapFilesize/2))*y, Vector3d.right))*Vector3d.forward,
                            vertHeight = pqs.radius
                        };

                        // Build from the Mods 
                        modOnVertexBuildHeight(data);
                        modOnVertexBuild(data);

                        // Adjust the height
                        double height = (data.vertHeight - pqs.radius)*(1d/pqs.mapMaxHeight);
                        if (height < 0)
                            height = 0;
                        else if (height > 1)
                            height = 1;

                        // Adjust the Color
                        Color color = data.vertColor;
                        if (!pqs.mapOcean)
                            color.a = 1f;
                        else if (height > pqs.mapOceanHeight)
                            color.a = transparentMaps ? 0f : 1f;
                        else
                            color = pqs.mapOceanColor.A(1f);

                        // Set the Pixels
                        colorMapValues[(y*pqs.mapFilesize) + x] = color;
                        heightMapValues[(y*pqs.mapFilesize) + x] = new Color((Single) height, (Single) height, (Single) height);
                    }
                    yield return null;
                }

                // Apply the maps
                colorMap.SetPixels(colorMapValues);
                colorMap.Apply();
                heightMap.SetPixels(heightMapValues);
                yield return null;

                // Close the Renderer
                pqs.isBuildingMaps = false;
                pqs.isFakeBuild = false;

                // Bump to Normal Map
                Texture2D normalMap = Utility.BumpToNormalMap(heightMap, UIController.NormalStrength);

                // Serialize them to disk
                string path = KSPUtil.ApplicationRootPath + "/GameData/KittopiaTech/Textures/" + body.name + "/";
                Directory.CreateDirectory(path);
                File.WriteAllBytes(path + body.name + "_Color.png", colorMap.EncodeToPNG());
                File.WriteAllBytes(path + body.name + "_Height.png", heightMap.EncodeToPNG());
                File.WriteAllBytes(path + body.name + "_Normal.png", normalMap.EncodeToPNG());
                yield return null;

                // Apply them to the ScaledVersion
                body.scaledBody.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", colorMap);
                body.scaledBody.GetComponent<MeshRenderer>().material.SetTexture("_BumpMap", normalMap);

                // Declare that we're done
                ScreenMessages.RemoveMessage(message);
                ScreenMessages.PostScreenMessage("Operation completed in: " + (DateTime.Now - now).TotalMilliseconds + " ms", 2f, ScreenMessageStyle.UPPER_CENTER);
            }

            /// <summary>
            /// Returns the Types that are supported by the Renderer
            /// </summary>
            public static Type[] supportedTypes => new Type[]
            {
                typeof (String),
                typeof (Boolean),
                typeof (Int32),
                typeof (Single),
                typeof (Double),
                typeof (Color),
                typeof (Vector3),
                typeof (Vector3d),
                typeof (Vector2),
                typeof (Vector2d),
                typeof (PQSLandControl.LandClass[]),
                typeof (PQSLandControl.LerpRange),
                typeof (PQSMod_VertexPlanet.LandClass[]),
                typeof (PQSMod_HeightColorMap.LandClass[]),
                typeof (PQSMod_HeightColorMap2.LandClass[]),
                typeof (PQSMod_HeightColorMapNoise.LandClass[]),
                typeof (PQS),
                typeof (PQSMod_VertexPlanet.SimplexWrapper),
                typeof (PQSMod_VertexPlanet.NoiseModWrapper),
                typeof (MapSO),
                typeof (CBAttributeMapSO),
                typeof (Texture2D),
                typeof (Texture),
                typeof (Material),
                typeof (CelestialBody),
                typeof (FloatCurve),
                typeof (AnimationCurve),
                typeof (Mesh)
            };

            /// <summary>
            /// Returns the Size for a Scrollbar
            /// </summary>
            public static Int32 GetScrollSize<T>()
            {
                return GetScrollSize(typeof (T));
            }

            /// <summary>
            /// Returns the Size for a Scrollbar
            /// </summary>
            public static Int32 GetScrollSize(Type t)
            {
                // Integer to store the values
                Int32 scrollSize = 0;

                // Get all parseable MemberInfos
                MemberInfo[] infos = t.GetMembers()
                    .Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                    .Where(m => !(m as FieldInfo)?.IsLiteral ?? true)
                    .Where(m => m is PropertyInfo ? (m as PropertyInfo).CanRead && (m as PropertyInfo).CanWrite : true)
                    .Where(m => supportedTypes.Contains(m.GetMemberType()))
                    .ToArray();

                // Get the count of the array
                scrollSize += infos.Length * Window<System.Object>.distance;

                // Handle special things
                scrollSize += infos.Where(o => o.GetMemberType() == typeof(MapSO) || o.GetMemberType() == typeof(CBAttributeMapSO)).ToArray().Length * Window<System.Object>.distance;

                // Return the Scroll-Size
                return scrollSize;
            }
        }
    }
}