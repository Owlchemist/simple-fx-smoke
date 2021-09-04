using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace Flecker
{
	public class CompFlecker : ThingComp
	{
		public CompFlecker()
		{
		}

		public CompProperties_Smoker Props
		{
			get
			{
				return (CompProperties_Smoker)this.props;
			}
		}

		public override void PostSpawnSetup(bool respawningAfterLoad)
		{
			this.fuelComp = this.parent.TryGetComp<CompRefuelable>();
			this.Props.cachedParticleSize = this.parent.RotatedSize.Magnitude / 4f * this.Props.particleSize;
			this.Props.cachedParticleOffset = this.parent.DrawPos + this.Props.particleOffset;
			this.parent.Map.GetComponent<MapComponent_FleckerRegistry>().fleckerRegistry.Add(this);
		}

		public override void PostDeSpawn(Map map)
		{
			map.GetComponent<MapComponent_FleckerRegistry>().fleckerRegistry.Remove(this);
		}

		public bool InUse
		{
			get
			{
				if (this.parent.Map == null)
				{
					return false;
				}
				Pawn pawn = this.parent.Map.thingGrid.ThingAt<Pawn>(this.parent.InteractionCell);
				return pawn != null && !pawn.pather.Moving && pawn.CurJob != null && pawn.CurJob.targetA != null && pawn.CurJob.targetA.HasThing && pawn.CurJob.targetA.Thing == this.parent;
			}
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.flickComp = this.parent.GetComp<CompFlickable>();
		}

		public void ThrowFleck()
		{
			if (this.Props.particleType == "white") ThingDefFlecks.ThrowVariableFleck(this.Props.cachedParticleOffset, this.parent.Map, this.Props.cachedParticleSize, RimWorld.FleckDefOf.Smoke);
			else if (this.Props.particleType == "vapor") ThingDefFlecks.ThrowVariableFleck(this.Props.cachedParticleOffset, this.parent.Map, this.Props.cachedParticleSize, FleckDefOf.Fleck_Smoker_Vapor);
			else if (this.Props.particleType == "heavy") ThingDefFlecks.ThrowVariableFleck(this.Props.cachedParticleOffset, this.parent.Map, this.Props.cachedParticleSize, FleckDefOf.Fleck_Smoker_Heavy);
		}

		public CompRefuelable fuelComp;

		public CompFlickable flickComp;
	}
}