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

            // Instance memeber
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

                if (isPQSBrowser)
                    pqsBrowserWindow = PQSBrowser.Render(pqsBrowserWindow, "PQS Explorer");

                if (isCBBrowser)
                    cbBrowserWindow = CBBrowser.Render(cbBrowserWindow, "CelestialBody Explorer");
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