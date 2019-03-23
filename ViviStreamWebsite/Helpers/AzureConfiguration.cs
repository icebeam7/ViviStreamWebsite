using Microsoft.Extensions.Configuration;
using System.IO;

namespace ViviStreamWebsite.Helpers
{
    public class AzureConfiguration
    {
        public static IConfiguration Configuration
        {
            get;
            set;
        }

        public static void GetConfiguration()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json");
            Configuration = configurationBuilder.Build();
        }
    }
}