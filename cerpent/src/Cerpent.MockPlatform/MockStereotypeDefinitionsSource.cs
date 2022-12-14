using Cerpent.Core.Contract.Stereotype;

namespace Cerpent.MockPlatform;

public class MockStereotypeDescriptionsSource : IStereotypeDescriptionSource
{
    private IEnumerable<StereotypeDescription> StereotypeDescriptions { get; set; }
    
    public MockStereotypeDescriptionsSource(IEnumerable<StereotypeDescription> stereotypeDescriptions)
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

    public Task Delete(int id)
    {
        throw new NotImplementedException();
    }
}
