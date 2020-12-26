using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace AudicaModding
{
    public class AimAssistChange : Modifier
    {
        public AimAssistChange(ModifierType _type, float _startTick, float _endTick, float _amount)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
        }

        public override void Activate()
        {           
            base.Activate();
            PlayerPreferences.I.AimAssistAmount.mVal = amount;       
        }

        public override void Deactivate()
        {
            base.Deactivate();
            PlayerPreferences.I.AimAssistAmount.mVal = 1f;
        }
    }
}
