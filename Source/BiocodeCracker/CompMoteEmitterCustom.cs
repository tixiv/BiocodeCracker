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
        private CompProperties_MoteEmitterCustom Props => (CompProperties_MoteEmitterCustom)props;

        public override void CompTick()
        {
            if (!parent.Spawned)
            {
                return;
            }

            CompPowerTrader comp = parent.GetComp<CompPowerTrader>();
            if (comp != null && !comp.PowerOn)
            {
                return;
            }

            CompSendSignalOnCountdown comp2 = parent.GetComp<CompSendSignalOnCountdown>();
            if (comp2 != null && comp2.ticksLeft <= 0)
            {
                return;
            }

            CompInitiatable comp3 = parent.GetComp<CompInitiatable>();
            if (comp3 != null && !comp3.Initiated)
            {
                return;
            }

            if (Props.emissionInterval != -1 && !Props.maintain)
            {
                if (ticksSinceLastEmitted >= Props.emissionInterval)
                {
                    Emit();
                    ticksSinceLastEmitted = 0;
                }
                else
                {
                    ticksSinceLastEmitted++;
                }
            }
            else if (mote == null || mote.Destroyed)
            {
                Emit();
            }

            if (mote != null && !mote.Destroyed)
            {
                if (Props.maintain)
                {
                    Log.Message("Mote: Maintain");
                    Maintain();
                }
            }
        }


        public override void Emit()
        {
            if (!parent.Spawned)
            {
                Log.Error("Thing tried spawning mote without being spawned!");
                return;
            }

            Vector3 vector = Props.offset + Props.RotationOffset(parent.Rotation);
            if (Props.offsetMin != Vector3.zero || Props.offsetMax != Vector3.zero)
            {
                vector = Props.EmissionOffset;
            }

            ThingDef thingDef = Props.RotationMote(parent.Rotation) ?? Props.mote;
            if (typeof(MoteAttached).IsAssignableFrom(thingDef.thingClass))
            {
                mote = MoteMaker.MakeAttachedOverlay(parent, thingDef, vector);
            }
            else
            {
                Vector3 vector2 = parent.DrawPos + vector;
                if (vector2.InBounds(parent.Map))
                {
                    mote = MoteMaker.MakeStaticMote(vector2, parent.Map, thingDef);
                }
            }

            if (mote != null && Props.useParentRotation)
            {
                mote.exactRotation = parent.Rotation.AsAngle;
            }

            if (!Props.soundOnEmission.NullOrUndefined())
            {
                Props.soundOnEmission.PlayOneShot(SoundInfo.InMap(parent));
            }
        }
    }
}
