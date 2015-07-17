/*using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using KSP.IO;

//This file is getting a bit large...
//Todo: Comment areas for easier navigation.

namespace Kopernicus
{
    namespace UI
    {
        public class ParticleSaveStorageHelper
        {
            public ParticleSaveStorageHelper(string Target, float speed, float ratemin, float ratemax, float lifemin, float lifemax, float sizemin, float sizemax, float Igrowrate, Color[] ColourArray, Vector3 IParticleRandVelocity)
            {
                TargetPlanet = Target;
                speedScale = speed;

                minEmission = ratemin;
                maxEmission = ratemax;

                lifespanMin = lifemin;
                lifespanMax = lifemax;

                sizeMin = sizemin;
                sizeMax = sizemax;

                growrate = Igrowrate;

                Colour1 = ColourArray[0];
                Colour2 = ColourArray[1];
                Colour3 = ColourArray[2];
                Colour4 = ColourArray[3];
                Colour5 = ColourArray[4];

                ParticleRandVelocity = IParticleRandVelocity;
            }
            public string TargetPlanet;
            public float minEmission, maxEmission, lifespanMin, lifespanMax, speedScale, sizeMin, sizeMax, growrate;
            public Color Colour1, Colour2, Colour3, Colour4, Colour5;
            public Vector3 ParticleRandVelocity;
        }

        //PlanetUI Class
        public class PlanetToolsUiController : MonoBehaviour
        {
            int numberGeneratedPlanets = 0;

            //Main UI
            bool ShouldExportScaledMap = false;
            void WindowFunction(int windowID)
            {
                if (GUI.Button(new Rect(20, 30, 200, 20), "Atmosphere SFX tools"))
                {
                    selector = 1;
                } if (GUI.Button(new Rect(220, 30, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("AtmoEdit"); }

                if (GUI.Button(new Rect(20, 60, 200, 20), "CB Editor"))
                {
                    selector = 4;
                } if (GUI.Button(new Rect(220, 60, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("CBEdit"); }

                if (GUI.Button(new Rect(20, 90, 200, 20), "PQS Editor"))
                {
                    selector = 3;
                    pqsModderStage = 0;
                } if (GUI.Button(new Rect(220, 90, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("PQSEdit"); }

                if (GUI.Button(new Rect(20, 120, 200, 20), "Planet Selection"))
                {
                    //Templates.Clear();
                    //ListPlanetsRecursive( PSystemManager.Instance.systemPrefab.rootBody );
                    Templates.Clear();
                    ListPlanetsRecursive(PSystemManager.Instance.systemPrefab.rootBody);

                    selector = 2;
                } if (GUI.Button(new Rect(220, 120, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("PlanetSelector"); }

                GUI.Label(new Rect(20, 150, 200, 20), "Planet Selected: " + TemplateName);

                if (GUI.Button(new Rect(20, 180, 200, 20), "Orbit Editor"))
                {
                    if (TemplateName != null)
                    {
                        Lookupbody = Utils.FindCB(TemplateName).orbitDriver.referenceBody.name;
                    }
                    selector = 6;
                } if (GUI.Button(new Rect(220, 180, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("OrbitEditor"); }

                if (GUI.Button(new Rect(20, 210, 200, 20), "UI Options (INCOMPLETE)"))
                {
                    //Todo: Options menu, export/import settings, for example.

                }

                //Export options
                ShouldExportScaledMap = GUI.Toggle(new Rect(250, 240, 300, 20), ShouldExportScaledMap, "Export?");
                if (GUI.Button(new Rect(20, 240, 200, 20), "ScaledSpace updater"))
                {
                    //Update scaledspace with as little lag as possible... (Nope, will lag like crazy.)
                    Texture2D PlanetColours;
                    Texture2D[] textures;
                    GameObject localSpace = Utils.FindLocal(TemplateName);
                    GameObject scaledSpace = Utils.FindScaled(TemplateName);

                    PQS pqsGrabtex = localSpace.GetComponentInChildren<PQS>();
                    textures = pqsGrabtex.CreateMaps(pqsGrabtex.mapFilesize, pqsGrabtex.mapMaxHeight, pqsGrabtex.mapOcean, pqsGrabtex.mapOceanHeight, pqsGrabtex.mapOceanColor);

                    Texture2D Normal = Utils.BumpToNormalMap(textures[1], 9);

                    //Save textures to file.
                    if (ShouldExportScaledMap)
                    {
                        System.IO.Directory.CreateDirectory("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName);

                        byte[] ExportColourMap = textures[0].EncodeToPNG();
                        System.IO.File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/colourMap.png", ExportColourMap);

                        ExportColourMap = textures[1].EncodeToPNG();
                        System.IO.File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/heightMap.png", ExportColourMap);

                        ExportColourMap = Normal.EncodeToPNG();
                        System.IO.File.WriteAllBytes("GameData/KittopiaSpace/Textures/ScaledSpace/" + TemplateName + "/bumpMap.png", ExportColourMap);
                    }

                    PlanetColours = textures[0];

                    MeshRenderer planettextures = scaledSpace.GetComponentInChildren<MeshRenderer>();
                    planettextures.material.SetTexture("_MainTex", PlanetColours);
                    planettextures.material.SetTexture("_BumpMap", Normal);

                    var scaledmesh = ScaledPlanetMesh.Generate(pqsGrabtex, scaledSpace.GetComponentInChildren<MeshFilter>().mesh);
                    scaledmesh.ApplyToScaledSpace(scaledSpace);

                    if (ShouldExportScaledMap)
                    {
                        ScaledPlanetMesh.Save(TemplateName, scaledmesh);
                    }
                } if (GUI.Button(new Rect(220, 240, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("ScaledSpaceUpdate"); }

                if (GUI.Button(new Rect(20, 270, 200, 20), "Ocean Tools"))
                {
                    OceanToolsUiSelector = 0;
                    selector = 5;
                } if (GUI.Button(new Rect(220, 270, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("Oceans"); }

                if (GUI.Button(new Rect(20, 300, 200, 20), "Save data"))
                {
                    if (TemplateName != null)
                    {
                        SaveData();
                    }
                } if (GUI.Button(new Rect(220, 300, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("Saving"); }

                if (GUI.Button(new Rect(20, 330, 200, 20), "Load data"))
                {
                    if (TemplateName != null)
                    {
                        LoadData(TemplateName, "GameData/KittopiaSpace/SaveLoad/");
                    }
                }

                if (GUI.Button(new Rect(20, 360, 200, 20), "Modify Starlight data"))
                {
                    //PlanetUtils.FixStar( TemplateName );
                    //SaveStarData( TemplateName );

                    selector = 9;
                } if (GUI.Button(new Rect(220, 360, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("StarFix"); }

                if (GUI.Button(new Rect(20, 390, 200, 20), "Ring tools"))
                {
                    selector = 7;
                } if (GUI.Button(new Rect(220, 390, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("RingTools"); }

                if (GUI.Button(new Rect(20, 420, 200, 20), "HACK: Instantiate " + TemplateName))
                {
                    //PQSMod_ScatterController

                    //Hack
                    foreach (PSystemBody PSB in Templates)
                    {
                        if (PSB.celestialBody.name == TemplateName)
                        {
                            print("Starting...\n");
                            numberGeneratedPlanets++;
                            PSystemBody NewPlanet = (PSystemBody)Instantiate(PSB);

                            NewPlanet.name = "GeneratedPlanet" + numberGeneratedPlanets;
                            NewPlanet.flightGlobalsIndex = 9001 + numberGeneratedPlanets;
                            NewPlanet.children.Clear();

                            GameObject LocalSpaceObj = (GameObject)Instantiate(Utils.FindLocal(TemplateName));
                            LocalSpaceObj.name = "GeneratedPlanet" + numberGeneratedPlanets;
                            LocalSpaceObj.transform.parent = LocalSpace.fetch.transform;

                            NewPlanet.pqsVersion = LocalSpaceObj.GetComponentInChildren<PQS>();
                            NewPlanet.pqsVersion.name = "GeneratedPlanet" + numberGeneratedPlanets;

                            CelestialBody NewPlanetBody = LocalSpaceObj.GetComponent<CelestialBody>();

                            NewPlanetBody.orbitingBodies.Clear();

                            NewPlanetBody.bodyName = "GeneratedPlanet" + numberGeneratedPlanets;
                            NewPlanetBody.bodyDescription = "GeneratedPlanet" + numberGeneratedPlanets;

                            //Stuff
                            NewPlanet.orbitRenderer.celestialBody = NewPlanetBody;
                            NewPlanet.orbitRenderer.driver = NewPlanetBody.orbitDriver;
                            NewPlanet.orbitRenderer.drawIcons = OrbitRenderer.DrawIcons.OBJ;

                            NewPlanetBody.orbitDriver.Renderer = NewPlanet.orbitRenderer;
                            NewPlanetBody.orbitDriver.orbitColor = new Color(1, 0, 0, 1);

                            System.Random rand = new System.Random();

                            int SMA = rand.Next(90000000, 2000000000);
                            int inc = UnityEngine.Random.Range(0, 360);

                            NewPlanetBody.orbitDriver.orbit = new Orbit(inc, 0, SMA, 0, 0, 0, 0, Utils.FindCB("Sun"));
                            NewPlanetBody.orbitDriver.UpdateOrbit();

                            NewPlanet.celestialBody = NewPlanetBody;

                            try
                            {
                                MapObject ClonedMapObj = (MapObject)Instantiate(PlanetariumCamera.fetch.GetTarget(PlanetariumCamera.fetch.GetTargetIndex(TemplateName)));
                                DontDestroyOnLoad(ClonedMapObj);
                                ClonedMapObj.name = "GeneratedPlanet" + numberGeneratedPlanets;
                                ClonedMapObj.celestialBody = NewPlanetBody;
                                ClonedMapObj.type = MapObject.MapObjectType.CELESTIALBODY;
                                ClonedMapObj.tgtRef = LocalSpaceObj.transform;

                                PlanetariumCamera.fetch.AddTarget(ClonedMapObj);
                            }
                            catch
                            {

                            }

                            NewPlanetBody.CBUpdate();

                            FlightGlobals.fetch.bodies.Add(NewPlanetBody);
                            Planetarium.fetch.orbits.Add(NewPlanetBody.orbitDriver);

                            Templates[0].children.Add(NewPlanet);

                            NewPlanet.pqsVersion.RebuildSphere();

                            PQSMod_CelestialBodyTransform cbt = Utils.FindLocal("GeneratedPlanet" + numberGeneratedPlanets).GetComponentInChildren<PQSMod_CelestialBodyTransform>();
                            cbt.body = NewPlanetBody;
                            cbt.RebuildSphere();
                        }
                    }

                    Templates.Clear();
                    ListPlanetsRecursive(PSystemManager.Instance.systemPrefab.rootBody);
                } if (GUI.Button(new Rect(220, 420, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("Instantiate"); }

                if (GUI.Button(new Rect(20, 450, 200, 20), "Planetary Particles"))
                {
                    //PlanetUtils.AddParticleEmitter( TemplateName );
                    selector = 8;
                } if (GUI.Button(new Rect(220, 450, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("Particles"); }

                if (GUI.Button(new Rect(20, 480, 200, 20), "Ground Scatter Editor"))
                {
                    selector = 10;
                } if (GUI.Button(new Rect(220, 450, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("ScatterEditor"); }

                GUI.EndScrollView();

                switch (selector)
                {
                    case 1: AFGEditorFunc(); break;
                    case 2: TemplateSelector(); break;
                    case 3: PQSModderPT1(); break;
                    case 4: CBModifier(); break;
                    case 5: OceanToolsUI(); break;
                    case 6: OrbitEditorUI(); break;
                    case 7: RingEditorFunc(); break;
                    case 8: ParticleEditorUI(); break;
                    case 9: StarEditorUI(); break;
                    case 10: ScatterEditorUI(); break;
                    default: break;
                }

                GUI.DragWindow();
            }
            string RadiusAddNumber = "0";
            //float RadiusAddNumberOld = 0.0f;

            private void AFGEditorFunc()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "No planet selected!");
                    return;
                }

                ScrollPosition2 = GUI.BeginScrollView(new Rect(0, 280, 400, 250), ScrollPosition2, new Rect(0, 280, 400, 290));

                AtmosphereFromGround AtmoToMod = Utils.FindScaled(TemplateName).GetComponentInChildren<AtmosphereFromGround>();

                if (AtmoToMod == null)
                {
                    if (GUI.Button(new Rect(20, yoffset, 200, 20), "Add atmosphere to: " + TemplateName))
                    {
                        PlanetUtils.AddAtmoFX(TemplateName, 1, new Color(1.0f, 0.5f, 0.2f, 0.0f), 0);
                        PlanetarySettings[TemplateName].AddAtmoFX = true;
                    }
                    return;
                }
                else
                {
                    //Atmo Settings
                    GUI.Label(new Rect(20, yoffset, 200, 20), "Radius Addition (Legacy)");
                    yoffset += 30;
                    RadiusAddNumber = GUI.TextField(new Rect(20, yoffset, 200, 20), RadiusAddNumber);
                    if (GUI.Button(new Rect(210, yoffset, 100, 20), "Update"))
                    {
                        PlanetUtils.RecalculateAtmo(TemplateName, (float)System.Convert.ToSingle(RadiusAddNumber));
                    }
                    yoffset += 30;
                    GUI.Label(new Rect(20, yoffset, 200, 20), "Transform Scale(Legacy)");
                    yoffset += 30;
                    AtmoToMod.gameObject.transform.localScale = Vector3.one * StrToFloat(GUI.TextField(new Rect(20, yoffset, 200, 20), "" + AtmoToMod.transform.localScale.x));
                    yoffset += 30;
                    GUI.Label(new Rect(20, yoffset, 200, 20), "Atmo Scale Var");
                    yoffset += 30;
                    GUI.TextField(new Rect(20, yoffset, 200, 20), "" + AtmoToMod.scale);

                    //RadiusAddNumberOld = StrToFloat( RadiusAddNumber );

                    yoffset += 30;

                    GUI.Label(new Rect(20, yoffset, 100, 20), "WaveColour");
                    if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                    {
                        Color getColour;
                        getColour = AtmoToMod.waveLength;
                        rVal = getColour.r;
                        gVal = getColour.g;
                        bVal = getColour.b;
                        aVal = getColour.a;

                        System.Object obj = (System.Object)AtmoToMod;
                        objToEdit = obj;
                        KeyToEdit = AtmoToMod.GetType().GetField("waveLength");

                        isshowingColourEditor = true;
                    }

                    yoffset += 30;

                    if (GUI.Button(new Rect(20, yoffset, 300, 20), "Update Scaledspace atmo shader"))
                    {
                        //Load rim texture...
                        string RimTex = "GameData/KittopiaSpace/Textures/" + TemplateName + "_rimtex.png";
                        if (!Utils.FileExists(RimTex))
                        {
                            RimTex = "GameData/KittopiaSpace/Textures/default/blank_rim_text.png";
                            if (!Utils.FileExists(RimTex))
                            {
                                GUI.EndScrollView();
                                return;
                            }

                            //Update atmo shader texture
                            Utils.LoadScaledPlanetAtmoShader(TemplateName, Utils.LoadTexture(RimTex, false));
                            PlanetarySettings[TemplateName].ModScaledAtmoShader = true;
                        }
                    }

                    GUI.EndScrollView();
                }
            }

            public static List<PSystemBody> Templates = new List<PSystemBody>();
            public void ListPlanetsRecursive(PSystemBody body)
            {
                Templates.Add(body);
                foreach (PSystemBody current in body.children)
                {
                    ListPlanetsRecursive(current);
                }
            }
            private void TemplateSelector()
            {
                int yoffset = 280;
                int trimmedScrollSize = (Templates.Count() + 1) * 30;
                ScrollPosition2 = GUI.BeginScrollView(new Rect(0, 280, 400, 250), ScrollPosition2, new Rect(0, 280, 400, trimmedScrollSize));
                foreach (PSystemBody body in Templates)
                {
                    if (GUI.Button(new Rect(20, yoffset, 200, 20), body.celestialBody.name))
                    {
                        TemplateName = body.celestialBody.name;
                        if (!PlanetarySettings.ContainsKey(TemplateName))
                        {
                            PlanetarySettings[TemplateName] = new AdditionalSettingsHandler(TemplateName);
                        }
                    }
                    yoffset += 30;
                }
                GUI.EndScrollView();
            }
            private float StrToFloat(string input)
            {
                return (float)System.Convert.ToSingle(input);
            }
            int pqsModderStage = 0;
            PQSMod pqsmodtoMod;
            PQS pqstoMod;

            //PQS Modder PT1
            //GUI for showing list of all PQS mods applied to a body (the initial PQS Editor window). 
            private void PQSModderPT1()
            {
                //Todo: swap with switch?
                if (pqsModderStage == 1)
                {
                    // Show the PQSMod modification GUI instead.
                    PQSModderPT2();
                    return;
                }
                if (pqsModderStage == 2)
                {
                    // Show the PQS modification GUI instead.
                    PQSModderPT3();
                    return;
                }
                if (pqsModderStage == 3)
                {
                    // Show the PQS addition GUI instead.
                    PQSAdderFunc();
                    return;
                }

                // If none of the conditional above is true,
                // then show a list of all PQS mods applied to a body.
                int yoffset = 280;
                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "No planet selected!");
                    return;
                }
                List<PQS> norm_PqsList = new List<PQS>();
                foreach (PQS pqs in Utils.FindLocal(TemplateName).GetComponentsInChildren<PQS>())
                {
                    norm_PqsList.Add(pqs);
                }

                List<PQSMod> PqsList = new List<PQSMod>();
                foreach (PQSMod pqs in Utils.FindLocal(TemplateName).GetComponentsInChildren(typeof(PQSMod)))
                {
                    PqsList.Add(pqs);
                }

                int trimmedScrollSize = ((PqsList.Count() + norm_PqsList.Count()) * 30) + 90;
                ScrollPosition2 = GUI.BeginScrollView(new Rect(0, 260, 420, 250), ScrollPosition2, new Rect(0, 250, 450, trimmedScrollSize));
                foreach (PQS pqs in norm_PqsList)
                {
                    if (GUI.Button(new Rect(20, yoffset, 400, 20), "" + pqs))
                    {
                        //TemplateName = body.celestialBody.name;
                        pqstoMod = pqs;
                        pqsModderStage = 2;
                    }
                    yoffset += 30;
                }
                foreach (PQSMod pqs in PqsList)
                {
                    if (GUI.Button(new Rect(20, yoffset, 400, 20), "" + pqs))
                    {
                        //TemplateName = body.celestialBody.name;
                        pqsmodtoMod = pqs;
                        pqsModderStage = 1;
                    } if (GUI.Button(new Rect(420, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("" + pqs.GetType()); }

                    yoffset += 30;
                }
                yoffset += 30;
                if (GUI.Button(new Rect(20, yoffset, 400, 20), "Add new PQSMod"))
                {
                    pqsModderStage = 3;
                } if (GUI.Button(new Rect(420, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("PQSAdder"); }


                GUI.EndScrollView();
            }

            bool showLandClassmenu;

            //PQS Modder PT2
            //GUI for modifying a PQSMod.
            private void PQSModderPT2()
            {
                ScrollPosition2 = GUI.BeginScrollView(new Rect(20, 280, 380, 250), ScrollPosition2, new Rect(20, 280, 380, 10000));

                int yoffset = 280;
                foreach (FieldInfo key in pqsmodtoMod.GetType().GetFields())
                {
                    try
                    {
                        System.Object obj = (System.Object)pqsmodtoMod;
                        if (key.FieldType == typeof(string))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj)));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, yoffset, 200, 20), (bool)key.GetValue(obj), "Bool"));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(int))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (int)StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (double)StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, yoffset, 100, 20), "" + key.Name);
                            if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                            {
                                Color getColour;
                                getColour = (Color)key.GetValue(obj);
                                rVal = getColour.r;
                                gVal = getColour.g;
                                bVal = getColour.b;
                                aVal = getColour.a;

                                objToEdit = obj;
                                KeyToEdit = key;

                                isshowingColourEditor = true;
                            }
                            //key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            yoffset += 30;

                            Vector3 blah = (Vector3)key.GetValue(obj);

                            blah.x = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 50, 20), "" + blah.x));
                            blah.y = Convert.ToSingle(GUI.TextField(new Rect(80, yoffset, 50, 20), "" + blah.y));
                            blah.z = Convert.ToSingle(GUI.TextField(new Rect(140, yoffset, 50, 20), "" + blah.z));

                            key.SetValue(obj, blah);

                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(PQSLandControl.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, yoffset, 200, 20), "Mod Land Classes"))
                            {
                                LandclassestoMod = (PQSLandControl.LandClass[])key.GetValue(obj);
                                landmodder_mode = 0;
                                landmodder_state = 0;
                                showLandClassmenu = true;
                            }
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, yoffset, 200, 20), "Mod Land Classes"))
                            {
                                VertexLandclassestoMod = (PQSMod_VertexPlanet.LandClass[])key.GetValue(obj);
                                landmodder_mode = 1;
                                landmodder_state = 0;
                                showLandClassmenu = true;
                            }
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(MapSO))
                        {
                            GUI.Label(new Rect(20, yoffset, 100, 20), "" + key.Name + ":");
                            //					if( GUI.Button( new Rect( 150 , yoffset, 50, 20), "Edit" ) )
                            //					{
                            //						//TextureBrowser browser = NewWindows[ "TextureBrowser" ] as TextureBrowser;
                            //						TextureBrowser.UpdateTextureList();
                            //					}
                            //					if( GUI.Button( new Rect( 200 , yoffset, 80, 20), "Save Edit" ) )
                            //					{
                            //						key.SetValue( obj, TextureBrowser.ReturnedMapSo );
                            //					}
                            yoffset += 30;
                            if (GUI.Button(new Rect(20, yoffset, 400, 20), "Load texture from: Textures/" + TemplateName + "/PQS/" + pqsmodtoMod.name))
                            {
                                if (Utils.FileExists("GameInfo/KittopiaSpace/Textures/" + TemplateName + "/PQS/" + pqsmodtoMod.name))
                                {
                                    Texture2D texture = Utils.LoadTexture("GameInfo/KittopiaSpace/Textures/" + TemplateName + "/PQS/" + pqsmodtoMod.name);

                                    MapSO ReturnedMapSo = (MapSO)ScriptableObject.CreateInstance(typeof(MapSO));
                                    ReturnedMapSo.CreateMap(MapSO.MapDepth.RGBA, texture);

                                    key.SetValue(obj, ReturnedMapSo);
                                    print("Pushed MAPSO to object!\n");
                                }
                                else
                                {
                                    print("Failed to load: GameInfo/KittopiaSpace/Textures/" + TemplateName + "/PQS/" + pqsmodtoMod.name + ".png\n");
                                }
                            }

                            yoffset += 30;
                        }
                        else if (key.GetValue(obj).GetType() == typeof(PQS))
                        {
                            //PQS Variable Selector.
                            GUI.Label(new Rect(20, yoffset, 100, 20), "" + key.Name);
                            if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                            {
                                NewWindows["PQSSelector"].ToggleWindow();
                            }
                            if (GUI.Button(new Rect(200, yoffset, 80, 20), "Save Edit"))
                            {
                                key.SetValue(obj, PQSSelector.ReturnedPQS);
                            }

                            yoffset += 30;
                        }
                    }
                    catch { }
                }
                yoffset += 30;

                //PQS Variable Selector.
                GUI.Label(new Rect(20, yoffset, 100, 20), "ParentSphere");
                if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                {
                    NewWindows["PQSSelector"].ToggleWindow();
                }
                if (GUI.Button(new Rect(200, yoffset, 80, 20), "Save Edit"))
                {
                    try
                    {
                        pqsmodtoMod.gameObject.transform.parent = PQSSelector.ReturnedPQS.gameObject.transform;
                    }
                    catch { }
                }

                yoffset += 30;

                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Rebuild"))
                {
                    pqsmodtoMod.RebuildSphere();
                }

                GUI.EndScrollView();
            }

            //PQS Modder PT3
            //GUI for modifying a PQS class.
            private void PQSModderPT3()
            {
                ScrollPosition2 = GUI.BeginScrollView(new Rect(20, 280, 380, 250), ScrollPosition2, new Rect(20, 280, 380, 10000));

                int yoffset = 280;
                foreach (FieldInfo key in pqstoMod.GetType().GetFields())
                {
                    try
                    {
                        System.Object obj = (System.Object)pqstoMod;
                        if (key.FieldType == typeof(string))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj)));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, yoffset, 200, 20), (bool)key.GetValue(obj), "Bool"));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(int))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (int)StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (double)StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, yoffset, 100, 20), "" + key.Name);
                            if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                            {
                                Color getColour;
                                getColour = (Color)key.GetValue(obj);
                                rVal = getColour.r;
                                gVal = getColour.g;
                                bVal = getColour.b;
                                aVal = getColour.a;

                                objToEdit = obj;
                                KeyToEdit = key;

                                isshowingColourEditor = true;
                            }
                            //key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            yoffset += 30;

                            Vector3 blah = (Vector3)key.GetValue(obj);

                            blah.x = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 50, 20), "" + blah.x));
                            blah.y = Convert.ToSingle(GUI.TextField(new Rect(80, yoffset, 50, 20), "" + blah.y));
                            blah.z = Convert.ToSingle(GUI.TextField(new Rect(140, yoffset, 50, 20), "" + blah.z));

                            key.SetValue(obj, blah);

                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(PQSLandControl.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, yoffset, 200, 20), "Mod Land Classes"))
                            {
                                LandclassestoMod = (PQSLandControl.LandClass[])key.GetValue(obj);
                                landmodder_mode = 0;
                                landmodder_state = 0;
                                showLandClassmenu = true;
                            }
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(PQSMod_VertexPlanet.LandClass[]))
                        {
                            if (GUI.Button(new Rect(20, yoffset, 200, 20), "Mod Land Classes"))
                            {
                                VertexLandclassestoMod = (PQSMod_VertexPlanet.LandClass[])key.GetValue(obj);
                                landmodder_mode = 1;
                                landmodder_state = 0;
                                showLandClassmenu = true;
                            }
                            yoffset += 30;
                        }
                    }
                    catch { }
                }
                yoffset += 30;
                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Rebuild"))
                {
                    pqstoMod.RebuildSphere();
                }

                GUI.EndScrollView();
            }

            //PQS Adder
            //GUI for adding more PQSMods to a body.
            void PQSAdderFunc()
            {
                //Urrgg... hacky at best :/
                Type[] types = Assembly.GetAssembly(typeof(PQSMod)).GetTypes();

                int scrollbaroffsetter = 30;
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(PQSMod)))
                    {
                        scrollbaroffsetter += 30;
                    }
                }

                int yoffset = 280;
                ScrollPosition2 = GUI.BeginScrollView(new Rect(20, 280, 380, 250), ScrollPosition2, new Rect(20, 280, 380, scrollbaroffsetter));

                //Still hacky, Im not proud.			
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(PQSMod)))
                    {
                        if (GUI.Button(new Rect(20, yoffset, 200, 20), "" + type.Name))
                        {
                            //Hack^6
                            PQS mainSphere = Utils.FindLocal(TemplateName).GetComponentInChildren<PQS>();
                            PlanetUtils.AddPQSMod(mainSphere, type);

                            pqsModderStage = 0;
                        } if (GUI.Button(new Rect(220, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("" + type.Name); }
                        yoffset += 30;
                    }
                }

                GUI.EndScrollView();
            }

            //Celestail Body Variable Editor
            private void CBModifier()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "No Planet selected!");
                    return;
                }

                CelestialBody cbBody;
                cbBody = Utils.FindCB(TemplateName);

                int scrollbaroffsetter = 30;
                foreach (FieldInfo key in cbBody.GetType().GetFields())
                {
                    scrollbaroffsetter += 30;
                }

                ScrollPosition2 = GUI.BeginScrollView(new Rect(0, 260, 400, 250), ScrollPosition2, new Rect(0, 260, 400, scrollbaroffsetter));

                foreach (FieldInfo key in cbBody.GetType().GetFields())
                {
                    try
                    {
                        System.Object obj = (System.Object)cbBody;
                        if (key.FieldType == typeof(string))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj)));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(bool))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, GUI.Toggle(new Rect(200, yoffset, 200, 20), (bool)key.GetValue(obj), "Bool"));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(int))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (int)StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(float))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(double))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            key.SetValue(obj, (double)StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + key.GetValue(obj))));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(Color))
                        {
                            GUI.Label(new Rect(20, yoffset, 100, 20), "" + key.Name);
                            if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                            {
                                Color getColour;
                                getColour = (Color)key.GetValue(obj);
                                rVal = getColour.r;
                                gVal = getColour.g;
                                bVal = getColour.b;
                                aVal = getColour.a;

                                objToEdit = obj;
                                KeyToEdit = key;

                                isshowingColourEditor = true;
                            }
                            //key.SetValue( obj, (double)StrToFloat( GUI.TextField( new Rect( 200 , yoffset, 200, 20), ""+key.GetValue(obj) ) ));
                            yoffset += 30;
                        }
                        else if (key.FieldType == typeof(Vector3))
                        {
                            GUI.Label(new Rect(20, yoffset, 200, 20), "" + key.Name);
                            yoffset += 30;

                            Vector3 blah = (Vector3)key.GetValue(obj);

                            blah.x = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 50, 20), "" + blah.x));
                            blah.y = Convert.ToSingle(GUI.TextField(new Rect(80, yoffset, 50, 20), "" + blah.y));
                            blah.z = Convert.ToSingle(GUI.TextField(new Rect(140, yoffset, 50, 20), "" + blah.z));

                            key.SetValue(obj, blah);

                            yoffset += 30;
                        }
                    }
                    catch { }
                }
                yoffset += 30;
                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Update"))
                {
                    cbBody.CBUpdate();
                }

                GUI.EndScrollView();
            }

            //Orbit Editor GUI
            string Lookupbody;
            private void OrbitEditorUI()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "No Planet selected!");
                    return;
                }

                CelestialBody cbBody;
                cbBody = Utils.FindCB(TemplateName);
                Orbit orbittoMod = cbBody.orbitDriver.orbit;
                ScrollPosition2 = GUI.BeginScrollView(new Rect(0, 280, 400, 250), ScrollPosition2, new Rect(0, 280, 400, 2000));

                GUI.Label(new Rect(20, yoffset, 200, 20), "Inclination");
                orbittoMod.inclination = StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + orbittoMod.inclination));
                yoffset += 30;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Eccentricity");
                orbittoMod.eccentricity = StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + orbittoMod.eccentricity));
                yoffset += 30;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Semi-Major Axis");
                orbittoMod.semiMajorAxis = StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + orbittoMod.semiMajorAxis));
                yoffset += 30;
                GUI.Label(new Rect(20, yoffset, 200, 20), "LAN");
                orbittoMod.LAN = StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + orbittoMod.LAN));
                yoffset += 30;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Argument Of Periapsis");
                orbittoMod.argumentOfPeriapsis = StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + orbittoMod.argumentOfPeriapsis));
                yoffset += 30;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Mean Anomaly At Epoch");
                orbittoMod.meanAnomalyAtEpoch = StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + orbittoMod.meanAnomalyAtEpoch));
                yoffset += 30;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Epoch");
                orbittoMod.epoch = StrToFloat(GUI.TextField(new Rect(200, yoffset, 200, 20), "" + orbittoMod.epoch));
                yoffset += 30;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Reference Body");

                Lookupbody = GUI.TextField(new Rect(200, yoffset, 200, 20), Lookupbody);

                yoffset += 30;

                if (GUI.Button(new Rect(200, yoffset, 200, 20), "Update Reference Body"))
                {
                    if (Utils.FindCB(Lookupbody) != null)
                    {
                        orbittoMod.referenceBody = Utils.FindCB(Lookupbody);
                    }
                }

                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 200, 20), "Orbit Colour:");
                //colour editor
                if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                {
                    Color getColour;
                    getColour = cbBody.orbitDriver.orbitColor;
                    rVal = getColour.r;
                    gVal = getColour.g;
                    bVal = getColour.b;
                    aVal = getColour.a;

                    objToEdit = (System.Object)cbBody.orbitDriver;
                    KeyToEdit = cbBody.orbitDriver.GetType().GetField("orbitColor");

                    isshowingColourEditor = true;
                }

                yoffset += 30;

                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Update Orbit"))
                {
                    cbBody.orbitDriver.UpdateOrbit();
                }

                yoffset += 30;

                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Deactivate orbit renderer"))
                {
                    cbBody.orbitDriver.Renderer.drawMode = OrbitRenderer.DrawMode.OFF;
                    PlanetarySettings[TemplateName].DeactivateOrbitRenderer = true;
                }

                GUI.EndScrollView();
            }

            //Scaled Space Updater
            /*
            private Vector3[] RegenerateModel(PQS bodyPQS, MeshFilter meshfilter_input, bool isStock = true)
            {
                float scale;
                if (isStock)
                {
                    var originalVert = meshfilter_input.mesh.vertices[0];
                    var originalHeight = (float)bodyPQS.GetSurfaceHeight(originalVert);
                    scale = originalHeight / originalVert.magnitude;
                }
                else // If the planet was made by Kopernicus...
                {
                    float joolScaledRad = 1000f;
                    scale = (float)bodyPQS.radius / joolScaledRad;
                }
				
                bodyPQS.isBuildingMaps = true;
                var newVerts = new Vector3[meshfilter_input.mesh.vertices.Count()];
                for (int i = 0; i < meshfilter_input.mesh.vertices.Count(); i++)
                {
                    var vertex = meshfilter_input.mesh.vertices[i];
                    var rootrad = (float)Math.Sqrt(vertex.x * vertex.x +
                                    vertex.y * vertex.y +
                                    vertex.z * vertex.z);
                    var radius = (float)bodyPQS.GetSurfaceHeight(vertex)/scale;
                    //radius = 1000;
                    newVerts[i] = vertex * (radius / rootrad);
                }
                bodyPQS.isBuildingMaps = false;
			
                meshfilter_input.mesh.vertices = newVerts;
			
                meshfilter_input.mesh.RecalculateNormals();
                Utils.RecalculateTangents(meshfilter_input.mesh);

                return newVerts;
            }
             * */
