using Newtonsoft.Json.Linq;

namespace Cerpent.Core.Contract.Stereotype
{
    public class StereotypeCheckResult
    {
        public string? StereotypeName { get; set; }
        public IEnumerable<StereotypeChartResult>? ChartResults { get; set; }
        public int TriggerEventId { get; set; }
        public JToken? Context { get; set; }

        public bool IsConfirmed => ChartResults?.All(chart => chart.IsConfirmed) == true;
    }

    public class StereotypeChartResult
    {
        public string? MetricName { get; set; }
        public int[] Ids { get; set; } = Array.Empty<int>();
        public DateTime[] Dates { get; set; } = Array.Empty<DateTime>();
        public double?[] Metrics { get; set; } = Array.Empty<double?>();
        public double?[] UpperBounds { get; set; } = Array.Empty<double?>();
        public double?[] LowerBounds { get; set; } = Array.Empty<double?>();
        public string? Accuracy { get; set; }

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
                if (Accuracy is null)
                    return true;
                
                var matchesCount = GetMatchesCount();
                if (matchesCount == 0)
                    return false;

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
