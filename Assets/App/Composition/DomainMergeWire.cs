#if AQ_ENABLE_DOMAIN_WIRE
using System;

namespace AQ.App
{
    public static class DomainMergeWire
    {
        public static void Configure(MergeService svc)
        {
            if (svc == null) return;

            // TODO: fill in with real Domain.MergeEngine + RecipeBook once ready.
            // svc.InjectDomainResolver((aLabel, bLabel) => { ... });
        }
    }
}
#endif