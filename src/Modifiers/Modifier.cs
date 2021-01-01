using MelonLoader;
using System.Collections;
using UnityEngine;

namespace AudicaModding
{
    public class Modifier
    {
        public ModifierType type;
        public float startTick;
        public float endTick;
        public float amount;     
        public bool active = false;
        public bool isSingleUseModule = false;

        public virtual void Activate()
        {
            //MelonLogger.Log(type.ToString() + " activated");
            active = true;      
        }

        public virtual void Deactivate()
        {
            //MelonLogger.Log(type.ToString() + " deactivated");
            active = false;
        }      
    }
}
