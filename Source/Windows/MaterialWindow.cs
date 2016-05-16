/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This class renders a window to edit a float curve
        /// </summary>
        [Position(500, 20, 420, 450)]
        public class MaterialWindow : Window<Material>
        {
            /// <summary>
            /// Returns the Title of the window
            /// </summary>
            protected override String Title()
            {
                return "KittopiaTech - Curve Editor";
            }

            /// <summary>
            /// The Kopernicus material wrapper
            /// </summary>
            private System.Object kopernicusMaterial { get; set; }

            /// <summary>
            /// Renders the Window
            /// </summary>
            protected override void Render(Int32 id)
            {
                // Get the Kopernicus-Parser Type
                Type type = null;
                if (kopernicusMaterial == null)
                {
                    IEnumerable<Type> types = AssemblyLoader.loadedAssemblies.SelectMany(a => a.assembly.GetTypes());
                    IEnumerable<Type> materials = types.Where(t => t.BaseType == typeof (Material));
                    foreach (Type t in materials)
                    {
                        // Big Reflection, because of internal class
                        Type singleton = types.First(t2 => t2.FullName == t.FullName + "+Properties");
                        String shaderName = (singleton.GetProperty("shader", BindingFlags.Static | BindingFlags.Public).GetValue(null, null) as Shader)?.name;

                        // Compare things and break
                        if (shaderName != Current.shader.name) continue;
                        type = t;
                        kopernicusMaterial = Activator.CreateInstance(t, Current);
                        break;
                    }
                }

                // Scroll
                BeginScrollView(400, Utils.GetScrollSize(type) + 75);

                // Render Object
                RenderObject(kopernicusMaterial);

                // Exit
                index++;
                Current = kopernicusMaterial as Material;
                Button("Exit", () => UIController.Instance.DisableWindow(KittopiaWindows.Material));

                // End scroll
                EndScrollView();
            }

            /// <summary>
            /// Resets objects
            /// </summary>
            protected override void SetEditedObject()
            {
                kopernicusMaterial = null;
            }
        }
    }
}