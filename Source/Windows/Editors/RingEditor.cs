/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Kopernicus.Components;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class represents the editor for the rings.
        /// </summary>
        public class RingEditor : Editor<CelestialBody>
        {
            /// <summary>
            /// The currently edited Ring
            /// </summary>
            private Ring ring { get; set; }

            /// <summary>
            /// The index of the current Ring in all rings
            /// </summary>
            private Int32 position { get; set; }

            /// <summary>
            /// All rings attached to the planet
            /// </summary>
            private List<GameObject> rings = null;

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Call base
                base.Render(id);

                // Scroll
                BeginScrollView(250, Utils.GetScrollSize<Ring>() + 125, 20);

                // Get the list
                if (rings == null)
                {
                    rings = new List<GameObject>();
                    foreach (Transform t in from Transform t in Current.scaledBody.transform where t.name.EndsWith("Ring") select t)
                        rings.Add(t.gameObject);
                }

                // Index
                index = 0;

                // Menu
                Enabled(() => position > 0, () => Button("<<", () => position--, new Rect(20, index * distance + 10, 30, 20))); index--;
                Button("Add new Ring", () =>
                {
                    // Add a new Ring
                    GameObject gameObject = new GameObject(Current.name + "Ring");
                    gameObject.transform.parent = Current.scaledBody.transform;
                    gameObject.transform.position = Current.scaledBody.transform.position;
                    ring = gameObject.AddComponent<Ring>();

                    // Get all rings
                    rings = new List<GameObject>();
                    foreach (Transform t in from Transform t in Current.scaledBody.transform where t.name.EndsWith("Ring") select t)
                        rings.Add(t.gameObject);
                    position = rings.Count - 1;
                }, new Rect(60, index * distance + 10, 250, 20)); index--;
                Enabled(() => position < rings.Count - 1, () => Button(">>", () => position++, new Rect(320, index * distance + 10, 30, 20))); index++;

                // Render the Ring Editor
                if (rings.Count <= 0) return;

                // Assign
                ring = rings[position].GetComponent<Ring>();

                // Loop through all the Fields
                RenderObject(ring);
                index++;

                // Rebuild the Ring
                Button("Rebuild Ring", () =>
                {
                    UnityEngine.Object.DestroyImmediate(ring.GetComponent<MeshFilter>());
                    UnityEngine.Object.DestroyImmediate(ring.GetComponent<MeshRenderer>());
                    ring.BuildRing();
                });

                // Delete Ring
                Button("Delete Ring", () =>
                {
                    UnityEngine.Object.Destroy(rings[index]);
                    rings = null;
                    position = 0;
                });

                // End Scroll
                EndScrollView();
            }
        }
    }
}