using Cerpent.IntegrationTest.Helpers;
using Cerpent.AWS.DB.Settings;
using Cerpent.Core.Contract.Stereotype;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Cerpent.AWS.DB.Sources;

namespace Cerpent.IntegrationTest.DBTests
{
    [TestClass]
    public class StereotypeDescriptionDbOperationTest : BaseDbOpeartionTest
    {
        private DbStereotypeDescriptionSource _stereotypeDescriptionSource;

        [TestInitialize]
        public void TestInitialize()
        {
            _stereotypeDescriptionSource = new DbStereotypeDescriptionSource(_databaseSettings.Value.ConnectionString);
        }

        [DataTestMethod]
        [DataRow("INSERT_TEST")]
        [DataRow("UPDATE_TEST")]
        public void StereotypeDescriptionPutAndGetShouldWork(string name)
        {
            string stereotypeName = name;
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

            var accuracy = "5%";

            var newStereotype = new StereotypeDescription(stereotypeName, stereotypeTriggerEvent,
                upperBoundDictionary, lowerBoundDictionary, accuracy);

            var newId = _stereotypeDescriptionSource.Put(newStereotype).Result;            

            var ruleFromDb = _stereotypeDescriptionSource.Get(stereotypeTriggerEvent)
                .Result.FirstOrDefault();

            Assert.IsTrue(ruleFromDb != null);
            Assert.IsTrue(ruleFromDb.Id == newId);
            Assert.IsTrue(ruleFromDb.TriggerEvent == newStereotype.TriggerEvent);
            Assert.IsTrue(ruleFromDb.UpperBounds.Count() == newStereotype.UpperBounds.Count());
            Assert.IsTrue(ruleFromDb.UpperBounds.First().Key == newStereotype.UpperBounds.First().Key);
            Assert.IsTrue(ruleFromDb.UpperBounds.First().Value == newStereotype.UpperBounds.First().Value);
            Assert.IsTrue(ruleFromDb.LowerBounds.Count() == newStereotype.LowerBounds.Count());
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
