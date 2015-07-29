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
                object[] objects = Utils.GetInfos<FieldInfo>(afgs[0]);
                int scrollSize = Utils.GetScrollSize(objects);

                // Render the Scrollbar
                scrollPosition = GUI.BeginScrollView(new Rect(10, 300, 400, 250), scrollPosition, new Rect(0, 280, 380, scrollSize + 50));

                // Render the Selection
                object obj = afgs[0] as System.Object;
                Utils.RenderSelection<FieldInfo>(objects, ref obj, ref offset);
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
