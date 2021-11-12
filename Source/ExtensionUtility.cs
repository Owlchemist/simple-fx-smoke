using Verse;
using HarmonyLib;
using System.Collections.Generic;

namespace Flecker
{
	//The flecker extension is an alternative way to add properties to defs, specifically defs that don't support comps
	static class ExtensionUtility
	{
		static bool usingExtensions;
		public static Dictionary<Thing, CompFlecker> registry = new Dictionary<Thing, CompFlecker>();
		public static FastRandom fastRandom;
		public static void Setup()
		{
			usingExtensions = DefDatabase<ThingDef>.AllDefsListForReading.Any(x => x.HasModExtension<Flecker>());
			fastRandom = new FastRandom();
		}


		[HarmonyPatch (typeof(Thing), nameof(Thing.SpawnSetup))]
		static class Patch_SpawnSetup
		{
			static void Postfix(ref Thing __instance, bool respawningAfterLoad)
			{
				if (!usingExtensions || !__instance.def.HasModExtension<Flecker>()) return;
				
				Flecker modX = __instance.def.GetModExtension<Flecker>();

				ThingWithComps fleckerHolder = new ThingWithComps()
				{
					def = __instance.def,
					Position = __instance.Position,
					mapIndexOrState = __instance.mapIndexOrState
				};

				CompProperties_Smoker props = new CompProperties_Smoker()
				{
					particleOffset = modX.particleOffset,
					particleOffsetEast = modX.particleOffsetEast,
					particleOffsetNorth = modX.particleOffsetNorth,
					particleOffsetSouth = modX.particleOffsetSouth,
					particleOffsetWest = modX.particleOffsetWest,
					particleSize = modX.particleSize,
					fleckDef = modX.fleckDef,
					indoorAlt = modX.indoorAlt,
					idleAlt = modX.idleAlt,
					billsOnly = modX.billsOnly,
					alwaysSmoke = modX.alwaysSmoke,
					driver = modX.driver,
					compClass = typeof(CompFlecker)
				};

				//Null checks
				if (props.fleckDef == null) props.fleckDef = RimWorld.FleckDefOf.Smoke;
				if (props.particleSize == 0) props.particleSize = 1f;

				CompFlecker comp = new CompFlecker()
				{
					parent = fleckerHolder,
					props = props,
					trueParent = __instance
				};

				registry.Add(__instance, comp);
				comp.PostSpawnSetup(false);
			}
		}

		[HarmonyPatch (typeof(Thing), nameof(Thing.DeSpawn))]
		static class Patch_DeSpawn
		{
			static void Prefix(ref Thing __instance)
			{
				if (usingExtensions && registry.TryGetValue(__instance, out CompFlecker comp))
				{
					comp.PostDeSpawn(__instance.Map);
					comp.parent.DeSpawn();
				}
			}
		}
	}
}