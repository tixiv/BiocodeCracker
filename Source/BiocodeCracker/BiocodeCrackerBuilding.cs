using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace Tixiv_BiocodeCracker
{
    [StaticConstructorOnStartup]
    public class BiocodeCrackerBuilding : Building
    {
        [Unsaved(false)]
        private CompPowerTrader cachedPowerComp;

        [Unsaved(false)]
        private CompCrackerContainer cachedContainerComp;

        [Unsaved(false)]
        private CompMoteEmitterCustom cachedMoteComp;

        [Unsaved(false)]
        private CompHeatPusher cachedHeatComp;

        [Unsaved(false)]
        private Sustainer sustainerWorking;

        [Unsaved(false)]
        private Effecter progressBar;

        private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

        private int ticksRemaining;

        private const int TicksToCrack = 86400;

        public bool Working => ticksRemaining > 0;

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
            if (progressBar != null)
            {
                progressBar.Cleanup();
                progressBar = null;
            }

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
                else
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Rotate Weapon",
                        action = delegate
                        {
                            var item = ContainerComp.innerContainer[0] as Thing;
                            if (item != null)
                            {
                                Rot4 rot = item.Rotation;
                                rot.Rotate(RotationDirection.Clockwise);
                                item.Rotation = rot;
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

                // SubcoreEncoder_Working
            }
            else
            {
                sustainerWorking.Maintain();
            }

            if (progressBar == null)
            {
                progressBar = EffecterDefOf.ProgressBarAlwaysVisible.Spawn();
            }

            progressBar.EffectTick(new TargetInfo(base.Position + IntVec3.North.RotatedBy(base.Rotation), base.Map), TargetInfo.Invalid);
            MoteProgressBar mote = ((SubEffecter_ProgressBar)progressBar.children[0]).mote;
            if (mote != null)
            {
                mote.progress = 1f - Mathf.Clamp01((float)ticksRemaining / (float)TicksToCrack);
                mote.offsetZ = ((base.Rotation == Rot4.North) ? 0.5f : (-0.5f));
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
            }

            if (ticksRemaining > 0 && PowerOn)
            {
                TickEffects();
                
                ticksRemaining--;

                // Find.TickManager.TicksGame;

                if (ticksRemaining <= 0)
                {
                    Finish();
                }
            }


            var moteComp = this.GetComp<CompMoteEmitterCustom>();
            if (moteComp != null)
            {
                
            }

        }

        public void Start()
        {
            ticksRemaining = TicksToCrack;

            var item = ContainerComp.innerContainer[0] as ThingWithComps;
            if (item != null)
            {
                item.Rotation = this.Rotation;
            }
        }


        private void Finish()
        {
            if (ticksRemaining == 0 && !ContainerComp.Empty)
            {
                // Remove biocoded

                var item = ContainerComp.innerContainer[0] as ThingWithComps;
                if (item != null)
                {
                    var compBiocodable = item.GetComp<CompBiocodable>();
                    if (compBiocodable != null)
                        compBiocodable.UnCode();
                    else
                        Log.Warning("BiocodeCrackerBuilding: Tried to remove code from non biocodable item.");
                }
                else
                {
                    Log.Warning("BiocodeCrackerBuilding: No ThingWithComps in container to remove biocode from.");
                }

                Messages.Message("The biocoding on " + item.Label + " has been cracked.", MessageTypeDefOf.PositiveEvent);
            }

            sustainerWorking = null;
            if (progressBar != null)
            {
                progressBar.Cleanup();
                progressBar = null;
            }

            ticksRemaining = 0;

            ContainerComp.DropItemsOntoFloor();
        }
    }
}
