using Microsoft.WindowsAzure.Storage.Table;

namespace ViviStreamWebsite.Models
{
    public class MySongs : TableEntity
    {
        public string Title { get; set; }
        public string OriginalGame { get; set; }
        public string Channel { get; set; }
        public string VideoId { get; set; }
        public string YouTubeLink { get; set; }
        public string Duration { get; set; }
    }
}
