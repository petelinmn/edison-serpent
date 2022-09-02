using Cerpent.AWS.DB.Settings;
using Cerpent.IntegrationTest.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Cerpent.IntegrationTest.DBTests
{
    public class BaseDbOpeartionTest
    {
        protected IOptions<DatabaseSettings> _databaseSettings { get; set; }
        public BaseDbOpeartionTest()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            _databaseSettings = OptionsHelper.CreateDatabaseSettings("Database", config);
            //For timestamp db columns
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
