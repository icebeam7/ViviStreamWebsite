namespace ViviStreamWebsite.Models
{
    // not a model
    public class SongTags
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public bool IsStreamTag { get; set; }
    }
}
