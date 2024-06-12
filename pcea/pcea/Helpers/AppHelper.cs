using Microsoft.Extensions.Configuration;
using System.IO;

namespace pcea.Helpers
{
    public class AppHelper
    {
        public static string GetCurrentSettings(string path)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .AddEnvironmentVariables();

            IConfigurationRoot configuration = builder.Build();

            var settings = configuration[$"{path}"];

            return settings;
        }
    }
}
