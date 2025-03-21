using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Tixiv_BiocodeCracker
{
    class CompCrackerContainer : CompThingContainer
    {
        public override bool Accepts(Thing thing)
        {
            return true;
        }

        public override bool Accepts(ThingDef thingDef)
        {
            return true;
        }
    }
}
