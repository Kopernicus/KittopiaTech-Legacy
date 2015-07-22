using PFUtilityAddon;
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
        [KSPAddon(KSPAddon.Startup.MainMenu, false)]
        public class UIController : MonoBehaviour
        {
            // Window states
            public bool isGUI = false;
            public bool isColor = false;
            public bool isLandClass = false;
            public bool isSimplexWrapper = false;
            public bool isNoiseModWrapper = false;
            public bool isLerpRange = false;
            public bool isFileBrowser = false;
            public bool isBiome = false;
            public bool isMaterial = false;

            // Instance memeber
            public static UIController Instance { get; private set; }

            // Window Positions
            Rect mainWindow = new Rect(20, 20, 420, 560);
            Rect colorWindow = new Rect(420, 20, 400, 200);
            Rect landClassWindow = new Rect(420, 400, 420, 250);
            Rect simplexWrapperWindow = new Rect(420, 20, 420, 190);
            Rect noiseModWrapperWindow = new Rect(420, 20, 420, 170);
            Rect lerpRangeWindow = new Rect(420, 20, 420, 260);
            Rect fileBrowserWindow = new Rect(500, 20, 530, 380);
            Rect biomeWindow = new Rect(420, 400, 420, 250);
            Rect materialWindow = new Rect(500, 20, 420, 450);

            // OnGUI() => Render the planetary UI
            public void OnGUI()
            {
                if (isGUI)
                    mainWindow = PlanetUI.Render(mainWindow, "KittopiaTech - a Kopernicus Visual Editor");

                if (isColor)
                    colorWindow = ColorPicker.Render(colorWindow, "Color Picker");

                if (isLandClass)
                    landClassWindow = LandClassModifier.Render(landClassWindow, "LandClass Modifier");

                if (isSimplexWrapper)
                    simplexWrapperWindow = SimplexWrapper.Render(simplexWrapperWindow, "Simplex Wrapper");

                if (isNoiseModWrapper)
                    noiseModWrapperWindow = NoiseModWrapper.Render(noiseModWrapperWindow, "NoiseMod Wrapper");

                if (isLerpRange)
                    lerpRangeWindow = LerpRange.Render(lerpRangeWindow, "LerpRange");

                if (isFileBrowser)
                    fileBrowserWindow = FileBrowser.Render(fileBrowserWindow, "Browse Files");

                if (isBiome)
                    biomeWindow = BiomeModifier.Render(biomeWindow, "Biome Modifier");

                if (isMaterial)
                    materialWindow = MaterialEditor.Render(materialWindow, "Material Modifier");
            }

            // Awake() => set our Instance member
            public void Awake()
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }

            // Toggles the UI
            public void Update()
            {
                if (Input.GetKeyDown(KeyCode.P) && Input.GetKey(KeyCode.LeftControl))
                    isGUI = !isGUI;
            }
        }
    }
}
