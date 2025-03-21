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
        private Thing Cracker { get { return this.job.GetTarget(CrackerInd).Thing; } }
        private Thing CrackerCell { get { return this.job.GetTarget(CrackerCellInd).Thing; } }


        bool ReliquaryFull()
        {
            // return pawn.jobs.curJob.GetTarget(TargetIndex.B).Thing.TryGetComp<CompRelicContainer>()?.Full ?? true;
            return false;
        }


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
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOn((Toil to) => ReliquaryFull());
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOn((Toil to) => ReliquaryFull());
            yield return Toils_Haul.CarryHauledThingToCell(TargetIndex.C).FailOn((Toil to) => ReliquaryFull());
            Toil toil = Toils_General.Wait(300, TargetIndex.B).WithProgressBarToilDelay(TargetIndex.B).FailOnDespawnedOrNull(TargetIndex.B)
                .FailOn((Toil to) => ReliquaryFull());
            toil.handlingFacing = true;
            yield return toil;

            yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.A, delegate
            {
                job.GetTarget(TargetIndex.A).Thing.def.soundDrop.PlayOneShot(new TargetInfo(job.GetTarget(TargetIndex.B).Cell, pawn.Map));
            });
        }
    }
}
