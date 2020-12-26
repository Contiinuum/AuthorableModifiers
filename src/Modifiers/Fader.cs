using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using System.Collections;

namespace AudicaModding
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
            if (amount > AuthorableModifiers.defaultArenaBrightness) amount = AuthorableModifiers.defaultArenaBrightness;
            MelonCoroutines.Start(Fade());

        }

        private IEnumerator Fade()
        {
            float oldExposure = RenderSettings.skybox.GetFloat("_Exposure");
            AudicaMod.currentSkyboxExposure = oldExposure;
            float startTick = AudioDriver.I.mCachedTick;
            while (active)
            {
                float percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (endTick - startTick);
                float currentExp = Mathf.Lerp(oldExposure, amount, percentage / 100f);
                RenderSettings.skybox.SetFloat("_Exposure", currentExp);
                AudicaMod.currentSkyboxExposure = currentExp;
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
                
            }
            yield return null;
        }

        public override void Deactivate()
        {
            RenderSettings.skybox.SetFloat("_Exposure", amount);
            base.Deactivate();
        }
    }
}
