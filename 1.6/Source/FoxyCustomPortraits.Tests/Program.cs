using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FoxyCustomPortraits.Tests
{
    public enum OperationType
    {
        portrait_context_name,
        rand_value
    }

    public enum InequalitySign
    {
        more, less, more_than, less_than, equal, def, unk
    }

    public class MultTypeValue { }

    public class Operation
    {
        public OperationType operation_type;
        public InequalitySign inequality_sign;
        public string operation_base_value;
        public string override_portrait_name;
    }

    public readonly struct ValidationContext
    {
        public List<string> ValidContextNames { get; }
        public ValidationContext(List<string> valid_context_names)
        {
            ValidContextNames = valid_context_names;
        }
    }

    public readonly struct EvaluationArgs
    {
        public List<string> ActiveContexts { get; }
        public EvaluationArgs(List<string> active_contexts = null)
        {
            ActiveContexts = active_contexts;
        }
    }

    public abstract class OperationBase
    {
        public abstract bool Init(Operation op, ValidationContext vc);
        public abstract bool Evaluate(EvaluationArgs arg);
        public abstract string ResolveOverrideContext();
    }

    public class PortraitContextName : OperationBase
    {
        Operation operation;
        public override bool Init(Operation op, ValidationContext vc)
        {
            operation = op;
            return true;
        }
        public override bool Evaluate(EvaluationArgs arg)
        {
            return arg.ActiveContexts.Contains(operation.operation_base_value);
        }
        public override string ResolveOverrideContext()
        {
            return operation.override_portrait_name;
        }
    }

    public class RandValue : OperationBase
    {
        Operation operation;
        int parsed_value;
        private static Random rand = new Random(42);
        public override bool Init(Operation op, ValidationContext vc)
        {
            operation = op;
            int.TryParse(operation.operation_base_value, out parsed_value);
            return true;
        }
        public override bool Evaluate(EvaluationArgs arg)
        {
            int random_value = rand.Next(0, 100);
            return random_value < parsed_value;
        }
        public override string ResolveOverrideContext()
        {
            return operation.override_portrait_name;
        }
    }

    public class RepeatLoopSettings
    {
        public int min_count = -1;
        public int max_count = -1;
        public List<string> interrupt_contexts = new List<string>();
    }

    public class RepeatEvaluationGroup
    {
        public List<string> interrupt_contexts = new List<string>();
        public List<OperationBase> operation_list = new List<OperationBase>();
    }

    public class RepeatRules
    {
        public bool is_enabled = false;
        private readonly Dictionary<string, RepeatLoopSettings> loop_settings_by_context = new Dictionary<string, RepeatLoopSettings>();
        private readonly Dictionary<(string context, int index), RepeatEvaluationGroup> operations = new Dictionary<(string, int), RepeatEvaluationGroup>();

        public void SetLoopSettings(string context_name, RepeatLoopSettings settings)
        {
            loop_settings_by_context[context_name] = settings;
        }

        public bool TryGetLoopSettings(string context_name, out RepeatLoopSettings result)
        {
            return loop_settings_by_context.TryGetValue(context_name, out result);
        }

        public void SetOperations(string context_name, int index, RepeatEvaluationGroup operation_list)
        {
            operations[(context_name, index)] = operation_list;
        }

        public bool TryGetOperations(string context_name, int index, out RepeatEvaluationGroup result)
        {
            return operations.TryGetValue((context_name, index), out result);
        }

        public bool TryResolveVariantContext(
            string portrait_context_name,
            int repeat_index,
            List<string> candidate_context_names,
            string previous_context_result,
            out string resolved_context_name,
            out bool should_increment_repeat)
        {
            resolved_context_name = portrait_context_name;
            should_increment_repeat = false;

            if (!TryGetOperations(portrait_context_name, repeat_index, out var group))
            {
                return false;
            }

            if (group.interrupt_contexts.Contains(portrait_context_name))
            {
                return true;
            }

            var args = new EvaluationArgs(candidate_context_names);
            foreach (var op in group.operation_list)
            {
                if (op.Evaluate(args))
                {
                    var override_ctx = op.ResolveOverrideContext();
                    if (!string.IsNullOrEmpty(override_ctx))
                    {
                        resolved_context_name = override_ctx;
                        break;
                    }
                }
            }

            should_increment_repeat = previous_context_result == resolved_context_name;
            return true;
        }

        public int TryApplyLoopRepeatEvent(
            string bef_context_name,
            string portrait_context_name,
            int repeat_index,
            List<string> candidate_context_names,
            out string resolved_context_name)
        {
            resolved_context_name = portrait_context_name;

            if (!TryGetLoopSettings(bef_context_name, out var loop_settings))
            {
                return 0;
            }

            if (loop_settings.interrupt_contexts.Contains(portrait_context_name))
            {
                return 0;
            }

            if (loop_settings.min_count > repeat_index)
            {
                return 0;
            }

            if (repeat_index > loop_settings.max_count)
            {
                return -1;
            }

            resolved_context_name = bef_context_name;

            if (!TryGetOperations(bef_context_name, repeat_index, out var group))
            {
                return 1;
            }

            var args = new EvaluationArgs(candidate_context_names);
            foreach (var op in group.operation_list)
            {
                if (op.Evaluate(args))
                {
                    var override_ctx = op.ResolveOverrideContext();
                    if (!string.IsNullOrEmpty(override_ctx))
                    {
                        resolved_context_name = override_ctx;
                        return 1;
                    }
                }
            }

            return 1;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting repeat_rules test (standalone)...");

            string json = @"
{
  ""idle"": {
    ""loop"": {
      ""min_count"": 1,
      ""max_count"": 5,
      ""interrupt_contexts"": [
        ""idle3""
      ]
    },
    ""repeat_events"": {
      ""0"": {
        ""operations"": [
          {
            ""operation_type"": ""rand_value"",
            ""operation_base_value"": ""100"",
            ""override_portrait_name"": ""idle2""
          }
        ]
      },
      ""1"": {
        ""operations"": [
          {
            ""operation_type"": ""portrait_context_name"",
            ""operation_base_value"": ""wet skin"",
            ""override_portrait_name"": ""idle_wet""
          }
        ],
        ""interrupt_contexts"": [
          ""idle3""
        ]
      }
    }
  }
}";

            RepeatRules repeat_rules = new RepeatRules();
            repeat_rules.is_enabled = true;

            List<string> validNames = new List<string> { "idle", "idle2", "idle_wet", "idle3", "wet skin", "sad" };
            ValidationContext vc = new ValidationContext(validNames);

            JObject root = JObject.Parse(json);
            foreach (var contextToken in root)
            {
                string contextName = contextToken.Key;
                JObject repeatObject = (JObject)contextToken.Value;

                if (repeatObject.TryGetValue("loop", out JToken loopToken) && loopToken is JObject loopObject)
                {
                    RepeatLoopSettings loopSettings = new RepeatLoopSettings();
                    if (loopObject.TryGetValue("min_count", out JToken minToken))
                        loopSettings.min_count = minToken.Value<int>();
                    if (loopObject.TryGetValue("max_count", out JToken maxToken))
                        loopSettings.max_count = maxToken.Value<int>();
                    if (loopObject.TryGetValue("interrupt_contexts", out JToken intrToken) && intrToken is JArray intrArray)
                    {
                        foreach (var item in intrArray)
                            loopSettings.interrupt_contexts.Add(item.ToString());
                    }
                    repeat_rules.SetLoopSettings(contextName, loopSettings);
                }

                if (repeatObject.TryGetValue("repeat_events", out JToken repeatEventsToken) && repeatEventsToken is JObject repeatEventsObject)
                {
                    foreach (var repeatToken in repeatEventsObject)
                    {
                        int repeatIndex = int.Parse(repeatToken.Key);
                        RepeatEvaluationGroup group = LoadRepeatEvaluationGroup(repeatToken.Value, vc);
                        repeat_rules.SetOperations(contextName, repeatIndex, group);
                    }
                }
            }

            Console.WriteLine("JSON loaded successfully.");

            Console.WriteLine("\n--- SIMULATION 1: Enter 'idle' repeatedly with 'wet skin' active context ---");
            RunSimulation(repeat_rules, new List<SimulationInput>
            {
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
            });

            Console.WriteLine("\n--- SIMULATION 2: Enter 'idle' then change to 'sad' then back to 'idle' ---");
            RunSimulation(repeat_rules, new List<SimulationInput>
            {
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("sad", new List<string>()),
                new SimulationInput("sad", new List<string>()),
                new SimulationInput("idle", new List<string> { "wet skin" }),
            });

            Console.WriteLine("\n--- SIMULATION 3:  ---");
            RunSimulation(repeat_rules, new List<SimulationInput>
            {
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
                new SimulationInput("idle3", new List<string> { "wet skin" }),
                new SimulationInput("idle", new List<string> { "wet skin" }),
            });
        }

        static RepeatEvaluationGroup LoadRepeatEvaluationGroup(JToken groupToken, ValidationContext vc)
        {
            RepeatEvaluationGroup group = new RepeatEvaluationGroup();
            JToken operationsToken = groupToken;

            if (groupToken is JObject groupObject)
            {
                if (groupObject.TryGetValue("interrupt_contexts", out JToken intrToken) && intrToken is JArray intrArray)
                {
                    foreach (var item in intrArray)
                        group.interrupt_contexts.Add(item.ToString());
                }
                if (groupObject.TryGetValue("operations", out JToken ops))
                    operationsToken = ops;
            }

            JArray opArray = (JArray)operationsToken;
            foreach (var opToken in opArray)
            {
                JObject opObj = (JObject)opToken;
                Operation op = new Operation();
                op.operation_type = (OperationType)Enum.Parse(typeof(OperationType), opObj.Value<string>("operation_type"));
                op.operation_base_value = opObj.Value<string>("operation_base_value");
                op.override_portrait_name = opObj.Value<string>("override_portrait_name");

                OperationBase opInstance = null;
                if (op.operation_type == OperationType.portrait_context_name)
                    opInstance = new PortraitContextName();
                else if (op.operation_type == OperationType.rand_value)
                    opInstance = new RandValue();

                opInstance.Init(op, vc);
                group.operation_list.Add(opInstance);
            }
            return group;
        }

        struct SimulationInput
        {
            public string context_name;
            public List<string> candidate_contexts;

            public SimulationInput(string ctx, List<string> cand)
            {
                context_name = ctx;
                candidate_contexts = cand;
            }
        }

        static void RunSimulation(RepeatRules repeat_rules, List<SimulationInput> inputs)
        {
            string repeat_base_context = null;
            int repeat_count = 0;
            string pending_context_result = null;

            int step = 1;
            foreach (var input in inputs)
            {
                string portrait_context_name = input.context_name;
                List<string> candidate_context_names = input.candidate_contexts;
                bool is_resolved = true;
                bool is_repeat = false;

                Console.WriteLine($"[Step {step}] Input Context: '{portrait_context_name}', Candidates: [{string.Join(", ", candidate_context_names)}]");
                Console.WriteLine($"   Before: repeat_count = {repeat_count}, repeat_base_context = '{(repeat_base_context ?? "null")}'");

                // repeat_rulesの事前評価(主にリピート)
                if (repeat_rules.is_enabled)
                {
                    if (repeat_base_context != null)
                    {
                        int repeat_index = repeat_count;
                        int loop_result = repeat_rules.TryApplyLoopRepeatEvent(
                            repeat_base_context,
                            portrait_context_name,
                            repeat_index,
                            candidate_context_names,
                            out var loop_resolved_context_name);

                        if (loop_result == 1)
                        {
                            pending_context_result = loop_resolved_context_name;
                            is_repeat = true;
                            ++repeat_count;
                        }
                        else if (loop_result == -1)
                        {
                            repeat_count = 0;
                        }
                    }
                }

                // repeat_rulesの事後評価
                if (!is_repeat && is_resolved && repeat_rules.is_enabled)
                {
                    // 同じコンテキストかどうかを判定する (前回の元のコンテキスト名 repeat_base_context と今回の元のコンテキスト名 portrait_context_name が同じか)
                    bool is_same_context = (repeat_base_context != null && portrait_context_name == repeat_base_context);
                    if (!is_same_context)
                    {
                        repeat_count = 0;
                    }

                    int repeat_index = repeat_count;
                    if (repeat_rules.TryResolveVariantContext(
                        portrait_context_name,
                        repeat_index,
                        candidate_context_names,
                        repeat_base_context,
                        out var resolved_context_name,
                        out var should_increment_repeat))
                    {
                        if (is_same_context)
                        {
                            repeat_count++;
                        }
                        else
                        {
                            repeat_count = 1;
                            repeat_base_context = portrait_context_name;
                        }

                        portrait_context_name = resolved_context_name;
                    }
                    else
                    {
                        // 該当する repeat_rules 操作がない場合
                        if (!is_same_context)
                        {
                            repeat_count = 1;
                            repeat_base_context = portrait_context_name;
                        }
                        else
                        {
                            // 同じコンテキスト名が続いているが、リピートイベントが存在しない場合
                            repeat_count++;
                        }
                    }

                    pending_context_result = portrait_context_name;
                }

                Console.WriteLine($"   After:  repeat_count = {repeat_count}, repeat_base_context = '{(repeat_base_context ?? "null")}', Result: '{pending_context_result}'");
                Console.WriteLine();
                step++;
            }
        }
    }
}
