using System.Collections.Generic;

namespace ViviStreamWebsite.Models
{
    public class NewSong
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string OriginalTitle { get; set; }
        public string OriginalGame { get; set; }
        public string Channel { get; set; }
        public string YouTubeLink { get; set; }
        public string YTLink { get; set; }
        public string Duration { get; set; }

        public List<StreamTags> StreamTags { get; set; }
    }
}
