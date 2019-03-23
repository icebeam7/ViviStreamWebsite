using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.Table;

namespace ViviStreamWebsite.Models
{
    public class CustomCommands : TableEntity
    {
        [Required]
        public string Command { get; set; }

        [Required]
        public string Message { get; set; }

        public string Userlevel { get; set; }
        public int Cooldown { get; set; }
        public string Alias { get; set; }
        public string Description { get; set; }
        public bool IsStreamCommand { get; set; } = false;
        public bool IsEnabled { get; set; }

        public string EncodedMessage
        {
            get
            {
                return Message.Contains("urlfetch") ? "(Encoded url)" : Message;
            }
        }

        public string IsEnabledMessage
        {
            get
            {
                return IsEnabled ? "Enabled" : "Disabled";
            }
        }
    }
}
