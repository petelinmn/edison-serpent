using Cerpent.Core.Contract.Event;
using Cerpent.Core.Contract.Stereotype;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core.StereotypeRecognizer;

public class StereotypeRecognizer
{
    private IStereotypeDescriptionSource StereotypeDescriptionSource { get; }
    public StereotypeRecognizer(IStereotypeDescriptionSource stereotypeDescriptionSource)
    {
        StereotypeDescriptionSource = stereotypeDescriptionSource;
    }

    public async Task<IEnumerable<StereotypeConfirmedResult>> FuzzySearch(Event @event)
    {
        var chartsData = GetCharts(@event.Data);
        
        var descriptions = (await StereotypeDescriptionSource.Get(@event.Name)).ToList();
        
        var result = descriptions.ToList().Where(description =>
        {
            if (description is null || description.Accuracy == "0"
                || (description.LowerBounds == null && description.UpperBounds == null))
                return true;

            foreach (var chartData in chartsData)
            {
                var lowerBoundFunc = description.LowerBounds.ContainsKey(chartData.Key)
                    ? description.LowerBounds[chartData.Key] : null;
                
                var upperBoundFunc = description.UpperBounds.ContainsKey(chartData.Key)
                    ? description.UpperBounds[chartData.Key] : null;
                
                var dict = chartData.Value.ToObject<Dictionary<string, JToken[]>>();
                var ids = dict["Id"];
                for (var i = 0; i < ids.Length; i++)
                {
                    var args = new Dictionary<string, JToken>();
                    foreach (var key in dict.Keys)
                    {
                        args.Add(key, dict[key][i]);

                        var arrKey = $"list{key}";
                        if (lowerBoundFunc?.Contains(arrKey) == true || upperBoundFunc?.Contains(arrKey) == true)
                            args.Add(arrKey, JToken.FromObject(dict[key]));
                    }
                }
            }

            return true;
        });
        
        return result.Select(steretype =>
        {
            return new StereotypeConfirmedResult
            {
                Name = steretype.Name
            };
        });
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