﻿using System;
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
        private readonly string text;
        private readonly float size;
        private readonly bool glow;
        private readonly bool faceForward;
        private readonly bool isEvent;
        private Vector3 modifierTextPosition = new Vector3(0f, 4f, 25f);
        public TextPopup(ModifierType _type = ModifierType.TextPopup, float _startTick = 0, float _endTick = 0, string _text = "", float _size = 20f, float _xoff = 0, float _yoff = 0, float _zoff = 0, bool _glow = false, bool _faceForward = false)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            text = _text;
            if(_size == 0f)
            {
                if (StartTick == EndTick || EndTick == 0)
                {
                    IsSingleUse = true;
                    isEvent = true;
                    return;
                }
                else _size = 20f;
            }
            size = _size;
            glow = _glow;
            faceForward = _faceForward;
            modifierTextPosition.x += _xoff;
            modifierTextPosition.y += _yoff;
            modifierTextPosition.z += _zoff;
            isEvent = false;
        }
        public void SetOffset(Vector3 offset)
        {
            modifierTextPosition += offset;
        }

        public override void Activate()
        {
            base.Activate();
            if (isEvent)
            {
                ApiController.SetEvent(text);
                return;
            }
            AuthorableModifiersMod.DebugTextPopup(text, size, modifierTextPosition, glow, faceForward, StartTick);
            
        }

        public override void Deactivate()
        {
            base.Deactivate();
            AuthorableModifiersMod.DestroyPopup(StartTick);            
        }


    }
}
