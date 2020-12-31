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
    
    public class ArenaRotation : Modifier
    {
        public bool continuous;
        public bool incremental;
        public ArenaRotation(ModifierType _type, float _startTick, float _endTick, float _amount, bool _continuous, bool _incremental)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
            continuous = _continuous;
            incremental = _incremental;
        }

        public override void Activate()
        {
            base.Activate();

            if (incremental)
            {
                amount *= 2f;
                MelonCoroutines.Start(DoRotationIncremental());
            }
            if (continuous)
            {
                MelonCoroutines.Start(DoRotation());
            }
            else
            {
                AudicaMod.RotateSkybox(amount);
            }
           
        }

        private IEnumerator DoRotationIncremental()
        {
            float oldRotation = RenderSettings.skybox.GetFloat("_Rotation");
            float startTick = AudioDriver.I.mCachedTick;
            while (active)
            {
               
                float percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (endTick - startTick);
                float currentRot = Mathf.Lerp(0f, amount, percentage / 100f);
                //RenderSettings.skybox.SetFloat("_Rotation", currentRot);
                AudicaMod.RotateSkybox(currentRot);
                yield return new WaitForSecondsRealtime(.01f);
            }
            yield return null;
        }

        private IEnumerator DoRotation()
        {
            while (active)
            {
                AudicaMod.RotateSkybox(amount);
                yield return new WaitForSecondsRealtime(.01f);
            }
            yield return null;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
        }
    }
}
