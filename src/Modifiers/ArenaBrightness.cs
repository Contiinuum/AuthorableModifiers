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

        public bool Continuous { get; set; }
        public bool Strobo { get; set; }
        /*public ArenaBrightness(ModifierType _type, float _startTick, float _endTick, float _amount, bool _continuous, bool _strobo)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            Amount = _amount;
            Continuous = _continuous;
            Strobo = _strobo;
        }*/

        public override void Activate()
        {
            base.Activate();
            if (AuthorableModifiersMod.skyboxLimitSet) Amount *= AuthorableModifiersMod.skyboxLimit;
            if (!Continuous) Amount *= Config.intensity;

            if (Strobo)
            {
                //if (amount == 0f) amount = maxBrightness * Config.intensity;
                MelonCoroutines.Start(DoStrobo());
            }
            else if (Continuous)
            {
                MelonCoroutines.Start(ExposureChangeContinuous());
            }
            else
            {
                ArenaLoaderMod.CurrentSkyboxExposure = 0f;
                float newExposure = AuthorableModifiersMod.defaultArenaBrightness / 100f;
                newExposure *= Amount;
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
            float interval = (480f / Amount);
            float nextStrobe = StartTick;
            while (Active)
            {
                if(nextStrobe <= AudioDriver.I.mCachedTick)
                {
                    ArenaLoaderMod.CurrentSkyboxExposure = 0f;
                    ArenaLoaderMod.CurrentSkyboxReflection = 0f;
                    float amnt = AuthorableModifiersMod.defaultArenaBrightness * dir;
                    if (dir == 1)
                    {
                        amnt *= Config.intensity;
                        if (AuthorableModifiersMod.skyboxLimitSet) amnt *= AuthorableModifiersMod.skyboxLimit;
                    }
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
            newAmount *= Amount;
            if (ArenaLoaderMod.CurrentSkyboxExposure > AuthorableModifiersMod.defaultArenaBrightness) ArenaLoaderMod.CurrentSkyboxExposure = AuthorableModifiersMod.defaultArenaBrightness;
            while (Active)
            {
                if (ArenaLoaderMod.CurrentSkyboxExposure >= AuthorableModifiersMod.defaultArenaBrightness) dir = -1;
                else if (ArenaLoaderMod.CurrentSkyboxExposure <= 0f) dir = 1;
                ArenaLoaderMod.ChangeExposure(newAmount * dir);
                ArenaLoaderMod.ChangeReflectionStrength(newAmount * dir);
                
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
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
