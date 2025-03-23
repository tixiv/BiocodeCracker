using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Tixiv_BiocodeCracker
{
    public static class UtilCreateBiocodedWeapon
    {
        public static Pawn GetRandomPawn()
        {
            // Get all pawns that exist in the world
            var allPawns = Find.WorldPawns.AllPawnsAliveOrDead;

            // Somehow this list at least sometimes has pawns who's name is null. Remove them.
            allPawns.RemoveAll(p => p == null || p.Name == null);

            // Ensure there are pawns in the world to select from
            if (allPawns.Count > 0)
            {
                // Select a random pawn from the list
                Pawn randomPawn = allPawns.RandomElement();

                // Log the random pawn's name
                Log.Message("Random Pawn: " + randomPawn.Name.ToStringFull);
                return randomPawn;
            }
            else
            {
                Log.Message("No pawns found in the world.");
                return null;
            }
        }

        public static Thing CreateRandomBiocodedWeapon()
        {
            // Step 1: Get all weapon ThingDefs
            List<ThingDef> weaponDefs = DefDatabase<ThingDef>.AllDefsListForReading
                .Where(def => def.IsWeapon) // Filter only weapons
                .ToList();

            // Step 2: Filter for weapons that have the CompBiocodable component
            List<ThingDef> biocodableWeapons = weaponDefs
                .Where(def => def.comps.Any(comp => comp.compClass == typeof(CompBiocodable)))
                .ToList();

            // Step 3: Select a random biocodable weapon from the list
            if (biocodableWeapons.Count > 0)
            {
                ThingDef randomWeaponDef = biocodableWeapons.RandomElement();

                // Step 4: Create the weapon using ThingMaker
                ThingWithComps randomWeapon = ThingMaker.MakeThing(randomWeaponDef) as ThingWithComps;

                // Optionally, you can set properties like quality or hit points
                randomWeapon.HitPoints = randomWeapon.MaxHitPoints; // Full health

                // Now you have a random biocodable weapon ready to use or spawn in the game.
                Log.Message($"Created a random biocodable weapon: {randomWeapon.Label}");

                CompBiocodable biocodableComp = randomWeapon.GetComp<CompBiocodable>();

                var pawn = GetRandomPawn();
                if (pawn != null)
                {
                    biocodableComp.CodeFor(pawn);
                    return randomWeapon;
                }
                return null;
            }
            else
            {
                Log.Message("No biocodable weapons found.");
                return null;
            }
        }
    }
}
