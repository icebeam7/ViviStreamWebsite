using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace ViviStreamWebsite.Models
{
    public class AllSongs : TableEntity
    {
        public string Title { get; set; }

        public string OriginalTitle { get; set; }

        public string OriginalGame { get; set; }

        public string Game { get; set; }

        public string Channel { get; set; }

        public string YouTubeLink { get; set; }

        public string Duration { get; set; }

        public string RecentlyAdded { get; set; }

        public DateTime? LastTimeRequested { get; set; }

        public string LastTimeRequested0
        {
            get
            {
                if (!LastTimeRequested.HasValue)
                {
                    return "N/A";
                }
                return LastTimeRequested.Value.ToString("yyyy-MM-dd hh:mm tt");
            }
        }

        public int? Counter { get; set; }

        public int Counter0 
        {
            get
            {
                if (!Counter.HasValue)
                {
                    return 0;
                }
                return Counter.Value;
            }
        }

        public int? Likes { get; set; }

        public int Likes0
        {
            get
            {
                if (!Likes.HasValue)
                {
                    return 0;
                }
                return Likes.Value;
            }
        }
    }
}