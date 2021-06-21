using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace AuthorableModifiers
{
    public class InvisibleGuns : Modifier
    {
        /*public InvisibleGuns(ModifierType _type, float _startTick, float _endTick)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
        }*/

        public override void Activate()
        {
            base.Activate();
            GameplayModifiers.I.ActivateModifier(GameplayModifiers.Modifier.InvisibleGuns);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            GameplayModifiers.I.DeactivateModifier(GameplayModifiers.Modifier.InvisibleGuns);
        }
    }
}
