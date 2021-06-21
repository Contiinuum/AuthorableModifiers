using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AuthorableModifiers
{
    public class ColorUpdate : Modifier
    {
        public Color LeftHandColor { get; set; }
        public Color RightHandColor { get; set; }
        /*public ColorUpdate(ModifierType _type, float _startTick, float[] _leftHandColor, float[] _rightHandColor)
        {
            Type = _type;
            StartTick = _startTick;
            LeftHandColor = new Color(_leftHandColor[0], _leftHandColor[1], _leftHandColor[2]);
            RightHandColor = new Color(_rightHandColor[0], _rightHandColor[1], _rightHandColor[2]);
            IsSingleUse = true;
        }*/

        public override void Activate()
        {
            base.Activate();
            if (AuthorableModifiersMod.activeColorChange != null) AuthorableModifiersMod.activeColorChange.UpdateColors(LeftHandColor, RightHandColor);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}
