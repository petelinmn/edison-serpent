using Cerpent.AWS.DB.Sources;
using Cerpent.Core.Contract.Event;
using Newtonsoft.Json.Linq;

namespace Cerpent.MockPlatform;

public class MockEventSource : IDbEventSource
{
    private Event[] Events { get; }
    
    public MockEventSource(Event[] events)
    {
        Events = events;
    }

    public async Task<IEnumerable<Event>> Get(string name,
        Dictionary<string, JToken?>? contextDictionary,
        double? timeSpanInSec = null) =>
        await Task.Run(() => Events
            .Where(@event => @event.Name == name)
            .Where(@event =>
                !timeSpanInSec.HasValue ||
                timeSpanInSec.Value > (DateTime.Now - @event.DateTime).Seconds));

    public Task<int> Put(Event newEvent)
    {
        throw new NotImplementedException();
    }
}