using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Tixiv_BiocodeCracker
{
    public class CompCrackerContainer : CompThingContainer
    {
        public override string CompInspectStringExtra()
        {
            // Polish: Override method from CompThingContainer to not say " x 1" (times 1) at the end. There can be only one item anyway.

            return "Contents".Translate() + ": " + (Empty ? ((string)"Nothing".Translate()) : ContainedThing.LabelCapNoCount);
        }
    }
}
