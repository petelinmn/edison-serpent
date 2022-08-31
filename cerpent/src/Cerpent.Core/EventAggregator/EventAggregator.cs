using Cerpent.AWS.DB.Sources;
using Cerpent.Core.Contract;
using Cerpent.Core.Contract.Event;
using Newtonsoft.Json.Linq;

namespace Cerpent.Core;

public class EventAggregator
{
    private IDbEventSource EventSource { get; set; }
    private IAggregationRuleSource AggregationRuleSource { get; set; }

    public EventAggregator(IDbEventSource eventSource, IAggregationRuleSource aggregationRuleSource)
    {
        EventSource = eventSource;
        AggregationRuleSource = aggregationRuleSource;
    }

    private async Task<AggregationRule[]> GetRules(string triggerEventName) =>
        (await AggregationRuleSource.Get(triggerEventName))
            .Where(rule => rule.Atomics?.ContainsKey(triggerEventName) == true)
            .ToArray();

    public async Task<IEnumerable<Event>> Aggregate(Event triggerAtomicEvent)
    {
        var rules = await GetRules(triggerAtomicEvent.Name);

        var dataDictionary = triggerAtomicEvent.Data;

        var contextDictionary = rules
            .Select(rule => rule.ContextFields)
            .SelectMany(contextFields => contextFields)
            .Distinct()
            .Where(field => rules.All(rule => rule.ContextFields.Contains(field)))
            .ToDictionary(field => field, field => dataDictionary[field]);

        var atomicEvents = rules
            .Select(rule => rule.Atomics)
            .SelectMany(queue => queue)
            .Distinct()
            .Select(a => a.Key)
            .ToArray();

        var timeSpan = rules.MaxBy(rule => rule.TimeSpan)?.TimeSpan ?? 3600;

        List<Event> eventList = new List<Event>();

        foreach (var atomicEvent in atomicEvents)
        {
            var events = (await EventSource.Get(atomicEvent, contextDictionary, timeSpan)).ToList();
            eventList.AddRange(events);
        }

        eventList = eventList
            .Where(e => contextDictionary
                .All(c => e.Data[c.Key]?.ToString() == c.Value?.ToString()))
            .ToList();
        
        if (eventList.All(e => e.Id != triggerAtomicEvent.Id))
            eventList.Add(triggerAtomicEvent);

        eventList = eventList.OrderByDescending(o => o.DateTime).ToList();

        var newEvents = new List<Event>();
        foreach (var rule in rules)
        {
            var ruleEvents = eventList
                .Where(e => rule.Atomics?.ContainsKey(e.Name) == true)
                .ToArray();
            
            if (rule.Atomics.Any(atomic =>
                    ruleEvents.Count(ruleEvent => ruleEvent.Name == atomic.Key) < atomic.Value))
                continue;

                       /*var newEventData = rule.Atomics.ToDictionary(atomic => atomic.Key,
                atomic => ruleEvents
                    .Where(ruleEvent => ruleEvent.Name == atomic.Key).Take(atomic.Value)
                    .Select(ruleEvent => ruleEvent?.Data)
                    .ToArray());*/

            var newEventData = rule.Atomics.ToDictionary(atomic => atomic.Key,
                atomic =>
                {
                    var currentAtomicEvents = ruleEvents
                        .Where(ruleEvent => ruleEvent.Name == atomic.Key).Take(atomic.Value)
                        .ToArray();

                    var properties = currentAtomicEvents.Select(e =>
                        (from JProperty p in e.Data where p.Name != null select p.Name)
                        .ToList())
                            .SelectMany(q => q).Distinct().ToArray();

                    var obj = properties
                        .Where(p => rule.ContextFields?.Any(c => c == p) is not true)
                        .ToDictionary(p => p, p =>
                        {
                            if (p == "Parents")
                            {
                                var arr = currentAtomicEvents.Select(e => e.Data[p]).ToList();
                                var r = JToken.FromObject(arr.FirstOrDefault());

                                if (arr.Count > 1)
                                    throw new Exception("Unexpected behaviour");
                                
                                return r;
                            }

                            var result = currentAtomicEvents.Select(e => e.Data[p]);
                            //if (rule.ContextFields?.Contains(p) == true)
                            //    return result.FirstOrDefault();

                            return JToken.FromObject(result);
                        });

                    obj.Add("Id", JToken.FromObject(currentAtomicEvents.Select(e => e.Id)));
                    obj.Add("Date", JToken.FromObject(currentAtomicEvents.Select(e => e.DateTime)));
                    
                    return JToken.FromObject(obj);
                });

            var data = new Dictionary<string, object?>();
            foreach (var contextField in rule.ContextFields)
            {
                data.Add(contextField, dataDictionary[contextField]);
            }
            
            data.Add("Parents", newEventData);

            newEvents.Add(new Event()
            {
                //Id = Guid.NewGuid(),
                Name = rule.Name,
                DateTime = DateTime.UtcNow,
                Data = JToken.FromObject(data)
            });
        }

        return newEvents;
    }
}
