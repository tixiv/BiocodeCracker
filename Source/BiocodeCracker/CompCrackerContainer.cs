using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Tixiv_BiocodeCracker
{
    class CompCrackerContainer : CompThingContainer
    {
        public void DropItemsOntoFloor()
        {
            {
                // Get the parent Thing (the container)
                Thing parentThing = parent;

                // Ensure that the parent Thing has a valid position and map
                if (parentThing != null && parentThing.Map != null)
                {
                    Map map = parentThing.Map;

                    // Get the position of the parent Thing (where the container is located)
                    IntVec3 dropLocation = parentThing.Position;

                    innerContainer.TryDropAll(dropLocation, map, ThingPlaceMode.Near);
                }
            }
        }
    }
}
