using Foxy.CustomPortraits.CustomPortraitsEx.Repository.VariantsHelperClass;
using System;
using System.Collections.Generic;

namespace Foxy.CustomPortraits.CustomPortraitsEx.Repository
{

    public class VariantEvaluationGroup
    {
        
        public int repeat_range_min = -1;
        public int repeat_range_max = -1;
        // repeat_range_min～maxの範囲で、同じポートレートコンテキストが選ばれるんだけど、
        // exception_contextsに含まれるものが選ばれた場合は異なるポートレートコンテキストが選ばれるようにする
        public List<string> exception_contexts = new List<string>();
        public List<OperationBase> operation_list = new List<OperationBase>();
    }


    // 例えばIdleが選出された際に、
    // 更にまたIdleが選出されたら10%の確立で"ずぶ濡れ"の画像に(Idle状態のままで)、
    // 20%の確立で"Idle2"の画像に、"死んだ"が成立していたら"Idle2"の画像を表示みたいな
    // 要はパチンコの演出みたいな感じ
    public class Variants
    {
        public bool is_enabled = false;
        // 以下のようなイメージ(keyが(string, int)になってるのだけ後で読み返した際の注意)
        // key = ポートレートコンテキスト名(idleとか)
        // value = key= idleが選ばれた後再度idleが選ばれた際、番号指定で挙動を変えるためのindex番号
        //         value=オペレーション、jsonの定義内容を上から順に格納する
        // "idle" : {
        //    "1" : [ { "operation_type" : "rand_value", "inequality_sign" : "more", "value" : 50, "result_context_name" : "idle2" } ],
        //        ※これは、idleが選ばれた後、再度idleが選ばれた際に、50%の確率でidle2に変化するという意味
        //    "2" : [ { "operation_type" : "rand_value", "inequality_sign" : "more", "value" : 20, "result_context_name" : "ずぶ濡れ" } ]
        //        ※これは、またまたidleが選ばれた際に、20%の確率でwet_idleに変化するという意味

        private readonly Dictionary<(string context, int index), VariantEvaluationGroup> operations =
            new Dictionary<(string, int), VariantEvaluationGroup>();
        public void SetOperations(string context_name, int index, VariantEvaluationGroup operation_list)
        {
            operations[(context_name, index)] = operation_list;
        }

        public bool TryGetOperations(string context_name, int index, out VariantEvaluationGroup result)
        {
            if (operations.TryGetValue((context_name, index), out var list))
            {
                result = list;
                return true;
            }

            result = null;
            return false;
        }

        public bool TryGetOperationsForRepeat(string context_name, int repeat_index, out VariantEvaluationGroup result)
        {
            if (TryGetOperations(context_name, repeat_index, out result))
            {
                return true;
            }

            foreach (var kvp in operations)
            {
                if (kvp.Key.context != context_name)
                {
                    continue;
                }

                var group = kvp.Value;
                if (group.repeat_range_min <= repeat_index && repeat_index <= group.repeat_range_max)
                {
                    result = group;
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
