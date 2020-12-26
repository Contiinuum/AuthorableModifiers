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
        public ArenaRotation(ModifierType _type, float _startTick, float _endTick, float _amount, bool _continuous)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
            continuous = _continuous;
        }

        public override void Activate()
        {
            base.Activate();

            if (continuous)
            {
                MelonCoroutines.Start(DoRotation());
            }
            else
            {
                AudicaMod.RotateSkybox(amount);
            }
           
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
