/** 
 * KittopiaTech - A Kopernicus Visual Editor
 * Copyright (c) Thomas P., BorisBee, KCreator, Gravitasi
 * Licensed under the Terms of a custom License, see LICENSE file
 */

using System;
using Kopernicus.UI.Enumerations;
using UnityEngine;

namespace Kopernicus
{
    namespace UI
    {
        /// <summary>
        /// This starts up the Mod Instance
        /// </summary>
        /// <seealso cref="Controller{KittopiaWindows}" />
        [KSPAddon(KSPAddon.Startup.MainMenu, false)]
        public class UIController : Controller<KittopiaWindows>
        {
            // Instance member
            public static UIController Instance { get; private set; }

            // Window Positions
            private Rect landClassWindow = new Rect(420, 400, 420, 250);

            // Settings
            public static Double PixelPerFrame = 5000d;
            public static Single NormalStrength = 9f;
            
            /// <summary>
            /// Start up, and create needed fields
            /// </summary>
            void Awake()
            {
                // Create the instance
                Instance = this;
                DontDestroyOnLoad(this);

                // Invoke Create
                Create(w => RectCache[w] = GUI.Window(w.GetHashCode(), GetRect(w), w.Render, w.Title()), false);

                // Load the settings
                ConfigNode settings = GameDatabase.Instance.GetConfigs("KittopiaTech")[0].config;
                Double.TryParse(settings.GetValue("pixelPerFrame"), out PixelPerFrame);
                Single.TryParse(settings.GetValue("normalStrength"), out NormalStrength);

                // Register Windows
                RegisterWindow<BiomeWindow>(KittopiaWindows.Biome);
                RegisterWindow<ColorWindow>(KittopiaWindows.Color);
                RegisterWindow<CurveWindow>(KittopiaWindows.Curve);
                RegisterWindow<FileWindow>(KittopiaWindows.Files);
                RegisterWindow<LerpRangeWindow>(KittopiaWindows.LerpRange);
                RegisterWindow<MaterialWindow>(KittopiaWindows.Material);
                RegisterWindow<NoiseModWindow>(KittopiaWindows.NoiseMod);
                RegisterWindow<PlanetWindow>(KittopiaWindows.Planet);
                RegisterWindow<SelectorWindow>(KittopiaWindows.Selector);
                RegisterWindow<SimplexWindow>(KittopiaWindows.Simplex);
            }

            /// <summary>
            /// Renders the UI
            /// </summary>
            void OnGUI()
            {
                RenderUI();
            }

            /// <summary>
            /// Toggles the UI
            /// </summary>
            public void Update()
            {
                if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftControl))
                    ToggleWindow(KittopiaWindows.Planet);
            }
        }
    }
}