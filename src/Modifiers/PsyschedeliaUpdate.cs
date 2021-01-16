using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthorableModifiers
{
    public class PsychedeliaUpdate : Modifier
    {

        public PsychedeliaUpdate(ModifierType _type, float _startTick, float _amount)
        {
            type = _type;
            startTick = _startTick;
            amount = _amount;
            isSingleUseModule = true;
        }

        public override void Activate()
        {
            amount /= 50f;
            base.Activate();
            if (AuthorableModifiersMod.activePsychedelia is null) return;
            AuthorableModifiersMod.activePsychedelia.amount = amount;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}
