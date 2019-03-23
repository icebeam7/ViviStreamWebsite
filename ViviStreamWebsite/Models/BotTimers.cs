using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace ViviStreamWebsite.Models
{
    public class BotTimers : TableEntity
    {
        [Required]
        public string Name { get; set; }

        [MaxLength(200, ErrorMessage = "Maximum length is 200 characters")]
        [Required]
        public string Message { get; set; }

        public int Interval { get; set; }

        public int ChatLines { get; set; }

        public string Alias { get; set; }

        public bool Status { get; set; }

        public bool IsVisible { get; set; }

        public string StatusMessage => Status ? "Enabled" : "Disabled";
        public string VisibleMessage => IsVisible ? "Visible" : "Hidden";
    }
}
