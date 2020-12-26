using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using System.Collections;
using ScoreOverlay;

namespace AudicaModding
{

    public class OverlaySetter : Modifier
    {

        public string mapper;
        public string songInfo;

        public OverlaySetter(ModifierType _type, float _startTick, float _endTick, string _value1, string _value2)
        {
            type = _type;
            startTick = _startTick;
            endTick = _endTick;
            songInfo = _value1;
            mapper = _value2;
        }

        public override void Activate()
        {           
            base.Activate();
            if (songInfo.Length > 0) ScoreOverlayMod.ui.songInfo.text = songInfo;
            if (mapper.Length > 0) ScoreOverlayMod.ui.mapperInfo.text = mapper;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}
