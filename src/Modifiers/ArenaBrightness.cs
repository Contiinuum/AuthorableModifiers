using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using System.Collections;
using ArenaLoader;
namespace AuthorableModifiers
{

    public class ArenaBrightness : Modifier
    {

        public bool continuous;
        public bool strobo;
        public ArenaBrightness(ModifierType _type, float _startTick, float _endTick, float _amount, bool _continuous, bool _strobo)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
            continuous = _continuous;
            strobo = _strobo;
        }

        public override void Activate()
        {
            base.Activate();
            if (!continuous) amount *= Config.intensity;

            if (strobo)
            {
                //if (amount == 0f) amount = maxBrightness * Config.intensity;
                MelonCoroutines.Start(DoStrobo());
            }
            else if (continuous)
            {
                MelonCoroutines.Start(ExposureChangeContinuous());
            }
            else
            {
                ArenaLoaderMod.CurrentSkyboxExposure = 0f;
                float newExposure = AuthorableModifiersMod.defaultArenaBrightness / 100f;
                newExposure *= amount;
                //if (amount < minBrightness) amount = minBrightness;
                ArenaLoaderMod.ChangeExposure(newExposure);
                ArenaLoaderMod.CurrentSkyboxReflection = 0f;
                //float newReflection = newExposure / AuthorableModifiersMod.defaultArenaBrightness;
                //newReflection = .5f + (newExposure * newReflection);
                float newReflection = .5f + (newExposure * .5f);
                ArenaLoaderMod.ChangeReflectionStrength(newReflection);
            }

        }
        private IEnumerator DoStrobo()
        {
            float dir = 1;
            if (AuthorableModifiersMod.defaultArenaBrightness / RenderSettings.skybox.GetFloat("_Exposure") >= .5f) dir = 0;          
            float interval = (480f / amount);
            float nextStrobe = startTick;
            while (active)
            {
                if(nextStrobe <= AudioDriver.I.mCachedTick)
                {
                    ArenaLoaderMod.CurrentSkyboxExposure = 0f;
                    ArenaLoaderMod.CurrentSkyboxReflection = 0f;
                    float amnt = AuthorableModifiersMod.defaultArenaBrightness * dir;
                    if (dir == 1) amnt *= Config.intensity;
                    ArenaLoaderMod.ChangeExposure(amnt);

                    //float newReflection = amnt / AuthorableModifiersMod.defaultArenaBrightness;
                    //newReflection = .5f + (amnt * newReflection);
                    float newReflection = .5f + (amnt * .5f);
                    ArenaLoaderMod.ChangeReflectionStrength(newReflection);
                    if (dir == 1) dir = 0;
                    else if (dir == 0) dir = 1;
                    nextStrobe += interval;
                }
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }
            yield return null;
        }

        private IEnumerator ExposureChangeContinuous()
        {
            float dir = 1;
            float newAmount = AuthorableModifiersMod.defaultArenaBrightness / 1000f;
            newAmount *= amount;
            if (ArenaLoaderMod.CurrentSkyboxExposure > AuthorableModifiersMod.defaultArenaBrightness) ArenaLoaderMod.CurrentSkyboxExposure = AuthorableModifiersMod.defaultArenaBrightness;
            while (active)
            {
                if (ArenaLoaderMod.CurrentSkyboxExposure >= AuthorableModifiersMod.defaultArenaBrightness) dir = -1;
                else if (ArenaLoaderMod.CurrentSkyboxExposure <= 0f) dir = 1;
                ArenaLoaderMod.ChangeExposure(newAmount * dir);
                ArenaLoaderMod.ChangeReflectionStrength(newAmount * dir);
                
                yield return new WaitForSecondsRealtime(.01f);
            }
            yield return null;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            ArenaLoaderMod.CurrentSkyboxExposure = AuthorableModifiersMod.userArenaBrightness;
            ArenaLoaderMod.CurrentSkyboxReflection = AuthorableModifiersMod.userArenaReflection;
            ArenaLoaderMod.ChangeExposure(0f);
            ArenaLoaderMod.ChangeReflectionStrength(0f);
        }
    }
}
