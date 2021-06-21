using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using System.Threading;

namespace AuthorableModifiers
{
    public class Psychedelia : Modifier
    {
        private readonly static float defaultPsychadeliaPhaseSeconds = 14.28f;
        private float psychadeliaTimer = 0.0f;

        /*public Psychedelia(ModifierType _type, float _startTick, float _endTick, float _amount)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            Amount = _amount;
        }*/

        public override void Activate()
        {
            Amount /= 20f;
            AuthorableModifiersMod.activePsychedelia = this;
            base.Activate();
            MelonCoroutines.Start(DoPsychedelia());
        }

        public override void Deactivate()
        {
            AuthorableModifiersMod.activePsychedelia = null;
            base.Deactivate();
            GameplayModifiers.I.mPsychedeliaPhase = 0.000001f;     
        }
        private IEnumerator DoPsychedelia()
        {
            while (Active)
            {              
                float phaseTime = defaultPsychadeliaPhaseSeconds / Amount;
                
                if (psychadeliaTimer <= phaseTime)
                {
                    
                    psychadeliaTimer += Time.deltaTime;

                    float forcedPsychedeliaPhase = psychadeliaTimer / phaseTime;
                    GameplayModifiers.I.mPsychedeliaPhase = forcedPsychedeliaPhase;
                }
                else
                {
                    psychadeliaTimer = 0;
                }
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }           
        }
    }
}
