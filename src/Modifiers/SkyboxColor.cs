using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using System.Collections;
using ArenaLoader;

namespace AuthorableModifiers
{
    public class SkyboxColor : Modifier
    {
        private Color targetColor;
        //private Color originalColor = new Color(0f, 0f, 0f);
        private Color oldColor;
        private bool isDefaultEnvironment = false;
        private bool reset;
        private Color defaultColor;
        public SkyboxColor(ModifierType _type, float _startTick, float _endTick, float r, float g, float b, bool _reset)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            reset = _reset;
            targetColor = new Color(r, g, b, defaultColor.a);               
            
            //if (!PlayerPreferences.I.Environment.Get().ToLower().Contains("environment")) targetColor *= .7f;
            //if (!PlayerPreferences.I.Environment.Get().ToLower().Contains("environment")) isDefaultEnvironment = true;
        }

        public override void Activate()
        {
            //if (reset && IsSingleUse && !isDefaultEnvironment) targetColor = new Color(0f, 0f, 0f, 0f);
            defaultColor = AuthorableModifiersMod.defaultSkyboxColor;
            if (reset) targetColor = defaultColor;
            SkyboxControl skyboxControl = GameObject.FindObjectOfType<SkyboxControl>();
            if(skyboxControl != null) skyboxControl.enabled = false;
            base.Activate();
            oldColor = RenderSettings.skybox.GetColor("_Tint");
            if(IsSingleUse)
            {
                UpdateColor(targetColor);
            }
            else
            {
                MelonCoroutines.Start(SmoothColorChange());
            }
        }

        private IEnumerator SmoothColorChange()
        {
            float startTick = AudioDriver.I.mCachedTick;
            float percentage = 0;
            while (Active || percentage < 100f)
            {
                percentage = ((AudioDriver.I.mCachedTick - startTick) * 100f) / (EndTick - startTick);
                Color currentColor = Color.Lerp(oldColor, targetColor, percentage / 100f);
                UpdateColor(currentColor);
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }
            yield return null;
        }

        private void UpdateColor(Color col)
        {
            RenderSettings.skybox.SetColor("_Tint", col);
            ApiController.SetColor(col);
        }

        public override void Deactivate()
        {
            /*if (!transition)
            {
                UpdateColor(new Color(1f, 1f, 1f));
            }*/
            base.Deactivate();
        }

        private void ResetColor()
        {
            if(isDefaultEnvironment && reset) UpdateColor(new Color(0f, 0f, 0f, 0f));
        }
       
    }
}
