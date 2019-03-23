using System.Collections.Generic;

namespace ViviStreamWebsite.Models
{
    public class SongsInfo
    {
        public AzureTableSong CurrentSong
        {
            get;
            set;
        }

        public IEnumerable<AzureTableSong> SongsQueue
        {
            get;
            set;
        }

        public byte[] GamePicture { get; set; }
    }
}
