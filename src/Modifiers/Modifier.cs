using MelonLoader;
using System.Collections;
using UnityEngine;

namespace AuthorableModifiers
{
    public class Modifier
    {
        public ModifierType Type { get; set; }
        public float StartTick { get; set; }
        public float EndTick { get; set; }
        public float Amount { get; set; }
        public bool Active { get; set; } = false;
        public bool IsSingleUse { get; set; } = false;

        public virtual void Activate()
        {
            Active = true;      
        }

        public virtual void Deactivate()
        {
            Active = false;
        }      
    }
}
