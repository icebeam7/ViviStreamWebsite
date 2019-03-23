using Microsoft.WindowsAzure.Storage.Table;

namespace ViviStreamWebsite.Models
{
    public class StreamUsers : TableEntity
    {
        public string ChannelID { get; set; }
        public string ChannelTitle { get; set; }
    }
}
