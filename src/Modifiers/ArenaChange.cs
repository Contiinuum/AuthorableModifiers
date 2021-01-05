using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using System.Collections;

namespace AudicaModding
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
            AuthorableModifiers.SetOldArena(oldArena);
            foreach (string option in options)
            {
                if (ChangeArena(option))
                {
                    MelonCoroutines.Start(AuthorableModifiers.ISetDefaultArenaBrightness());
                    break;
                }
                   
            }              
        }

        private bool ChangeArena(string option)
        {
            if (ArenaExists(option))
            {
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
            bool inDefaultArenas = AudicaMod.defaultEnvironments.Contains(currentArena);
            bool inCustomArenas = AudicaMod.arenaNames.Contains(currentArena);
            return inDefaultArenas || inCustomArenas;
        }
    }
}
