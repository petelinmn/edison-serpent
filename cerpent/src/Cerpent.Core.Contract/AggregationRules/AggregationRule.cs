using Newtonsoft.Json;

namespace Cerpent.Core.Contract.AggregationRules
{
    public class AggregationRule
    {
        public AggregationRule()
        {

        }

        public AggregationRule(string name, IDictionary<string, int> atomics,
            IEnumerable<string> contextFields, string? condition, double? timeSpan)
        {
            Name = name;
            Atomics = atomics;
            ContextFields = contextFields.ToList();
            Condition = condition;
            TimeSpan = timeSpan;
        }

        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("name")]
        public string? Name { get; set; }
        /// <summary>
        /// Event names those are triggers for rule
        /// </summary>
        [JsonProperty("atomics")]
        public IDictionary<string, int>? Atomics { get; set; }
        [JsonProperty("fields")]
        public IEnumerable<string>? ContextFields { get; set; }
        [JsonProperty("condition")]
        public string? Condition { get; set; }
        [JsonProperty("timespan")]
        public double? TimeSpan { get; set; }
    }

}
