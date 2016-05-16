/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;
using System.Collections.Generic;
using System.Linq;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to edit a biome
        /// </summary>
        [Position(420, 400, 420, 250)]
        public class BiomeWindow : BulkWindow<CBAttributeMapSO.MapAttribute>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return "KittopiaTech - Biome Editor";
            }

            /// <summary>
            /// Closes the Window
            /// </summary>
            protected override void Exit()
            {
                UIController.Instance.DisableWindow(KittopiaWindows.Biome);
            }

            /// <summary>
            /// Renders the Editor
            /// </summary>
            protected override void RenderEditor(Int32 id)
            {
                // Index
                index++;

                // Scroll
                BeginScrollView(200, Utils.GetScrollSize<CBAttributeMapSO.MapAttribute>() + 50);

                // Render the Object
                RenderObject(Current);

                // Remove Biomes
                Button("Remove Biome", () =>
                {
                    List<CBAttributeMapSO.MapAttribute> collection = Collection.ToList();
                    collection.Remove(Current);
                    Collection = collection;
                    Callback(Collection);
                });

                // Callback
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
                Button("Add Biome", () =>
                {
                    CBAttributeMapSO.MapAttribute biome = new CBAttributeMapSO.MapAttribute { name = "Biome" };
                    List<CBAttributeMapSO.MapAttribute> collection = Collection.ToList();
                    collection.Add(biome);
                    Collection = collection;
                    Callback(Collection);
                });
            }
        }
    }
}