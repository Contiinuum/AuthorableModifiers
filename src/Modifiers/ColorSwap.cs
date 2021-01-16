using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;

namespace AuthorableModifiers
{
    public class ColorSwap : Modifier
    {

        private Color leftHandColor;
        private Color rightHandColor;

        public ColorSwap(ModifierType _type, float _startTick, float _endTick)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
        }

        public override void Activate()
        {
           // ModifierManager.colorSwapActive = true;
            base.Activate();           
            SwapColors(true);
            Hooks.updateChainColor = true;
        }

        public override void Deactivate()
        {
            //ModifierManager.colorSwapActive = false;
            base.Deactivate();          
            SwapColors(false);         
        }

        public void SwapColors(bool enable)
        {
            //if (ModifierManager.randomColorsActive && !enable)
            if(enable)
            {
                leftHandColor = KataConfig.I.rightHandColor;
                rightHandColor = KataConfig.I.leftHandColor;               
            }
            else
            {
                leftHandColor = KataConfig.I.leftHandColor;
                rightHandColor = KataConfig.I.rightHandColor;
            }
         
            PlayerPreferences.I.GunColorLeft.Set(leftHandColor);
            PlayerPreferences.I.GunColorRight.Set(rightHandColor);
            KataConfig.I.leftHandColor = leftHandColor;
            KataConfig.I.rightHandColor = rightHandColor;
            for (int i = 0; i < PlayerPreferences.I.mColorPrefs.Count; i++)
            {
                if (PlayerPreferences.I.mColorPrefs[i].mName == "gun_color_left")
                {
                    PlayerPreferences.I.mColorPrefs[i].mVal = leftHandColor;
                }
                else if (PlayerPreferences.I.mColorPrefs[i].mName == "gun_color_right")
                {
                    PlayerPreferences.I.mColorPrefs[i].mVal = rightHandColor;
                }
            }
            
            Target[] targets = GameObject.FindObjectsOfType<Target>();
            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i].mCue.behavior != Target.TargetBehavior.Chain) continue;
                if (targets[i].mCue.handType == Target.TargetHandType.Left)
                {
                    targets[i].chainLine.startColor = leftHandColor;
                    targets[i].chainLine.endColor = leftHandColor;
                }
                else
                {
                    targets[i].chainLine.startColor = rightHandColor;
                    targets[i].chainLine.endColor = rightHandColor;
                }

            }

            TargetColorSetter.I.updateColors = true;
            TargetColorSetter.I.UpdateSlowColors(leftHandColor, rightHandColor);
            TargetColorSetter.I.UpdateFastColors(leftHandColor, rightHandColor);
            TargetColorSetter.I.UpdatePreviewBeamColors(leftHandColor, rightHandColor);
            if(MenuState.sState == MenuState.State.Launched) CueDartManager.I.SetUpColors();
        }
    }
}
