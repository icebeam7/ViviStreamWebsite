using Microsoft.WindowsAzure.Storage.Table;

namespace ViviStreamWebsite.Models
{
    public class Friends : TableEntity
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
