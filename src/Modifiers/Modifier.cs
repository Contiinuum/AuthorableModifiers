using MelonLoader;
using System.Collections;
using UnityEngine;

namespace AuthorableModifiers
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
            active = true;      
        }

        public virtual void Deactivate()
        {
            active = false;
        }      
    }
}
