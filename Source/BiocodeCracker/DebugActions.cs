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
        [DebugAction("Spawning", "Spawn biocoded weapon", false, false, false, false, 0, false, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = 1000)]
        private static List<DebugActionNode> SpawnBiocodedWeapon()
        {
            List<DebugActionNode> list = new List<DebugActionNode>();

                list.Add(new DebugActionNode("Spawn biocoded weapon", DebugActionType.ToolMap, null, null)
                {
                    action = delegate ()
                    {
                        var w = UtilCreateBiocodedWeapon.CreateRandomBiocodedWeapon();
                        if (w != null)
                        {
                            GenSpawn.Spawn(w, UI.MouseCell(), Find.CurrentMap, WipeMode.Vanish);
                        }
                    }
                });
            return list;
        }
    }
}
