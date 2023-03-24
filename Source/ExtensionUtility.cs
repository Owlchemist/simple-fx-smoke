using Verse;
using HarmonyLib;
using System.Collections.Generic;
using RimWorld.Planet;

namespace Flecker
{
	[StaticConstructorOnStartup]
	static class Setup
	{
		static Setup()
		{
			var list = DefDatabase<ThingDef>.AllDefsListForReading;
			for (int i = list.Count; i-- > 0;)
			{
				if (list[i].HasModExtension<Flecker>())
				{
					ExtensionUtility.usingExtensions = true;
					break;
				}
			}
			new Harmony("owlchemist.simplefx.smoke2").PatchAll();
		}
	}
	//The flecker extension is an alternative way to add properties to defs, specifically defs that don't support comps
	static class ExtensionUtility
	{
		public static bool usingExtensions;
		public static Dictionary<Thing, CompFlecker> registry = new Dictionary<Thing, CompFlecker>();
		public static FastRandom fastRandom = new FastRandom();

		[HarmonyPatch (typeof(Thing), nameof(Thing.SpawnSetup))]
		static class Patch_SpawnSetup
		{
			static bool Prepare()
			{
				return usingExtensions;
			}
			static void Postfix(ref Thing __instance, bool respawningAfterLoad)
			{
				Flecker modX = __instance.def.GetModExtension<Flecker>();
				if (modX == null) return;

				ThingWithComps fleckerHolder = new ThingWithComps()
				{
					def = ResourceBank.ThingDefOf.DummySmokeEmitter,
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
					trueParent = __instance,
					fireParent = __instance is RimWorld.Fire fire ? fire : null
				};

				if (!registry.ContainsKey(__instance)) registry.Add(__instance, comp);
				else registry[__instance] = comp;
				comp.PostSpawnSetup(false);
			}
		}

		[HarmonyPatch (typeof(Thing), nameof(Thing.DeSpawn))]
		static class Patch_DeSpawn
		{
			static bool Prepare()
			{
				return usingExtensions;
			}
			static void Prefix(ref Thing __instance)
			{
				if (registry.TryGetValue(__instance, out CompFlecker comp))
				{
					comp.PostDeSpawn(__instance.Map);
					comp.parent.Map?.listerThings.Add(comp.parent); //Add entity to the directory right before it's removed. This is only needed for autotesting.
					comp.parent.DeSpawn();
				}
			}
		}

		//Clear cache
		[HarmonyPatch (typeof(World), nameof(World.FinalizeInit))]
		static class Patch_FinalizeInit
		{
			static bool Prepare()
			{
				return usingExtensions;
			}
			static void Postfix()
			{
				registry.Clear();
			}
		}
	}
}