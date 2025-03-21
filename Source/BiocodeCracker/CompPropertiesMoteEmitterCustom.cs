using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Tixiv_BiocodeCracker
{
    public class CompProperties_MoteEmitterCustom : CompProperties_MoteEmitter
    {
        public CompProperties_MoteEmitterCustom()
        {
            compClass = typeof(CompMoteEmitterCustom);
        }
    }
}
