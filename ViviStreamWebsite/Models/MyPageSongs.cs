using System.Collections.Generic;

namespace ViviStreamWebsite.Models
{
    public class MyPageSongs
    {
        public Dictionary<int, string> PageSize { get; set; }
        public IEnumerable<MySongs> MySongs { get; set; }

        public MyPageSongs()
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
