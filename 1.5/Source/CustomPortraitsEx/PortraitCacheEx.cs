using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using Verse;
using Verse.Noise;
using static System.Net.Mime.MediaTypeNames;


namespace Foxy.CustomPortraits.CustomPortraitsEx
{

    public static class PortraitCacheEx
    {

        public static Dictionary<string, Refs> MoodRefs = new Dictionary<string, Refs>(StringComparer.OrdinalIgnoreCase);

        
        private static readonly string setting = "setting.json";

        private static DirectoryInfo RimWorldRootDirectory { get; } = new DirectoryInfo(GenFilePaths.ModsFolderPath).Parent;
        public static DirectoryInfo Directory { get; } = RimWorldRootDirectory.CreateSubdirectory("CustomPortraitsEx");

        public static DirectoryInfo PresetDirectory { get; } = Directory.CreateSubdirectory("Presets");

        public static void Update()
        {
            Log.Message($"[PortraitsEx] Updating cache from directory: {Directory.FullName}");
            if (!Directory.Exists) Directory.Create();
            ReadDirectory(Directory);
        }

        private static void ReadDirectory(DirectoryInfo directory)
        {
            Log.Message($"[PortraitsEx] Target directory: {PresetDirectory.FullName}");
            System.IO.FileInfo[] files = PresetDirectory.GetFiles("*.json", System.IO.SearchOption.TopDirectoryOnly);

            foreach (FileInfo file in files)
            {
                JObject root = JObject.Parse(File.ReadAllText(@file.FullName));
                string preset_name = root["preset_name"].ToString();
                Refs r = new MoodRefs();

                foreach (var token in root["mood"])
                {
                    var mood_prop = (JProperty)token;
                    string key = mood_prop.Name;
                    JToken value = mood_prop.Value;
                    try
                    {
                        if(key == "fallback_mood")
                        {
                            if (value is JValue fallback_mood)
                            {
                                r.fallback_mood = fallback_mood.Value.ToString();
                                
                            }
                            
                        }
                        else if (key == "mood_refs")
                        {
                            Refts(preset_name, key, value, r);
                        }
                        else if (key == "group")
                        {
                            Group(preset_name, key, value, r);
                        }
                        else if (key == "priority_weights")
                        {
                            PriorityWeights(preset_name, key, value, r);
                        }
                        else
                        {
                            throw new Exception("The preset JSON definition is incorrect." + preset_name);
                        }
                    }catch(Exception e)
                    {
                        throw new Exception("The preset JSON definition is incorrect." + preset_name + " [wt?]: " + e.Message);
                    }
                }
                Log.Message($"[PortraitsEx] Result ==> Target preset: {preset_name} MoodRefs Count: {r.txs.Count} Group Filter Count: {r.group_filter.Count} PriorityWeight Count: {r.priority_weights.Count}");
                MoodRefs.Add(preset_name, r);
            }
        }

        private static void Refts(string preset_name, string k, JToken n, Refs r)
        {
            Log.Message($"[PortraitsEx] Refts ==> Target preset: {preset_name}");

            foreach (var token in n)
            {
                var prop = (JProperty)token;

                string MoodRefs_key = prop.Name;

                //Log.Message($"[PortraitsEx] Refts ==> Target preset: {preset_name} ==> {MoodRefs_key}");
                JToken value = prop.Value;

                foreach (var token_n in value)
                {
                    var prop_n = (JProperty)token_n;
                    string cont = prop_n.Name;

                    //Log.Message($"[PortraitsEx] Refts ==> Target preset: {preset_name} ==> {MoodRefs_key} ==> {cont}");

                    if (cont == "textures")
                    {
                        var tx = Textures(preset_name, cont, prop_n.Value, r);
                        r.txs.Add(MoodRefs_key, tx);
                    }
                    
                }

            }
        }

        private static void Group(string preset_name, string k, JToken n, Refs r)
        {
            Log.Message($"[PortraitsEx] Group ==> Target preset: {preset_name}");

            foreach (var token in n)
            {
                var prop = (JProperty)token;

                string g_k = prop.Name;

                JToken value = prop.Value;

                foreach (var v in (JArray)prop.Value)
                {
                    var MoodRefs_key = v.ToString();
                    r.group_filter.Add(MoodRefs_key, g_k);

                    //Log.Message($"[PortraitsEx] Group ==> Target preset: {preset_name} Group Key ==> {g_k} Value ==> {MoodRefs_key}");
                }
            }
        }

