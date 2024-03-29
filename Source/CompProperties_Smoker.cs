using UnityEngine;
using Verse;

namespace Flecker
{
	public class CompProperties_Smoker : CompProperties
	{
		public CompProperties_Smoker()
		{
			this.compClass = typeof(CompFlecker);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			if (fleckDef == null) fleckDef = RimWorld.FleckDefOf.Smoke;
		}

		public Vector3 particleOffset = Vector3.zero;
        public Vector3 particleOffsetEast = Vector3.zero;
        public Vector3 particleOffsetNorth = Vector3.zero;
        public Vector3 particleOffsetSouth = Vector3.zero;
        public Vector3 particleOffsetWest = Vector3.zero;
		public float particleSize = 1f;
		public FleckDef fleckDef;
		public FleckDef indoorAlt;
        public FleckDef idleAlt;
		public bool billsOnly;
		public bool alwaysSmoke;
		public enum Driver {Normal, Fire};
		public Driver driver;
	}

	public class Flecker : DefModExtension
	{
		public Vector3 particleOffset = Vector3.zero;
        public Vector3 particleOffsetEast = Vector3.zero;
        public Vector3 particleOffsetNorth = Vector3.zero;
        public Vector3 particleOffsetSouth = Vector3.zero;
        public Vector3 particleOffsetWest = Vector3.zero;
		public float particleSize = 1f;
		public FleckDef fleckDef = RimWorld.FleckDefOf.Smoke;
		public FleckDef indoorAlt;
        public FleckDef idleAlt;
		public bool billsOnly;
		public bool alwaysSmoke;
		public CompProperties_Smoker.Driver driver;
	}
}
