using Newtonsoft.Json.Linq;

namespace Cerpent.Core.Contract.Stereotype
{
    public class StereotypeCheckResult
    {
        public string StereotypeName { get; set; }
        public IEnumerable<StereotypeChartResult> ChartResults { get; set; }
        public int TriggerEventId { get; set; }
        public Dictionary<string, JToken> Context { get; set; }

        public bool IsConfirmed => ChartResults?.All(chart => chart.IsConfirmed) == true;
    }

    public class StereotypeChartResult
    {
        public string MetricName { get; set; }
        public int[] Ids { get; set; }
        public DateTime[] Dates { get; set; }
        public double?[] Metrics { get; set; }
        public double?[] UpperBounds { get; set; }
        public double?[] LowerBounds { get; set; }
        public string Accuracy { get; set; }
        private int GetMatchesCount()
        {
            var result = 0;
            for (var i = 0; i < Metrics.Length; i++)
            {
                var metric = Metrics[i];
                var upper = UpperBounds[i];
                var lower = LowerBounds[i];

                if (!metric.HasValue) continue;

                var upperMatch = !upper.HasValue || metric.Value < upper.Value;
                var lowerMatch = !lower.HasValue || metric.Value > lower.Value;

                if (upperMatch && lowerMatch)
                    result++;
            }
            
            return result;
        }

        public bool IsConfirmed
        {
            get
            {
                var matchesCount = GetMatchesCount();
                if (Accuracy.EndsWith('%'))
                {
                    var percentRequired = Convert.ToDouble(Accuracy.Replace("%", ""));
                    var percent = Metrics.Length * 100 / matchesCount;
                    return percent >= percentRequired;
                }

                var countRequired = Convert.ToInt32(Accuracy);
                return matchesCount >= countRequired;
            }
        }
    }
}
