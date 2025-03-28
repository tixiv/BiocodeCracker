using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.Code;
using static UnityEngine.Scripting.GarbageCollector;

namespace Tixiv_BiocodeCracker
{
    class CompMoteEmitterCustom : CompMoteEmitter
    {
        public override void CompTick()
        {
            // Need to override this function in CompMoteEmitter because that
            // one otherwise runs into a null reference exception becuase of some
            // crazy skyfaller check that is in there?!?

            // We don't need to do anything in here anyway since the mote emitter is
            // activated by calling CustomMaintain() on it from BiocodeCrackerBuilding::Tick()
        }


        public void CustomMaintain()
        {
            // create mote if necessary. This is the only part that was done in
            // CompMoteEmitter::CompTick() that we actually need to do

            if (mote == null || mote.Destroyed)
            {
                Emit();
            }

            Maintain();
        }
    }
}
