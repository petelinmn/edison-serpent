﻿using Newtonsoft.Json;

namespace Cerpent.Core.Contract.Stereotype
{
    public class StereotypeDescription : BaseEntity
    {
        public StereotypeDescription()
        {

        }
        public StereotypeDescription(string name, string triggerEvent,
            IDictionary<string, string> upperBounds, IDictionary<string, string> lowerBounds, string accuracy)
        {
            Name = name;
            TriggerEvent = triggerEvent;
            UpperBounds = upperBounds;
            LowerBounds = lowerBounds;
            Accuracy = accuracy;
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("triggerevent")]
        public string TriggerEvent { get; set; }
        [JsonProperty("upperbounds")]
        public IDictionary<string, string> UpperBounds { get; set; }
        [JsonProperty("lowerbounds")]
        public IDictionary<string, string> LowerBounds { get; set; }
        [JsonProperty("accuracy")]
        public string Accuracy { get; set; }
    }
}