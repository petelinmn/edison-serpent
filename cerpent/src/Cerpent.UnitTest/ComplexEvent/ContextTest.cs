using Cerpent.Core.Contract.Event;
using Cerpent.Core;
using Cerpent.MockPlatform;
using Newtonsoft.Json.Linq;
using Cerpent.Core.Contract.AggregationRules;

namespace Cerpent.UnitTest.ComplexEvent
{
    [TestClass]
    public class ContextTest
    {
        MockEventSource GetMockEventSource(Dictionary<string, object[]> events)
        {
            var eventsPrepared = events.Select((arg, index) =>
            {
                return arg.Value.Select(val => new AutoIncIdMockEvent()
                {
                    Name = arg.Key,
                    DateTime = DateTime.Now.AddSeconds(arg.Value.Length - index * 2 - 1),
                    Data = JToken.FromObject(val)
                });
            }).SelectMany(i => i).ToArray();

            return new MockEventSource(eventsPrepared);
        }


        [TestMethod]
        public void ContextTestShouldWork()
        {
            const string atomicEventName = "Pulse";
            const string complexEventName = "PulseRise";

            var johnId = Guid.NewGuid();
            var tomId = Guid.NewGuid();

            var eventSource = GetMockEventSource(new Dictionary<string, object[]>()
            {
                {
                    atomicEventName,
                    new object[]
                    {
                        new { PersonId = johnId, Value = 100 },
                        new { PersonId = tomId, Value = 110 },
                        new { PersonId = johnId, Value = 120 }
                    }
                }
            }
            );

            var aggregationRuleSource = new MockAggregationRuleSource(new AggregationRule[]
            {
            new (complexEventName, new Dictionary<string, int>
                    { { atomicEventName, 3 } },
                new [] { "PersonId" }, null, null)
            });

            var eventAggregator = new EventAggregator<AutoIncIdMockEvent>(eventSource, aggregationRuleSource);
            var complexEvents = (eventAggregator.Aggregate(new Event
            {
                Name = atomicEventName,
                DateTime = DateTime.Now,
                Data = JToken.FromObject(new { PersonId = johnId, Value = 130 })
            }).Result).ToList();

            Assert.IsTrue(complexEvents.Count == 1);
            Assert.IsTrue(complexEvents[0].Name == complexEventName);
        }
    }
}
