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
            if (strobo)
            {
                if (amount == 0f) amount = 1f;
                MelonCoroutines.Start(DoStrobo());
            }
            else if (continuous)
            {
                MelonCoroutines.Start(ExposureChangeContinuous());
            }
            else
            {
                AudicaMod.currentSkyboxExposure = 0f;
                float newExposure = AuthorableModifiers.defaultArenaBrightness / 100f;
                newExposure *= amount;
                AudicaMod.ChangeExposure(newExposure);
            }

        }
        private IEnumerator DoStrobo()
        {
            float dir = 1;
            if (AuthorableModifiers.defaultArenaBrightness / RenderSettings.skybox.GetFloat("_Exposure") >= .5f) dir = 0;          
            float interval = (480f / amount);
            float nextStrobe = startTick;
            while (active)
            {
                if(nextStrobe <= AudioDriver.I.mCachedTick)
                {
                    AudicaMod.currentSkyboxExposure = 0f;
                    AudicaMod.ChangeExposure(AuthorableModifiers.defaultArenaBrightness * dir);
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
            float newAmount = AuthorableModifiers.defaultArenaBrightness / 1000f;
            newAmount *= amount;
            if (AudicaMod.currentSkyboxExposure > AuthorableModifiers.defaultArenaBrightness) AudicaMod.currentSkyboxExposure = AuthorableModifiers.defaultArenaBrightness;
            while (active)
            {
                if (AudicaMod.currentSkyboxExposure >= AuthorableModifiers.defaultArenaBrightness) dir = -1;
                else if (AudicaMod.currentSkyboxExposure <= 0f) dir = 1;
                AudicaMod.ChangeExposure(newAmount * dir);
                
                yield return new WaitForSecondsRealtime(.01f);
            }
            yield return null;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            AudicaMod.currentSkyboxExposure = AuthorableModifiers.defaultArenaBrightness;
            AudicaMod.ChangeExposure(0f);
        }
    }
}
