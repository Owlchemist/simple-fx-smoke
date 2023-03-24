using RimWorld;
using Verse;
using UnityEngine;
using static Flecker.ExtensionUtility;

namespace Flecker
{
	public class CompFlecker : ThingComp
	{
		public CompProperties_Smoker cachedProp;
		public CompRefuelable fuelComp;
		public CompFlickable flickComp;
		public Vector3 cachedParticleOffset = Vector3.zero;
		public bool isRoofed = false;
		public int cachedParticleSizeMin = 1;
		public int cachedParticleSizeMax = 1;
		float cachedAltitude = 6f;
		public Thing trueParent; //Only used by extensions
		public Fire fireParent;
		FleckSystem fleckSystemCache;
		Map cachedMap;
		int interactionCell;

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			cachedProp = (CompProperties_Smoker)this.props;
			fuelComp = parent.TryGetComp<CompRefuelable>();
			cachedParticleSizeMin = (int)(1.5f * (parent.RotatedSize.Magnitude / 4f * cachedProp.particleSize) * 100);
			cachedParticleSizeMax = (int)(2.5f * (parent.RotatedSize.Magnitude / 4f * cachedProp.particleSize) * 100);
			var offset = cachedProp.particleOffset;
            switch (parent.Rotation.AsInt)
            {
                case 0:
                    offset += cachedProp.particleOffsetNorth;
                    break;
                case 1:
                    offset += cachedProp.particleOffsetEast;
                    break;
                case 2:
                    offset += cachedProp.particleOffsetSouth;
                    break;
                case 3:
                    offset += cachedProp.particleOffsetWest;
                    break;
            }
			cachedParticleOffset = parent.DrawPos + offset;

			//Cache map
			cachedMap = this.parent.Map;
			if (cachedMap == null) return; //Stop, don't add to registry.

			interactionCell = cachedMap.cellIndices.CellToIndex(parent.InteractionCell);

			//Add to registry
			cachedMap.GetComponent<MapComponent_FleckManager>().compCache.Add(this);

			CheckIfRoofed();

			//Cache system
			if (cachedProp.idleAlt == null && cachedProp.indoorAlt == null)
			{
				cachedMap.flecks.systems.TryGetValue(cachedProp.fleckDef.fleckSystemClass, out fleckSystemCache);
				cachedAltitude = cachedProp.fleckDef.altitudeLayer.AltitudeFor(cachedProp.fleckDef.altitudeLayerIncOffset);
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
				var things = cachedMap.thingGrid.ThingsListAtFast(interactionCell);
				for (int i = things.Count; i-- > 0;)
				{
					if (things[i] is Pawn pawn)
					{
						var job = pawn.CurJob;
						return !pawn.pather.Moving && job != null && job.targetA != null && job.targetA.HasThing && job.targetA.Thing == parent;
					}
				}
				return false;
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
	}
}