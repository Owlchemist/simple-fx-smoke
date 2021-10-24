using System.Collections.Generic;
using Verse;

namespace Flecker
{
    public class MapComponent_FleckerRegistry : MapComponent
    {
        public List<CompFlecker>
            compCache; //Cache to avoid using laggy TryGetComponent. These two lists are generated in sync.

        private int numberOfFleckers;
        private int ticker;
        private int tickerLong;

        public MapComponent_FleckerRegistry(Map map) : base(map)
        {
            compCache = new List<CompFlecker>();
        }

        public override void MapComponentTick()
        {
            if (ticker++ != 35)
            {
                return;
            }

            if (Find.CurrentMap != map)
            {
                return;
            }

            //Catch length for optimization
            numberOfFleckers = compCache.Count;

            //Recycled rands
            float angle = Rand.Range(30, 40);
            ;
            float rate = Rand.Range(-30, 30);

            for (var i = 0; i < numberOfFleckers; i++)
            {
                var comp = compCache[i];

                //Every now and then, update the isOutside check
                if (tickerLong++ == 100)
                {
                    comp.CheckIfRoofed();
                    tickerLong = 0;
                }

                if ((comp.fuelComp != null && comp.fuelComp.HasFuel && !comp.Props.billsOnly &&
                     (comp.flickComp == null || comp.flickComp.SwitchIsOn) || comp.Props.alwaysSmoke ||
                     comp.Props.billsOnly && comp.InUse) &&
                    comp.cachedParticleOffset.ShouldSpawnMotesAt(map))
                {
                    comp.ThrowFleck(angle, rate);
                }
            }

            ticker = 0;
        }
    }
}