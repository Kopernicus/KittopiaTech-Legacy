using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

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

            // Return an OnGUI()-Window.
            public static void Render()
            {
                // Render variables
                int offset = 290;

                // Is a body selected?
                if (PlanetUI.currentBody == null)
                {
                    GUI.Label(new Rect(20, offset, 200, 20), "NO PLANET SELECTED");
                    return;
                }

                // Render the Window
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, 370));

                GUI.Label(new Rect(20, offset, 200, 20), "Inner Radius:");
                offset += 20;
                ring.innerRadius = Double.Parse(GUI.TextField(new Rect(20, offset, 200, 20), "" + ring.innerRadius));

                offset += 40;
                GUI.Label(new Rect(20, offset, 200, 20), "Outer Radius:");
                offset += 20;
                ring.outerRadius = Double.Parse(GUI.TextField(new Rect(20, offset, 200, 20), "" + ring.outerRadius));

                offset += 40;
                GUI.Label(new Rect(20, offset, 200, 20), "Inclination:");
                offset += 20;
                ring.angle = Single.Parse(GUI.TextField(new Rect(20, offset, 200, 20), "" + ring.angle));

                offset += 40;
                GUI.Label(new Rect(20, offset, 200, 20), "Color:");
                if (GUI.Button(new Rect(150, offset, 50, 20), "Edit"))
                {
                    PropertyInfo property = ring.GetType().GetProperty("color");
                    ColorPicker.SetEditedObject(property, ring.color, ring);
                }

                offset += 40;

                ring.lockRotation = GUI.Toggle(new Rect(20, offset, 200, 20), ring.lockRotation, "Lock Rotation? ");
                offset += 20;
                ring.unlit = GUI.Toggle(new Rect(20, offset, 200, 20), ring.unlit, "Unlit? ");
                offset += 40;

                if (GUI.Button(new Rect(20, offset, 200, 20), "Generate ring for: " + PlanetUI.currentName))
                {
                    ring.texture = Utils.LoadTexture("KittopiaTech/Textures/ring");
                    RingLoader.AddRing(PlanetUI.currentBody.scaledBody, ring);
                }
                offset += 30;
                if (GUI.Button(new Rect(20, offset, 200, 20), "Delete rings on: " + PlanetUI.currentName))
                {
                    GameObject ScaledPlanet = Utils.FindScaled(PlanetUI.currentName);
                    GameObject RingGobj = ScaledPlanet.transform.FindChild("PlanetaryRingObject").gameObject;
                    if (RingGobj != null)
                    {
                        MonoBehaviour.Destroy(RingGobj);
                    }
                } 

                GUI.EndScrollView();
            }
        }
    }
}
