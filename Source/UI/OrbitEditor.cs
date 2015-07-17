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
        // Class that renders a Orbit-Editor
        public class OrbitEditor
        {
            // GUI stuff
            private static Vector2 scrollPosition;

            // Orbital stuff
            public static string reference = "";

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

                // Orbital Stuff
                Orbit orbit = PlanetUI.currentBody.orbitDriver.orbit;

                // Initial setting of the referenceBody
                if (reference == "")
                    reference = PlanetUI.currentBody.orbitDriver.referenceBody.bodyName;

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, 305));

                GUI.Label(new Rect(20, offset, 178, 20), "Inclination");
                orbit.inclination = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + orbit.inclination));
                offset += 25;
                GUI.Label(new Rect(20, offset, 178, 20), "Eccentricity");
                orbit.eccentricity = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + orbit.eccentricity));
                offset += 25;
                GUI.Label(new Rect(20, offset, 178, 20), "Semi-Major Axis");
                orbit.semiMajorAxis = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + orbit.semiMajorAxis));
                offset += 25;
                GUI.Label(new Rect(20, offset, 178, 20), "LAN");
                orbit.LAN = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + orbit.LAN));
                offset += 25;
                GUI.Label(new Rect(20, offset, 178, 20), "Argument Of Periapsis");
                orbit.argumentOfPeriapsis = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + orbit.argumentOfPeriapsis));
                offset += 25;
                GUI.Label(new Rect(20, offset, 178, 20), "Mean Anomaly At Epoch");
                orbit.meanAnomalyAtEpoch = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + orbit.meanAnomalyAtEpoch));
                offset += 25;
                GUI.Label(new Rect(20, offset, 178, 20), "Epoch");
                orbit.epoch = Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + orbit.epoch));
                offset += 25;
                GUI.Label(new Rect(20, offset, 178, 20), "Reference Body");
                reference = GUI.TextField(new Rect(200, offset, 170, 20), reference);
                offset += 25;

                if (GUI.Button(new Rect(200, offset, 178, 20), "Update Reference Body"))
                    if (Utils.FindCB(reference) != null)
                        orbit.referenceBody = Utils.FindCB(reference);
                offset += 25;

                GUI.Label(new Rect(20, offset, 178, 20), "Orbit Colour:");
                //colour editor
                if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                {
                    FieldInfo field = PlanetUI.currentBody.orbitDriver.GetType().GetField("orbitColor");
                    ColorPicker.SetEditedObject(field, PlanetUI.currentBody.orbitDriver.orbitColor, PlanetUI.currentBody.orbitDriver);
                }
                offset += 30;

                if (GUI.Button(new Rect(20, offset, 178, 20), "Update Orbit"))
                    PlanetUI.currentBody.orbitDriver.UpdateOrbit();

                offset += 25;

                if (GUI.Button(new Rect(20, offset, 178, 20), "Deactivate orbit renderer"))
                    PlanetUI.currentBody.orbitDriver.Renderer.drawMode = OrbitRenderer.DrawMode.OFF;

                GUI.EndScrollView();
            }
        }
    }
}