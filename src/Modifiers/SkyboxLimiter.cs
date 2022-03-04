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

    public class SkyboxLimiter : Modifier
    {
        public override void Activate()
        {
            base.Activate();
            AuthorableModifiersMod.skyboxLimit = Amount;

        }

        public override void Deactivate()
        {
            base.Deactivate();
        }
    }
}
