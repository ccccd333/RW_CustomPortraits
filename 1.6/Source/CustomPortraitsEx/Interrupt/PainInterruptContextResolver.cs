using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx.Interrupt
{
    public class PainInterruptContextResolver
    {
        Pawn tracked_pawn;
        // 痛みの初期値
        float initial_pain_total = 0.0f;
        // 痛みの現在値
        float last_pain_total = 0.0f;

        public bool TryResolveInterruptContext(Pawn target_pawn, Dictionary<string, float> impact_map)
        {
            // 監視対象が切り替わった場合、値を控える
            if (tracked_pawn != target_pawn)
            {
                if (!ResetTracking(target_pawn))
                {
                    // 監視対象がないためそのまま返却
                    return false;
                }
            }

            if (tracked_pawn.Dead && tracked_pawn.health == null && tracked_pawn.health.hediffSet == null)
            {

                return false;
            }

            float now_value = tracked_pawn.health.hediffSet.PainTotal;
            if (now_value > last_pain_total)
            {
                Log.Message($"[PortraitsEx] PainInterruptContextResolver ADD ==> tracked_pawn {tracked_pawn} now_value {now_value} last_pain_total {last_pain_total}");

                // 痛みが発生した(ダメージを受けたか、持病の悪化など)
                last_pain_total = now_value;
                impact_map[PortraitContextKeys.PAIN_INCREASE] = 1.0f;
                return true;
            }
            else
            {
                last_pain_total = now_value;
            }

            return false;
        }

        private bool ResetTracking(Pawn target_pawn)
        {
            // そもそも死んでたり対象がいないなら痛みを監視しない
            // 公式がhealthとhediffSetのnullチェックしてるから一応入れとく
            if (target_pawn == null) {
                if(target_pawn.health == null && target_pawn.health.hediffSet == null && target_pawn.Dead) return false;
            }

            // ここからは監視対象のポーンが切り替わった場合の値を控える場所

            // 監視対象のポーンを控えておく
            tracked_pawn = target_pawn;
            // 監視対象のポーンの痛みの初期値を保管
            initial_pain_total = target_pawn.health.hediffSet.PainTotal;
            // 監視対象のポーンの痛みの一つ前の値(初期値からの増減を管理)を保管
            last_pain_total = initial_pain_total;
            return true;
        }
    }
}
