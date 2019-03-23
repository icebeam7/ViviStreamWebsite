using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ViviStreamWebsite.Helpers;

namespace ViviStreamWebsite.Services
{
    public static class ImageService
    {
        static List<string> fileExtensions = new List<string>() { "png", "bmp", "jpg", "jpeg" };

        public static async Task<byte[]> DownloadImage(string game)
        {
            game = game.Replace(':', '-').Replace('/', '-').Replace('*', '-')
                .Replace('|', '-').Replace('<', '-').Replace('>', '-')
                .Replace('\\', '-').Replace('"', '-').Replace('?', '-');
            var gameBlobUrl = Constants.BlobContainerBaseUrl + game + ".";

            using (var client = new HttpClient() { BaseAddress = new Uri(Constants.BlobContainerBaseUrl) })
            {
                foreach (var extension in fileExtensions)
                {
                    try
                    {
                        var url = new Uri(gameBlobUrl + extension);

                        using (var response = await client.GetAsync(url))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                using (var stream = await response.Content.ReadAsStreamAsync())
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        await stream.CopyToAsync(ms);
                                        return ms.ToArray();
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }

            return null;
        }
    }
}
