using RimWorld;
using Verse;
using UnityEngine;
using static Flecker.ExtensionUtility;

namespace Flecker
{
	public class CompFlecker : ThingComp
	{

		public CompProperties_Smoker Props
		{
			get
			{
				return (CompProperties_Smoker)this.props;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			fuelComp = parent.TryGetComp<CompRefuelable>();
			cachedParticleSizeMin = 1.5f * (parent.RotatedSize.Magnitude / 4f * Props.particleSize);
			cachedParticleSizeMax = 2.5f * (parent.RotatedSize.Magnitude / 4f * Props.particleSize);
			var offset = Props.particleOffset;
            switch (parent.Rotation.AsInt)
            {
                case 0:
                    offset += Props.particleOffsetNorth;
                    break;
                case 1:
                    offset += Props.particleOffsetEast;
                    break;
                case 2:
                    offset += Props.particleOffsetSouth;
                    break;
                case 3:
                    offset += Props.particleOffsetWest;
                    break;
            }
			cachedParticleOffset = parent.DrawPos + offset;

			//Cache map
			cachedMap = this.parent.Map;

			//Add to registry
			cachedMap.GetComponent<MapComponent_FleckManager>().compCache.Add(this);

			CheckIfRoofed();

			//Cache system
			if (Props.idleAlt == null && Props.indoorAlt == null)
			{
				cachedMap.flecks.systems.TryGetValue(Props.fleckDef.fleckSystemClass, out fleckSystemCache);
				cachedAltitude = Props.fleckDef.altitudeLayer.AltitudeFor(Props.fleckDef.altitudeLayerIncOffset);
			}
		}

		public override void PostDeSpawn(Map map)
		{
			map.GetComponent<MapComponent_FleckManager>().compCache.Remove(this);
		}

		public bool InUse
		{
			get
			{
				if (cachedMap == null)
				{
					return false;
				}
				Pawn pawn = cachedMap.thingGrid.ThingAt<Pawn>(parent.InteractionCell);
				return pawn != null && !pawn.pather.Moving && pawn.CurJob != null && pawn.CurJob.targetA != null && pawn.CurJob.targetA.HasThing && pawn.CurJob.targetA.Thing == parent;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			flickComp = parent.GetComp<CompFlickable>();
		}

		public void ThrowFleck(float angle, float rotationRate, float speed, FleckDef fleckDef, float size)
		{
			FleckCreationData dataStatic = FleckMaker.GetDataStatic(cachedParticleOffset, cachedMap, fleckDef, size);
			dataStatic.rotationRate = rotationRate;
			dataStatic.velocityAngle = angle;
			dataStatic.velocitySpeed = (fastRandom.Next(1,20) / 100f) + speed;

			if (fleckSystemCache != null)
			{
				dataStatic.spawnPosition.y = cachedAltitude;
				fleckSystemCache.CreateFleck(dataStatic);
			}
			else cachedMap.flecks.CreateFleck(dataStatic);
		}

		public void CheckIfRoofed()
		{
			isRoofed = cachedMap.roofGrid.Roofed(parent.Position);
		}

		public CompRefuelable fuelComp;
		public CompFlickable flickComp;
		public Vector3 cachedParticleOffset = Vector3.zero;
		public bool isRoofed = false;
		public float cachedParticleSizeMin = 1f;
		public float cachedParticleSizeMax = 1f;
		float cachedAltitude = 6f;
		public Thing trueParent; //Only used by extensions
		FleckSystem fleckSystemCache;
		Map cachedMap;
	}
}