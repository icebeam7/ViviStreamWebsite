using System.Collections.Generic;

namespace ViviStreamWebsite.Models
{
    public class PageCommands
    {
        public Dictionary<int, string> PageSize { get; set; }
        public IEnumerable<CustomCommands> Commands { get; set; }

        public PageCommands()
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