        private static void PriorityWeights(string preset_name, string k, JToken n, Refs r)
        {
            Log.Message($"[PortraitsEx] PriorityWeights ==> Target preset: {preset_name}");

            foreach (var v in n)
            {
                PriorityWeights pw = new PriorityWeights();
                var obj = (JProperty)v;
                string MoodRefs_key = obj.Name;

                JToken wvalue = obj.Value;

                pw.filter_name = MoodRefs_key;
                foreach (var vvv in wvalue)
                {
                    var nw = (JProperty)vvv;
                    string nkey = nw.Name;
                    JToken nvalue = nw.Value;

                    if (nkey == "category")
                    {
                        if (nvalue is JValue category)
                        {
                            pw.category = (PriorityWeightCategory)Enum.ToObject(typeof(PriorityWeightCategory), category.Value<int>());
                        }
                    }
                    else if (nkey == "weight")
                    {

                        if (nvalue is JValue weight)
                        {
                            pw.weight = weight.Value<int>();
                        }
                    }
                    else
                    {
                        throw new Exception("The preset JSON definition is incorrect." + preset_name);
                    }
                }
                //Log.Message($"[PortraitsEx] PriorityWeights ==> Target preset: {preset_name} filter_name: {pw.filter_name} weight: {pw.weight}");
                r.priority_weights.Add(MoodRefs_key, pw);
            }

            
        }

        private static Textures Textures(string preset_name, string k, JToken n, Refs r)
        {
            Log.Message($"[PortraitsEx] Textures ==> Target preset: {preset_name}");

            Textures tx = new Textures();

            foreach (var token in n)
            {
                var prop = (JProperty)token;

                // todo:もし評価基準が増えればtypeとともに処理を作る
                string conf = prop.Name;
                if (conf == "animation_mode")
                {
                    if (prop.Value is JValue animation_mode)
                    {
                        tx.IsAnimation = animation_mode.Value<int>() == 0 ? false : true;
                    }
                }
                else if (conf == "frame_interval")
                {
                    if (prop.Value is JValue frame_interval)
                    {
                        tx.frame_interval = frame_interval.Value<float>();
                    }
                }
                else if (conf == "files")
                {

                    foreach (var v in (JArray)prop.Value)
                    {
                        string portrait_path = v.ToString();

                        if (portrait_path.Contains("~"))
                        {
                            string[] parts = portrait_path.Split('~');

                            string first = parts[0];
                            string second = parts[1];
                            string base_path = first.Substring(0, first.LastIndexOf('/') + 1);
                            string first_file = first.Substring(base_path.Length);
                            string second_file = second;
                            int range_from = 0;
                            int range_to = 0;
                            string d = "";
                            if (!int.TryParse(Utility.Delimiter(first_file, out d), out range_from))
                            {
                                throw new Exception($"The image could not be read. Please choose from png, jpeg, or jpg formats." + preset_name + "." + k);
                            }
                            if (!int.TryParse(Utility.Delimiter(second_file, out d), out range_to))
                            {
                                throw new Exception($"The image could not be read. Please choose from png, jpeg, or jpg formats." + preset_name + "." + k);
                            }

                            if (range_from > range_to)
                            {
                                int escp = range_to;
                                range_to = range_from;
                                range_from = escp;
                            }

                            for (; range_from <= range_to; range_from++)
                            {
                                string f = Directory.FullName + "/" + base_path + range_from.ToString() + d;

                                //Log.Message($"[PortraitsEx] Load Protraits: {f}");
                                byte[] data = File.ReadAllBytes(f);
                                Texture2D tex = new Texture2D(2, 2);
                                if (!tex.LoadImage(data))
                                {
                                    Log.Message($"[PortraitsEx] Portrait Load Error: {f}");
                                    throw new Exception($"Failed to load image. Processing will end." + preset_name + "." + k);
                                }
                                tx.txs.Add(tex);
                            }
                        }
                        else
                        {
                            string tx_filepath = Directory.FullName + "/" + v;
                            //Log.Message($"[PortraitsEx] Load Protraits: {tx_filepath}");
                            byte[] data = File.ReadAllBytes(tx_filepath);
                            Texture2D tex = new Texture2D(2, 2);
                            if (!tex.LoadImage(data))
                            {
                                Log.Message($"[PortraitsEx] Portrait Load Error: {tx_filepath}");
                                throw new Exception($"Failed to load image. Processing will end." + preset_name + "." + k);
                            }
                            tx.txs.Add(tex);
                        }
                    }
                }
                else
                {
                    throw new Exception("The preset JSON definition is incorrect." + preset_name + "." + k);
                }
            }

            return tx;
        }
    }
}
