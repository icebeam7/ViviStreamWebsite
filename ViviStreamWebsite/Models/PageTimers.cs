using System.Collections.Generic;

namespace ViviStreamWebsite.Models
{
    public class PageTimers
    {
        public Dictionary<int, string> PageSize { get; set; }
        public IEnumerable<BotTimers> Bot_Timers { get; set; }

        public PageTimers()
        {
            PageSize = new Dictionary<int, string>()
            {
                { 10, "10" },
                { 25, "25" },
                { 50, "50" },
                { 100, "100" },
            };
        }
    }
}
