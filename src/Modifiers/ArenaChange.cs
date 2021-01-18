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

    public class ArenaChange : Modifier
    {
        public string oldArena;
        public List<string> options = new List<string>();
        public bool preload;
        public ArenaChange(ModifierType _type, float _startTick, string _option1, string _option2 , bool _preload)
        {
            type = _type;
            startTick = _startTick;
            endTick = 0f;
            preload = _preload;
            if (_option1 != "") options.Add(_option1);
            if (_option2 != "") options.Add(_option2);           
        }

        public override void Activate()
        {
            base.Activate();           
            oldArena = PlayerPreferences.I.Environment.Get();
            foreach (string option in options)
            {
                if (ChangeArena(option))
                {
                    
                    MelonCoroutines.Start(AuthorableModifiersMod.ISetDefaultArenaBrightness());
                    break;
                }
                   
            }
            AuthorableModifiersMod.SetOldArena(oldArena);
            RenderSettings.skybox.SetFloat("_Exposure", AuthorableModifiersMod.defaultArenaBrightness);
            RenderSettings.reflectionIntensity = AuthorableModifiersMod.defaultArenaReflection;
        }

        private bool ChangeArena(string option)
        {
            if (ArenaExists(option))
            {
                if (oldArena == option) return false;
                PlayerPreferences.I.Environment.Set(option);

                EnvironmentLoader.I.SwitchEnvironment();
                return true;
            }
            return false;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        private bool ArenaExists(string currentArena)
        {
            bool inDefaultArenas = ArenaLoaderMod.defaultEnvironments.Contains(currentArena);
            bool inCustomArenas = ArenaLoaderMod.arenaNames.Contains(currentArena);
            return inDefaultArenas || inCustomArenas;
        }
    }
}
