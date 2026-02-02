using Foxy.CustomPortraits.CustomPortraitsEx.Interrupt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx
{
    public static class PawnPortraitInterruptContext
    {
        static PainInterruptContextResolver pain_interrupt_context_resolver = new PainInterruptContextResolver();

        public static Dictionary<string, float> ComposeImpactMap(Pawn pawn, out bool is_value_fetched)
        {
            Dictionary<string, float> impact_map = new Dictionary<string, float>();
            // TODO:数が多くなってきたら監視のインターバルを作るかも、コストと相談
            pain_interrupt_context_resolver.TryResolveInterruptContext(pawn, impact_map);

            if (impact_map.Count > 0)
            {
                is_value_fetched = true;
            }
            else
            {
                is_value_fetched = false;
            }

            return impact_map;
        }
    }
}
