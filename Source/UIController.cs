using System;
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
            public bool isPQSBrowser = false;
            public bool isCBBrowser = false;
            public bool isCurveWindow = false;

            // Instance member
            public static UIController Instance { get; private set; }

            // Window Positions
            private Rect mainWindow = new Rect(20, 20, 420, 560);
            private Rect colorWindow = new Rect(420, 20, 400, 200);
            private Rect landClassWindow = new Rect(420, 400, 420, 250);
            private Rect simplexWrapperWindow = new Rect(420, 20, 420, 190);
            private Rect noiseModWrapperWindow = new Rect(420, 20, 420, 170);
            private Rect lerpRangeWindow = new Rect(420, 20, 420, 260);
            private Rect fileBrowserWindow = new Rect(500, 20, 530, 380);
            private Rect biomeWindow = new Rect(420, 400, 420, 250);
            private Rect materialWindow = new Rect(500, 20, 420, 450);
            private Rect pqsBrowserWindow = new Rect(420, 100, 220, 350);
            private Rect cbBrowserWindow = new Rect(420, 100, 220, 350);
            private Rect curveWindow = new Rect(420, 400, 420, 250);

            // Settings
            public static double pixelPerFrame = 5000d;
            public static float normalStrength = 9f;

            // OnGUI() => Render the planetary UI
            public void OnGUI()
            {
                if (isGUI)
                    mainWindow = PlanetUI.Render(mainWindow, "KittopiaTech - a Kopernicus Visual Editor");

                if (isColor)
                    colorWindow = ColorPicker.Render(colorWindow, "Color Picker");

                if (isLandClass)
                    landClassWindow = LandClassModifier.Render(landClassWindow, "LandClass Editor");

                if (isSimplexWrapper)
                    simplexWrapperWindow = SimplexWrapper.Render(simplexWrapperWindow, "Simplex Editor");

                if (isNoiseModWrapper)
                    noiseModWrapperWindow = NoiseModWrapper.Render(noiseModWrapperWindow, "NoiseMod Editor");

                if (isLerpRange)
                    lerpRangeWindow = LerpRange.Render(lerpRangeWindow, "LerpRange Editor");

                if (isFileBrowser)
                    fileBrowserWindow = FileBrowser.Render(fileBrowserWindow, "File Browser");

                if (isBiome)
                    biomeWindow = BiomeModifier.Render(biomeWindow, "Biome Editor");

                if (isMaterial)
                    materialWindow = MaterialEditor.Render(materialWindow, "Material Editor");

                if (isPQSBrowser)
                    pqsBrowserWindow = PQSBrowser.Render(pqsBrowserWindow, "PQS Explorer");

                if (isCBBrowser)
                    cbBrowserWindow = CBBrowser.Render(cbBrowserWindow, "CelestialBody Explorer");

                if (isCurveWindow)
                    curveWindow = CurveWindow.Render(curveWindow, "Curve Editor");
            }

            // Awake() => set our Instance member
            public void Awake()
            {
                Instance = this;
                DontDestroyOnLoad(this);

                // Load the settings
                ConfigNode settings = GameDatabase.Instance.GetConfigs("KittopiaTech")[0].config;
                Double.TryParse(settings.GetValue("pixelPerFrame"), out pixelPerFrame);
                Single.TryParse(settings.GetValue("normalStrength"), out normalStrength);
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