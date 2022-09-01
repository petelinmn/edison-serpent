using Newtonsoft.Json.Linq;

namespace Cerpent.Core.Contract.Event
{
    public interface IEventSource<TEvent> where TEvent : Event, new()
    {
        Task<int> Put(TEvent newEvent);
        Task<IEnumerable<TEvent>> Get(string name, Dictionary<string,
            JToken?>? contextDictionary, double? timeSpanInSec = null);
        Task Delete(int id);
    }
}
