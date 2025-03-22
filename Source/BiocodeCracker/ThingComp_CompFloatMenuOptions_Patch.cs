using System.Collections.Generic;
using System.Linq;
using Verse;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace Tixiv_BiocodeCracker
{
    public class MyMod : Mod
    {
        public MyMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.Tixiv_BiocodeCracker");
            harmony.PatchAll();  // This applies all Harmony patches in the mod
        }
    }

    [HarmonyPatch(typeof(ThingComp), "CompFloatMenuOptions")]
    public static class ThingComp_CompFloatMenuOptions_Patch
    {
        public static BiocodeCrackerBuilding getCrackerBuilding(Pawn selPawn)
        {
            // Get all buildings on the map
            var allCrackers = Find.CurrentMap.listerThings.AllThings.OfType<BiocodeCrackerBuilding>();

            // Try to return an empty, powered cracker that is unreserved
            foreach (var cracker in allCrackers)
                if (cracker.Empty && cracker.PowerOn && !cracker.Map.reservationManager.IsReserved(cracker))
                    return cracker;
            
            // Fall back to return any cracker that is empty and not reserved, we will give an appropriate menu entry for what is wrong later
            foreach (var cracker in allCrackers)
                if (cracker.Empty && !cracker.Map.reservationManager.IsReserved(cracker))
                    return cracker;

            // Fall back to return any empty cracker
            foreach (var cracker in allCrackers)
                if (cracker.Empty)
                    return cracker;

            // Fall back to return any cracker
            foreach (var cracker in allCrackers)
                return cracker;

            return null;
        }

        public static void Postfix(ThingComp __instance, Pawn selPawn, ref IEnumerable<FloatMenuOption> __result)
        {
            // We would like to implement this method for 'CompBiocodable', but it is not possible to
            // override a non existent method with a patch.
            // That is why we patch the method of the base class and check here whether we are
            // a CompBiocodable to pratically implement it for that class

            CompBiocodable compBiocodable = __instance as CompBiocodable;
            if (compBiocodable != null)
            {
                // Get the ThingWithComps (the object that this Comp is attached to)
                ThingWithComps thingWithComps = __instance.parent as ThingWithComps;

                if (thingWithComps != null && compBiocodable.Biocoded)
                {
                    var cb = getCrackerBuilding(selPawn);

                    if (cb != null)
                    {
                        // Create a new list of options including the existing options
                        List<FloatMenuOption> newOptions = new List<FloatMenuOption>(__result);

                        if (!cb.Empty)
                        {
                            newOptions.Add(new FloatMenuOption("Can't insert in biocode cracker: No empty cracker.", null));
                        }
                        else if (!cb.PowerOn)
                        {
                            newOptions.Add(new FloatMenuOption("Can't insert in biocode cracker: No powered empty cracker.", null));
                        }
                        else if (selPawn.Map.reservationManager.CanReserve(selPawn, thingWithComps) &&
                                 selPawn.Map.reservationManager.CanReserve(selPawn, cb))
                        {
                            string optionString = "Insert " + thingWithComps.Label;
                            if (thingWithComps.Map.reservationManager.IsReserved(thingWithComps))
                            {
                                Pawn reserver;
                                thingWithComps.Map.reservationManager.TryGetReserver(thingWithComps, null, out reserver);
                                if (reserver != null)
                                    optionString += "(reserved by " + reserver.Label + ")";
                            }

                            optionString += " into biocode cracker";

                            if (cb.Map.reservationManager.IsReserved(cb))
                            {
                                Pawn reserver;
                                cb.Map.reservationManager.TryGetReserver(cb, null, out reserver);
                                if (reserver != null)
                                    optionString += "(reserved by " + reserver.Label + ")";

                            }

                            // Add your custom option to the list
                            newOptions.Add(new FloatMenuOption(optionString, () =>
                            {
                                // Create the job for the pawn to use the Biocode Cracker building
                                Job job = JobMaker.MakeJob(Tixiv_BiocodeCracker_DefOf.InsertInBiocodeCracker, thingWithComps, cb, cb.InteractionCell);
                                job.count = 1;

                                // Assign the job to the pawn
                                selPawn.jobs.TryTakeOrderedJob(job);

                            }));

                        }
                        else
                        {
                            newOptions.Add(new FloatMenuOption("Can't insert in biocode cracker: No reachable unforbidden cracker.", null));
                        }

                        // Return the modified list of options
                        __result = newOptions;
                    }
                }
            }
        }
    }
}
