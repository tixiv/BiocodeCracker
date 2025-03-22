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
        private Thing CrackerCell { get { return this.job.GetTarget(CrackerCellInd).Thing; } }


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
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
            Toil toil = Toils_General.Wait(300, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B);
            toil.handlingFacing = true;
            yield return toil;

            yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A, delegate
            {
                job.GetTarget(TargetIndex.A).Thing.def.soundDrop.PlayOneShot(new TargetInfo(job.GetTarget(TargetIndex.B).Cell, pawn.Map));
                if (Cracker != null)
                    Cracker.Start();
                else
                    Log.Warning("JobDriver_InsertInBiocodeCracker: Executed on target that is no BioCodeCrackerBuilding.");
            });
        }
    }
}
