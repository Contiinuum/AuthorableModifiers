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
    
    public class ArenaRotation : Modifier
    {
        public bool Continuous { get; set; }
        public bool Incremental { get; set; }
        /*public ArenaRotation(ModifierType _type, float _startTick, float _endTick, float _amount, bool _continuous, bool _incremental)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            Amount = _amount;
            continuous = _continuous;
            incremental = _incremental;
        }*/

        public override void Activate()
        {
            if (!IsSingleUse) Amount /= 100f;
            base.Activate();

            if (Incremental)
            {
                Amount *= 2f;
                MelonCoroutines.Start(DoRotationIncremental());
            }
            if (Continuous)
            {
                MelonCoroutines.Start(DoRotation());
            }
            else
            {
                ArenaLoaderMod.RotateSkybox(Amount);
            }
           
        }

        private IEnumerator DoRotationIncremental()
        {
            //float oldRotation = RenderSettings.skybox.GetFloat("_Rotation");
            float startTick = AudioDriver.I.mCachedTick;
            while (Active)
            {
               
                float percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (EndTick - startTick);
                float currentRot = Mathf.Lerp(0f, Amount, percentage / 100f);
                //RenderSettings.skybox.SetFloat("_Rotation", currentRot);
                ArenaLoaderMod.RotateSkybox(currentRot);
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }
            yield return null;
        }

        private IEnumerator DoRotation()
        {
            while (Active)
            {
                ArenaLoaderMod.RotateSkybox(Amount);
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }
            yield return null;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            
        }
    }
}
