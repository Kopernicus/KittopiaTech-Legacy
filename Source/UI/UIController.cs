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
        [KSPAddon(KSPAddon.Startup.Instantly, false)] // For GUI-Testing, will be changed later
        public class UIController : MonoBehaviour
        {
            // Window states
            public bool isGUI = false;
            public bool isColor = false;
            public bool isLandClass = false;
            public bool isSimplexWrapper = false;
            public bool isNoiseModWrapper = false;

            // Instance memeber
            public static UIController Instance { get; private set; }

            // Window Positions
            Rect mainWindow = new Rect(20, 20, 420, 560);
            Rect colorWindow = new Rect(420, 20, 400, 200);
            Rect landClassWindow = new Rect(420, 400, 400, 400);
            Rect simplexWrapperWindow = new Rect(420, 20, 400, 400);
            Rect noiseModWrapperWindow = new Rect(420, 20, 400, 400);

            // OnGUI() => Render the planetary UI
            public void OnGUI()
            {
                if (isGUI)
                    mainWindow = PlanetUI.Render(mainWindow, "KittopiaTech");

                if (isColor)
                    colorWindow = ColorPicker.Render(colorWindow, "Color Picker");

                if (isLandClass)
                    landClassWindow = LandClassModifier.Render(landClassWindow, "LandClass Modifier");

                if (isSimplexWrapper)
                    simplexWrapperWindow = SimplexWrapper.Render(simplexWrapperWindow, "Simplex Wrapper");

                if (isNoiseModWrapper)
                    noiseModWrapperWindow = NoiseModWrapper.Render(noiseModWrapperWindow, "NoiseMod Wrapper");
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
