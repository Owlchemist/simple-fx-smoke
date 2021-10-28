using Verse;
using System.Collections.Generic;

namespace Flecker
{
	public class MapComponent_FleckerRegistry : MapComponent
	{
        public List<CompFlecker> compCache; //Cache to avoid using laggy TryGetComponent. These two lists are generated in sync.
        int ticker, tickerRoofCheck, tickerDirection;
        float windDirection, transitiveDirection;

        const float LowWind = 0.25f;
        const float HighWind = 0.75f;
        const float DirectionSteps = 0.01f;
        const float MaxAngle = 80f;
        const float IndoorSpeed = 0.5f;
        const float IndoorAngle = 35f;
        const int DirectionWaitTime = 600;

        public MapComponent_FleckerRegistry(Map map) : base(map)
        {
            this.compCache = new List<CompFlecker>();
            transitiveDirection = windDirection = IndoorAngle;
        }

        public override void MapComponentTick()
        {
            float currentSpeed = UnityEngine.Mathf.Clamp(map.windManager.WindSpeed, LowWind, HighWind);
            updateWindDirection();

            //The tick trigger rate varies depending on wind speed to avoid particle gaps
            if (++ticker > 35 / (currentSpeed + 0.5f))
            {
                if (Find.CurrentMap != map) return;

                //Catch length for optimization
                int numberOfFleckers = compCache.Count;

                //Recycled rands
                float angleOffset = Rand.Range(-5, 5);
                float rotationRate = Rand.Range(-30, 30);

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
                    var angle = windDirection + angleOffset;

                    //Indoor
                    if (comp.isRoofed)
                    {
                        currentSpeed = IndoorSpeed;
                        angle = IndoorAngle + angleOffset;
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

        // Moves the wind direction when low wind
        private void updateWindDirection()
        {
            //Slowly move the direction towards the target angle
            if(windDirection != transitiveDirection)
            {
                windDirection = windDirection > transitiveDirection ? windDirection -= DirectionSteps : windDirection += DirectionSteps;
            }

            //If the wind is high, set the timer and return
            if(map.windManager.WindSpeed > LowWind)
            {
                tickerDirection = DirectionWaitTime;
                return;
            }

            //Change the target angle when it has been low wind for a while, once
            //Angle can be negative as the fleckspawner clamps it within the circle
            tickerDirection--;
            if(tickerDirection == 0)
            {
                transitiveDirection = Rand.Range(-MaxAngle, MaxAngle);   
            }
        }
    }
}