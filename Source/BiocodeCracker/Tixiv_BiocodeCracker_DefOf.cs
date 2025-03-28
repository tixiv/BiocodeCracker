using Verse;
using RimWorld;

namespace Tixiv_BiocodeCracker
{
    [DefOf]
    public static class Tixiv_BiocodeCracker_DefOf
    {
        public static JobDef InsertInBiocodeCracker;

        // The game itself doesn't have this DefOf, although it is an asset from Biotech.
        // Since only the name gets used in .net reflection and not the namespace we can define it here.
        public static SoundDef SubcoreEncoder_Working;
    }
}
