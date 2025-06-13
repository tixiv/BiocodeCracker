using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace Tixiv_BiocodeCracker
{
    [StaticConstructorOnStartup]
    public class BiocodeCrackerBuilding : Building_WorkTable
    {
        private CompPowerTrader cachedPowerComp;

        private CompThingContainer cachedContainerComp;

        private CompMoteEmitterCustom cachedMoteComp;

        private CompHeatPusher cachedHeatComp;

        private Sustainer sustainerWorking;

        private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        private int ticksRemaining = 0;
        private int ticksUntilGuaranteedFind = 0;

        private int lastGameTickSeen = 0;

        private bool working = false;

        private int MaxTicksToCrack => DebugSettings.godMode ? 5000 : 60000 * 7; // Very quick if god, otherwise it takes a week

        public bool Working
        {
            get
            {
                return working;
            }
        }

        private CompPowerTrader PowerTraderComp
        {
            get
            {
                if (cachedPowerComp == null)
                {
                    cachedPowerComp = this.TryGetComp<CompPowerTrader>();
                }

                return cachedPowerComp;
            }
        }
        private CompThingContainer ContainerComp
        {
            get
            {
                if (cachedContainerComp == null)
                {
                    cachedContainerComp = this.TryGetComp<CompThingContainer>();
                }

                return cachedContainerComp;
            }
        }

        private CompMoteEmitterCustom MoteEmitterComp
        {
            get
            {
                if (cachedMoteComp == null)
                {
                    cachedMoteComp = this.TryGetComp<CompMoteEmitterCustom>();
                }

                return cachedMoteComp;
            }
        }
        private CompHeatPusher HeatPusherComp
        {
            get
            {
                if (cachedHeatComp == null)
                {
                    cachedHeatComp = this.TryGetComp<CompHeatPusher>();
                }

                return cachedHeatComp;
            }
        }

        public bool PowerOn => PowerTraderComp.PowerOn;

        public bool Empty => ContainerComp.Empty;

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            Finish();
            base.DeSpawn(mode);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos()) // Include the base gizmos if needed
            {
                yield return gizmo;
            }

            if (!Empty)
                // Create a new gizmo (button) to drop the items
                yield return new Command_Action
                {
                    defaultLabel = "BCC_CancelCracking".Translate(),  // Text to show on the button
                    defaultDesc = "BCC_CancelCracking_desc".Translate(), // Tooltip text
                    icon = CancelIcon,
                    action = () =>
                    {
                        Finish();
                    }
                };
        }

        private void TickEffects()
        {
            if (sustainerWorking == null || sustainerWorking.Ended)
            {
                sustainerWorking = Tixiv_BiocodeCracker_DefOf.SubcoreEncoder_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
            }
            else
            {
                sustainerWorking.Maintain();
            }

            MoteEmitterComp.CustomMaintain();
        }


        private void TickIntervalInternal(int delta)
        {
            PowerTraderComp.PowerOutput = (Working ? (0f - base.PowerComp.Props.PowerConsumption) : (0f - base.PowerComp.Props.idlePowerDraw));
            HeatPusherComp.enabled = Working;

            if (Working && PowerOn)
            {
                ticksRemaining -= delta;
                ticksUntilGuaranteedFind -= delta;

                if (ticksRemaining <= 0)
                    Finish(true);
            }
        }

#if RIMWORLD_1_5

        public override void Tick()
        {
            base.Tick();

            if (this.IsHashIntervalTick(250))
            {
                int thisGameTick = Find.TickManager.TicksGame;
                
                // initialization after load
                if (lastGameTickSeen == 0)
                    lastGameTickSeen = thisGameTick;

                int deltaTicks = thisGameTick - lastGameTickSeen;
                lastGameTickSeen = thisGameTick;

                TickIntervalInternal(deltaTicks);
            }

            if (Working && PowerOn)
                TickEffects();
        }

#elif RIMWORLD_1_6

        protected override void Tick()
        {
            base.Tick();

            if (Working && PowerOn)
                TickEffects();
        }
        protected override void TickInterval(int delta)
        {
            base.TickInterval(delta);

            TickIntervalInternal(delta);
        }
#endif

        public void Start()
        {
            ticksRemaining = Rand.Range(MaxTicksToCrack / 8, MaxTicksToCrack);
            ticksUntilGuaranteedFind = MaxTicksToCrack;

            working = true;
        }

        private void Finish(bool cracked = false)
        {
            if (cracked)
            {
                // Remove biocoded

                var item = ContainerComp.ContainedThing as ThingWithComps;
                if (item != null)
                {
                    var compBiocodable = item.GetComp<CompBiocodable>();
                    if (compBiocodable != null)
                    {
                        compBiocodable.UnCode();
                        Messages.Message("BCC_BiocodingCracked".Translate(item.Label), MessageTypeDefOf.PositiveEvent);
                    }
                    else
                        Log.Warning("BiocodeCrackerBuilding: Tried to remove code from non biocodable item.");
                }
                else
                {
                    Log.Warning("BiocodeCrackerBuilding: No ThingWithComps in container to remove biocode from.");
                }
            }

            sustainerWorking = null;
            working = false;

            DropItemsOntoFloor();
        }

        private void DropItemsOntoFloor()
        {
            if (this.Map != null)                    
                ContainerComp.innerContainer.TryDropAll(this.Position, this.Map, ThingPlaceMode.Near);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", defaultValue: 0);
            Scribe_Values.Look(ref ticksUntilGuaranteedFind, "ticksUntilGuaranteedFind", defaultValue: 0);
            Scribe_Values.Look(ref working, "working", defaultValue: false);
        }

        public override string GetInspectString()
        {
            if (Working && PowerOn)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(base.GetInspectString());

                if (stringBuilder.Length > 0)
                {
                    stringBuilder.AppendLine();
                }

                if (!DebugSettings.godMode)
                {
                    stringBuilder.Append("BCC_WorkingGuaranteedCrackIn".Translate(GenDate.ToStringTicksToPeriodVague(ticksUntilGuaranteedFind)));
                }
                else
                {
                    stringBuilder.Append("BCC_WorkingGuaranteedCrackIn".Translate(GenDate.ToStringTicksToPeriodVague(ticksUntilGuaranteedFind)));
                    stringBuilder.AppendLine();
                    stringBuilder.Append("BCC_GodmodeWillCrackIn".Translate(GenDate.ToStringTicksToPeriod(ticksRemaining)));
                }
                
                return stringBuilder.ToString();
            }
            else
            {
                return base.GetInspectString();
            }
        }
    }
}
