using Cerpent.Core.Contract.Event;
using Cerpent.Core.Contract.Stereotype;
using Cerpent.Core.Expression;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core.StereotypeRecognizer;

public class StereotypeRecognizer
{
    private IStereotypeDescriptionSource StereotypeDescriptionSource { get; }
    public StereotypeRecognizer(IStereotypeDescriptionSource stereotypeDescriptionSource)
    {
        StereotypeDescriptionSource = stereotypeDescriptionSource;
    }

    public async Task<IEnumerable<StereotypeCheckResult>> FuzzySearch(Event @event)
    {
        var chartsData = GetCharts(@event.Data);
        
        var descriptions = (await StereotypeDescriptionSource.Get(@event.Name)).ToList();

        var result = descriptions.Select(description =>
        {
            return new StereotypeCheckResult()
            {
                StereotypeName = description.Name,
                TriggerEventId = @event.Id,
                ChartResults = chartsData.Select(chartData =>
                {
                    var metricFunc = description.Metrics?.ContainsKey(chartData.Key) == true
                        ? description.Metrics[chartData.Key] : null;
                
                    var lowerBoundFunc = description.LowerBounds?.ContainsKey(chartData.Key) == true
                        ? description.LowerBounds[chartData.Key] : null;
                
                    var upperBoundFunc = description.UpperBounds?.ContainsKey(chartData.Key) == true
                        ? description.UpperBounds[chartData.Key] : null;
                    
                    var accuracy = description.Accuracy?.ContainsKey(chartData.Key) == true
                        ? description.Accuracy[chartData.Key] : null;
                    
                    var chartProps = chartData.Value.ToObject<Dictionary<string, JToken[]>>();
                
                    var ids = chartProps["Id"];
                    var dates = chartProps["Date"];
                    
                    var metricResult = new List<double?>();
                    var lowerResult = new List<double?>();
                    var upperResult = new List<double?>();

                    for (var i = 0; i < ids.Length; i++)
                    {
                        var args = new Dictionary<string, JToken>();
                        foreach (var key in chartProps.Keys)
                        {
                            var values = chartProps[key];
                            args.Add(key, values[i]);

                            var arrKey = $"list{key}";
                            if (metricFunc?.Contains(arrKey) == true
                                || lowerBoundFunc?.Contains(arrKey) == true 
                                || upperBoundFunc?.Contains(arrKey) == true)
                                args.Add(arrKey, JToken.FromObject(values));
                        }

                        var metricValue = metricFunc is null ? (double?)null 
                            : JSExpression.Calculate(metricFunc, args); 
                        var lowerValue = lowerBoundFunc is null ? (double?)null 
                            : JSExpression.Calculate(lowerBoundFunc, args);
                        var upperValue = upperBoundFunc is null ? (double?)null 
                            : JSExpression.Calculate(upperBoundFunc, args);

                        metricResult.Add(metricValue);
                        lowerResult.Add(lowerValue);
                        upperResult.Add(upperValue);
                    }
                    
                    return new StereotypeChartResult()
                    {
                        Accuracy = accuracy,
                        MetricName = description.Name,
                        Ids = ids.Select(d => d.ToObject<int>()).ToArray(),
                        Dates = dates.Select(d => d.ToObject<DateTime>()).ToArray(),
                        Metrics = metricResult.ToArray(),
                        UpperBounds = upperResult.ToArray(),
                        LowerBounds = lowerResult.ToArray(),
                    };
                }).ToList()
            };
        }).ToList();

        return result;
    }

    private const string Parents = "Parents";
    private Dictionary<string, JToken> GetCharts(JToken? token)
    {
        var result = new Dictionary<string, JToken>();
        
        if (token is null || token.Type != JTokenType.Object)
            return result;

        var parents = token[Parents];
        
        foreach (JProperty prop in parents)
        {
            var child = parents[prop.Name];
            if (child is null)
                continue;
            var hasParents = child.Children().Any(c => ((JProperty) c).Name == Parents) == true;

            if (hasParents)
            {
                foreach (var r in GetCharts(child))
                {
                    result.Add(r.Key, r.Value);
                }
            }
            else
            {
                result.Add(prop.Name, child);
            }
        }
        return result;
    }
}