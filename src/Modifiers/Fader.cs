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

        /*public Fader(ModifierType _type, float _startTick, float _endTick, float _amount)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            Amount = _amount;
        }*/

        public override void Activate()
        {
            base.Activate();

            if (Amount > AuthorableModifiersMod.defaultArenaBrightness) Amount = AuthorableModifiersMod.defaultArenaBrightness;
            Amount *= Config.intensity;
            MelonCoroutines.Start(Fade());

        }

        private IEnumerator Fade()
        {
            float oldExposure = RenderSettings.skybox.GetFloat("_Exposure");
            float oldReflection = RenderSettings.reflectionIntensity;
            ArenaLoaderMod.CurrentSkyboxExposure = oldExposure;
            float startTick = AudioDriver.I.mCachedTick;
            while (Active)
            {
                float percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (EndTick - startTick);
                float currentExp = Mathf.Lerp(oldExposure, Amount, percentage / 100f);
                //float targetReflection = amount / AuthorableModifiersMod.defaultArenaBrightness;
                float targetReflection = .5f + (Amount * .5f);
                //targetReflection = .5f + (amount * targetReflection);
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
            RenderSettings.skybox.SetFloat("_Exposure", Amount * Config.intensity);
            RenderSettings.reflectionIntensity = .5f + (.5f * Amount);            
        }
    }
}
