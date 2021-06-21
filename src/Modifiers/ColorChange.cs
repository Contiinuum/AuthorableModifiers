using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;

namespace AuthorableModifiers
{
    public class ColorChange : Modifier
    {
        public Color LeftHandColor { get; set; }
        public Color RightHandColor { get; set; }

        private Color oldLeftHandColor;
        private Color oldRightHandColor;

        /*public ColorChange(ModifierType _type, float _startTick, float _endTick, float[] _leftHandColor, float[] _rightHandColor)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            LeftHandColor = new Color(_leftHandColor[0], _leftHandColor[1], _leftHandColor[2]);
            RightHandColor = new Color(_rightHandColor[0], _rightHandColor[1], _rightHandColor[2]);
        }*/

        public override void Activate()
        {
            base.Activate();      
            ChangeColors(true);
            Hooks.updateChainColor = true;
            AuthorableModifiersMod.activeColorChange = this;
        }

        public override void Deactivate()
        {
            base.Deactivate();            
            ChangeColors(false);
            AuthorableModifiersMod.activeColorChange = null;
        }

        public void ChangeColors(bool enable)
        {
            Color lhColor;
            Color rhColor;
            if (enable)
            {
                oldLeftHandColor = KataConfig.I.leftHandColor;
                oldRightHandColor = KataConfig.I.rightHandColor;
                AuthorableModifiersMod.SetOldColors(oldLeftHandColor, oldRightHandColor);

                lhColor = LeftHandColor;
                rhColor = RightHandColor;
            }
            else
            {
                lhColor = oldLeftHandColor;
                rhColor = oldRightHandColor;
                             
            }

            UpdateTargets(lhColor, rhColor);
          
        }

        public void UpdateColors(Color lhColor, Color rhColor)
        {           
            Color lh = lhColor;
            Color rh = rhColor;           
            
           /* if(lhColor.Length == 0) lh = KataConfig.I.leftHandColor;
            else ColorUtility.TryParseHtmlString(lhColor, out lh);
            if (rhColor.Length == 0) rh = KataConfig.I.rightHandColor;
            else ColorUtility.TryParseHtmlString(rhColor, out rh);*/

            UpdateTargets(lh, rh);
        }
        private void UpdateTargets(Color lhColor, Color rhColor)
        {
            PlayerPreferences.I.GunColorLeft.Set(lhColor);
            PlayerPreferences.I.GunColorRight.Set(rhColor);
            KataConfig.I.leftHandColor = lhColor;
            KataConfig.I.rightHandColor = rhColor;
            for (int i = 0; i < PlayerPreferences.I.mColorPrefs.Count; i++)
            {
                if (PlayerPreferences.I.mColorPrefs[i].mName == "gun_color_left")
                {
                    PlayerPreferences.I.mColorPrefs[i].mVal = lhColor;
                }
                else if (PlayerPreferences.I.mColorPrefs[i].mName == "gun_color_right")
                {
                    PlayerPreferences.I.mColorPrefs[i].mVal = rhColor;
                }
            }

            TargetColorSetter.I.updateColors = true;
            TargetColorSetter.I.UpdateSlowColors(lhColor, rhColor);
            TargetColorSetter.I.UpdateFastColors(lhColor, rhColor);
            TargetColorSetter.I.UpdatePreviewBeamColors(lhColor, rhColor);
            if (MenuState.sState == MenuState.State.Launched) CueDartManager.I.SetUpColors();
        }
    }
}
