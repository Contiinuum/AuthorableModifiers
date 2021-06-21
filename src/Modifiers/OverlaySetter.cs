using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using System.Collections;
using ScoreOverlay;

namespace AuthorableModifiers
{

    public class OverlaySetter : Modifier
    {

        public string Mapper { get; set; }
        public string SongInfo { get; set; }

        /*public OverlaySetter(ModifierType _type, float _startTick, float _endTick, string _value1, string _value2)
        {
            Type = _type;
            StartTick = _startTick;
            EndTick = _endTick;
            SongInfo = _value1;
            Mapper = _value2;
        }*/

        public override void Activate()
        {           
            base.Activate();
            if (SongInfo.Length > 0) ScoreOverlayMod.ui.songInfo.text = SongInfo;
            if (Mapper.Length > 0) ScoreOverlayMod.ui.mapperInfo.text = Mapper;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}
