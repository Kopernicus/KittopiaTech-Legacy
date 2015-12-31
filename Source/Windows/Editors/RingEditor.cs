using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Kopernicus.Components;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a Ring-Editor
        public class RingEditor
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

                // Get the list
                if (rings == null)
                {
                    rings = new List<GameObject>();
                    foreach (Transform t in PlanetUI.currentBody.scaledBody.transform)
                        if (t.name.EndsWith("Ring")) rings.Add(t.gameObject);
                }

                // Render the Window
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, 300));

                // Ring-Selector
                if (index > 0)
                {
                    if (GUI.Button(new Rect(20, offset, 30, 20), "<<"))
                        index--;
                }
                if (GUI.Button(new Rect(60, offset, 250, 20), "Add new Ring"))
                {
                    // Add a new Ring
                    GameObject @object = new GameObject(PlanetUI.currentName + "Ring");
                    @object.transform.parent = PlanetUI.currentBody.scaledBody.transform;
                    @object.transform.position = PlanetUI.currentBody.scaledBody.transform.position;
                    ring = @object.AddComponent<Ring>();

                    // Get all rings
                    rings = new List<GameObject>();
                    foreach (Transform t in PlanetUI.currentBody.scaledBody.transform)
                        if (t.name.EndsWith("Ring")) rings.Add(t.gameObject);
                    index = rings.Count - 1;
                }
                if (index < rings.Count - 1)
                {
                    if (GUI.Button(new Rect(320, offset, 30, 20), ">>"))
                        index++;
                }
                offset += 35;

                if (rings.Count > 0)
                {
                    // Assign
                    ring = rings[index].GetComponent<Ring>();

                    // Loop through all the Fields
                    object[] objects = Utils.GetInfos<FieldInfo>(ring);
                    object obj = ring as System.Object;
                    Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
                    offset += 20;

                    if (GUI.Button(new Rect(20, offset, 200, 20), "Rebuild Rings"))
                    {
                        UnityEngine.Object.DestroyImmediate(ring.GetComponent<MeshFilter>());
                        UnityEngine.Object.DestroyImmediate(ring.GetComponent<MeshRenderer>());
                        ring.BuildRing();
                    }
                    offset += 25;

                    if (GUI.Button(new Rect(20, offset, 200, 20), "Delete rings on: " + PlanetUI.currentName))
                    {
                        UnityEngine.Object.Destroy(rings[index]);

                        // Refresh the Ring-List
                        rings = null;
                        index = 0;
                    }
                }

                GUI.EndScrollView();
            }
        }
    }
}