using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core.Contract.Event
{
    public class Event : BaseEntity
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("datetime")]
        public DateTime DateTime { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }
    }
}
