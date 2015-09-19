using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using CompiledResources = Kopernicus.Configuration.Shaders.Shaders;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a Ring-Editor
        public class Rings
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Ring
            public static Ring ring = new Ring();

            private static List<GameObject> rings = null;
            private static int index = 0;

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

                if (rings == null)
                {
                    rings = new List<GameObject>();
                    foreach (Transform t in PlanetUI.currentBody.scaledBody.transform)
                        if (t.name == "PlanetaryRingObject")
                            rings.Add(t.gameObject);
                    if (rings.Count > 0)
                        ring = RebuildRing(rings[index]);
                }

                // Render the Window
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, 330));

                // Ring-Selector
                if (index > 0)
                {
                    if (GUI.Button(new Rect(20, offset, 30, 20), "<<"))
                    {
                        // Rebuild the Ring
                        rings[index].GetComponent<MeshRenderer>().materials = new Material[] { rings[index].GetComponent<MeshRenderer>().materials[0] };
                        index--;
                        ring = RebuildRing(rings[index]);
                    }
                }
                if (GUI.Button(new Rect(60, offset, 250, 20), "Add new Ring"))
                {
                    // Add a new Ring
                    RingLoader.AddRing(PlanetUI.currentBody.scaledBody, new Ring() { texture = Utils.defaultRing });
                    rings = new List<GameObject>();
                    foreach (Transform t in PlanetUI.currentBody.scaledBody.transform)
                        if (t.name == "PlanetaryRingObject")
                            rings.Add(t.gameObject);
                }
                if (index < rings.Count - 1)
                {
                    if (GUI.Button(new Rect(320, offset, 30, 20), ">>"))
                    {
                        // Rebuild the Ring
                        rings[index].GetComponent<MeshRenderer>().materials = new Material[] { rings[index].GetComponent<MeshRenderer>().materials[0] };
                        index++;
                        ring = RebuildRing(rings[index]);
                    }
                }
                offset += 35;

                if (rings.Count > 0)
                {
                    GUI.Label(new Rect(20, offset, 178, 20), "Inner Radius");
                    ring.innerRadius = Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + ring.innerRadius));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "Outer Radius");
                    ring.outerRadius = Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + ring.outerRadius));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "Inclination");
                    ring.angle = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + ring.angle));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "Steps");
                    ring.steps = Int32.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + ring.steps));
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "Color");
                    if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                    {
                        PropertyInfo property = ring.GetType().GetProperty("color");
                        ColorPicker.SetEditedObject(property, ring.color, ring);
                    }
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
                        ring.texture = texture;
                    }
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "Lock Rotation");
                    ring.lockRotation = GUI.Toggle(new Rect(200, offset, 170, 20), ring.lockRotation, "Bool");
                    offset += 25;

                    GUI.Label(new Rect(20, offset, 178, 20), "Unlit");
                    ring.unlit = GUI.Toggle(new Rect(200, offset, 170, 20), ring.unlit, "Bool");
                    offset += 45;

                    if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild Rings"))
                        RebuildRingObject();
                    offset += 25;

                    if (GUI.Button(new Rect(20, offset, 200, 20), "Delete rings on: " + PlanetUI.currentName))
                    {
                        MonoBehaviour.Destroy(rings[index]);

                        // Refresh the Ring-List
                        rings = null;
                        index = 0;
                    }
                }

                GUI.EndScrollView();
            }

            // Rebuild the Ring-Object, code taken from Kopernicus (and the Kopernicus code was adapted from KittopiaTech. :D)
            private static void RebuildRingObject()
            {
                Vector3 StartVec = new Vector3(1, 0, 0);
                var vertices = new List<Vector3>();
                var Uvs = new List<Vector2>();
                var Tris = new List<int>();
                var Normals = new List<Vector3>();

                for (float i = 0.0f; i < 360.0f; i += (360.0f / ring.steps))
                {
                    var eVert = Quaternion.Euler(0, i, 0) * StartVec;

                    //Inner Radius
                    vertices.Add(eVert * ((float)ring.innerRadius * (1f / rings[index].transform.localScale.x)));
                    Normals.Add(-Vector3.right);
                    Uvs.Add(new Vector2(0, 0));

                    //Outer Radius
                    vertices.Add(eVert * ((float)ring.outerRadius * (1f / rings[index].transform.localScale.x)));
                    Normals.Add(-Vector3.right);
                    Uvs.Add(new Vector2(1, 1));
                }

                for (float i = 0.0f; i < 360.0f; i += (360.0f / ring.steps))
                {
                    var eVert = Quaternion.Euler(0, i, 0) * StartVec;

                    //Inner Radius
                    vertices.Add(eVert * ((float)ring.innerRadius * (1f / rings[index].transform.localScale.x)));
                    Normals.Add(-Vector3.right);
                    Uvs.Add(new Vector2(0, 0));

                    //Outer Radius
                    vertices.Add(eVert * ((float)ring.outerRadius * (1f / rings[index].transform.localScale.x)));
                    Normals.Add(-Vector3.right);
                    Uvs.Add(new Vector2(1, 1));
                }

                //Tri Wrapping
                int Wrapping = (ring.steps * 2);
                for (int i = 0; i < (ring.steps * 2); i += 2)
                {
                    Tris.Add((i) % Wrapping);
                    Tris.Add((i + 1) % Wrapping);
                    Tris.Add((i + 2) % Wrapping);

                    Tris.Add((i + 1) % Wrapping);
                    Tris.Add((i + 3) % Wrapping);
                    Tris.Add((i + 2) % Wrapping);
                }

                for (int i = 0; i < (ring.steps * 2); i += 2)
                {
                    Tris.Add(Wrapping + (i + 2) % Wrapping);
                    Tris.Add(Wrapping + (i + 1) % Wrapping);
                    Tris.Add(Wrapping + (i) % Wrapping);

                    Tris.Add(Wrapping + (i + 2) % Wrapping);
                    Tris.Add(Wrapping + (i + 3) % Wrapping);
                    Tris.Add(Wrapping + (i + 1) % Wrapping);
                }

                // Rotate the Ring
                rings[index].transform.localRotation = Quaternion.Euler(ring.angle, 0, 0);

                // Build the Ring-Mesh
                MeshFilter ringFilter = rings[index].GetComponent<MeshFilter>();
                ringFilter.mesh = new Mesh();
                ringFilter.mesh.vertices = vertices.ToArray();
                ringFilter.mesh.triangles = Tris.ToArray();
                ringFilter.mesh.uv = Uvs.ToArray();
                ringFilter.mesh.RecalculateNormals();
                ringFilter.mesh.RecalculateBounds();
                ringFilter.mesh.Optimize();
                ringFilter.sharedMesh = ringFilter.mesh;

                // Ring-Material
                MeshRenderer renderer = rings[index].GetComponent<MeshRenderer>();
                if (ring.unlit)
                    renderer.material = new Material(CompiledResources.UnlitNew);
                else
                    renderer.material = new Material(CompiledResources.DiffuseNew);
                renderer.material.mainTexture = ring.texture == null ? Utils.defaultRing : ring.texture;
                renderer.material.color = ring.color;

                // Lock Rotation
                if (ring.lockRotation)
                {
                    if (rings[index].GetComponents<AngleLocker>().Length == 1)
                        rings[index].GetComponent<AngleLocker>().RotationLock = rings[index].transform.localRotation;
                    else
                        rings[index].AddComponent<AngleLocker>().RotationLock = rings[index].transform.localRotation;
                }
                else if (!ring.lockRotation && rings[index].GetComponents<AngleLocker>().Length == 1)
                {
                    MonoBehaviour.Destroy(rings[index].GetComponent<AngleLocker>());
                }
            }

            public static Ring RebuildRing(GameObject ringObj, bool save = false)
            {
                Ring ring = new Ring();
                ring.angle = ringObj.transform.localRotation.eulerAngles.x;
                ring.texture = ringObj.GetComponent<MeshRenderer>().material.mainTexture as Texture2D;
                ring.color = ringObj.GetComponent<MeshRenderer>().material.color;
                ring.unlit = ringObj.GetComponent<MeshRenderer>().material.shader.name == "Unlit/Transparent";
                ring.lockRotation = ringObj.GetComponents<AngleLocker>().Length == 1;
                Mesh ringMesh = ringObj.GetComponent<MeshFilter>().mesh;
                ring.innerRadius = ringMesh.vertices[0].x;
                ring.outerRadius = ringMesh.vertices[1].x;
                ring.steps = ringMesh.triangles.Length / 12;

                if (!save)
                {
                    MeshRenderer renderer = rings[index].GetComponent<MeshRenderer>();
                    Material outline = new Material(Kopernicus.UI.Shaders.Outline);
                    outline.SetColor("_OutlineColor", Color.red);
                    renderer.materials = new Material[] { renderer.material, outline };
                }

                return ring;
            }
        }
    }
}