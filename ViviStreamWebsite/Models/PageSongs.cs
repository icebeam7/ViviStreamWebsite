using System.Collections.Generic;

namespace ViviStreamWebsite.Models
{
    public class PageSongs
    {
        public Dictionary<int, string> PageSize { get; set; }
        public IEnumerable<AllSongs> AllSongs { get; set; }

        public PageSongs()
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
