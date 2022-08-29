using Newtonsoft.Json;

namespace Cerpent.Core.Contract
{
    public class BaseEntity
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
