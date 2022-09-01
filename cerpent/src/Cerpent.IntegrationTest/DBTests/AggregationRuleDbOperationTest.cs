using Cerpent.AWS.DB.Repositories;
using Cerpent.Core.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cerpent.IntegrationTest.DBTests
{
    [TestClass]
    public class AggregationRuleDbOperationTest : BaseDbOpeartionTest
    {
        private DbAggregationRuleSource? _dbAggregationRuleSource;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbAggregationRuleSource = new DbAggregationRuleSource(_databaseSettings.Value.ConnectionString);
        }

        [TestMethod]
        public void SourceTest()
        {
            const string ruleName = "PulseRise";
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

            var newId = _dbAggregationRuleSource.Put(newRule).Result;

            var ruleFromDb = _dbAggregationRuleSource.Get(firstAtomicEventName)
                .Result.FirstOrDefault(rule => rule.Name == newRule.Name);

            Assert.IsTrue(ruleFromDb != null);
            Assert.IsTrue(ruleFromDb.Id == newId);
            Assert.IsTrue(ruleFromDb.Condition == newRule.Condition);
            Assert.IsTrue(ruleFromDb.ContextFields?.Count() == newRule.ContextFields?.Count());
            Assert.IsTrue(ruleFromDb.Atomics?.Count == newRule.Atomics?.Count);
            Assert.IsTrue(ruleFromDb?.Atomics?[firstAtomicEventName] == newRule?.Atomics?[firstAtomicEventName]);
            Assert.IsTrue(ruleFromDb?.TimeSpan == newRule?.TimeSpan);
        }
    }
}
