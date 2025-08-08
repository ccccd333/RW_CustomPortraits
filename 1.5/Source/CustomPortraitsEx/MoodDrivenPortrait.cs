
using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
using Newtonsoft.Json.Linq;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using Verse;

namespace Foxy.CustomPortraits.CustomPortraitsEx
{

    public static class MoodDrivenPortrait
    {
        private static List<Texture2D> temp = new List<Texture2D>();
        private static int temp_index = 0;
        private static string temp_refs_key = "";
        private static bool temp_animation_mode = false;
        private static string temp_preset_name = "";
        private static float temp_display_duration = 2.0f;


        private static float last_update_time = Time.realtimeSinceStartup;
        private static float frame_interval = 0.1f;

        private static float disp_last_update_time = Time.realtimeSinceStartup;

        public static void Reset()
        {
            //temp.Clear();
            temp_index = 0;
            temp_refs_key = "";
            temp_animation_mode = false;
            temp_preset_name = "";
            temp_display_duration = PortraitCacheEx.Settings.DisplayDuration;
            last_update_time = Time.realtimeSinceStartup;
            disp_last_update_time = Time.realtimeSinceStartup;
        }



        public static Texture2D GetPortraitTexture(Pawn pawn, string filename, Texture2D def)
        {
            //Log.Message($"[PortraitsEx] Try Visible Portrait: {filename}");
            if (filename != null && filename != "")
            {
                string preset_name = "";
                //foreach (string ext in PortraitCacheEx.texture_type)
                //{
                //    if (filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                //    {
                //        preset_name = filename.Substring(0, filename.Length - ext.Length);
                //        break;
                //    }
                //}
                string d = "";
                preset_name = Utility.Delimiter(filename, out d);

                if (preset_name == "")
                {
                    // たぶんないとは思うけど。
                    return def;
                }

                bool nextPortrait = false;
                // ゲーム内時間だとFPSに依存してしまうのでUnityの内部タイマーでフレーム計算する
                float currentTime = Time.realtimeSinceStartup;
                if (currentTime - last_update_time >= frame_interval)
                {
                    // 大体60FPSで4フレーム目くらいで次の画像表示する
                    nextPortrait = true;
                    last_update_time = currentTime;
                }
                var mood_refs = PortraitCacheEx.MoodRefs;
                if (mood_refs.ContainsKey(preset_name))
                {

                    if (temp_preset_name != preset_name)
                    {
                        //Log.Message($"[PortraitsEx] temp_preset_name {temp_preset_name} preset_name {preset_name}");

                        // ポートレートが別々のポーンの場合、退避情報をクリアして、後続処理をする。
                        Reset();
                    }
                    else
                    {
                        //Log.Message($"[PortraitsEx] disp_last_update_time ==> {disp_last_update_time} currentTime ==> {currentTime} temp_display_duration ==> {temp_display_duration}");

                        // 毎回後続の重い処理を実行したくないのでjsonのdisplay_durationの間は退避した情報で
                        // アニメーションor画像表示を行う。
                        if (currentTime - disp_last_update_time <= temp_display_duration)
                        {
                            if (nextPortrait)
                            {
                                return AdvanceToNextPortrait(def);
                            }
                            else
                            {
                                return GetCurrentPortrait(def);
                            }
                        }
                        else
                        {
                            disp_last_update_time = Time.realtimeSinceStartup;
                        }
                    }

                    var refs = mood_refs[preset_name];

                    
                    string mood_name = "";

                    if (pawn.Dead)
                    {
                        // ポーンが死亡している場合thoughtsがnullを返すため、
                        if (refs.fallback_mood_on_death == "") return def;
                        // 死んでいる場合はjsonのfallback_mood_on_deathを使う。
                        mood_name = refs.fallback_mood_on_death;
                    }
                    else
                    {
                        Dictionary<string, float> mood = BuildMoodDictionary(pawn);
                        //List<Thought> outThoughts = new List<Thought>();
                        //pawn.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
                        //// 心情値の文字と値のリスト化
                        //foreach (var need in outThoughts)
                        //{
                        //    if (need == null || need.LabelCap == null)
                        //    {
                        //        // 豪華な宿舎みたいにstage[0]がnullのものがあったりする。
                        //        // なのでこれはそれ用
                        //        Log.Warning($"[PortraitsEx] WARN: need, LabelCap is null");
                        //        continue;
                        //    }

                        //    try
                        //    {
                        //        // TODO:心情値の値のほうで重みをつけるようにするかもしれない。
                        //        if (mood.ContainsKey(need.LabelCap))
                        //        {
                        //            float weight1 = mood[need.LabelCap];
                        //            float weight2 = need.MoodOffset();
                        //            if (weight1 < weight2) mood[need.LabelCap] = weight2;
                        //        }
                        //        else
                        //        {
                        //            mood.Add(need.LabelCap, need.MoodOffset());
                        //        }
                        //    }
                        //    catch (Exception e)
                        //    {
                        //        Log.Warning($"[PortraitsEx] WARN?(Processing will continue) Exception for need.LabelCap={need.LabelCap}: {e}");
                        //        mood.Add(need.LabelCap, 1.0f);
                        //    }

                        //}


                        //foreach(var k in refs.group_filter)
                        //{
                        //    Log.Message($"[PortraitsEx] refs.group_filter ==> Key {k.Key} Value {k.Value}");
                        //}

                        //foreach (var k in mood)
                        //{
                        //    Log.Message($"[PortraitsEx] mood ==> Key {k.Key} Value {k.Value}");
                        //}


                        // jsonのグループのキー(Group名)と値(心情値名)の値がmood(心情値の文字と値)のキーと一致する場合
                        // filtered_group_filterに一旦重複してもいいので入れていく。
                        var filtered_group_filter = FilterGroupMatches(refs, mood);
                        //foreach (var kvp in refs.group_filter)
                        //{
                        //    bool match_found = false;

                        //    if (refs.g_regex_cache.ContainsKey(kvp.Key))
                        //    {
                        //        var reg = refs.g_regex_cache[kvp.Key];
                        //        foreach (var mood_key in mood.Keys)
                        //        {
                        //            if (reg.IsMatch(mood_key))
                        //            {
                        //                match_found = true;
                        //                break;
                        //            }
                        //        }

                        //        if (match_found)
                        //        {
                        //            filtered_group_filter.Add(kvp);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (mood.ContainsKey(kvp.Key)) match_found = true;

                        //        if (match_found)
                        //        {
                        //            filtered_group_filter.Add(kvp);
                        //        }
                        //    }
                        //}

                        // 心情名一切ない(?)。
                        if (filtered_group_filter.Count() <= 0 && mood.Count() <= 0) return def;

                        
                        var matched_group = new Dictionary<string, List<string>>();

                        // filtered_group_filterをキー：値のものを、値：キーにしていく。
                        // 同時に重複した値を取り除いていく
                        foreach (var kvp in filtered_group_filter)
                        {
                            if (!matched_group.ContainsKey(kvp.Value))
                            {
                                matched_group[kvp.Value] = new List<string>();
                            }
                            matched_group[kvp.Value].Add(kvp.Key);

                            //Log.Message($"[PortraitsEx] aaaa mood: {kvp.Value} {kvp.Key}");
                        }

                        var merged_keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        // mood側を先に入れる（値はnull）
                        foreach (var kvp in mood)
                        {
                            //Log.Message($"[PortraitsEx] pic mood: {kvp.Key}");
                            merged_keys[kvp.Key] = null;
                        }

                        // group側を上書き（値を反映）
                        // これでmerged_keys=mood＋group(グループ名)という形になる。
                        foreach (var kvp in matched_group)
                        {
                            //Log.Message($"[PortraitsEx] pic group: {kvp.Key} relative mood ==> {kvp.Value}");
                            merged_keys[kvp.Key] = kvp.Key;
                        }

                        //foreach (var test in mergedKeys)
                        //{
                        //    Log.Message($"[PortraitsEx] bbb mood: {test.Key} {test.Value}");
                        //}

                        // jsonのpriority_weightsの上から順にとmerged_keysのキーと突き合わせて行く。
                        // priority_weightsと一致するもののみがmatched_priority_weightsに入る。
                        var matched_priority_weights = ExtractMatchedPriorityWeights(refs, merged_keys);
                        //    new Dictionary<string, PriorityWeights>(StringComparer.OrdinalIgnoreCase);
                        //foreach (var kvp in refs.priority_weights)
                        //{
                        //    bool match_found = false;

                        //    if (refs.pw_regex_cache.ContainsKey(kvp.Key))
                        //    {
                        //        var reg = refs.pw_regex_cache[kvp.Key];
                        //        var lis = new List<string>();

                        //        foreach (var mk in merged_keys)
                        //        {
                        //            if (reg.IsMatch(mk.Key))
                        //            {
                        //                lis.Add(mk.Key);
                        //                match_found = true;
                        //                break;
                        //            }
                        //        }

                        //        if (match_found)
                        //        {
                        //            foreach (var elm in lis)
                        //            {
                        //                if (!matched_priority_weights.ContainsKey(elm))
                        //                {
                        //                    matched_priority_weights.Add(elm, kvp.Value);
                        //                }
                        //            }
                        //        }
                        //    }
                        //    else
                        //    {
                        //        if (merged_keys.ContainsKey(kvp.Key)) match_found = true;

                        //        if (match_found)
                        //        {
                        //            if (!matched_priority_weights.ContainsKey(kvp.Key))
                        //            {
                        //                matched_priority_weights.Add(kvp.Key, kvp.Value);
                        //            }
                        //        }
                        //    }
                        //}


                        //refs.priority_weights
                        //.Where(kvp => merged_keys.ContainsKey(kvp.Key))
                        //.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        //int logc = 1;
                        //foreach (var mpw in matched_priority_weights)
                        //{
                        //    Log.Message($"[PortraitsEx] Matched Priority Weights priority: {logc} category ==> {mpw.Value.category} mood ==> {mpw.Value.filter_name} weight: {mpw.Value.weight}");
                        //    ++logc;
                        //}


                        if (matched_priority_weights.Count() <= 0 && refs.fallback_mood == "")
                        {
                            return def;
                        }
                        else if (matched_priority_weights.Count() > 0)
                        {
                            // matched_priority_weightsの始まりから順に優先となっているので、
                            // weightとランダム結果を比べて、weight以下だったらその名前を後続へ。
                            foreach (var elm in matched_priority_weights)
                            {
                                int weight = elm.Value.weight;
                                int seed = UnityEngine.Random.Range(0, 100);
                                Log.Message($"[PortraitsEx] name: {elm.Value.filter_name} seed: {seed} weight: {weight}");
                                if (seed < weight)
                                {
                                    mood_name = elm.Value.filter_name;

                                    break;
                                }
                            }
                        }

                        //Log.Message($"[PortraitsEx] mood_name: {mood_name} ");

                        // 抽出した心情値名がなければ、jsonのfallback_moodを使う。
                        if (mood_name == "")
                        {
                            if (refs.fallback_mood == "")
                            {
                                return def;
                            }
                            mood_name = refs.fallback_mood;
                        }
                    }
                    //Log.Message($"[PortraitsEx] mood_name2: {mood_name} ");

                    if (mood_name != temp_refs_key)
                    {
                        // 心情値名と既に退避済みの心情値名が一致しないとき
                        Reset();
                        string access_key = "";

                        if (refs.MatchDictKeysByRegex(mood_name, out access_key))
                        {
                            // 心情値名とjsonのmood_refsのキー名と一致するものがあるとき
                            // 次回から同じ重い処理しないようにするため、画像表示用の変数を退避する。
                            var txs = refs.txs;
                            var tt = txs[access_key];
                            temp = tt.txs;
                            temp_animation_mode = tt.IsAnimation;
                            temp_index = 0;
                            temp_preset_name = preset_name;
                            temp_refs_key = access_key;
                            temp_display_duration = tt.display_duration;
                            //Log.Message($"[PortraitsEx] preset_name {temp_preset_name} disp_d {temp_display_duration}");
                            if (temp.Count <= 0)
                            {
                                return ImageLoadError(preset_name, def);
                            }
                            else
                            {
                                if (temp.Count <= 0) return ImageLoadError(preset_name, def);

                                Texture2D texture = temp[temp_index];
                                if (temp_animation_mode) ++temp_index;

                                if (texture == null)
                                {
                                    Log.Error("[PortraitsEx] The image was successfully generated, but disappeared at the point when it was registered in the dictionary.");
                                    return ImageLoadError(preset_name, def);
                                }
                                return texture;
                            }
                        }
                        else
                        {
                            return ImageLoadError(preset_name, def);
                        }
                    }
                    else
                    {
                        if (nextPortrait)
                        {

                            return AdvanceToNextPortrait(def);
                        }
                        else
                        {

                            return GetCurrentPortrait(def);
                        }
                    }
                }
            }

            return def;
        }

