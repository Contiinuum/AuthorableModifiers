using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace AuthorableModifiers
{
    public class ZOffset : Modifier
    {
        public float transitionNumberOfTargets;
        private bool endTickSet;

        /*public ZOffset(ModifierType _type, float _startTick, float _endTick, float _amount, float _transitionNumberOfTargets)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
            transitionNumberOfTargets = _transitionNumberOfTargets;
            endTickSet = _endTick != 0 && _startTick != _endTick;
        }*/

        public override void Activate()
        {
            endTickSet = EndTick != 0 && StartTick != EndTick;
            base.Activate();
            SetZOffset(Amount);
        }

        public override void Deactivate()
        {
            SetZOffset(0f);
            base.Deactivate();
           
        }

        public void SetZOffset(float zOffset)
        {
            SongCues.Cue[] songCues = SongCues.I.mCues.cues;            
            float currentCount = 1f;
            for (int i = 0; i < songCues.Length; i++)
            {
                if (songCues[i].tick < StartTick) continue;
                if (songCues[i].tick > EndTick && endTickSet) break;
                if (songCues[i].behavior != Target.TargetBehavior.Melee && songCues[i].behavior != Target.TargetBehavior.Dodge)
                {
                    if (transitionNumberOfTargets > 0)
                    {
                        songCues[i].zOffset = Mathf.Lerp(songCues[i].zOffset, zOffset, currentCount / (float)transitionNumberOfTargets);
                    }
                    else
                    {
                        songCues[i].zOffset = zOffset;
                    }
                    songCues[i].zOffset += AuthorableModifiersMod.oldOffsetDict[songCues[i].tick + (int)songCues[i].handType];
                    if (currentCount < transitionNumberOfTargets) currentCount++;
                }
            }
        }
    }
}
