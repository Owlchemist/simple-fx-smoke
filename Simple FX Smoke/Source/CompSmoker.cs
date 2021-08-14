using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace Flecker
{
	public class CompSmoker : ThingComp
	{
		public CompSmoker()
		{
		}

		public override void CompTick()
		{
			if (this.Props.alwaysSmoke || (this.Props.billsOnly && this.InUse))
			{
				this.Smoke();
				return;
			}
			if (this.fuelComp != null && this.fuelComp.HasFuel && !this.Props.billsOnly && (this.flickComp == null || this.flickComp.SwitchIsOn))
			{
				this.Smoke();
			}
		}

		static CompSmoker()
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

		private void Smoke()
		{
			ThrowFleck(this.parent.DrawPos + this.Props.particleOffset, this.parent.RotatedSize.Magnitude / 4f * this.Props.particleSize, this.parent.Map, this.Props.particleType);
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.flickComp = this.parent.GetComp<CompFlickable>();
		}

		public static void ThrowFleck(Vector3 loc, float size, Map map, string particleType)
		{
			if (Find.TickManager.TicksGame % 30 == 0)
			{
				
				if (particleType == "white")
				{
					ThingDefFlecks.ThrowVariableFleck(loc, map, size, RimWorld.FleckDefOf.Smoke);
				}
				else if (particleType == "vapor")
				{
					ThingDefFlecks.ThrowVariableFleck(loc, map, size, FleckDefOf.Fleck_Smoker_Vapor);
				}
				else if (particleType == "heavy")
				{
					ThingDefFlecks.ThrowVariableFleck(loc, map, size, FleckDefOf.Fleck_Smoker_Heavy);
				}
			}
		}

		private CompRefuelable fuelComp;

		private CompFlickable flickComp;
	}
}