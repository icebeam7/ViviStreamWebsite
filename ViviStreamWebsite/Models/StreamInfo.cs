using Microsoft.WindowsAzure.Storage.Table;

namespace ViviStreamWebsite.Models
{
    public class StreamInfo : TableEntity
    {
        public string StreamID { get; set; }
        public string BotName { get; set; }
        public string Title { get; set; }
        public int Good { get; set; }
        public int Bad { get; set; }
        public int Zelda { get; set; }
    }
}
