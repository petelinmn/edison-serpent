using Cerpent.AWS.DB.Sources;
using Cerpent.Core.Contract.Event;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Cerpent.IntegrationTest.DBTests
{
    [TestClass]
    public class EventDbOperationTest : BaseDbOpeartionTest
    {
        private DbEventSource? _eventSource;
        private int _testEventId;

        [TestInitialize]
        public void TestInitialize()
        {
            _eventSource = new DbEventSource(_databaseSettings.Value.ConnectionString);
        }

        [TestMethod]
        public void EventPutAndGetShouldWork()
        {
            var newEventName = "INSERT_EVENT_TEST";
            var firstPersonGuid = Guid.NewGuid();
            var newEvent = new Event
            {
               Name = newEventName,
               Data = JToken.FromObject(new { PersonId = firstPersonGuid, Value = 130 })
            };

            var newEventId = _eventSource.Put(newEvent).Result;
            _testEventId = newEventId;

            Assert.IsTrue(newEventId != -1);

            var testEventsInserted = _eventSource.Get(newEventName, new Dictionary<string, JToken?>()
            {
                {"PersonId", JToken.FromObject(firstPersonGuid) }
            }, 60).Result;


            Assert.IsTrue(testEventsInserted != null);
            Assert.IsTrue(testEventsInserted.Count() == 1);
            Assert.IsTrue(testEventsInserted.First().Id == newEventId);
            Assert.IsTrue(testEventsInserted.First().Name == newEventName);

            Assert.IsTrue((string?)testEventsInserted.First().Data["PersonId"] == (string?)newEvent.Data["PersonId"]);
            Assert.IsTrue((string?)testEventsInserted.First().Data["Value"] == (string?)newEvent.Data["Value"]);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _eventSource?.Delete(_testEventId).GetAwaiter().GetResult();
        }
    }
}
