
using Foxy.CustomPortraits.CustomPortraitsEx.Repository;
using System;
using System.Collections.Generic;

namespace Foxy.CustomPortraits.CustomPortraitsEx
{
    public class Refs
    {
        public Dictionary<string, Textures> txs = new Dictionary<string, Textures>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> group_filter = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, PriorityWeights> priority_weights = new Dictionary<string, PriorityWeights>(StringComparer.OrdinalIgnoreCase);
        public string fallback_mood = "";

        public virtual bool contain(string key) { return false; }
    }

    public class MoodRefs : Refs
    {
        
        // todo:Win32で無理やり音を鳴らしても、linuxとかが無理
        // rimworldならsound defが無難か。一旦やめておく。
        //public Dictionary<string, Sounds> sds = new Dictionary<string, Sounds>();
        public override bool contain(string key) { return txs.ContainsKey(key); }
    }
}