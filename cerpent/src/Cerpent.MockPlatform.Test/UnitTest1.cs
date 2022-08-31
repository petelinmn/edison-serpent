using Cerpent.Core.Contract;
using Cerpent.Core.Contract.Event;
using Newtonsoft.Json.Linq;

namespace Cerpent.MockPlatform.Test;

[TestClass]
public class UnitTest1
{
    MockEventSource GetMockEventSource(string eventName, params object[] args)
    {
        return new MockEventSource(args.Select((arg, index) =>
        {
            var token = JToken.FromObject(arg);
            return new Event()
            {
                Id = index++,
                Name = eventName,
                DateTime = DateTime.Now.AddSeconds(args.Length - index - 1),
                Data = token
            };
        }).ToArray());
    }
    
    
    [TestMethod]
    public void ContextTest()
    {
        const string atomicEventName = "Pulse";
        const string complexEventName = "PulseRise";

        var johnId = Guid.NewGuid();
        var tomId = Guid.NewGuid();
        
        var eventSource = GetMockEventSource(atomicEventName, 
            new { PersonId = johnId, Value = 100 },
            new { PersonId = tomId, Value = 110 },
            new { PersonId = johnId, Value = 120 }
        );

        var aggregationRuleSource = new MockAggregationRuleSource(new AggregationRule[]
        {
            new (complexEventName, new Dictionary<string, int>
                    { { atomicEventName, 3 } },
                new [] { "PersonId" }, null, null)
        });
        
        var eventAggregator = new EventAggregator(eventSource, aggregationRuleSource);
        var complexEvents = (eventAggregator.Aggregate(new Event
        {
            Id = 1,
            Name = atomicEventName,
            DateTime = DateTime.Now,
            Data = JToken.FromObject(new { PersonId = johnId, Value = 130 })
        }).Result).ToList();

        Assert.IsTrue(complexEvents.Count == 1);
        Assert.IsTrue(complexEvents[0].Name == complexEventName);
    }
}
