using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorableModifiers
{
    public class PsychedeliaUpdate : Modifier
    {
        /*
        public PsychedeliaUpdate(ModifierType _type, float _startTick, float _amount)
        {
            Type = _type;
            StartTick = _startTick;
            Amount = _amount;
            IsSingleUse = true;
        }
        */
        public override void Activate()
        {
            Amount /= 50f;
            base.Activate();
            if (AuthorableModifiersMod.activePsychedelia is null) return;
            AuthorableModifiersMod.activePsychedelia.Amount = Amount;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}
