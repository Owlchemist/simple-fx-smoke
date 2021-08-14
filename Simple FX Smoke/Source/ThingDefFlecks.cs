using RimWorld;
using Verse;
using UnityEngine;

namespace Smoker
{
	public static class ThingDefFlecks
	{
		public static void ThrowVariableFleck(Vector3 loc, Map map, float size, FleckDef def)
		{
			if (!loc.ShouldSpawnMotesAt(map))
			{
				return;
			}
			FleckCreationData dataStatic = FleckMaker.GetDataStatic(loc, map, def, Rand.Range(1.5f, 2.5f) * size);
			dataStatic.rotationRate = Rand.Range(-30f, 30f);
			dataStatic.velocityAngle = (float)Rand.Range(30, 40);
			dataStatic.velocitySpeed = Rand.Range(0.5f, 0.7f);
			map.flecks.CreateFleck(dataStatic);
		}
	}
}
