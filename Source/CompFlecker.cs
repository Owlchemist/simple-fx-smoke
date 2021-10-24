using RimWorld;
using UnityEngine;
using Verse;

namespace Flecker
{
    public class CompFlecker : ThingComp
    {
        public Vector3 cachedParticleOffset = Vector3.zero;
        private float cachedParticleSizeMax = 1f;
        private float cachedParticleSizeMin = 1f;
        public CompFlickable flickComp;

        public CompRefuelable fuelComp;
        public bool isRoofed;

        public CompProperties_Smoker Props => (CompProperties_Smoker)props;

        public bool InUse
        {
            get
            {
                if (parent.Map == null)
                {
                    return false;
                }

                var pawn = parent.Map.thingGrid.ThingAt<Pawn>(parent.InteractionCell);
                return pawn != null && !pawn.pather.Moving && pawn.CurJob != null && pawn.CurJob.targetA != null &&
                       pawn.CurJob.targetA.HasThing && pawn.CurJob.targetA.Thing == parent;
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

            //Add to registry
            parent.Map.GetComponent<MapComponent_FleckerRegistry>().compCache.Add(this);

            CheckIfRoofed();
        }

        public override void PostDeSpawn(Map map)
        {
            map.GetComponent<MapComponent_FleckerRegistry>().compCache.Remove(this);
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            flickComp = parent.GetComp<CompFlickable>();
        }

        public void ThrowFleck(float angle, float rate)
        {
            var def = Props.indoorAlt != null && isRoofed ? Props.indoorAlt : Props.fleckDef;
            var dataStatic = FleckMaker.GetDataStatic(cachedParticleOffset, parent.Map, def,
                Rand.Range(cachedParticleSizeMin, cachedParticleSizeMax));
            dataStatic.rotationRate = rate;
            dataStatic.velocityAngle = angle;
            dataStatic.velocitySpeed = Rand.Range(50, 70) / 100f;
            parent.Map.flecks.CreateFleck(dataStatic);
        }

        public void CheckIfRoofed()
        {
            isRoofed = parent.Map.roofGrid.Roofed(parent.Position);
        }
    }
}