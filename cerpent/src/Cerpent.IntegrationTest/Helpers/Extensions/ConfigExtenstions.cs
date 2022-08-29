using Microsoft.Extensions.Configuration;

namespace Cerpent.IntegrationTest.Helpers.Extensions
{
    public static class ConfigExtenstions
    {
        public static T GetValue<T>(this IConfiguration configuration, string configSection, string keyName)
        {
            return (T)Convert.ChangeType(configuration[$"{configSection}:{keyName}"], typeof(T));
        }
    }
}