/*
            //Ocean Tools
            int OceanToolsUiSelector;
            bool UnlitShader = false;
            private void OceanToolsUI()
            {
                switch (OceanToolsUiSelector)
                {
                    case 1:
                        OceanToolsUI_AddOcean();
                        return;
                    case 2:
                        OceanToolsUI_HazardOcean();
                        return;
                    case 3:
                        //OceanToolsUI_ExportOcean();
                        return;
                    default:
                        break;
                }

                int yoffset = 280;
                if (GUI.Button(new Rect(20, yoffset, 400, 20), "Add ocean"))
                {
                    OceanToolsUiSelector = 1;
                }
                yoffset += 60;
                UnlitShader = GUI.Toggle(new Rect(20, yoffset, 200, 20), UnlitShader, "Unlit?");
                yoffset -= 30;
                if (GUI.Button(new Rect(20, yoffset, 400, 20), "Import ocean Texture for " + TemplateName))
                {
                    OceanToolsUI_ImportOcean(UnlitShader);
                }
                yoffset += 60;
                if (GUI.Button(new Rect(20, yoffset, 400, 20), "Hazardous Ocean Module"))
                {
                    OceanToolsUiSelector = 2;
                }
                yoffset += 30;

            }

            private void OceanToolsUI_AddOcean()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "No Planet selected!");
                    return;
                }

                int scrollbaroffsetter = 60;
                foreach (PSystemBody body in Templates)
                {
                    foreach (PQS pqs in Utils.FindLocal(body.celestialBody.name).GetComponentsInChildren(typeof(PQS)))
                    {
                        if (pqs.gameObject.name == (string)(body.celestialBody.name + "Ocean"))
                        {
                            scrollbaroffsetter += 30;
                        }
                    }
                }

                ScrollPosition2 = GUI.BeginScrollView(new Rect(0, 280, 400, 250), ScrollPosition2, new Rect(0, 280, 400, scrollbaroffsetter));
                GUI.Label(new Rect(20, yoffset, 400, 250), "Add ocean of type:");
                yoffset += 30;
                //Get a list of "oceans"
                foreach (PSystemBody body in Templates)
                {
                    foreach (PQS pqs in Utils.FindLocal(body.celestialBody.name).GetComponentsInChildren(typeof(PQS)))
                    {
                        if (pqs.gameObject.name == (string)(body.celestialBody.name + "Ocean"))
                        {
                            if (GUI.Button(new Rect(20, yoffset, 200, 20), pqs.gameObject.name))
                            {
                                PQS LaytheOceanClone = (PQS)Instantiate(pqs.GetComponentInChildren<PQS>());
                                LaytheOceanClone.name = TemplateName + "Ocean";
                                LaytheOceanClone.transform.position = Utils.FindLocal(TemplateName).transform.position;
                                LaytheOceanClone.transform.parent = Utils.FindLocal(TemplateName).transform;
                                LaytheOceanClone.parentSphere = Utils.FindLocal(TemplateName).GetComponentInChildren<PQS>();
                                LaytheOceanClone.radius = Utils.FindCB(TemplateName).Radius;

                                PQSMod_AerialPerspectiveMaterial oceanAPM = LaytheOceanClone.GetComponentInChildren<PQSMod_AerialPerspectiveMaterial>();
                                oceanAPM.sphere = Utils.FindLocal(TemplateName).GetComponentInChildren<PQS>();
                                //LaytheOceanClone.isActive = true;

                                //Instantaiate celestialbodytransform.
                                PQSMod_CelestialBodyTransform CBTransform = (PQSMod_CelestialBodyTransform)Instantiate(Utils.FindLocal(TemplateName).GetComponentInChildren<PQSMod_CelestialBodyTransform>());
                                CBTransform.sphere = LaytheOceanClone;
                                CBTransform.transform.parent = LaytheOceanClone.transform;
                                CBTransform.RebuildSphere();

                                //PQSMod_OceanFX oceanFX_Old = pqs.GetComponentInChildren<PQSMod_OceanFX>();
                                PQSMod_OceanFX oceanFX = LaytheOceanClone.GetComponentInChildren<PQSMod_OceanFX>();
                                oceanFX.sphere = LaytheOceanClone;
                                oceanFX.OnSetup();
                                oceanFX.OnUpdateFinished();

                                //int tempI = 0;
                                //foreach( Texture2D tex in oceanFX_Old.watermain )
                                //{
                                //	oceanFX.watermain[tempI] = tex ;
                                //	tempI++;
                                //}

                                LaytheOceanClone.RebuildSphere();

                                PlanetarySettings[TemplateName].OceanTemplate = pqs.gameObject.name;
                                PlanetarySettings[TemplateName].AddOceanFx = true;
                                PlanetarySettings[TemplateName].HasOceanFx = true;
                            }
                            yoffset += 30;
                        }
                    }
                }

                GUI.EndScrollView();
            }

            private void OceanToolsUI_ImportOcean(bool Unlit = false)
            {
                //Grab ocean Gobj
                foreach (PSystemBody body in Templates)
                {
                    foreach (PQS pqs in Utils.FindLocal(body.celestialBody.name).GetComponentsInChildren(typeof(PQS)))
                    {
                        if (pqs.gameObject.name == TemplateName + "Ocean")
                        {
                            if (!Unlit)
                            {
                                Texture2D Deftex = Utils.LoadTexture("GameData/KittopiaSpace/Textures/" + TemplateName + "/Ocean/Ocean_Def.png");

                                PQSMod_OceanFX oceanFX = pqs.GetComponentInChildren<PQSMod_OceanFX>();

                                pqs.surfaceMaterial = (Material)Instantiate(pqs.surfaceMaterial); //Create a new clone of the texture

                                for (int i = 0; i <= (oceanFX.watermain.Length - 1); i++)
                                {
                                    if (Utils.FileExists("GameData/KittopiaSpace/Textures/" + TemplateName + "/Ocean/Ocean_Frame" + i + ".png"))
                                    {
                                        Texture2D FrameTex = Utils.LoadTexture("GameData/KittopiaSpace/Textures/" + TemplateName + "/Ocean/Ocean_Frame" + i + ".png");
                                        oceanFX.watermain[i] = FrameTex;
                                    }
                                    else
                                    {
                                        pqs.surfaceMaterial.mainTexture = Deftex;
                                        oceanFX.OnSetup();
                                        oceanFX.watermain[i] = Deftex;
                                    }
                                }
                            }
                            else
                            {
                                pqs.surfaceMaterial.shader = Shader.Find("Unlit/Texture");
                                //pqs.fallbackMaterial.shader = Shader.Find("Unlit/Texture");

                                pqs.surfaceMaterial.mainTexture = Utils.LoadTexture("GameData/KittopiaSpace/Textures/" + TemplateName + "/Ocean/Ocean_Def.png");
                                //pqs.fallbackMaterial.mainTexture = Utils.LoadTexture( "GameData/KittopiaSpace/Textures/"+TemplateName+"/Ocean/Ocean_Def.png" );

                            }
                        }
                    }
                }

                PlanetarySettings[TemplateName].OceanLoadTextures = true;
                PlanetarySettings[TemplateName].UnlitOcean = Unlit;
            }

            //Ocean Tools: Hazard Ocean vars
            float HeatRate;
            double MaxDist = 10;
            private void OceanToolsUI_HazardOcean()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "No Planet selected!");
                    return;
                }
                GUI.Label(new Rect(20, yoffset, 300, 20), "Maximum distance from sea level to effect:");
                yoffset += 30;
                MaxDist = Convert.ToDouble(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + MaxDist));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Rate of heating:");
                yoffset += 30;

                HeatRate = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + HeatRate));

                yoffset += 30;

                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Confirm"))
                {
                    PlanetUtils.AddHazardOceanModule(TemplateName, MaxDist, HeatRate);
                    PlanetarySettings[TemplateName].HasHazardOcean = true;
                    PlanetarySettings[TemplateName].HazardOceanRange = MaxDist;
                    PlanetarySettings[TemplateName].HazardOceanRate = HeatRate;
                }
            }

            //Ring tool vars:
            double OuterRadius;
            double InnerRadius;
            float Tilt = 0;
            public Color RingColour = new Color(1, 1, 1);
            bool LockRingRotation;
            bool RingUnlit;
            private void RingEditorFunc()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "NO PLANET SELECTED");
                    return;
                }

                ScrollPosition2 = GUI.BeginScrollView(new Rect(20, 280, 380, 250), ScrollPosition2, new Rect(20, 280, 380, 600));

                GUI.Label(new Rect(20, yoffset, 200, 20), "Inner Radius:");
                yoffset += 30;
                InnerRadius = (double)System.Convert.ToDouble(GUI.TextField(new Rect(20, yoffset, 200, 20), "" + InnerRadius));
                if (GUI.Button(new Rect(220, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("RingInnerRadius"); }

                yoffset += 60;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Outer Radius:");
                yoffset += 30;
                OuterRadius = (double)System.Convert.ToDouble(GUI.TextField(new Rect(20, yoffset, 200, 20), "" + OuterRadius));
                if (GUI.Button(new Rect(220, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("RingOuterRadius"); }

                yoffset += 60;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Inclination:");
                yoffset += 30;
                Tilt = (float)System.Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 200, 20), "" + Tilt));

                yoffset += 60;
                GUI.Label(new Rect(20, yoffset, 200, 20), "Colour:");
                if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                {
                    Color getColour = new Color(1, 1, 1);
                    rVal = getColour.r;
                    gVal = getColour.g;
                    bVal = getColour.b;
                    aVal = getColour.a;

                    objToEdit = (System.Object)this;
                    KeyToEdit = this.GetType().GetField("RingColour");

                    isshowingColourEditor = true;
                }

                yoffset += 60;

                LockRingRotation = GUI.Toggle(new Rect(20, yoffset, 200, 20), LockRingRotation, "Lock Rotation? ");
                if (GUI.Button(new Rect(220, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("RingLockRotation"); }
                yoffset += 30;
                RingUnlit = GUI.Toggle(new Rect(20, yoffset, 200, 20), RingUnlit, "Unlit? ");
                if (GUI.Button(new Rect(220, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("RingUnlit"); }

                yoffset += 60;

                GameObject Ring = null;

                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Generate ring for: " + TemplateName))
                {
                    string PlanetRingTexName = "GameData/KittopiaSpace/Textures/" + TemplateName + "_ring.png";
                    if (Utils.FileExists(PlanetRingTexName))
                    {
                        Ring = PlanetUtils.AddRingToPlanet(Utils.FindScaled(TemplateName), InnerRadius, OuterRadius, Tilt, Utils.LoadTexture(PlanetRingTexName, false), RingColour, LockRingRotation, RingUnlit);
                    }
                    else
                    {
                        PlanetRingTexName = "GameData/KittopiaSpace/Textures/Default/ring.png";
                        if (Utils.FileExists(PlanetRingTexName))
                        {
                            Ring = PlanetUtils.AddRingToPlanet(Utils.FindScaled(TemplateName), InnerRadius, OuterRadius, Tilt, Utils.LoadTexture(PlanetRingTexName, false), RingColour, LockRingRotation, RingUnlit);
                        }
                        else
                        {
                            print("PlanetUI: Critical failure: Default ring texture not found!\n");
                            return;
                        }
                    }
                    PlanetarySettings[TemplateName].AddRing = true;
                    PlanetarySettings[TemplateName].Rings.Add(new RingSaveStorageHelper(Tilt, OuterRadius, InnerRadius, RingColour, Ring, RingUnlit, LockRingRotation));
                } if (GUI.Button(new Rect(220, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("RingCreate"); }
                yoffset += 30;
                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Delete rings on: " + TemplateName))
                {
                    GameObject ScaledPlanet = Utils.FindScaled(TemplateName);
                    GameObject RingGobj = ScaledPlanet.transform.FindChild("PlanetaryRingObject").gameObject;
                    if (RingGobj != null)
                    {
                        Destroy(RingGobj);
                        //todo: delete ring storage class

                        PlanetarySettings[TemplateName].Rings.RemoveAt(0);
                    }
                } if (GUI.Button(new Rect(220, yoffset, 20, 20), "?")) { (NewWindows["HelpWindow"] as HelpWindow).CustomToggle("RingRemove"); }
                yoffset += 30;
                //if( GUI.Button( new Rect( 20 , yoffset, 200, 20), "Volumetrics" ) )
                //{
                //}

                GUI.EndScrollView();
            }

            string ParticleTarget;

            float ParticleEmitMin = 50.0f, ParticleEmitMax = 50.0f;
            float ParticleLifeMin = 1.0f, ParticleLifeMax = 3.0f;
            float ParticleSizeMin = 50.0f, ParticleSizeMax = 50.0f;
            float ParticleSpeedScale = 0.002f;
            float ParticleGrowSpeed = -0.5f;

            Vector3 ParticleRandVelocity = new Vector3(0, 0, 0);

            public Color[] ParticleColours =
		{
			new Color( 1, 1, 1 ),
			new Color( 1, 1, 1 ),
			new Color( 1, 1, 1 ),
			new Color( 1, 1, 1 ),
			new Color( 1, 1, 1 ),
		};

            private void ParticleEditorUI()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "NO PLANET SELECTED");
                    return;
                }

                ScrollPosition2 = GUI.BeginScrollView(new Rect(20, 280, 380, 250), ScrollPosition2, new Rect(20, 280, 380, 900));

                GUI.Label(new Rect(20, yoffset, 300, 20), "Minimum particles to emit each iteration:");
                yoffset += 30;
                ParticleEmitMin = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleEmitMin));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Maximum particles to emit each iteration:");
                yoffset += 30;
                ParticleEmitMax = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleEmitMax));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Minimum particle lifespan:");
                yoffset += 30;
                ParticleLifeMin = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleLifeMin));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Maximum particle lifespan:");
                yoffset += 30;
                ParticleLifeMax = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleLifeMax));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Minimum particle size:");
                yoffset += 30;
                ParticleSizeMin = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleSizeMin));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Maximum particle size:");
                yoffset += 30;
                ParticleSizeMax = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleSizeMax));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Speed Scale ( 1 = reach target planet, -ve values move away from target planet ):");
                yoffset += 30;
                ParticleSpeedScale = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleSpeedScale));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Particle grownth rate:");
                yoffset += 30;
                ParticleGrowSpeed = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleGrowSpeed));
                yoffset += 30;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Planet to reach (Required):");
                yoffset += 30;
                ParticleTarget = GUI.TextField(new Rect(20, yoffset, 300, 20), "" + ParticleTarget);
                yoffset += 30;

                //Colour gradients
                GUI.Label(new Rect(20, yoffset, 300, 20), "Colour Gradient:");
                yoffset += 30;

                for (int i = 0; i <= 4; i++)
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "Colour " + (i + 1) + ":");
                    if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                    {
                        Color getColour = ParticleColours[i];
                        rVal = getColour.r;
                        gVal = getColour.g;
                        bVal = getColour.b;
                        aVal = getColour.a;

                        objToEdit = (System.Object)this;
                        KeyToEdit = GetType().GetField("ParticleColours");
                        ColourArrayIndex = i;

                        isshowingColourEditor = true;
                    }
                    yoffset += 30;
                }

                yoffset += 60;

                GUI.Label(new Rect(20, yoffset, 300, 20), "Random Velocity: " + ParticleRandVelocity);
                yoffset += 30;

                ParticleRandVelocity.x = Convert.ToSingle(GUI.TextField(new Rect(20, yoffset, 50, 20), "" + ParticleRandVelocity.x));
                ParticleRandVelocity.y = Convert.ToSingle(GUI.TextField(new Rect(80, yoffset, 50, 20), "" + ParticleRandVelocity.y));
                ParticleRandVelocity.z = Convert.ToSingle(GUI.TextField(new Rect(140, yoffset, 50, 20), "" + ParticleRandVelocity.z));

                yoffset += 30;

                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Spawn particle system on: " + TemplateName))
                {
                    PlanetUtils.AddParticleEmitter(TemplateName, ParticleTarget, ParticleSpeedScale, ParticleEmitMin, ParticleEmitMax, ParticleLifeMin, ParticleLifeMax, ParticleSizeMin, ParticleSizeMax, ParticleGrowSpeed, ParticleColours.ToArray(), ParticleRandVelocity);
                    PlanetarySettings[TemplateName].AddParticles = true;

                    PlanetarySettings[TemplateName].particles = new ParticleSaveStorageHelper(ParticleTarget, ParticleSpeedScale, ParticleEmitMin, ParticleEmitMax, ParticleLifeMin, ParticleLifeMax, ParticleSizeMin, ParticleSizeMax, ParticleGrowSpeed, ParticleColours.ToArray(), ParticleRandVelocity);
                }

                GUI.EndScrollView();
            }

            CustomStar StarToEdit;
            public Color FlareColour = new Color(0, 0, 0, 0);
            public Color LightColour = new Color(0, 0, 0, 0);
            public Color RimColour = new Color(0, 0, 0, 0);
            public Color SpotColour = new Color(0, 0, 0, 0);
            public Color EmitColour = new Color(0, 0, 0, 0);
            private void StarEditorUI()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "NO PLANET SELECTED");
                    return;
                }

                if (PlanetarySettings[TemplateName].HasStarFix == true)
                {
                    StarToEdit = PlanetarySettings[TemplateName].Star;

                    GUI.Label(new Rect(20, yoffset, 200, 20), "Flare Colour");
                    if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                    {
                        Color getColour = StarToEdit.sunFlare.color;
                        rVal = getColour.r;
                        gVal = getColour.g;
                        bVal = getColour.b;
                        aVal = getColour.a;

                        objToEdit = (System.Object)this;
                        KeyToEdit = this.GetType().GetField("FlareColour");

                        isshowingColourEditor = true;
                    }
                    yoffset += 30;

                    GUI.Label(new Rect(20, yoffset, 200, 20), "Light Colour");
                    if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                    {
                        Color getColour = StarToEdit.sunFlare.color;
                        rVal = getColour.r;
                        gVal = getColour.g;
                        bVal = getColour.b;
                        aVal = getColour.a;

                        objToEdit = (System.Object)this;
                        KeyToEdit = this.GetType().GetField("LightColour");

                        isshowingColourEditor = true;
                    }
                    yoffset += 30;

                    GUI.Label(new Rect(20, yoffset, 200, 20), "Rim Colour");
                    if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                    {
                        Color getColour = RimColour = Utils.FindScaled(TemplateName).renderer.material.GetColor("_RimColor");
                        rVal = getColour.r;
                        gVal = getColour.g;
                        bVal = getColour.b;
                        aVal = getColour.a;

                        objToEdit = (System.Object)this;
                        KeyToEdit = this.GetType().GetField("RimColour");

                        isshowingColourEditor = true;
                    }
                    yoffset += 30;

                    GUI.Label(new Rect(20, yoffset, 200, 20), "Spot Colour");
                    if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                    {
                        Color getColour = SpotColour = Utils.FindScaled(TemplateName).renderer.material.GetColor("_SunspotColor");
                        rVal = getColour.r;
                        gVal = getColour.g;
                        bVal = getColour.b;
                        aVal = getColour.a;

                        objToEdit = (System.Object)this;
                        KeyToEdit = this.GetType().GetField("SpotColour");

                        isshowingColourEditor = true;
                    }
                    yoffset += 30;

                    GUI.Label(new Rect(20, yoffset, 200, 20), "Emit Colour");
                    if (GUI.Button(new Rect(150, yoffset, 50, 20), "Edit"))
                    {
                        Color getColour = EmitColour = Utils.FindScaled(TemplateName).renderer.material.GetColor("_SunspotColor");
                        rVal = getColour.r;
                        gVal = getColour.g;
                        bVal = getColour.b;
                        aVal = getColour.a;

                        objToEdit = (System.Object)this;
                        KeyToEdit = this.GetType().GetField("EmitColour");

                        isshowingColourEditor = true;
                    }
                    yoffset += 30;

                    StarToEdit.sunFlare.color = FlareColour;
                    StarToEdit.MYLight.color = LightColour;
                    Utils.FindScaled(TemplateName).renderer.material.SetColor("_RimColor", RimColour);
                    Utils.FindScaled(TemplateName).renderer.material.SetColor("_SunspotColor", SpotColour);
                    Utils.FindScaled(TemplateName).renderer.material.SetColor("_EmitColor0", EmitColour);
                    Utils.FindScaled(TemplateName).renderer.material.SetColor("_EmitColor1", EmitColour);

                    foreach (SunCoronas corona in Utils.FindScaled(TemplateName).GetComponentsInChildren<SunCoronas>())
                    {
                        corona.gameObject.GetComponentInChildren<MeshRenderer>().material.color = FlareColour;
                    }

                    PlanetarySettings[TemplateName].FlareLight = StarToEdit.sunFlare.color;
                    PlanetarySettings[TemplateName].LightColour = StarToEdit.MYLight.color;
                    PlanetarySettings[TemplateName].RimColour = RimColour;
                    PlanetarySettings[TemplateName].SpotColour = SpotColour;
                    PlanetarySettings[TemplateName].EmitColour = EmitColour;
                }
                else if (PlanetarySettings[TemplateName].HasStarFix == false)
                {
                    if (GUI.Button(new Rect(20, yoffset, 200, 20), "Add StarFix to: " + TemplateName))
                    {
                        PlanetarySettings[TemplateName].HasStarFix = true;
                        PlanetarySettings[TemplateName].Star = PlanetUtils.FixStar(TemplateName);

                        FlareColour = PlanetarySettings[TemplateName].Star.sunFlare.color;
                        LightColour = PlanetarySettings[TemplateName].Star.MYLight.color;
                    }
                }

            }

            private void ScatterEditorUI()
            {
                int yoffset = 280;

                if (TemplateName == "")
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "NO PLANET SELECTED");
                    return;
                }

                //Look for the ground scatter component:
                PQSLandControl scatter = Utils.FindLocal(TemplateName).GetComponentInChildren<PQSLandControl>();
                if (scatter == null)
                {
                    GUI.Label(new Rect(20, yoffset, 200, 20), "Planet has no PQSLandControl!");
                    return;
                }

                yoffset += 30;

                if (GUI.Button(new Rect(20, yoffset, 200, 20), "Import scatter models for: " + TemplateName))
                {
                    int i = 0;
                    foreach (PQSLandControl.LandClassScatter obj in scatter.scatters)
                    {
                        GameObject temporyGObj = GameDatabase.Instance.GetModel("GameData/KittopiaSpace/Models/" + TemplateName + "/scattermesh" + i);
                        MeshFilter ObjMeshFilter = temporyGObj.GetComponent<MeshFilter>();
                        obj.baseMesh = ObjMeshFilter.sharedMesh;
                        obj.material = temporyGObj.renderer.sharedMaterial;
                        i++;
                    }
                }
            }

            public ConfigNode cfgNodes;

            //Data Saver
            private void SaveData()
            {
                string save_dir;
                //string curSave = HighLogic.SaveFolder;

                //if( curSave == null )
                //{
                save_dir = "GameData/KittopiaSpace/SaveLoad/" + TemplateName + ".cfg";
                //}
                //else
                //{
                //	save_dir = "GameData/KittopiaSpace/PlanetUI/"+curSave+".cfg";
                //}

                //Ok, here goes...
                cfgNodes = ConfigNode.Load(save_dir);
                if (cfgNodes == null)
                {
                    cfgNodes = new ConfigNode();
                    cfgNodes.Save(save_dir, TemplateName);
                }

                //Save to file
                if (cfgNodes.HasNode("Planet" + TemplateName))
                {
                    cfgNodes.RemoveNode("Planet" + TemplateName);
                }
                ConfigNode planet_rootnode = cfgNodes.AddNode("Planet" + TemplateName);

                ConfigNode additionTools_root = planet_rootnode.AddNode("AdditionalData");
                additionTools_root.AddValue("Stock", PlanetarySettings[TemplateName].IsStock);
                additionTools_root.AddValue("AddAtmoFx", PlanetarySettings[TemplateName].AddAtmoFX);
                try
                {
                    AtmosphereFromGround AtmoToMod = Utils.FindScaled(TemplateName).GetComponentInChildren<AtmosphereFromGround>();
                    additionTools_root.AddValue("AtmoWaveColour", AtmoToMod.waveLength);
                }
                catch { }
                //additionTools_root.AddValue( "AtmoWaveColour", PlanetarySettings[ TemplateName ].AtmoInvColour );
                additionTools_root.AddValue("AddOceanFx", PlanetarySettings[TemplateName].AddOceanFx);
                if (PlanetarySettings[TemplateName].AddOceanFx == true)
                {
                    additionTools_root.AddValue("OceanTemplate", PlanetarySettings[TemplateName].OceanTemplate);
                }

                additionTools_root.AddValue("OceanLoadTextures", PlanetarySettings[TemplateName].OceanLoadTextures);
                additionTools_root.AddValue("UnlitOcean", PlanetarySettings[TemplateName].UnlitOcean);

                if (PlanetarySettings[TemplateName].HasHazardOcean)
                {
                    ConfigNode OceanNode = additionTools_root.AddNode("HazardOcean");
                    OceanNode.AddValue("Range", PlanetarySettings[TemplateName].HazardOceanRange);
                    OceanNode.AddValue("Rate", PlanetarySettings[TemplateName].HazardOceanRate);
                }

                additionTools_root.AddValue("ModScaledAtmoShader", PlanetarySettings[TemplateName].ModScaledAtmoShader);

                additionTools_root.AddValue("AddRings", PlanetarySettings[TemplateName].AddRing);
                if (PlanetarySettings[TemplateName].AddRing == true)
                {
                    ConfigNode Rings_root = additionTools_root.AddNode("Rings");

                    foreach (RingSaveStorageHelper ring in PlanetarySettings[TemplateName].Rings)
                    {
                        ConfigNode RingNode = Rings_root.AddNode("Ring");
                        RingNode.AddValue("Tilt", ring.tilt);
                        RingNode.AddValue("OuterRadius", ring.OuterRadius);
                        RingNode.AddValue("InnerRadius", ring.InnerRadius);
                        RingNode.AddValue("Colour", ring.Colour);

                        RingNode.AddValue("LockRot", ring.LockRot);
                        RingNode.AddValue("Unlit", ring.Unlit);
                    }
                }

                //Particles...
                additionTools_root.AddValue("AddParticles", PlanetarySettings[TemplateName].AddParticles);
                if (PlanetarySettings[TemplateName].AddParticles == true)
                {
                    ConfigNode Particles_root = additionTools_root.AddNode("Particle");

                    //Save particles!
                    Particles_root.AddValue("TargetPlanet", PlanetarySettings[TemplateName].particles.TargetPlanet);
                    Particles_root.AddValue("minEmission", PlanetarySettings[TemplateName].particles.minEmission);
                    Particles_root.AddValue("maxEmission", PlanetarySettings[TemplateName].particles.maxEmission);
                    Particles_root.AddValue("lifespanMin", PlanetarySettings[TemplateName].particles.lifespanMin);
                    Particles_root.AddValue("lifespanMax", PlanetarySettings[TemplateName].particles.lifespanMax);
                    Particles_root.AddValue("sizeMin", PlanetarySettings[TemplateName].particles.sizeMin);
                    Particles_root.AddValue("sizeMax", PlanetarySettings[TemplateName].particles.sizeMax);
                    Particles_root.AddValue("speedScale", PlanetarySettings[TemplateName].particles.speedScale);
                    Particles_root.AddValue("growrate", PlanetarySettings[TemplateName].particles.growrate);
                    Particles_root.AddValue("ParticleRandVelocity", PlanetarySettings[TemplateName].particles.ParticleRandVelocity);

                    ConfigNode Particles_colours = Particles_root.AddNode("Colours");
                    Particles_colours.AddValue("Colour1", PlanetarySettings[TemplateName].particles.Colour1);
                    Particles_colours.AddValue("Colour2", PlanetarySettings[TemplateName].particles.Colour2);
                    Particles_colours.AddValue("Colour3", PlanetarySettings[TemplateName].particles.Colour3);
                    Particles_colours.AddValue("Colour4", PlanetarySettings[TemplateName].particles.Colour4);
                    Particles_colours.AddValue("Colour5", PlanetarySettings[TemplateName].particles.Colour5);
                }

                //StarFix:
                if (PlanetarySettings[TemplateName].HasStarFix)
                {
                    ConfigNode StarFix_root = planet_rootnode.AddNode("StarFix");

                    StarFix_root.AddValue("FlareColour", PlanetarySettings[TemplateName].FlareLight);
                    StarFix_root.AddValue("LightColour", PlanetarySettings[TemplateName].LightColour);

                    StarFix_root.AddValue("RimColour", PlanetarySettings[TemplateName].RimColour);
                    StarFix_root.AddValue("SpotColour", PlanetarySettings[TemplateName].SpotColour);
                    StarFix_root.AddValue("EmitColour", PlanetarySettings[TemplateName].EmitColour);
                }

                additionTools_root.AddValue("DisableOrbitRenderer", PlanetarySettings[TemplateName].DeactivateOrbitRenderer);

                //Orbit
                CelestialBody cbBody;
                cbBody = Utils.FindCB(TemplateName);

                if (cbBody.GetOrbitDriver() != null)
                {
                    ConfigNode Orbit_Node = planet_rootnode.AddNode("Orbit");
                    Orbit_Node.AddValue("semiMajorAxis", cbBody.orbitDriver.orbit.semiMajorAxis);
                    Orbit_Node.AddValue("eccentricity", cbBody.orbitDriver.orbit.eccentricity);
                    Orbit_Node.AddValue("inclination", cbBody.orbitDriver.orbit.inclination);
                    Orbit_Node.AddValue("meanAnomalyAtEpoch", cbBody.orbitDriver.orbit.meanAnomalyAtEpoch);
                    Orbit_Node.AddValue("epoch", cbBody.orbitDriver.orbit.epoch);
                    Orbit_Node.AddValue("argumentOfPeriapsis", cbBody.orbitDriver.orbit.argumentOfPeriapsis);
                    Orbit_Node.AddValue("LAN", cbBody.orbitDriver.orbit.LAN);

                    Orbit_Node.AddValue("RefBody", cbBody.orbitDriver.orbit.referenceBody.bodyName);

                    Orbit_Node.AddValue("orbitColor", cbBody.orbitDriver.orbitColor);
                }

                ConfigNode CelestialBody_Node = planet_rootnode.AddNode("CelestialBody");

                //CB Dumps
                foreach (FieldInfo key in cbBody.GetType().GetFields())
                {
                    try
                    {
                        System.Object obj = (System.Object)cbBody;
                        if (key.FieldType == typeof(string)
                            || key.FieldType == typeof(double)
                            || key.FieldType == typeof(int)
                            || key.FieldType == typeof(float)
                            || key.FieldType == typeof(bool)
                            || key.FieldType == typeof(Color)
                            || key.FieldType == typeof(Vector3))
                        {
                            CelestialBody_Node.AddValue(key.Name, key.GetValue(obj));
                        }
                    }
                    catch { }
                }
                ConfigNode PQSRoot = planet_rootnode.AddNode("PQS");
                //PQS Dumps
                foreach (PQSMod pqs in Utils.FindLocal(TemplateName).GetComponentsInChildren(typeof(PQSMod)))
                {
                    ConfigNode savePQS = PQSRoot.AddNode("" + pqs.GetType());

                    ConfigIO.SaveConfigNode(pqs, savePQS);
                }
                cfgNodes.Save(save_dir, "CustomData");
            }

            //Data Loader
            public void LoadData(string name, string path)
            {
                Debug.LogWarning("Deactivated Planet-Loader! Could not load data for " + name + "!");
            }
        }
    }
}

*/