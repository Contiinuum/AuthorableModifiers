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
        public string AmountX { get; set; }
        public string AmountY { get; set; }
        public string AmountZ { get; set; }
        private Vector3 targetAmount = new Vector3();
        //private float? parsedX, parsedY, parsedZ;
        private readonly Transform world;
        private Vector3 currentAmount;
        //private Quaternion targetRotation;
        //private Quaternion currentRotation;
        public bool Preload { get; set; }
        public bool Reset { get; set; }
        /*protected ArenaManipulation(ModifierType _type, float _startTick, float _endTick, string _amountX, string _amountY, string _amountZ, bool _reset, bool _preload)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            world = GameObject.Find("World").transform;
            AmountX = _amountX;
            AmountY = _amountY;
            AmountZ = _amountZ;
            Reset = _reset;
            Preload = _preload;           
        }*/
        public ArenaManipulation()
        {
            world = GameObject.Find("World").transform;
        }

        public override void Activate()
        {
            if (world is null) return;
            GetCurrentAmount();
            GetTargetAmount();
            //MelonLogger.Msg("Target " + Type.ToString() + ": " + targetAmount.x + "/" + targetAmount.y + "/" + targetAmount.z);
            if (Preload)
            {
                SetAmount(targetAmount);
            }
            else
            {
                base.Activate();
                if (EndTick == StartTick || EndTick == 0)
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
            switch (Type)
            {
                case ModifierType.ArenaPosition:
                    currentAmount = world.position;
                    break;
                case ModifierType.ArenaScale:
                    //currentAmount = world.localScale;
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
            if (Reset)
            {
                ResetArena();
            }
            else
            {
                if(AmountX == "")
                {
                    targetAmount.x = currentAmount.x;
                }
                else if (float.TryParse(AmountX, out float _x))
                {
                    //targetAmount.x = _x + (Type == ModifierType.ArenaScale ? 0f : currentAmount.x);
                    targetAmount.x = _x;
                    //parsedX = _x;
                }
                else 
                { 
                    targetAmount.x = currentAmount.x;
                }
                if (AmountY == "")
                {
                    targetAmount.y = currentAmount.y;
                }
                else if (float.TryParse(AmountY, out float _y))
                {
                    //targetAmount.y = _y + (Type == ModifierType.ArenaScale ? 0f : currentAmount.y);
                    targetAmount.y = _y;
                    //parsedY = _y;
                }
                else
                {
                    targetAmount.y = currentAmount.y;
                }
                if (AmountZ == "")
                {
                    targetAmount.z = currentAmount.z;
                }
                else if (float.TryParse(AmountZ, out float _z))
                {
                    //targetAmount.z = _z + (Type == ModifierType.ArenaScale ? 0f : currentAmount.z);
                    targetAmount.z = _z;
                    //parsedZ = _z;
                }
                else
                {
                    targetAmount.z = currentAmount.z;
                }

                if(Type == ModifierType.ArenaSpin)
                {
                    float tickSpan = EndTick - StartTick;
                    if (Reset) tickSpan = 1;
                    amountPerTick = new Vector3(Mathf.DeltaAngle(currentAmount.x, targetAmount.x), Mathf.DeltaAngle(currentAmount.y, targetAmount.y), Mathf.DeltaAngle(currentAmount.z, targetAmount.z));
                    amountPerTick /= tickSpan;
                    //amountPerTick = (targetAmount - currentAmount) / tickSpan;
                    //MelonLogger.Msg($"{amountPerTick.x}/{amountPerTick.y}/{amountPerTick.z} over {tickSpan} ticks (Target: {targetAmount.x}/{targetAmount.y}/{targetAmount.z} || Calculated: {amountPerTick.x * tickSpan}/{amountPerTick.y * tickSpan}/{amountPerTick.z * tickSpan})");
                    lastTick = StartTick;
                }
            }
        }
        private Vector3 amountPerTick = new Vector3();
        private float lastTick;
        protected Vector3 GetAmount(float percentage)
        {
           
            Vector3 amnt;
            /*if(Type == ModifierType.ArenaSpin)
            {
                amnt = Quaternion.Euler(CalculateRotation(currentAmount, targetAmount, percentage)).eulerAngles;
            }
            else
            {
                amnt.x = CalculateAmount(currentAmount.x, targetAmount.x, percentage);
                amnt.y = CalculateAmount(currentAmount.y, targetAmount.y, percentage);
                amnt.z = CalculateAmount(currentAmount.z, targetAmount.z, percentage);
            }*/
            amnt.x = CalculateAmount(currentAmount.x, targetAmount.x, percentage);
            amnt.y = CalculateAmount(currentAmount.y, targetAmount.y, percentage);
            amnt.z = CalculateAmount(currentAmount.z, targetAmount.z, percentage);
            return amnt;
        }

        protected Vector3 CalculateRotation(Vector3 current, Vector3 target, float percentage)
        {
            return Vector3.Lerp(current, target, percentage);
        }

        /*protected Vector3 GetRotation(float percentage)
        {
            float seconds = AudioDriver.TickSpanToMs(SongDataHolder.I.songData, StartTick, EndTick);
            seconds *= 1000;
            return targetAmount / seconds;

            //return Quaternion.Lerp(currentRotation, targetRotation, percentage);            
        }*/

        private IEnumerator DoManipulation()
        {
            while (Active)
            {
                float percentage = ((AudioDriver.I.mCachedTick - StartTick) * 100f) / (EndTick - StartTick);
                percentage /= 100f;
                SetAmount(GetAmount(percentage));
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }
            //if (Type == ModifierType.ArenaSpin) SetAmount(targetRotation);
            SetAmount(targetAmount);
        }




        private float CalculateAmount(float current, float target, float percentage)
        {
            //if(Type == ModifierType.ArenaSpin) target = target * target * (3f - 2f * target); //test this

            return Mathf.Lerp(current, target, percentage);
        }

        /*private void SetAmount(Quaternion rot)
        {
            world.transform.rotation = rot;
        }*/

        private void SetAmount(Vector3 _amount)
        {
            switch (Type)
            {       
                case ModifierType.ArenaPosition:
                    world.transform.position = _amount;
                    break;
                case ModifierType.ArenaScale:
                    world.transform.localScale = _amount;
                    break;
                case ModifierType.ArenaSpin:
                    /*float currentTick = AudioDriver.I.mCachedTick;
                    float numTicksPassed = currentTick - lastTick;
                    lastTick = currentTick;*/

                    /*world.Rotate(Vector3.right, amountPerTick.x * numTicksPassed);
                    world.Rotate(Vector3.up, amountPerTick.y * numTicksPassed);
                    world.Rotate(Vector3.forward, amountPerTick.z * numTicksPassed);*/
                    //world.Rotate(amountPerTick * numTicksPassed);

                    /*world.rotation *= Quaternion.AngleAxis(amountPerTick.x * numTicksPassed, Vector3.right);
                    world.rotation *= Quaternion.AngleAxis(amountPerTick.y * numTicksPassed, Vector3.up);
                    world.rotation *= Quaternion.AngleAxis(amountPerTick.z * numTicksPassed, Vector3.forward);*/
                    /*
                    world.transform.rotation *= Quaternion.AngleAxis(_amount.x, Vector3.right);
                    world.transform.rotation *= Quaternion.AngleAxis(_amount.y, Vector3.up);
                    world.transform.rotation *= Quaternion.AngleAxis(_amount.z, Vector3.forward);
                    */

                    //world.eulerAngles = _amount;
                    world.rotation = Quaternion.Euler(_amount);
                    //world.localRotation = Quaternion.Euler(_amount);
                    break;
                default:
                    break;
            }
        }

        private void SetAmount(Quaternion rotation)
        {
            world.rotation = rotation;
        }

        private void ResetArena()
        {
            switch (Type)
            {
                case ModifierType.ArenaPosition:
                    targetAmount = Vector3.zero;
                    break;
                case ModifierType.ArenaScale:
                    targetAmount = new Vector3(1f, 1f, 1f);
                    break;
                case ModifierType.ArenaSpin:
                    //targetRotation = Quaternion.identity;
                    targetAmount = Vector3.zero;
                    break;
                default:
                    break;
            }
        }
    }
}
