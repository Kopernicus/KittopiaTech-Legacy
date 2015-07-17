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

            // Credit goes to Kragrathea.
            public static Texture2D BumpToNormalMap(Texture2D source, float strength)
            {
                strength = Mathf.Clamp(strength, 0.0F, 10.0F);
                var result = new Texture2D(source.width, source.height, TextureFormat.ARGB32, true);
                for (int by = 0; by < result.height; by++)
                {
                    for (var bx = 0; bx < result.width; bx++)
                    {
                        var xLeft = source.GetPixel(bx - 1, by).grayscale * strength;
                        var xRight = source.GetPixel(bx + 1, by).grayscale * strength;
                        var yUp = source.GetPixel(bx, by - 1).grayscale * strength;
                        var yDown = source.GetPixel(bx, by + 1).grayscale * strength;
                        var xDelta = ((xLeft - xRight) + 1) * 0.5f;
                        var yDelta = ((yUp - yDown) + 1) * 0.5f;
                        result.SetPixel(bx, by, new Color(xDelta, yDelta, 1.0f, xDelta));
                    }
                }
                result.Apply();
                return result;
            }

        }
    }

}