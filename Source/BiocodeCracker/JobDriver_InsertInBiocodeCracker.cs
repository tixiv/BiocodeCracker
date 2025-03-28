using System.Collections.Generic;
using Verse.AI;
using Verse;
using RimWorld;
using Verse.Sound;

namespace Tixiv_BiocodeCracker
{
    public class JobDriver_InsertInBiocodeCracker : JobDriver
    {
        private const TargetIndex ItemIndex = TargetIndex.A;

        private const TargetIndex CrackerInd = TargetIndex.B;

        private const TargetIndex CrackerCellInd = TargetIndex.C;

        private Thing Item { get { return this.job.GetTarget(ItemIndex).Thing; } }
        private BiocodeCrackerBuilding Cracker { get { return this.job.GetTarget(CrackerInd).Thing as BiocodeCrackerBuilding; } }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Make sure the colonist can interact with the item and the cracker.
            if (pawn.Reserve(Cracker, job, 1, -1, null, errorOnFailed))
            {
                return pawn.Reserve(Item, job, 1, -1, null, errorOnFailed);
            }

            return false;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            // Mostly copied from JobDriver_InstallRelic

            yield return Toils_Goto.GotoThing(ItemIndex, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(ItemIndex);
            yield return Toils_Haul.StartCarryThing(ItemIndex, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
            yield return Toils_Haul.CarryHauledThingToCell(CrackerCellInd);
            Toil toil = Toils_General.Wait(150, CrackerInd).WithProgressBarToilDelay(CrackerInd).FailOnDespawnedOrNull(CrackerInd);
            toil.handlingFacing = true;
            yield return toil;

            yield return Toils_Haul.DepositHauledThingInContainer(CrackerInd, ItemIndex, delegate
            {
                Item.def.soundDrop.PlayOneShot(new TargetInfo(job.GetTarget(CrackerInd).Cell, pawn.Map));
                if (Cracker != null)
                    Cracker.Start();
                else
                    Log.Warning("JobDriver_InsertInBiocodeCracker: Executed on target that is no BioCodeCrackerBuilding.");
            });
        }
    }
}
