using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;

namespace AuthorableModifiers
{
    public class TextPopup : Modifier
    {
        private string text;
        private float size;
        private bool glow;
        private Vector3 modifierTextPosition = new Vector3(0f, 4f, 25f);
        public TextPopup(ModifierType _type, float _startTick, float _endTick, string _text, float _size, bool _glow)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            text = _text;
            size = _size;
            glow = _glow;
        }

        public override void Activate()
        {
            base.Activate();
            AuthorableModifiersMod.DebugTextPopup(text, size, modifierTextPosition, glow);
            
        }

        public override void Deactivate()
        {
            base.Deactivate();
            AuthorableModifiersMod.DestroyPopup();            
        }


    }
}
