using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using ViviStreamWebsite.Models;
using ViviStreamWebsite.Helpers;

namespace ViviStreamWebsite.Services
{
    public static class YouTubeApiService
    {
        public static async Task<string> GetChannelID(YouTubeService youTubeService)
        {
            var channelsRequest = youTubeService.Channels.List("id, snippet");
            channelsRequest.Mine = true;
            var channelsResponse = await channelsRequest.ExecuteAsync();
            var youTubeChannels = channelsResponse.Items;

            var table = TableStorageService.ConnectToTable(Constants.StreamUsersTableName);
            TableContinuationToken token = null;
            var streamUsers = new List<StreamUsers>();

            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(new TableQuery<StreamUsers>(), token);
                streamUsers.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            var users = (from user in streamUsers
                        let channels = youTubeChannels
                        where channels.Any(x => x.Id == user.ChannelID)
                        select user).ToList();

            if (users.Count > 0)
            {
                return users.First().ChannelID;
            }
            else
            {
                var channel = youTubeChannels.First();

                var streamUser = new StreamUsers()
                {
                    PartitionKey = channel.Id,
                    RowKey = "1",
                    ChannelID = channel.Id,
                    ChannelTitle = channel.Snippet.Title
                };

                await TableStorageService.InsertEntity(streamUser, Constants.StreamUsersTableName);
                return channel.Id;
            }
        }

        public static async Task<AllSongs> GetSongInfo(IConfiguration configuration, string url)
        {
            var song = new AllSongs();
            var videoID = GetYouTubeVideoID(url);

            if (!string.IsNullOrWhiteSpace(videoID))
            {
                var ytService = GetYouTubeService(configuration);

                if (ytService != null)
                {
                    try
                    {
                        var videoRequest = ytService.Videos.List("snippet,contentDetails");
                        videoRequest.Id = videoID;

                        var videoItemsResponse = await videoRequest.ExecuteAsync();
                        var videoItem = videoItemsResponse.Items.FirstOrDefault();

                        if (videoItem != null)
                        {
                            var availableVideo = !(videoItem.Snippet.Title.ToLower().Contains("deleted") &&
                                videoItem.Snippet.Description.Contains("unavailable"));

                            if (availableVideo)
                            {
                                song = new AllSongs()
                                {
                                    OriginalGame = "NEW",
                                    OriginalTitle = videoItem.Snippet.Title,
                                    Channel = videoItem.Snippet.ChannelTitle,
                                    Duration = ConvertToTime(videoItem.ContentDetails.Duration),
                                    YouTubeLink = $"https://youtu.be/{videoID}"
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            return song;
        }

        public static string GetYouTubeVideoID(string url)
        {
            var videoId = string.Empty;

            try
            {
                var uri = new Uri(url);
                var query = HttpUtility.ParseQueryString(uri.Query);

                if (query.AllKeys.Contains("v"))
                    videoId = query["v"];
                else
                    videoId = uri.Segments.Last();
            }
            catch(Exception ex) { }

            return videoId;
        }

        public static YouTubeService GetYouTubeService(IConfiguration configuration)
        {
            try
            {
                return new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = configuration.GetSection("Google")["apikey"],
                    ApplicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
                });
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public static string ConvertToTime(string duration)
        {
            var timeSpan = XmlConvert.ToTimeSpan(duration);
            return $"{timeSpan.Minutes.ToString().PadLeft(2, '0')}:{timeSpan.Seconds.ToString().PadLeft(2, '0')}";
        }
    }
}
