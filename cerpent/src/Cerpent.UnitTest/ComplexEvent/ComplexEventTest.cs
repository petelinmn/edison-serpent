﻿using Cerpent.Core.Contract.Event;
using Cerpent.Core.Contract.Stereotype;
using Cerpent.Core.Contract;
using Cerpent.Core.StereotypeRecognizer;
using Cerpent.Core;
using Cerpent.MockPlatform;
using Newtonsoft.Json.Linq;
using Cerpent.Core.Contract.AggregationRules;

namespace Cerpent.UnitTest.ComplexEvent
{
    [TestClass]
    public class ComplexEventTest
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
        public void ComplexEventFirstLevelTest()
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
            var complexEvents = (eventAggregator.Aggregate<AutoIncIdMockEvent>(new AutoIncIdMockEvent
            {
                Name = atomicEventName,
                DateTime = DateTime.Now,
                Data = JToken.FromObject(new { PersonId = johnId, Value = 130 })
            }).Result).ToList();

            Assert.IsTrue(complexEvents.Count == 1);
            Assert.IsTrue(complexEvents[0].Name == complexEventName);
        }

        [TestMethod]
        public void ComplexEventSecondLevelTest()
        {
            const string contextFieldName = "PersonId";

            var johnId = Guid.NewGuid();
            var tomId = Guid.NewGuid();

            const string atomic1EventName = "Pulse";
            const string atomic2EventName = "Pressure";
            var eventSource = GetMockEventSource(new Dictionary<string, object[]>()
            {
                {
                    atomic1EventName,
                    new object[]
                    {
                        new { PersonId = johnId, Value = 100 },
                    }
                },
                {
                    atomic2EventName,
                    new object[]
                    {
                        new { PersonId = johnId, Value = 120 },
                    }
                }
            }
            );

            const string complexEvent1Name = "PulseRise";
            const string complexEvent2Name = "PressureRise";
            var aggregationRuleSource = new MockAggregationRuleSource(new AggregationRule[]
            {
            new (complexEvent1Name, new Dictionary<string, int>
                { { atomic1EventName, 2 } },
                new [] { contextFieldName }, null, null),
            new (complexEvent2Name, new Dictionary<string, int>
                    { { atomic2EventName, 2 } },
                new [] { contextFieldName }, null, null)
            });

            var eventAggregator = new EventAggregator<AutoIncIdMockEvent>(eventSource, aggregationRuleSource);

            var complexEvents1 = (eventAggregator.Aggregate<AutoIncIdMockEvent>(new AutoIncIdMockEvent
            {
                Name = atomic1EventName,
                DateTime = DateTime.Now,
                Data = JToken.FromObject(new { PersonId = johnId, Value = 130 })
            }).Result).ToList();

            Assert.IsTrue(complexEvents1.Count == 1);
            Assert.IsTrue(complexEvents1[0].Name == complexEvent1Name);

            var complexEvents2 = (eventAggregator.Aggregate<AutoIncIdMockEvent>(new AutoIncIdMockEvent
            {
                Name = atomic2EventName,
                DateTime = DateTime.Now,
                Data = JToken.FromObject(new { PersonId = johnId, Value = 150 })
            }).Result).ToList();

            Assert.IsTrue(complexEvents2.Count == 1);
            Assert.IsTrue(complexEvents2[0].Name == complexEvent2Name);

            eventSource = GetMockEventSource(new Dictionary<string, object[]>
                {
                    {complexEvent1Name, new object[] {complexEvents1[0].Data}}
                }
            );

            const string complexEventSecondLevelName = "PulseAndPressureRise";
            aggregationRuleSource = new MockAggregationRuleSource(new AggregationRule[]
            {
            new (complexEventSecondLevelName, new Dictionary<string, int>
                    { { complexEvent1Name, 1 }, { complexEvent2Name, 1 } },
                new [] { contextFieldName }, null, null)
            });

            eventAggregator = new EventAggregator<AutoIncIdMockEvent>(eventSource, aggregationRuleSource);
            var complexEventsSecondLevel = (eventAggregator.Aggregate<AutoIncIdMockEvent>(complexEvents2[0]).Result).ToList();

            Assert.IsTrue(complexEventsSecondLevel.Count == 1);

            var personId = complexEventsSecondLevel[0].Data[contextFieldName]?.ToString()
                ?? throw new ArgumentNullException($"PersonId don't have to be null");
            Assert.IsTrue(Guid.Parse(personId) == johnId);
            Assert.IsTrue(complexEventsSecondLevel[0].Name == complexEventSecondLevelName);

            const string stereotypeName = "Hypertonia";
            var stereotypeSource = new MockStereotypeDescriptionsSource(new[]
            {
            new StereotypeDescription(stereotypeName, complexEventSecondLevelName,
                new Dictionary<string, string>
                {
                    { atomic1EventName, "Value" }, { atomic2EventName, "Value" }
                },
                new Dictionary<string, string>
                {
                    { atomic1EventName, "Value - 1" }, { atomic2EventName, "Value - 2 - listValue[2]" }
                },
                new Dictionary<string, string>
                {
                    { atomic1EventName, "Value * 2" }, { atomic2EventName, "Value * 3" }
                },
                "1")
        });

            var stereotypeRecognizer = new StereotypeRecognizer(stereotypeSource);
            var stereotypeRecognitionResult =
                stereotypeRecognizer.FuzzySearch(complexEventsSecondLevel[0]).Result;


        }
    }
}
