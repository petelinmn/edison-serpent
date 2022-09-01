using Cerpent.AWS.DB.Repositories;
using Cerpent.IntegrationTest.Helpers;
using Cerpent.AWS.DB.Settings;
using Cerpent.Core.Contract.Stereotype;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cerpent.AWS.DB.Sources;
using Cerpent.Core.Contract;

namespace Cerpent.IntegrationTest.DBTests
{
    [TestClass]
    public class AggregationRulesDbOperationTest
    {
        private IOptions<DatabaseSettings>? DatabaseSettings { get; set; }
        private DbAggregationRuleSource? _aggregationRuleSource;
        private DbStereotypeDescriptionSource? _stereotypeDescriptionSource;

        [TestInitialize]
        public void TestInitialize()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            DatabaseSettings = OptionsHelper.CreateDatabaseSettings("Database", config);
            _aggregationRuleSource = new DbAggregationRuleSource(DatabaseSettings.Value.ConnectionString);
            _stereotypeDescriptionSource = new DbStereotypeDescriptionSource(DatabaseSettings.Value.ConnectionString);
        }

        [DataTestMethod]
        [DataRow("INSERT_TEST")]
        [DataRow("UPDATE_TEST")]
        public void AggregationRulePutAndGetShouldWork(string name)
        {
            string ruleName = name;
            const string firstAtomicEventName = "Pulse";
            const string secondAtomicEventName = "Pressure";
            var guidLabel = Guid.NewGuid().ToString();
            var atomicsDictionary = new Dictionary<string, int>
            {
                { firstAtomicEventName, Random.Shared.Next(1, 10) },
                { secondAtomicEventName, Random.Shared.Next(1, 10)}
            };

            var contextFields = new[] { "personId" };
            var timeSpan = Random.Shared.Next(1, 10);
     
            var newRule = new AggregationRule(ruleName, atomicsDictionary,
                contextFields, guidLabel, timeSpan);

            var newId = _aggregationRuleSource.Put(newRule).Result;

            var ruleFromDb = _aggregationRuleSource.Get(firstAtomicEventName)
                .Result.FirstOrDefault(rule => rule.Name == newRule.Name);

            Assert.IsTrue(ruleFromDb != null);
            Assert.IsTrue(ruleFromDb.Id == newId);
            Assert.IsTrue(ruleFromDb.Condition == newRule.Condition);
            Assert.IsTrue(ruleFromDb.ContextFields?.Count() == newRule.ContextFields?.Count());
            Assert.IsTrue(ruleFromDb.Atomics?.Count == newRule.Atomics?.Count);
            Assert.IsTrue(ruleFromDb?.Atomics?[firstAtomicEventName] == newRule?.Atomics?[firstAtomicEventName]);
            Assert.IsTrue(ruleFromDb.TimeSpan == newRule.TimeSpan);
        }

        [DataTestMethod]
        [DataRow("INSERT_TEST")]
        [DataRow("UPDATE_TEST")]
        public void StereotypeDescriptionPutAndGetShouldWork(string name)
        {
            var stereotypeName = name;
            const string stereotypeTriggerEvent = "TRIGGER_EVENT_TEST";

            var upperBoundDictionary = new Dictionary<string, string>
            {
                { "Pulse", "{Value} + {Age}/3 - {Mood}" },
                { "Pressure", "{Value} + {Age}/2 - {Mood}"}
            };

            var lowerBoundDictionary = new Dictionary<string, string>
            {
                { "Pulse", "{Value} + {Age}/4 - {Mood}" },
                { "Pressure", "{Value} + {Age}/5 - {Mood}"}
            };

            const string accuracy = "5%";

            var newStereotype = new StereotypeDescription(stereotypeName, stereotypeTriggerEvent,
                upperBoundDictionary, lowerBoundDictionary, accuracy);

            var newId = _stereotypeDescriptionSource.Put(newStereotype).Result;

            var ruleFromDb = _stereotypeDescriptionSource.Get(stereotypeTriggerEvent)
                .Result.FirstOrDefault();

            Assert.IsTrue(ruleFromDb != null);
            Assert.IsTrue(ruleFromDb.Id == newId);
            Assert.IsTrue(ruleFromDb.TriggerEvent == newStereotype.TriggerEvent);
            Assert.IsTrue(ruleFromDb.UpperBounds.Count == newStereotype.UpperBounds.Count);
            Assert.IsTrue(ruleFromDb.UpperBounds.First().Key == newStereotype.UpperBounds.First().Key);
            Assert.IsTrue(ruleFromDb.UpperBounds.First().Value == newStereotype.UpperBounds.First().Value);
            Assert.IsTrue(ruleFromDb.LowerBounds.Count == newStereotype.LowerBounds.Count);
            Assert.IsTrue(ruleFromDb.LowerBounds.First().Key == newStereotype.LowerBounds.First().Key);
            Assert.IsTrue(ruleFromDb.LowerBounds.First().Value == newStereotype.LowerBounds.First().Value);
            Assert.IsTrue(ruleFromDb.Accuracy == newStereotype.Accuracy);

            //TODO: Test cleanup
            //if (_lastIdInserted != 0)
            //{
            //    _stereotypeDescriptionSource.Delete(_lastIdInserted);
            //}

            //_lastIdInserted = ruleFromDb.Id;
        }
    }
}
