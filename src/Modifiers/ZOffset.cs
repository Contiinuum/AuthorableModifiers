using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace AudicaModding
{
    public class ZOffset : Modifier
    {
        private Dictionary<int, float> oldOffsets = new Dictionary<int, float>();
        //private Direction direction = Direction.Up;
        public float transitionNumberOfTargets;
        private bool endTickSet;

        public ZOffset(ModifierType _type, float _startTick, float _endTick, float _amount, float _transitionNumberOfTargets)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            amount = _amount;
            transitionNumberOfTargets = _transitionNumberOfTargets;
            endTickSet = _endTick != 0 && _startTick != _endTick;
        }

        public override void Activate()
        {
            base.Activate();
            SetZOffset(amount);
        }

        public override void Deactivate()
        {
            SetZOffset(0f);
            base.Deactivate();
           
        }

        public void SetZOffset(float zOffset)
        {
            SongCues.Cue[] songCues = SongCues.I.mCues.cues;
            //float count = 20f;
            float currentCount = 1f;
            for (int i = 0; i < songCues.Length; i++)
            {
                if (songCues[i].tick < startTick) continue;
                if (songCues[i].tick > endTick && endTickSet) break;
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
                    if(currentCount < transitionNumberOfTargets) currentCount++;
                }
                //songCues[i].target.transform.position = TargetSpawnerLayoutUtil.I.GetPosition(songCues[i].pitch, songCues[i].gridOffset, songCues[i].zOffset);
                //songCues[i].target.transform.rotation = TargetSpawnerLayoutUtil.GetRotation(songCues[i].pitch, songCues[i].gridOffset);
            }

            //direction = Direction.Down;
        }

        /* public void SetZOffset(float zOffset)
         {
             SongCues.Cue[] songCues = SongCues.I.mCues.cues;
             float currentTick = AudioDriver.I.mCachedTick;
             //float count = 20f;
             float currentCount = 1f;
             for (int i = 0; i < songCues.Length; i++)
             {
                 if (songCues[i].tick > endTick) return;
                 if (songCues[i].tick < currentTick) continue;

                 if(songCues[i].behavior != Target.TargetBehavior.Melee && songCues[i].behavior != Target.TargetBehavior.Dodge)
                 {

                     if(direction == Direction.Up)
                     {
                         if(!oldOffsets.ContainsKey(songCues[i].tick + songCues[i].pitch))
                         {
                             oldOffsets.Add(songCues[i].tick + songCues[i].pitch, songCues[i].zOffset);
                         }
                         if (transitionNumberOfTargets > 0) songCues[i].zOffset = Mathf.Lerp(songCues[i].zOffset, zOffset + songCues[i].zOffset, currentCount / (float)transitionNumberOfTargets);
                         else songCues[i].zOffset = zOffset;

                     }

                     else
                     {
                         if (!oldOffsets.ContainsKey(songCues[i].tick + songCues[i].pitch)) continue;
                         if (transitionNumberOfTargets > 0) songCues[i].zOffset = Mathf.Lerp(songCues[i].zOffset, oldOffsets[songCues[i].tick + songCues[i].pitch], currentCount / (float)transitionNumberOfTargets);
                         else songCues[i].zOffset = zOffset;
                     }

                     currentCount++;
                 }
                 //songCues[i].target.transform.position = TargetSpawnerLayoutUtil.I.GetPosition(songCues[i].pitch, songCues[i].gridOffset, songCues[i].zOffset);
                 //songCues[i].target.transform.rotation = TargetSpawnerLayoutUtil.GetRotation(songCues[i].pitch, songCues[i].gridOffset);
             }

             direction = Direction.Down;
         }     */

        private enum Direction
        {
            Up,
            Down
        }
    }
}
