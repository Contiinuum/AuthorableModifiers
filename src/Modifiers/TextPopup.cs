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
        private bool faceForward;
        private Vector3 modifierTextPosition = new Vector3(0f, 4f, 25f);
        public TextPopup(ModifierType _type = ModifierType.TextPopup, float _startTick = 0, float _endTick = 0, string _text = "", float _size = 12, float _xoff = 0, float _yoff = 0, float _zoff = 0, bool _glow = false, bool _faceForward = false)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            text = _text;
            size = _size;
            glow = _glow;
            faceForward = _faceForward;

            modifierTextPosition.x += _xoff;
            modifierTextPosition.y += _yoff;
            modifierTextPosition.z += _zoff;
        }

        public override void Activate()
        {
            base.Activate();
            AuthorableModifiersMod.DebugTextPopup(text, size, modifierTextPosition, glow, faceForward, startTick);
            
        }

        public override void Deactivate()
        {
            base.Deactivate();
            AuthorableModifiersMod.DestroyPopup(startTick);            
        }


    }
}
