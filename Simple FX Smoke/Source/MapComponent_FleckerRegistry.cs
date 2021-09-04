using Verse;
using System.Collections.Generic;

namespace Flecker
{
	public class MapComponent_FleckerRegistry : MapComponent
	{
		
        public List<CompFlecker> fleckerRegistry;
        public MapComponent_FleckerRegistry(Map map) : base(map)
        {
            this.fleckerRegistry = new List<CompFlecker>();
        }

        public override void MapComponentTick()
        {
            if (Find.TickManager.TicksGame % 30 != 0) return;
            foreach (var flecker in fleckerRegistry)
            {
                if (flecker.Props.alwaysSmoke || (flecker.Props.billsOnly && flecker.InUse))
		    	{
			    	flecker.ThrowFleck();
			    	return;
			    }
                if (flecker.fuelComp != null && flecker.fuelComp.HasFuel && !flecker.Props.billsOnly && (flecker.flickComp == null || flecker.flickComp.SwitchIsOn))
                {
                    flecker.ThrowFleck();
                }
            }
        }
    }
}