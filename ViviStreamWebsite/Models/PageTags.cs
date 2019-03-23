using System.Collections.Generic;

namespace ViviStreamWebsite.Models
{
    public class PageTags
    {
        public Dictionary<int, string> PageSize { get; set; }
        public IEnumerable<Tags> Tags { get; set; }

        public PageTags()
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
