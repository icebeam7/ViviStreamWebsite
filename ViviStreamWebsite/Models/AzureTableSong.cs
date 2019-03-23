using Microsoft.WindowsAzure.Storage.Table;
using System;
using ViviStreamWebsite.Helpers;

namespace ViviStreamWebsite.Models
{
    public class AzureTableSong : TableEntity
    {
        public string Title { get; set; }

        public string OriginalGame { get; set; }

        public string LowerCaseTitle { get; set; }

        public string Channel { get; set; }

        public string VideoId { get; set; }

        public string YouTubeLink { get; set; }

        public string Duration { get; set; }

        public int TotalTime { get; set; }

        public string RecentlyAdded { get; set; }

        public string RequestedBy { get; set; }

        public string RequestedById { get; set; }

        public string UserType { get; set; }

        public DateTime? LastTimeRequested { get; set; }

        public int Counter { get; set; }
        public int Likes { get; set; }

        public int Position { get; set; }

        public int CurrentTime { get; set; }

        public string CurrentTimeDisplay => SecondsToString(CurrentTime) + " / " + Duration;

        public string ScrollTextDisplay
        {
            get
            {
                string str = (!string.IsNullOrWhiteSpace(RequestedBy)) ? RequestedBy : Constants.StreamOwner;
                return Title + " - Requested by " + str;
            }
        }

        private static string SecondsToString(int secs)
        {
            int num = secs % 3600 / 60;
            secs %= 60;
            return $"{num:D2}:{secs:D2}";
        }
    }
}