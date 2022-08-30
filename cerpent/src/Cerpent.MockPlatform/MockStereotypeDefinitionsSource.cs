using Cerpent.Core.Contract;
using Cerpent.Core.Contract.Stereotype;
using Newtonsoft.Json.Linq;

namespace Cerpent.MockPlatform;

public class MockStereotypeDefinitionsSource : IStereotypeDefinitionSource
{
    private IEnumerable<StereotypeDescription> StereotypeDescriptions { get; set; }
    
    public MockStereotypeDefinitionsSource(IEnumerable<StereotypeDescription> stereotypeDescriptions)
    {
        StereotypeDescriptions = stereotypeDescriptions;
    }

    public async Task<IEnumerable<StereotypeDescription>> Get(string triggerEventName) =>
        await Task.Run(() =>
            StereotypeDescriptions.Where(rule => rule.TriggerEvent == triggerEventName));

    public async Task<int> Put(StereotypeDescription stereotype)
    {
        throw new NotImplementedException();
    }
}