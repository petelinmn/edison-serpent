using Cerpent.IntegrationTest.Helpers.Extensions;
using Cerpent.AWS.DB.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Cerpent.IntegrationTest.Helpers
{
    public class OptionsHelper
    {
        public static IOptions<DatabaseSettings> CreateDatabaseSettings(string configSection, IConfigurationRoot configuration)
        {
            return Options.Create(new DatabaseSettings()
            {
                ConnectionString = configuration.GetValue<string>(configSection, "ConnectionString"),
            });
        }
    }
}
