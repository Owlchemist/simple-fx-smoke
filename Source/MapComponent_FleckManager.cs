using Verse;
using System.Collections.Generic;

namespace Flecker
{
	public class MapComponent_FleckManager : MapComponent
	{
        public List<CompFlecker> compCache;
        int ticker = 35, tickerWindDirection;
        float windDirection, transitiveDirection;

        const float lowWind = 0.25f,
            highWind = 0.75f,
            directionSteps = 0.25f,
            indoorSpeed = 0.5f,
            indoorAngle = 35f,
            epsilon = 0.02f;
        const int maxAngle = 70,
            directionWaitTime = 25;

        FastRandom fastRandom;

        public MapComponent_FleckManager(Map map) : base(map)
        {
            this.compCache = new List<CompFlecker>();
            transitiveDirection = windDirection = indoorAngle;
            fastRandom = new FastRandom();
        }

        public override void ExposeData()
		{
			Scribe_Values.Look(ref windDirection, "windDirection", 35f);
            Scribe_Values.Look(ref transitiveDirection, "transitiveDirection", 35f);
            Scribe_Values.Look(ref tickerWindDirection, "tickerDirection", -1);
		}

        public override void MapComponentTick()
        {
            var length = compCache.Count;
            if (length == 0) return;
            var gameTicks = Current.gameInt.tickManager.ticksGameInt;
            UpdateWindDirection(gameTicks);

            if (gameTicks % ticker == 0)
            {
                float currentSpeed = UnityEngine.Mathf.Clamp(map.windManager.cachedWindSpeed, lowWind, highWind);
                ticker = (int)(35f / (currentSpeed + 0.5f)); //The tick trigger rate varies depending on wind speed to avoid particle gaps
                if (Find.CurrentMap != map) return;

                //Recycled rands
                float angleOffset = fastRandom.Next(-5, 5);
                var tmp = fastRandom.Next(-30, 30);
                float rotationRate = tmp;

                if (gameTicks % 350 == 0)
                {
                    for (int i = length; i-- > 0;) compCache[i].CheckIfRoofed();
                }

                //Prepare the camera driver
                CellRect expandedCameraArea = ExtensionUtility.usingExtensions ? CameraDriver.lastViewRect.ExpandedBy(CameraDriver.lastViewRect.maxZ - CameraDriver.lastViewRect.minZ) : default;

                for (int i = length; i-- > 0;)
                {
                    CompFlecker comp = compCache[i];
                    CompProperties_Smoker props = comp.cachedProp;

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
                        currentSpeed = indoorSpeed;
                        angle = indoorAngle + angleOffset;
                        //Indoor smoke
                        if (props.indoorAlt != null) fleckDef = props.indoorAlt;
                    }

                    //Idle smoke
                    if (props.idleAlt != null && props.billsOnly && !comp.InUse) fleckDef = props.idleAlt;

                    //Speed instance
                    float speed = currentSpeed;
                    
                    //Check for special drivers
                    float size = fastRandom.Next(comp.cachedParticleSizeMin, comp.cachedParticleSizeMax) / 100f;
                    if (ExtensionUtility.usingExtensions && comp.fireParent != null)
                    {
                        if (!expandedCameraArea.Contains(comp.parent.positionInt)) continue;

                        float fireSizeModifier = comp.fireParent.fireSize / 1.25f;
                        if (fireSizeModifier < 0.5f) fireSizeModifier = 0.5f;
                        size *= fireSizeModifier;
                        speed *= fireSizeModifier;
                    }

                    //Push results to comp
                    comp.ThrowFleck(angle, rotationRate, speed, fleckDef, size);
                }
            }
        }

        // Moves the wind direction when low wind
        void UpdateWindDirection(int gameTicks)
        {
            if (gameTicks % 25 == 0)
            {
                //Slowly move the direction towards the target angle
                if (windDirection != transitiveDirection)
                {
                    windDirection = windDirection > transitiveDirection ? windDirection -= directionSteps : windDirection += directionSteps;
                }

                //If the wind is high, set the timer and return
                if(map.windManager.cachedWindSpeed > lowWind)
                {
                    tickerWindDirection = directionWaitTime;
                    return;
                }

                //Change the target angle when it has been low wind for a while, once
                //Angle can be negative as the fleckspawner clamps it within the circle
                if(--tickerWindDirection == 0)
                {
                    transitiveDirection = fastRandom.Next(-maxAngle, maxAngle);
                }
            }
        }
    }
}