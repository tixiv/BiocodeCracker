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

            foreach (var cracker in allCrackers)
            {
                // TODO: if cracker useable by pawn
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
                    var cb = getCrackerBuilding(selPawn);

                    if (cb != null)
                    { 

                        // Create a new list of options including the existing options
                        List<FloatMenuOption> newOptions = new List<FloatMenuOption>(__result);

                        // Add your custom option to the list
                        newOptions.Add(new FloatMenuOption("Insert in biocode cracker", () =>
                        {
                            // Define what happens when the player clicks the custom option
                            Log.Message("Custom Biocode Action executed on " + thingWithComps.Label);
                            // Your custom logic here...


                            // Create the job for the pawn to use the Biocode Cracker building
                            Job job = JobMaker.MakeJob(Tixiv_BiocodeCracker_DefOf.InsertInBiocodeCracker, thingWithComps, cb, cb);
                            job.count = 1;

                            // Assign the job to the pawn
                            selPawn.jobs.TryTakeOrderedJob(job);

                        }));

                        // Return the modified list of options
                        __result = newOptions;
                    }
                }
            }
        }
    }
}
