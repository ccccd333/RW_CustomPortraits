using Newtonsoft.Json;

namespace Foxy.CustomPortraits.CustomPortraitsEx
{
    public class PExSetting
    {
        public float display_duration { get; set; }
        public LogRetention recipient_log_retention { get; set; }
        public LogRetention initiator_log_retention { get; set; }
    }

    public class LogRetention
    {
        public int max_entries { get; set; }
        public float seconds { get; set; }
    }
}
