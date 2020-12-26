using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AudicaModding
{
    public class ColorUpdate : Modifier
    {
        private Color leftHandColor;
        private Color rightHandColor;
        public ColorUpdate(ModifierType _type, float _startTick, float[] _leftHandColor, float[] _rightHandColor)
        {
            type = _type;
            startTick = _startTick;
            leftHandColor = new Color(_leftHandColor[0], _leftHandColor[1], _leftHandColor[2]);
            rightHandColor = new Color(_rightHandColor[0], _rightHandColor[1], _rightHandColor[2]);
            isSingleUseModule = true;
        }

        public override void Activate()
        {
            base.Activate();
            if (AuthorableModifiers.activeColorChange != null) AuthorableModifiers.activeColorChange.UpdateColors(leftHandColor, rightHandColor);
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}
