using Foxy.CustomPortraits.CustomPortraitsEx.Interrupt;
using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
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

        public static Dictionary<string, float> ComposeImpactMap(Pawn pawn, PortraitInterrupt interrupt, out bool is_value_fetched)
        {
            Dictionary<string, float> impact_map = new Dictionary<string, float>();
            // TODO:数が多くなってきたら監視のインターバルを作るかも、コストと相談

            if (interrupt.enabled_monitors[(int)MonitorType.PainIncrease])
            {
                pain_interrupt_context_resolver.TryResolveInterruptContext(pawn, impact_map);
            }

            if (interrupt.enabled_monitors[(int)MonitorType.Downed])
            {
                AppendDownedContext(pawn, impact_map);
            }

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

        // 以降特に値の保持による監視をする必要ない場合はここにメソッドを書いていく
        // 特定の値が必要ならInterrupt配下にクラスを作って

        // ダウン中
        public static void AppendDownedContext(Pawn pawn, Dictionary<string, float> impact_map)
        {
            //Log.Message($"[PortraitsEx] AppendDownedContext1 ==> downed?");
            bool downed = pawn?.health?.Downed ?? false;

            //Log.Message($"[PortraitsEx] AppendDownedContext2 ==> downed? {downed}");
            if (downed)
            {
                impact_map[PortraitContextKeys.DOWNED] = 1.0f;
            }
        }
    }
}
