using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;

namespace AuthorableModifiers
{
    public class Particles : Modifier
    {
        public Particles(ModifierType _type, float _startTick, float _endTick, float _amount)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
        }

        public override void Activate()
        {
            base.Activate();
            SetParticleScale(amount);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            SetParticleScale(1f);
        }

        public void SetParticleScale(float particleAmount)
        {
            SongCues.Cue[] songCues = SongCues.I.mCues.cues;
            for (int i = 0; i < songCues.Length; i++)
            {
                songCues[i].particleReductionScale = particleAmount;
            }
        }
    }
}
