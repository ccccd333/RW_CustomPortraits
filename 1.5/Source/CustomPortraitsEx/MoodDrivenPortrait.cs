
using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
using Newtonsoft.Json.Linq;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        private static bool first_call = true;
        private static float temp_disp_frame_interval = 2.0f;


        private static float last_update_time = Time.realtimeSinceStartup;
        private static float frame_interval = 0.1f;

        private static float disp_last_update_time = Time.realtimeSinceStartup;

        public static void Reset()
        {
            //temp.Clear();
            temp_index = 0;
            temp_refs_key = "";
            temp_animation_mode = false;
            first_call = true;
            temp_preset_name = "";
            temp_disp_frame_interval = 2.0f;
            last_update_time = Time.realtimeSinceStartup;
            disp_last_update_time = Time.realtimeSinceStartup;
        }



        public static Texture2D GetPortraitTexture(Pawn pawn, string filename, Texture2D def)
        {
            if (first_call)
            {
                Init();
                first_call = false;
            }

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
                    return def;
                }

                bool nextPortrait = false;
                // ゲーム内時間だとFPSに依存してしまうのでOS時刻でフレーム計算する
                float currentTime = Time.realtimeSinceStartup;
                if (currentTime - last_update_time >= frame_interval)
                {
                    nextPortrait = true;
                    last_update_time = currentTime;
                }

                var mood_refs = PortraitCacheEx.MoodRefs;
                if (mood_refs.ContainsKey(preset_name))
                {

                    if (temp_preset_name != preset_name)
                    {
                        Reset();
                        disp_last_update_time = Time.realtimeSinceStartup;
                    }
                    else
                    {
                        if (currentTime - disp_last_update_time <= temp_disp_frame_interval)
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

                    List<Thought> outThoughts = new List<Thought>();
                    pawn.needs.mood.thoughts.GetAllMoodThoughts(outThoughts);

                    Dictionary<string, float> mood = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
                    foreach (var need in outThoughts)
                    {
                        mood.Add(need.LabelCap, need.MoodOffset());
                    }

                    var mergedKeys = new Dictionary<string, string>();

                    var matchedGroup = new Dictionary<string, List<string>>();

                    foreach (var kvp in refs.group_filter.Where(kvp => mood.ContainsKey(kvp.Key)))
                    {
                        if (!matchedGroup.ContainsKey(kvp.Value))
                        {
                            matchedGroup[kvp.Value] = new List<string>();
                        }
                        matchedGroup[kvp.Value].Add(kvp.Key);

                        //Log.Message($"[PortraitsEx] aaaa mood: {kvp.Value} {kvp.Key}");
                    }
                    // mood側を先に入れる（値はnull）
                    foreach (var kvp in mood)
                    {
                        //Log.Message($"[PortraitsEx] pic mood: {kvp.Key}");
                        mergedKeys[kvp.Key] = null;
                    }

                    // group_filter側を上書き（値を反映）
                    foreach (var kvp in matchedGroup)
                    {
                        //Log.Message($"[PortraitsEx] pic group: {kvp.Key} relative mood ==> {kvp.Value}");
                        mergedKeys[kvp.Key] = kvp.Key;
                    }

                    //foreach (var test in mergedKeys)
                    //{
                    //    Log.Message($"[PortraitsEx] bbb mood: {test.Key} {test.Value}");
                    //}

                    var matchedPriorityWeights = refs.priority_weights
                        .Where(kvp => mergedKeys.ContainsKey(kvp.Key))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    //int logc = 1;
                    //foreach (var mpw in matchedPriorityWeights)
                    //{
                    //    Log.Message($"[PortraitsEx] Matched Priority Weights priority: {logc} category ==> {mpw.Value.category} mood ==> {mpw.Value.filter_name} weight: {mpw.Value.weight}");
                    //    ++logc;
                    //}

                    string mood_name = "";

                    var list = matchedPriorityWeights.ToList();
                    int lastIndex = list.Count - 1;
                    var lastItem = list[lastIndex];

                    bool selected = false;
                    
                    for (int i = 0; i < lastIndex; i++)
                    {
                        int weight = list[i].Value.weight;
                        int seed = UnityEngine.Random.Range(0, 100);
                        if (seed < weight)
                        {
                            mood_name = list[i].Value.filter_name;
                            selected = true;

                            break;
                        }
                    }

                    if (!selected)
                    {
                        mood_name = lastItem.Value.filter_name;
                    }

                    if (mood_name == "") 
                    { 
                        if (refs.fallback_mood == "")
                        {
                            return def;
                        }
                        mood_name = refs.fallback_mood;
                    }

                    if (mood_name != temp_refs_key)
                    {
                        if (refs.txs.ContainsKey(mood_name))
                        {
                            var txs = refs.txs;
                            var tt = txs[mood_name];
                            temp = tt.txs;
                            temp_animation_mode = tt.IsAnimation;
                            temp_index = 0;
                            temp_preset_name = preset_name;
                            temp_refs_key = mood_name;
                            temp_disp_frame_interval = tt.frame_interval;
                            if (temp.Count <= 0)
                            {
                                return def;
                            }
                            else
                            {
                                if (temp.Count <= 0) return def;

                                Texture2D texture = temp[temp_index];
                                if (temp_animation_mode) ++temp_index;

                                if (texture == null)
                                {
                                    Log.Message("[PortraitsEx] The image was successfully generated, but disappeared at the point when it was registered in the dictionary.");
                                }
                                return texture;
                            }
                        }
                        else
                        {
                            return def;
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

        private static void Init()
        {
            first_call = false;
            last_update_time = Time.realtimeSinceStartup;
            disp_last_update_time = Time.realtimeSinceStartup;
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
    }
}
