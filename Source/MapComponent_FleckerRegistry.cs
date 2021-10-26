using Verse;
using System.Collections.Generic;
using RimWorld;

namespace Flecker
{
	public class MapComponent_FleckerRegistry : MapComponent
	{
		
        public List<CompFlecker> compCache; //Cache to avoid using laggy TryGetComponent. These two lists are generated in sync.
        private int numberOfFleckers;
        int ticker;
        int tickerLong;
        float currentWindDirection;
        float mapWindSpeed;
        public MapComponent_FleckerRegistry(Map map) : base(map)
        {
            this.compCache = new List<CompFlecker>();
            currentWindDirection = Rand.Range(0, 360f);
            mapWindSpeed = map.windManager.WindSpeed;
        }

        public override void MapComponentTick()
        {
            //Update the wind direction and speed slightly every 1000 ticks, slightly more positive
            if (GenTicks.TicksGame.ToString().EndsWith("000")) {
                currentWindDirection = GenMath.PositiveMod(currentWindDirection + Rand.Range(-10, 20), 360f);
                mapWindSpeed = map.windManager.WindSpeed;
            }

            if (ticker++ == 35)
            {
                if (Find.CurrentMap != map) return;

                //Catch length for optimization
                numberOfFleckers = compCache.Count;

                //Recycled rands
                float indoorAngle = Rand.Range(30, 40);
                float outdoorAngle = GenMath.PositiveMod(Rand.Range(-10, 10) + currentWindDirection, 360f);
                float rate = Rand.Range(-30, 30);

                for (int i = 0; i < numberOfFleckers; i++)
                {
                    var comp = compCache[i];

                    //Every now and then, update the isOutside check
                    if (tickerLong++ == 100)
                    {
                        comp.CheckIfRoofed();
                        tickerLong = 0;
                    }

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
                    var currentAngle = outdoorAngle;
                    var currentSpeed = mapWindSpeed;

                    //Indoor
                    if (comp.isRoofed)
                    {
                        currentAngle = indoorAngle;
                        currentSpeed = 0.5f;
                        //Alternative smoke
                        if (comp.Props.indoorAlt != null)
                        {
                            fleckDef = comp.Props.indoorAlt;
                        }
                    }

                    //Idle alternative
                    if (comp.Props.idleAlt != null && comp.Props.billsOnly && !comp.InUse)
                    {
                        fleckDef = comp.Props.idleAlt;
                    }

                    comp.ThrowFleck(currentAngle, rate, currentSpeed, fleckDef);
                }
                ticker = 0;
            }
        }
    }
}