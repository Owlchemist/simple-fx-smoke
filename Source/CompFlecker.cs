using System;
using RimWorld;
using Verse;
using UnityEngine;
using System.Collections.Generic;

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
			cachedParticleOffset = parent.DrawPos + Props.particleOffset;

			//Add to registry
			parent.Map.GetComponent<MapComponent_FleckerRegistry>().compCache.Add(this);

			CheckIfRoofed();
		}

		public override void PostDeSpawn(Map map)
		{
			map.GetComponent<MapComponent_FleckerRegistry>().compCache.Remove(this);
		}

		public bool InUse
		{
			get
			{
				if (parent.Map == null)
				{
					return false;
				}
				Pawn pawn = parent.Map.thingGrid.ThingAt<Pawn>(parent.InteractionCell);
				return pawn != null && !pawn.pather.Moving && pawn.CurJob != null && pawn.CurJob.targetA != null && pawn.CurJob.targetA.HasThing && pawn.CurJob.targetA.Thing == parent;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			flickComp = parent.GetComp<CompFlickable>();
		}

		public void ThrowFleck(float angle, float rate)
		{
			FleckDef def = (Props.indoorAlt != null && isRoofed) ? Props.indoorAlt : Props.fleckDef;
			FleckCreationData dataStatic = FleckMaker.GetDataStatic(cachedParticleOffset, parent.Map, def, Rand.Range(cachedParticleSizeMin, cachedParticleSizeMax));
			dataStatic.rotationRate = rate;
			dataStatic.velocityAngle = angle;
			dataStatic.velocitySpeed = Rand.Range(50, 70) / 100f;
			this.parent.Map.flecks.CreateFleck(dataStatic);
		}

		public void CheckIfRoofed()
		{
			isRoofed = parent.Map.roofGrid.Roofed(parent.Position);
		}

		public CompRefuelable fuelComp;
		public CompFlickable flickComp;
		public Vector3 cachedParticleOffset = Vector3.zero;
		public bool isRoofed = false;
		float cachedParticleSizeMin = 1f;
		float cachedParticleSizeMax = 1f;
	}
}