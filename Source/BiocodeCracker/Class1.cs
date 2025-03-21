using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse.AI;

namespace Tixiv_BiocodeCracker
{
    public class Command_UseWeapon : Command_Action
    {
        public ThingWithComps weapon;

        public Command_UseWeapon(ThingWithComps weapon)
        {
            this.weapon = weapon;
            defaultLabel = "Use Weapon";  // The label that will show in the right-click menu
            defaultDesc = "Right-click to use this weapon.";  // Description for the right-click action
        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            // Trigger your custom Job when the player clicks the option.
            StartUseWeaponJob();
        }

        private void StartUseWeaponJob()
        {
            // Here you would want to create a job for the colonist to use the weapon (you'll likely need a JobDriver here).
            // This is a basic example that would call your custom logic.
            Log.Message("Using weapon: " + weapon.Label);
        }
    }

    [DefOf]
    public static class Tixiv_BiocodeCracker_DefOf
    {
        public static JobDef InsertInBiocodeCracker;
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


    public class BiocodeCrackerBuilding : Building
    {
    }

    public class MyMod : Mod
    {
        public MyMod(ModContentPack content) : base(content)
        {
            var harmony = new Harmony("com.Tixiv_BiocodeCracker");
            harmony.PatchAll();  // This applies all Harmony patches in the mod
        }
    }



}
