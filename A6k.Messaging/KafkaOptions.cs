using System.Collections.Generic;

namespace A6k.Messaging
{
    public class KafkaOptions
    {
        public Dictionary<string, string> Configuration { get; set; }
        public string Topic { get; set; }
    }
}
