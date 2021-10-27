using Verse;
using System.Collections.Generic;

namespace Flecker
{
	public class MapComponent_FleckerRegistry : MapComponent
	{
        public List<CompFlecker> compCache; //Cache to avoid using laggy TryGetComponent. These two lists are generated in sync.
        int ticker, tickerRoofCheck;

        public MapComponent_FleckerRegistry(Map map) : base(map)
        {
            this.compCache = new List<CompFlecker>();
        }

        public override void MapComponentTick()
        {
            float currentSpeed = UnityEngine.Mathf.Clamp(map.windManager.WindSpeed, 0.25f, 0.75f);

            //The tick trigger rate varies depending on wind speed to avoid particle gaps
            if (++ticker > 35 / (currentSpeed + 0.5f))
            {
                if (Find.CurrentMap != map) return;

                //Catch length for optimization
                int numberOfFleckers = compCache.Count;

                //Recycled rands
                float angle = Rand.Range(30, 40);
                float rotationRate = Rand.Range(-30, 30);

                /*
                if (++tickerWind == 100) //Every 3500 ticks, about once per 1.5 hours
                {
                    //currentWindDirection = GenMath.PositiveMod(currentWindDirection + Rand.Range(-10, 20), 360f);
                    mapWindSpeed = map.windManager.WindSpeed;
                    tickerWind = 0;
                }
                */

                if (++tickerRoofCheck > 9) //Every 350 ticks
                {
                    compCache.ForEach(x => x.CheckIfRoofed());
                    tickerRoofCheck = 0;
                }

                for (int i = 0; i < numberOfFleckers; ++i)
                {
                    var comp = compCache[i];

                    //Fuelable but has no fuel or flickable but turned off
                    if(!comp.Props.alwaysSmoke && (comp.fuelComp != null && !comp.fuelComp.HasFuel || comp.flickComp != null && !comp.flickComp.SwitchIsOn))
                    {
                        continue;
                    }

                    //Bills only but not in use and no idle-smoke
                    if(comp.Props.billsOnly && !comp.InUse && comp.Props.idleAlt == null)
                    {
                        continue;
                    }

                    //Not spawnable motes
                    if(!comp.cachedParticleOffset.ShouldSpawnMotesAt(map))
                    {
                        continue;
                    }

                    //Get the base fleckDef and angle
                    var fleckDef = comp.Props.fleckDef;

                    //Indoor
                    if (comp.isRoofed)
                    {
                        currentSpeed = 0.5f;
                        //Indoor smoke
                        if (comp.Props.indoorAlt != null) fleckDef = comp.Props.indoorAlt;
                    }

                    //Idle smoke
                    if (comp.Props.idleAlt != null && comp.Props.billsOnly && !comp.InUse) fleckDef = comp.Props.idleAlt;

                    //Push results to comp
                    comp.ThrowFleck(angle, rotationRate, currentSpeed, fleckDef);
                }
                ticker = 0;
            }
        }
    }
}