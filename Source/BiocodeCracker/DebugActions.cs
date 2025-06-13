using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LudeonTK;
using RimWorld;
using Verse;

namespace Tixiv_BiocodeCracker
{
    public static class DebugActions
    {
        [DebugAction("Spawning", "Spawn biocoded weapon", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap /* ,hideInSubMenu = true*/)]
        private static void SpawnBiocodedWeapon()
        {
            var w = UtilCreateBiocodedWeapon.CreateRandomBiocodedWeapon();
            if (w != null)
            {
                GenSpawn.Spawn(w, UI.MouseCell(), Find.CurrentMap, WipeMode.Vanish);
            }
        }
    }
}
