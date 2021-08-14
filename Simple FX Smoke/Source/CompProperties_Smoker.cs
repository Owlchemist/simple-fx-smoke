using System;
using UnityEngine;
using Verse;

namespace Flecker
{
	public class CompProperties_Smoker : CompProperties
	{
		public CompProperties_Smoker()
		{
			this.compClass = typeof(CompSmoker);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
		}

		static CompProperties_Smoker()
		{
		}

		public Vector3 particleOffset = Vector3.zero;

		public float particleSize = 1f;

		public float particleDelay = 30f;

		public string particleType = "white";

		public bool billsOnly;

		public bool alwaysSmoke;
	}
}
