using Verse;
using HarmonyLib;
using static Flecker.ExtensionUtility;
 
namespace Flecker
{
    public class Mod_Flecker : Mod
	{
		public Mod_Flecker(ModContentPack content) : base(content)
		{
			new Harmony(this.Content.PackageIdPlayerFacing).PatchAll();
			LongEventHandler.QueueLongEvent(() => Setup(), null, false, null);
		}
	}
}
