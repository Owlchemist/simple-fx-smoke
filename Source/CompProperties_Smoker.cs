using RimWorld;
using UnityEngine;
using Verse;

namespace Flecker
{
    public class CompProperties_Smoker : CompProperties
    {
        public bool alwaysSmoke;
        public bool billsOnly;
        public FleckDef fleckDef;
        public FleckDef indoorAlt;

        public Vector3 particleOffset = Vector3.zero;
        public Vector3 particleOffsetEast = Vector3.zero;
        public Vector3 particleOffsetNorth = Vector3.zero;
        public Vector3 particleOffsetSouth = Vector3.zero;
        public Vector3 particleOffsetWest = Vector3.zero;
        public float particleSize = 1f;

        public CompProperties_Smoker()
        {
            compClass = typeof(CompFlecker);
        }

        public override void ResolveReferences(ThingDef parentDef)
        {
            base.ResolveReferences(parentDef);
            if (fleckDef == null)
            {
                fleckDef = FleckDefOf.Smoke;
            }
        }
    }
}