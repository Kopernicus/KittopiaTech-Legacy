using Kopernicus.UI;
using UnityEngine;

namespace PFUtilityAddon
{
    public class Particle_controller_module : MonoBehaviour
    {
        private ParticleEmitter MainEmitter;
        private ParticleAnimator ParticleAnim;
        private ParticleRenderer ParticleRender;

        private GameObject scaledPlanet;
        private GameObject scaledPlanet2;

        private float speedScale;
        private Vector3 RndVel;

        private float minEmission, maxEmission;
        private float lifespanMin, lifespanMax;

        public void Init(string PlanetName, string OtherScaledSpace, float IspeedScale, float IminEmission, float ImaxEmission, float IlifespanMin, float IlifespanMax, float sizeMin, float sizeMax, float sizeGrow, Color[] ColourArray, Vector3 ParticleRandVelocity)
        {
            speedScale = IspeedScale;

            scaledPlanet = Utils.FindScaled(PlanetName);
            scaledPlanet2 = Utils.FindScaled(OtherScaledSpace);
            MainEmitter = (ParticleEmitter)scaledPlanet.AddComponent("MeshParticleEmitter");
            ParticleRender = scaledPlanet.AddComponent<ParticleRenderer>();
            ParticleAnim = scaledPlanet.AddComponent<ParticleAnimator>();

            ParticleRender.material = new Material(Shader.Find("Particles/Alpha Blended"));

            if (System.IO.File.Exists("GameData/KittopiaSpace/Textures/" + PlanetName + "/Particle.png"))
            {
                ParticleRender.material.mainTexture = Utils.LoadTexture("GameData/KittopiaSpace/Textures/" + PlanetName + "/Particle.png");
            }
            else
            {
                ParticleRender.material.mainTexture = Utils.LoadTexture("GameData/KittopiaSpace/Textures/Default/Glow.png");
            }

            ParticleAnim.doesAnimateColor = true;
            ParticleAnim.colorAnimation = ColourArray;

            minEmission = IminEmission;
            maxEmission = ImaxEmission;
            lifespanMin = IlifespanMin;
            lifespanMax = IlifespanMax;

            MainEmitter.maxEmission = maxEmission;
            MainEmitter.maxSize = sizeMax;
            MainEmitter.maxEnergy = lifespanMax;
            MainEmitter.minSize = sizeMin;
            MainEmitter.minEmission = minEmission;
            MainEmitter.minEnergy = lifespanMin;

            MainEmitter.useWorldSpace = false;

            MainEmitter.rndVelocity = ParticleRandVelocity;
            RndVel = ParticleRandVelocity;

            //MainEmitter.localVelocity = new Vector3(0f, 0f, 100f);

            //MainEmitter.emit = true;
            ParticleAnim.sizeGrow = sizeGrow;
        }

        public void Modify(string PlanetName, string OtherScaledSpace, float IspeedScale, float IminEmission, float ImaxEmission, float IlifespanMin, float IlifespanMax, float sizeMin, float sizeMax, float sizeGrow, Color[] ColourArray, Vector3 ParticleRandVelocity)
        {
            speedScale = IspeedScale;

            scaledPlanet2 = Utils.FindScaled(OtherScaledSpace);

            ParticleRender.material.mainTexture = Utils.LoadTexture("GameData/KittopiaSpace/Textures/Default/Glow.png");

            ParticleAnim.doesAnimateColor = true;
            ParticleAnim.colorAnimation = ColourArray;

            minEmission = IminEmission;
            maxEmission = ImaxEmission;
            lifespanMin = IlifespanMin;
            lifespanMax = IlifespanMax;

            MainEmitter.maxEmission = maxEmission;
            MainEmitter.maxSize = sizeMax;
            MainEmitter.maxEnergy = lifespanMax;
            MainEmitter.minSize = sizeMin;
            MainEmitter.minEmission = minEmission;
            MainEmitter.minEnergy = lifespanMin;

            MainEmitter.useWorldSpace = false;

            MainEmitter.rndVelocity = ParticleRandVelocity;
            RndVel = ParticleRandVelocity;

            //MainEmitter.localVelocity = new Vector3(0f, 0f, 100f);

            //MainEmitter.emit = true;
            ParticleAnim.sizeGrow = sizeGrow;
        }

        public void Update()
        {
            Vector3 Speedvec = scaledPlanet2.transform.position;
            Speedvec -= scaledPlanet.transform.position;

            Speedvec *= speedScale;

            MainEmitter.minEnergy = lifespanMin / TimeWarp.CurrentRate;
            MainEmitter.maxEnergy = lifespanMax / TimeWarp.CurrentRate;

            MainEmitter.maxEmission = maxEmission * TimeWarp.CurrentRate;
            MainEmitter.minEmission = minEmission * TimeWarp.CurrentRate;

            MainEmitter.rndVelocity = RndVel * TimeWarp.CurrentRate;

            Speedvec *= TimeWarp.CurrentRate;

            MainEmitter.worldVelocity = Speedvec;
        }
    }
}