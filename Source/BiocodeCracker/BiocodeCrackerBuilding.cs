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

        private CompCrackerContainer cachedContainerComp;

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
        private CompCrackerContainer ContainerComp
        {
            get
            {
                if (cachedContainerComp == null)
                {
                    cachedContainerComp = this.TryGetComp<CompCrackerContainer>();
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
            sustainerWorking = null;

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
                    defaultLabel = "Cancel cracking",  // Text to show on the button
                    defaultDesc = "Cancels cracking the biocoding on this item", // Tooltip text
                    icon = CancelIcon,
                    action = () =>
                    {
                        Finish();
                    }
                };

            if (DebugSettings.ShowDevGizmos)
            {
                if (Empty)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Fill with Weapon",
                        action = delegate
                        {
                            Thing thing = UtilCreateBiocodedWeapon.CreateRandomBiocodedWeapon();
                            if (thing != null)
                            {
                                ContainerComp.innerContainer.TryAdd(thing);
                                Start();
                            }
                        }
                    };
                }
            }
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

            MoteEmitterComp.Maintain();
        }

        public override void Tick()
        {
            base.Tick();

            if (this.IsHashIntervalTick(250))
            {
                PowerTraderComp.PowerOutput = (Working ? (0f - base.PowerComp.Props.PowerConsumption) : (0f - base.PowerComp.Props.idlePowerDraw));
                HeatPusherComp.enabled = Working;

                int thisGameTick = Find.TickManager.TicksGame;
                
                // initialization after load
                if (lastGameTickSeen == 0)
                    lastGameTickSeen = thisGameTick;

                int deltaTicks = thisGameTick - lastGameTickSeen;

                if (Working && PowerOn)
                {
                    ticksRemaining -= deltaTicks;
                    ticksUntilGuaranteedFind -= deltaTicks;

                    if (ticksRemaining <= 0)
                        Finish(true);
                }

                lastGameTickSeen = thisGameTick;
            }

            if (Working && PowerOn)
                TickEffects();
        }

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
                        Messages.Message("The biocoding on " + item.Label + " has been cracked.", MessageTypeDefOf.PositiveEvent);
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

            ContainerComp.DropItemsOntoFloor();
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
                    stringBuilder.Append("Working. Guaranteed crack in " + GenDate.ToStringTicksToPeriodVague(ticksUntilGuaranteedFind));
                }
                else
                {
                    stringBuilder.Append("Working. Guaranteed crack in " + GenDate.ToStringTicksToPeriod(ticksUntilGuaranteedFind));
                    stringBuilder.AppendLine();
                    stringBuilder.Append("Godmode: will crack in " + GenDate.ToStringTicksToPeriod(ticksRemaining));
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
