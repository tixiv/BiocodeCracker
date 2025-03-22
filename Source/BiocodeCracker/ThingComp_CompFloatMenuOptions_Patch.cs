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
        public static BiocodeCrackerBuilding getCrackerBuilding(Pawn selPawn, out bool anyCrackersOnMap)
        {
            // Get all buildings on the map
            var allCrackers = Find.CurrentMap.listerThings.AllThings.OfType<BiocodeCrackerBuilding>();

            anyCrackersOnMap = false;

            foreach (var cracker in allCrackers)
            {
                anyCrackersOnMap = true;

                if (cracker.Empty && cracker.PowerOn && !cracker.Map.reservationManager.IsReserved(cracker) && selPawn.CanReach(cracker, PathEndMode.Touch, Danger.None) && !cracker.IsForbidden(selPawn))
                    return cracker;
            }
            
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
                    if (selPawn.CanReach(thingWithComps, PathEndMode.Touch, Danger.None) && !thingWithComps.PositionHeld.IsForbidden(selPawn))
                    {
                        bool anyCrackersOnMap;
                        var cb = getCrackerBuilding(selPawn, out anyCrackersOnMap);

                        if (anyCrackersOnMap)
                        {
                            // Create a new list of options including the existing options
                            List<FloatMenuOption> newOptions = new List<FloatMenuOption>(__result);

                            if (cb != null)
                            {
                                string optionString = "Insert " + thingWithComps.Label + " into biocode cracker";

                                newOptions.Add(new FloatMenuOption(optionString, () =>
                                {
                                    thingWithComps.SetForbidden(false);

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
}
