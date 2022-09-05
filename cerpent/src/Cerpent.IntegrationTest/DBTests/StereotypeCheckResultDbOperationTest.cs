using Cerpent.AWS.DB.Sources;
using Cerpent.Core.Contract.Event;
using Cerpent.Core.Contract.Stereotype;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Cerpent.IntegrationTest.DBTests
{
    [TestClass]
    public class StereotypeCheckResultDbOperationTest : BaseDbOpeartionTest
    {
        private DbEventSource? _eventSource;
        private DbStereotypeDescriptionSource? _stereotypeDescriptionSource;
        private DbStereotypeCheckResultSource? _dbStereotypeCheckResultSource;

        private int _testEventId;
        private int _testStereotypeId;
        private int _testStereotypeCheckResultId;

        [TestInitialize]
        public void TestInitialize()
        {
            _eventSource = new DbEventSource(_databaseSettings.Value.ConnectionString);
            _dbStereotypeCheckResultSource = new DbStereotypeCheckResultSource(_databaseSettings.Value.ConnectionString);
            _stereotypeDescriptionSource = new DbStereotypeDescriptionSource(_databaseSettings.Value.ConnectionString);
        }

        [TestMethod]
        public void StereotypeCheckResultPutAndGetShouldWork()
        {
            var newEventName = "STEREOTYPE_CHECK_RESULT_TEST_EVENT";
            var firstPersonGuid = Guid.NewGuid();
            var newEvent = new Event
            {
                Name = newEventName,
                Data = JToken.FromObject(new { PersonId = firstPersonGuid, Value = 120 })
            };

            var newEventId = _eventSource?.Put(newEvent).Result;
            _testEventId = newEventId.Value;

            var stereotypeName = "STEREOTYPE_CHECK_RESULT_TEST_STEREOTYPE";
            const string stereotypeTriggerEvent = "STEREOTYPE_CHECK_RESULT_TEST_TRIGGER_EVENT";

            var metricsDictionary = new Dictionary<string, string>
            {
                { "Pulse", "Value" },
                { "Pressure", "Value"}
            };

            var upperBoundDictionary = new Dictionary<string, string>
            {
                { "Pulse", "Value + Age/3 - Mood" },
                { "Pressure", "Value + Age/2 - Mood"}
            };

            var lowerBoundDictionary = new Dictionary<string, string>
            {
                { "Pulse", "Value + Age/4 - Mood" },
                { "Pressure", "Value + Age/5 - Mood"}
            };

            var accuracyDictionary = new Dictionary<string, string>
            {
                { "Pulse", "50%" },
                { "Pressure", "1"}
            };

            var newStereotype = new StereotypeDescription(stereotypeName, stereotypeTriggerEvent,
                metricsDictionary, upperBoundDictionary, lowerBoundDictionary, accuracyDictionary);

            if (_stereotypeDescriptionSource == null)
                throw new ArgumentNullException(nameof(_stereotypeDescriptionSource));

            var newStereotypeId = _stereotypeDescriptionSource.Put(newStereotype).Result;
            _testStereotypeId = newStereotypeId;

            var stereotypeCheckResult = new StereotypeCheckResult
            {
                StereotypeDescriptionId = newStereotypeId,
                TriggerEventId = newEventId.Value,
                ChartResults = new List<StereotypeChartResult>()
                {
                    new StereotypeChartResult
                    {
                        Accuracy = "abc",
                        MetricName = "abc",
                        Ids = new int[3] { 10, 20, 30 },
                        Dates = new DateTime[1] { DateTime.UtcNow },
                        Metrics = new double?[1] { 10 },
                        UpperBounds = new double?[1] { 20 },
                        LowerBounds = new double?[1] { 30 },
                    }
                }
            };

            var newStereotypeCheckResultId = _dbStereotypeCheckResultSource.Put(stereotypeCheckResult).Result;
            _testStereotypeCheckResultId = newStereotypeCheckResultId;

            var stereotypeCheckResultInserted = _dbStereotypeCheckResultSource.Get(newStereotypeId).Result;
            Assert.IsNotNull(stereotypeCheckResultInserted);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _dbStereotypeCheckResultSource?.Delete(_testStereotypeCheckResultId).GetAwaiter().GetResult();
            _eventSource?.Delete(_testEventId).GetAwaiter().GetResult();
            _stereotypeDescriptionSource?.Delete(_testStereotypeId).GetAwaiter().GetResult();
        }
    }
}
