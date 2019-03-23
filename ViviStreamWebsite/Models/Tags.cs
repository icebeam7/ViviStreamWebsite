using Microsoft.WindowsAzure.Storage.Table;

namespace ViviStreamWebsite.Models
{
    public class Tags : TableEntity
    {
        public string Name { get; set; }
        public string Emoji { get; set; }
        public string Description { get; set; }
    }
}
