using System;
using RimWorld;
using Verse;
using UnityEngine;

namespace Smoker
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
			ThrowMote(this.parent.DrawPos + this.Props.particleOffset, this.parent.RotatedSize.Magnitude / 4f * this.Props.particleSize, this.parent.Map, this.Props.particleDelay, this.Props.particleType);
		}

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			this.flickComp = this.parent.GetComp<CompFlickable>();
		}

		public static void ThrowMote(Vector3 loc, float size, Map map, float smokeSpeedDelay, string particleType)
		{
			if ((float)Find.TickManager.TicksGame % smokeSpeedDelay == 0f)
			{
				ThingDef def = new ThingDef();
				if (particleType == "white")
				{
					def = ThingDefMotes.Mote_Smoker_White;
				}
				else if (particleType == "vapor")
				{
					def = ThingDefMotes.Mote_Smoker_Vapor;
				}
				else if (particleType == "heavy")
				{
					def = ThingDefMotes.Mote_Smoker_Heavy;
				}
				MoteThrown moteThrown = (MoteThrown)ThingMaker.MakeThing(def, null);
				if (!GenView.ShouldSpawnMotesAt(loc, map) || map.moteCounter.SaturatedLowPriority)
				{
					return;
				}
				moteThrown.Scale = Rand.Range(1.5f, 3f) * size;
				moteThrown.rotationRate = Rand.Range(-25f, 50f);
				moteThrown.exactPosition = loc;
				moteThrown.SetVelocity((float)Rand.Range(25, 50), Rand.Range(0.5f, 0.75f));
				GenSpawn.Spawn(moteThrown, IntVec3Utility.ToIntVec3(loc), map, WipeMode.Vanish);
			}
		}

		private CompRefuelable fuelComp;

		private CompFlickable flickComp;
	}
}