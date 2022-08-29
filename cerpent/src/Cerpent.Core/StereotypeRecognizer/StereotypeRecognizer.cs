using Cerpent.Core.Contract;
using Cerpent.Core.Contract.Stereotype;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core.StereotypeRecognizer;

public class StereotypeRecognizer
{
    private IStereotypeDefinitionSource StereotypeDefinitionSource { get; }
    public StereotypeRecognizer(IStereotypeDefinitionSource stereotypeDefinitionSource)
    {
        StereotypeDefinitionSource = stereotypeDefinitionSource;
    }

    public async Task<IEnumerable<StereotypeConfirmedResult>> FuzzySearch(Event @event)
    {
        var parents = @event.Data["Parents"];

        var charts = GetCharts(@event.Data);
        
        
        var descriptions = (await StereotypeDefinitionSource.Get(@event.Name)).ToList();
        
        var result = descriptions.ToList().Where(description =>
        {
            if (description.Accuracy == "0" || (description.LowerBounds == null && description.UpperBounds == null))
                return true;

            
            return true;
        });

        var r = result.ToList();
        
        return result.Select(steretype =>
        {
            return new StereotypeConfirmedResult
            {
                Name = steretype.Name
            };
        });
    }

    private List<JToken> GetCharts(JToken? token)
    {
        var result = new List<JToken>();
        
        if (token is null)
            return result;

        foreach (var node in token)
        {
            if (node is null)
                continue;

            var children = node.Children().ToList();
            
            if (children.Any(c =>
                {
                    var t = c;
                    return true;
                }))
                result.Add(node);
            else
            {
                var nodes = GetCharts(node);
                result.AddRange(nodes);
            }
                
            //IN Development
        }
        return result;
    }
}