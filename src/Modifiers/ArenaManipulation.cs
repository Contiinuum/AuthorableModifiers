using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using System.Collections;
using ArenaLoader;

namespace AuthorableModifiers
{
    public class ArenaManipulation : Modifier
    {
        private string amountX, amountY, amountZ;
        private Vector3 targetAmount = new Vector3();
        private float? parsedX, parsedY, parsedZ;
        private Transform world;
        private Vector3 currentAmount;
        private Quaternion targetRotation;
        private Quaternion currentRotation;
        private bool preload;
        private bool reset;
        protected ArenaManipulation(ModifierType _type, float _startTick, float _endTick, string _amountX, string _amountY, string _amountZ, bool _reset, bool _preload)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            world = GameObject.Find("World").transform;
            amountX = _amountX;
            amountY = _amountY;
            amountZ = _amountZ;
            reset = _reset;
            preload = _preload;           
        }

        public override void Activate()
        {
            if (world is null) return;
            GetCurrentAmount();
            GetTargetAmount();

            if (preload)
            {
                SetAmount(targetAmount);
            }
            else
            {
                base.Activate();
                if (endTick == startTick || endTick == 0)
                {
                    SetAmount(targetAmount);
                }
                else
                {
                    MelonCoroutines.Start(DoManipulation());
                }
            }          
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        private void GetCurrentAmount()
        {
            switch (type)
            {
                case ModifierType.ArenaPosition:
                    currentAmount = world.position;
                    break;
                case ModifierType.ArenaScale:
                    currentAmount = world.localScale;
                    break;
                case ModifierType.ArenaSpin:
                    //currentRotation = world.rotation;
                    currentAmount = world.eulerAngles;
                    break;
                default:
                    break;
            }
        }

        private void GetTargetAmount()
        {
            if (reset)
            {
                Reset();
            }
            else
            {
                float _x, _y, _z;

                if (float.TryParse(amountX, out _x))
                {
                    targetAmount.x = _x + currentAmount.x;
                    parsedX = _x;
                }
                else 
                { 
                    targetAmount.x = currentAmount.x;
                }
                if (float.TryParse(amountY, out _y))
                {
                    targetAmount.y = _y + currentAmount.y;
                    parsedY = _y;
                }
                else
                {
                    targetAmount.y = currentAmount.y;
                }
                if (float.TryParse(amountZ, out _z))
                {
                    targetAmount.z = _z + currentAmount.z;
                    parsedZ = _z;
                }
                else
                {
                    targetAmount.z = currentAmount.z;
                }

                //if (type == ModifierType.ArenaSpin) targetRotation = Quaternion.Euler(targetAmount);
            }
        }

        protected Vector3 GetAmount(float percentage)
        {
           
            Vector3 amnt;
            amnt.x = CalculateAmount(currentAmount.x, targetAmount.x, percentage);
            amnt.y = CalculateAmount(currentAmount.y, targetAmount.y, percentage);
            amnt.z = CalculateAmount(currentAmount.z, targetAmount.z, percentage);
            return amnt;
        }

        protected Vector3 GetRotation(float percentage)
        {
            float seconds = AudioDriver.TickSpanToMs(SongDataHolder.I.songData, startTick, endTick);
            seconds *= 1000;
            return targetAmount / seconds;

            //return Quaternion.Lerp(currentRotation, targetRotation, percentage);            
        }

        private IEnumerator DoManipulation()
        {
            while (active)
            {
                float percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (endTick - startTick);
                percentage /= 100f;
                if (type == ModifierType.ArenaSpin) SetAmount(GetAmount(percentage));
                else SetAmount(GetAmount(percentage));

                yield return new WaitForSecondsRealtime(.001f);
            }
            SetAmount(targetAmount);
        }

        private float CalculateAmount(float current, float target, float percentage)
        {
            target = target * target * (3f - 2f * target); //test this
            return Mathf.Lerp(current, target, percentage);
        }

        private void SetAmount(Quaternion rot)
        {
            world.transform.rotation = rot;
        }

        private void SetAmount(Vector3 _amount)
        {
            switch (type)
            {       
                case ModifierType.ArenaPosition:
                    world.transform.position = _amount;
                    break;
                case ModifierType.ArenaScale:
                    world.transform.localScale = _amount;
                    break;
                case ModifierType.ArenaSpin:
                    /*world.transform.rotation *= Quaternion.AngleAxis(_amount.x, Vector3.right);
                    world.transform.rotation *= Quaternion.AngleAxis(_amount.y, Vector3.up);
                    world.transform.rotation *= Quaternion.AngleAxis(_amount.z, Vector3.forward);*/
                    world.eulerAngles = _amount;                    
                    break;
                default:
                    break;
            }
        }

        private void Reset()
        {
            switch (type)
            {
                case ModifierType.ArenaPosition:
                    targetAmount = Vector3.zero;
                    break;
                case ModifierType.ArenaScale:
                    targetAmount = new Vector3(1f, 1f, 1f);
                    break;
                case ModifierType.ArenaSpin:
                    targetRotation = Quaternion.identity;
                    targetAmount = Vector3.zero;
                    break;
                default:
                    break;
            }
        }
    }
}
