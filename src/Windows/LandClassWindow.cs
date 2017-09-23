/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kopernicus.UI.Enumerations;
using UnityEngine;
using Object = System.Object;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to edit pqs lerp ranges
        /// </summary>
        [Position(420, 400, 420, 250)]
        public class LandClassWindow : BulkWindow<Object>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return $"KittopiaTech - {Localization.LOC_KITTOPIATECH_LANDCLASSWINDOW}"; ;
            }

            /// <summary>
            /// Closes the Window
            /// </summary>
            protected override void Exit()
            {
                UIController.Instance.DisableWindow(KittopiaWindows.LandClass);
            }

            /// <summary>
            /// Formats the specified object.
            /// </summary>
            protected override String Format(Object obj)
            {
                if (obj is PQSMod_VertexPlanet.LandClass)
                    return ((PQSMod_VertexPlanet.LandClass) obj).name;
                if (obj is PQSMod_HeightColorMap.LandClass)
                    return ((PQSMod_HeightColorMap.LandClass) obj).name;
                if (obj is PQSMod_HeightColorMap2.LandClass)
                    return ((PQSMod_HeightColorMap2.LandClass) obj).name;
                if (obj is PQSMod_HeightColorMapNoise.LandClass)
                    return ((PQSMod_HeightColorMapNoise.LandClass) obj).name;
                return (obj as PQSLandControl.LandClass)?.landClassName;
            }

            /// <summary>
            /// Renders the Editor
            /// </summary>
            protected override void RenderEditor(Int32 id)
            {
                // Render the Object
                RenderObject(Current);

                // Remove Biomes
                Button(Localization.LOC_KITTOPIATECH_LANDCLASSWINDOW_REMOVE, () =>
                {
                    List<Object> collection = Collection.ToList();
                    collection.Remove(Current);
                    Collection = collection;
                    Callback(Collection);
                });
                Callback(Collection);
            }

            /// <summary>
            /// Renders Collection Modifiers
            /// </summary>
            protected override void RenderModifiers(Int32 id)
            {
                // Index
                index++;

                // Add Biomes
                Button(Localization.LOC_KITTOPIATECH_LANDCLASSWINDOW_ADD, () =>
                {
                    Object landClass_ = null;
                    if (Collection is IEnumerable<PQSMod_VertexPlanet.LandClass>)
                    {
                        landClass_ = new PQSMod_VertexPlanet.LandClass("LandClass", 0, 0, new Color(), new Color(), 0);
                        (landClass_ as PQSMod_VertexPlanet.LandClass).colorNoiseMap = new PQSMod_VertexPlanet.SimplexWrapper(0, 0, 0, 0);
                    }
                    if (Collection is IEnumerable<PQSMod_HeightColorMap.LandClass>)
                    {
                        landClass_ = new PQSMod_HeightColorMap.LandClass("LandClass", 0, 0, new Color(), new Color(), 0);
                    }
                    if (Collection is IEnumerable<PQSMod_HeightColorMap2.LandClass>)
                    {
                        landClass_ = new PQSMod_HeightColorMap2.LandClass("LandClass", 0, 0, new Color(), new Color(), 0);
                    }
                    if (Collection is IEnumerable<PQSMod_HeightColorMapNoise.LandClass>)
                    {
                        landClass_ = new PQSMod_HeightColorMapNoise.LandClass("LandClass", 0, 0, new Color(), new Color(), 0);
                    }
                    if (Collection is IEnumerable<PQSLandControl.LandClass>)
                    {
                        PQSLandControl.LandClass landClass = new PQSLandControl.LandClass();
                        PQSLandControl.LerpRange range;

                        // Initialize default parameters
                        landClass.altDelta = 1;
                        landClass.color = new Color(0, 0, 0, 0);
                        landClass.coverageFrequency = 1;
                        landClass.coverageOctaves = 1;
                        landClass.coveragePersistance = 1;
                        landClass.coverageSeed = 1;
                        landClass.landClassName = "Base";
                        landClass.latDelta = 1;
                        landClass.lonDelta = 1;
                        landClass.noiseColor = new Color(0, 0, 0, 0);
                        landClass.noiseFrequency = 1;
                        landClass.noiseOctaves = 1;
                        landClass.noisePersistance = 1;
                        landClass.noiseSeed = 1;

                        range = new PQSLandControl.LerpRange();
                        range.endEnd = 1;
                        range.endStart = 1;
                        range.startEnd = 0;
                        range.startStart = 0;
                        landClass.altitudeRange = range;

                        range = new PQSLandControl.LerpRange();
                        range.endEnd = 1;
                        range.endStart = 1;
                        range.startEnd = 0;
                        range.startStart = 0;
                        landClass.latitudeRange = range;

                        range = new PQSLandControl.LerpRange();
                        range.endEnd = 1;
                        range.endStart = 1;
                        range.startEnd = 0;
                        range.startStart = 0;
                        landClass.latitudeDoubleRange = range;

                        range = new PQSLandControl.LerpRange();
                        range.endEnd = 2;
                        range.endStart = 2;
                        range.startEnd = -1;
                        range.startStart = -1;
                        landClass.longitudeRange = range;

                        landClass_ = landClass;
                    }
                    List<Object> collection = Collection.ToList();
                    collection.Add(landClass_);
                    Collection = collection;
                    Callback(Collection);
                });
            }
        }
    }
}