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
		public float particleSize = 1f;
		public FleckDef fleckDef;
		public FleckDef indoorAlt;
		public bool billsOnly;
		public bool alwaysSmoke;
	}
}
