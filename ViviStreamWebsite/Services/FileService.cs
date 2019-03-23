using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViviStreamWebsite.Helpers;
using ViviStreamWebsite.Models;

namespace ViviStreamWebsite.Services
{
    public static class FileService
    {
        public static List<CustomCommands> ReadCommands()
        {
            try
            {
                if (File.Exists(Constants.StreamCommandsFilename))
                {
                    var json = File.ReadAllText(Constants.StreamCommandsFilename);
                    return JsonConvert.DeserializeObject<List<CustomCommands>>(json);
                }
            }
            catch (Exception ex)
            {
            }

            return new List<CustomCommands>();
        }
    }
}
