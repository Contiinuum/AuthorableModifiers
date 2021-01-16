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
        private static float defaultPsychadeliaPhaseSeconds = 14.28f;
        private float psychadeliaTimer = 0.0f;

        public Psychedelia(ModifierType _type, float _startTick, float _endTick, float _amount)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
        }

        public override void Activate()
        {
            amount /= 20f;
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
            while (active)
            {              
                float phaseTime = defaultPsychadeliaPhaseSeconds / amount;
                
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
