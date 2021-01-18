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
    public class Fader : Modifier
    {

        public Fader(ModifierType _type, float _startTick, float _endTick, float _amount)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
        }

        public override void Activate()
        {
            base.Activate();

            if (amount > AuthorableModifiersMod.defaultArenaBrightness) amount = AuthorableModifiersMod.defaultArenaBrightness;
            MelonCoroutines.Start(Fade());

        }

        private IEnumerator Fade()
        {
            float oldExposure = RenderSettings.skybox.GetFloat("_Exposure");
            float oldReflection = RenderSettings.reflectionIntensity;
            ArenaLoaderMod.CurrentSkyboxExposure = oldExposure;
            float startTick = AudioDriver.I.mCachedTick;
            while (active)
            {
                float percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (endTick - startTick);
                float currentExp = Mathf.Lerp(oldExposure, amount, percentage / 100f);
                float targetReflection = amount / AuthorableModifiersMod.defaultArenaBrightness;
                targetReflection = .5f + (amount * targetReflection);
                float currentReflection = Mathf.Lerp(oldReflection, targetReflection, percentage / 100f);
                RenderSettings.skybox.SetFloat("_Exposure", currentExp);
                ArenaLoaderMod.CurrentSkyboxReflection = 0f;
                ArenaLoaderMod.ChangeReflectionStrength(currentReflection);
                ArenaLoaderMod.CurrentSkyboxExposure = currentReflection;
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
                
            }
            yield return null;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            RenderSettings.skybox.SetFloat("_Exposure", amount);
            RenderSettings.reflectionIntensity = amount;            
        }
    }
}
