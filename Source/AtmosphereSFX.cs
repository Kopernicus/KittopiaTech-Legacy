using Kopernicus.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        // Class that renders a AFG-Editor
        public class AtmosphereSFX
        {
            // GUI stuff
            private static Vector2 scrollPosition;

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

                // Get the AtmosphereFromGround
                AtmosphereFromGround[] afgs = Utils.FindScaled(PlanetUI.currentBody.transform.name).GetComponentsInChildren<AtmosphereFromGround>(true);

                // If afg == null
                if (afgs.Length == 0)
                {
                    // Render a button to add one
                    if (GUI.Button(new Rect(20, 300, 350, 20), "Add AtmosphereFromGround to " + PlanetUI.currentName))
                    {
                        // Create the atmosphere shell game object
                        GameObject scaledAtmosphere = new GameObject("atmosphere");
                        scaledAtmosphere.transform.parent = PlanetUI.currentBody.scaledBody.transform;
                        scaledAtmosphere.layer = Constants.GameLayers.ScaledSpaceAtmosphere;
                        MeshRenderer renderer = scaledAtmosphere.AddComponent<MeshRenderer>();
                        renderer.material = new Kopernicus.MaterialWrapper.AtmosphereFromGround();
                        MeshFilter meshFilter = scaledAtmosphere.AddComponent<MeshFilter>();
                        meshFilter.sharedMesh = Utility.ReferenceGeosphere();

                        // Create the AFGParser, because I'm lazy
                        AtmosphereFromGroundParser parser = new AtmosphereFromGroundParser(scaledAtmosphere.AddComponent<AtmosphereFromGround>(), PlanetUI.currentBody);
                        (parser as IParserEventSubscriber).Apply(new ConfigNode());
                    }
                    return;
                }

                // Create the height of the Scroll-List
                Type[] supportedTypes = new Type[] { typeof(string), typeof(bool), typeof(int), typeof(float), typeof(double), typeof(Color), typeof(Vector3) };
                int scrollOffset = afgs[0].GetType().GetFields().Where(f => supportedTypes.Contains(f.FieldType)).Count() * 25;
                
                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollOffset + 50));

                // Loop through all fields and display them
                foreach (FieldInfo key in afgs[0].GetType().GetFields())
                {
                    try
                    {
                        object obj = afgs[0];
                        if (key.FieldType == typeof(string))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj)));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, offset, 170, 20), (bool)key.GetValue(obj), "Bool"));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(int))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Int32.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Single.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            key.SetValue(obj, Double.Parse(GUI.TextField(new Rect(200, offset, 170, 20), "" + key.GetValue(obj))));
                            offset += 25;
                        }
                        else if (key.FieldType == typeof(Color) && key.Name == "waveLength")
                        {
                            GUI.Label(new Rect(20, offset, 178, 20), "" + key.Name);
                            if (GUI.Button(new Rect(200, offset, 50, 20), "Edit"))
                                ColorPicker.SetEditedObject(key, (Color)key.GetValue(obj), obj);

                            offset += 25;
                        }
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, offset, 200, 20), "" + key.Name);

                            Vector3 value = (Vector3)key.GetValue(obj);

                            value.x = Single.Parse(GUI.TextField(new Rect(200, offset, 50, 20), "" + value.x));
                            value.y = Single.Parse(GUI.TextField(new Rect(260, offset, 50, 20), "" + value.y));
                            value.z = Single.Parse(GUI.TextField(new Rect(320, offset, 50, 20), "" + value.z));

                            key.SetValue(obj, value);

                            offset += 25;
                        }
                    }
                    catch { }
                }
                offset += 20;
                if (GUI.Button(new Rect(20, offset, 200, 20), "Update"))
                {
                    AtmosphereFromGroundParser parser = new AtmosphereFromGroundParser(afgs[0], PlanetUI.currentBody);
                    (parser as IParserEventSubscriber).PostApply(new ConfigNode());
                }

                GUI.EndScrollView();
            }
        }
    }
}
