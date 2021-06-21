using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace AuthorableModifiers
{
    public class AimAssistChange : Modifier
    {
        /*public AimAssistChange(ModifierType _type, float _startTick, float _endTick, float _amount)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            Amount = _amount;
        }*/

        public override void Activate()
        {           
            base.Activate();
            PlayerPreferences.I.AimAssistAmount.mVal = Amount;       
        }

        public override void Deactivate()
        {
            base.Deactivate();
            PlayerPreferences.I.AimAssistAmount.mVal = 1f;
        }
    }
}