        private static Texture2D AdvanceToNextPortrait(Texture2D def)
        {
            if (temp.Count <= 0) return def;
            if (temp_index < temp.Count)
            {
                Texture2D tx = temp[temp_index];
                if (temp_animation_mode) ++temp_index;
                return tx;
            }
            else
            {
                temp_index = 0;
                return temp[temp_index];
            }
        }

        private static Texture2D GetCurrentPortrait(Texture2D def)
        {
            if (temp.Count <= 0) return def;
            if (temp_animation_mode)
            {
                if (temp_index < temp.Count)
                {
                    return temp[temp_index];
                }
                else
                {
                    return temp[0];
                }
            }
            else
            {
                return temp[0];
            }
        }

        private static Dictionary<string, float> BuildMoodDictionary(Pawn pawn)
        {
            Dictionary<string, float> mood = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
            List<Thought> outThoughts = new List<Thought>();
            pawn.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);
            // 心情値の文字と値のリスト化
            foreach (var need in outThoughts)
            {
                if (need == null || need.LabelCap == null)
                {
                    // 豪華な宿舎みたいにstage[0]がnullのものがあったりする。
                    // なのでこれはそれ用
                    Log.Warning($"[PortraitsEx] WARN: need, LabelCap is null");
                    continue;
                }

                try
                {
                    // TODO:心情値の値のほうで重みをつけるようにするかもしれない。
                    if (mood.ContainsKey(need.LabelCap))
                    {
                        float weight1 = mood[need.LabelCap];
                        float weight2 = need.MoodOffset();
                        if (weight1 < weight2) mood[need.LabelCap] = weight2;
                    }
                    else
                    {
                        mood.Add(need.LabelCap, need.MoodOffset());
                    }
                }
                catch (Exception e)
                {
                    Log.Warning($"[PortraitsEx] WARN?(Processing will continue) Exception for need.LabelCap={need.LabelCap}: {e}");
                    mood.Add(need.LabelCap, 1.0f);
                }

            }

