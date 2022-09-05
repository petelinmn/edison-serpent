using Cerpent.Core.Contract.Event;
using Newtonsoft.Json.Linq;

namespace Cerpent.MockPlatform;

public class AutoIncIdMockEvent : Event
{
    private static int _id = 0;

    public AutoIncIdMockEvent() : base()
    {
        Id = ++_id;
    }
}

public class MockEventSource : IEventSource<AutoIncIdMockEvent>
{
    private AutoIncIdMockEvent[] Events { get; }
    
    public MockEventSource(AutoIncIdMockEvent[] events)
    {
        Events = events;
    }

    public async Task<IEnumerable<AutoIncIdMockEvent>> Get(string name,
        Dictionary<string, JToken?>? contextDictionary,
        double? timeSpanInSec = null) =>
        await Task.Run(() => Events
            .Where(@event => @event.Name == name)
            .Where(@event =>
                !timeSpanInSec.HasValue ||
                timeSpanInSec.Value > (DateTime.Now - @event.DateTime).Seconds));

    public Task<int> Put(AutoIncIdMockEvent newMockEvent)
    {
        throw new NotImplementedException();
    }

    public Task Delete(int id)
    {
        throw new NotImplementedException();
    }
}
