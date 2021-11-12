using Verse;
using RimWorld;
using System.Collections.Generic;

namespace Flecker
{
	public class MapComponent_FleckManager : MapComponent
	{
        public List<CompFlecker> compCache; //Cache to avoid using laggy TryGetComponent.
        int ticker, tickerRoofCheck, tickerWindDirection, tickWindCheck;
        float windDirection, transitiveDirection;

        const float LowWind = 0.25f;
        const float HighWind = 0.75f;
        const float DirectionSteps = 0.25f;
        const int MaxAngle = 70;
        const float IndoorSpeed = 0.5f;
        const float IndoorAngle = 35f;
        const int DirectionWaitTime = 25;
        const float epsilon = 0.02f;
        FastRandom fastRandom;

        public MapComponent_FleckManager(Map map) : base(map)
        {
            this.compCache = new List<CompFlecker>();
            transitiveDirection = windDirection = IndoorAngle;
            fastRandom = new FastRandom();
        }

        public override void ExposeData()
		{
			Scribe_Values.Look<float>(ref this.windDirection, "windDirection", 35f, false);
            Scribe_Values.Look<float>(ref this.transitiveDirection, "transitiveDirection", 35f, false);
            Scribe_Values.Look<int>(ref this.tickerWindDirection, "tickerDirection", -1, false);
		}

        public override void MapComponentTick()
        {
            float currentSpeed = UnityEngine.Mathf.Clamp(map.windManager.WindSpeed, LowWind, HighWind);
            UpdateWindDirection();

            //The tick trigger rate varies depending on wind speed to avoid particle gaps
            if (++ticker > 35f / (currentSpeed + 0.5f))
            {
                if (Find.CurrentMap != map) return;

                //Catch length for optimization
                int numberOfFleckers = compCache.Count;

                //Recycled rands
                float angleOffset = fastRandom.Next(-5, 5);
                float rotationRate = fastRandom.Next(-30, 30);

                if (++tickerRoofCheck > 9) //Every 350 ticks
                {
                    compCache.ForEach(x => x.CheckIfRoofed());
                    tickerRoofCheck = 0;
                }

                for (int i = 0; i < numberOfFleckers; ++i)
                {
                    CompFlecker comp = compCache[i];
                    CompProperties_Smoker props = comp.Props;

                    if
                    (
                        //Fuelable but has no fuel or flickable but turned off
                        (!props.alwaysSmoke && (comp.fuelComp != null && !comp.fuelComp.HasFuel || comp.flickComp != null && !comp.flickComp.SwitchIsOn))
                        ||
                        //Bills only but not in use and no idle-smoke
                        (props.billsOnly && !comp.InUse && props.idleAlt == null)
                        ||
                        //Not spawnable motes
                        (!comp.cachedParticleOffset.ShouldSpawnMotesAt(map))
                    ) continue;

                    //Get the base fleckDef and angle
                    FleckDef fleckDef = props.fleckDef;
                    float angle = windDirection + angleOffset;

                    //Indoor
                    if (comp.isRoofed)
                    {
                        currentSpeed = IndoorSpeed;
                        angle = IndoorAngle + angleOffset;
                        //Indoor smoke
                        if (props.indoorAlt != null) fleckDef = props.indoorAlt;
                    }

                    //Idle smoke
                    if (props.idleAlt != null && props.billsOnly && !comp.InUse) fleckDef = props.idleAlt;

                    //Speed instance
                    float speed = currentSpeed;
                    
                    //Check for special drivers
                    float size = Rand.Range(comp.cachedParticleSizeMin, comp.cachedParticleSizeMax);
                    if (props.driver == CompProperties_Smoker.Driver.Fire && comp.trueParent is Fire)
                    {
                        float fireSizeModifier = System.Math.Max(0.5f, ((Fire)comp.trueParent).fireSize / 1.25f);
                        size *= fireSizeModifier;
                        speed *= fireSizeModifier;
                    }

                    //Push results to comp
                    comp.ThrowFleck(angle, rotationRate, speed, fleckDef, size);
                }
                ticker = 0;
            }
        }

        // Moves the wind direction when low wind
        void UpdateWindDirection()
        {
            if (++tickWindCheck == 25)
            {
                tickWindCheck = 0;
                //Slowly move the direction towards the target angle
                if (windDirection != transitiveDirection)
                {
                    windDirection = windDirection > transitiveDirection ? windDirection -= DirectionSteps : windDirection += DirectionSteps;
                }

                //If the wind is high, set the timer and return
                if(map.windManager.WindSpeed > LowWind)
                {
                    tickerWindDirection = DirectionWaitTime;
                    return;
                }

                //Change the target angle when it has been low wind for a while, once
                //Angle can be negative as the fleckspawner clamps it within the circle
                if(--tickerWindDirection == 0)
                {
                    transitiveDirection = fastRandom.Next(-MaxAngle, MaxAngle);
                }
            }
        }
    }
}