            return mood;
        }

        private static List<KeyValuePair<string, string>> FilterGroupMatches(Refs refs, Dictionary<string, float> mood)
        {
            List<KeyValuePair<string, string>> filtered_group_filter = new List<KeyValuePair<string, string>>();
            foreach (var kvp in refs.group_filter)
            {
                bool match_found = false;

                if (refs.g_regex_cache.ContainsKey(kvp.Key))
                {
                    var reg = refs.g_regex_cache[kvp.Key];
                    foreach (var mood_key in mood.Keys)
                    {
                        if (reg.IsMatch(mood_key))
                        {
                            match_found = true;
                            break;
                        }
                    }

                    if (match_found)
                    {
                        filtered_group_filter.Add(kvp);
                    }
                }
                else
                {
                    if (mood.ContainsKey(kvp.Key)) match_found = true;

                    if (match_found)
                    {
                        filtered_group_filter.Add(kvp);
                    }
                }
            }

            return filtered_group_filter;
        }

        private static Dictionary<string, PriorityWeights> ExtractMatchedPriorityWeights(Refs refs, Dictionary<string, string> merged_keys)
        {
            var matched_priority_weights = new Dictionary<string, PriorityWeights>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in refs.priority_weights)
            {
                bool match_found = false;

                if (refs.pw_regex_cache.ContainsKey(kvp.Key))
                {
                    var reg = refs.pw_regex_cache[kvp.Key];
                    var lis = new List<string>();

                    foreach (var mk in merged_keys)
                    {
                        if (reg.IsMatch(mk.Key))
                        {
                            lis.Add(mk.Key);
                            match_found = true;
                            break;
                        }
                    }

                    if (match_found)
                    {
                        foreach (var elm in lis)
                        {
                            if (!matched_priority_weights.ContainsKey(elm))
                            {
                                matched_priority_weights.Add(elm, kvp.Value);
                            }
                        }
                    }
                }
                else
                {
                    if (merged_keys.ContainsKey(kvp.Key)) match_found = true;

                    if (match_found)
                    {
                        if (!matched_priority_weights.ContainsKey(kvp.Key))
                        {
                            matched_priority_weights.Add(kvp.Key, kvp.Value);
                        }
                    }
                }
            }

            return matched_priority_weights;
        }


        private static Texture2D ImageLoadError(string preset_name, Texture2D def)
        {
            temp = new List<Texture2D>() { def };
            temp_animation_mode = false;
            temp_index = 0;
            temp_preset_name = preset_name;
            temp_refs_key = "def";
            temp_display_duration = PortraitCacheEx.Settings.DisplayDuration;

            return def;
        }
    }
}
