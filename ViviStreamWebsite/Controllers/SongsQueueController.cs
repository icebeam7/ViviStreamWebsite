using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using ViviStreamWebsite.Models;
using ViviStreamWebsite.Helpers;
using System.Threading.Tasks;
using ViviStreamWebsite.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System;

namespace ViviStreamWebsite.Controllers
{
    public class SongsQueueController : Controller
    {
        private UserManager<ApplicationUser> userManager;

        public SongsQueueController(UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        // GET: SongsInfo
        public async Task<ActionResult> Index()
        {
            var songsInfo = new SongsInfo();
            var table = TableStorageService.ConnectToTable(Constants.CurrentSongTableName);
            var queryCS = await table.ExecuteQuerySegmentedAsync(new TableQuery<AzureTableSong>(), null);
            songsInfo.CurrentSong = queryCS.Results.FirstOrDefault();

            var bytes = await ImageService.DownloadImage(songsInfo.CurrentSong.OriginalGame);

            if (bytes == null)
                bytes = await ImageService.DownloadImage(Constants.DefaultBlobImage);

            songsInfo.GamePicture = bytes;

            var songs = await TableStorageService.RetrieveAllEntities<AzureTableSong>(Constants.QueueTableName);
            songsInfo.SongsQueue = songs.OrderBy(x => x.Position);
            return View(songsInfo);
        }

        public async Task<ActionResult> RemoveRequest(string pk, string rk)
        {
            var song = await TableStorageService.RetrieveEntity<AzureTableSong>(pk, rk, Constants.QueueTableName);

            if (song != null)
            {
                await TableStorageService.DeleteEntity(song, Constants.QueueTableName);

                var updateSong = await TableStorageService.RetrieveEntity<AllSongs>(pk, rk, Constants.AllSongsTableName);
                updateSong.PartitionKey = song.PartitionKey;
                updateSong.RowKey = song.RowKey;
                updateSong.Counter = song.Counter - 1;
                updateSong.LastTimeRequested = DateTime.UtcNow.AddHours(-2);

                await TableStorageService.MergeEntity(updateSong, Constants.AllSongsTableName);

                var queueSongs = await TableStorageService.RetrieveAllEntities<AzureTableSong>(Constants.QueueTableName);
                var table = TableStorageService.ConnectToTable(Constants.QueueTableName);

                if (queueSongs.Count > 0)
                {
                    queueSongs = queueSongs.OrderBy(x => x.Position).ToList();
                    var position = 1;

                    foreach (var queueSong in queueSongs)
                    {
                        queueSong.Position = position;
                        queueSong.Counter = queueSong.Counter - 1;

                        var replace = TableOperation.Merge(queueSong);
                        await table.ExecuteAsync(replace);

                        position++;
                    }
                }

                return RedirectToAction("Index", "SongsQueue", new { ac = "The song was removed from the queue.", type = "success" });
            }
            else
                return RedirectToAction("Index", "SongsQueue", new { ac = "Error. The song does not exist.", type = "danger" });
        }

        public async Task<ActionResult> RequestSong(IFormCollection collection)
        {
            var searchString = collection["search"];

            var playlist = await TableStorageService.RetrieveAllEntities<AllSongs>(Constants.AllSongsTableName);
            var song = EasyCustomSearch.SearchSongFirst(searchString, playlist);

            if (song != null)
            {
                var tableSong = TableStorageService.ConnectToTable(Constants.CurrentSongTableName);
                var queryCS = await tableSong.ExecuteQuerySegmentedAsync(new TableQuery<AzureTableSong>(), null);
                var currentSong = queryCS.Results.FirstOrDefault();

                if (currentSong.RowKey != song.RowKey)
                {
                    var position = 1;
                    var channel = CookieService.Get(Request, Constants.ChannelCookieName);

                    var user = await userManager.GetUserAsync(User);
                    var userName = user.UserName;

                    var isModOwner = await userManager.IsInRoleAsync(user, "Moderator") ||
                        await userManager.IsInRoleAsync(user, "Owner");

                    var queueSongs = await TableStorageService.RetrieveAllEntities<AzureTableSong>(Constants.QueueTableName);

                    if (queueSongs.Count > 0)
                    {
                        if (!isModOwner)
                        {
                            var friend = await TableStorageService.RetrieveEntity<Friends>(channel, "1", Constants.FriendsTableName);

                            if (friend != null)
                            {
                                var friendRequests = queueSongs.Count(x => x.RequestedById == channel);
                                if (friendRequests > 1)
                                    return RedirectToAction("Index", "SongsQueue", new { ac = "Error. You already have two song requests on the queue.", type = "danger" });
                            }
                            else
                            {
                                var userExists = queueSongs.Any(x => x.RequestedById == channel);
                                if (userExists)
                                    return RedirectToAction("Index", "SongsQueue", new { ac = "Error. You already have a song request on the queue.", type = "danger" });
                            }
                        }

                        var songExists = queueSongs.Any(x => x.RowKey == song.RowKey);
                        if (songExists)
                            return RedirectToAction("Index", "SongsQueue", new { ac = $"Error. That song is already on the queue.", type = "danger" });

                        queueSongs = queueSongs.OrderBy(x => x.Position).ToList();
                        position = queueSongs.Last().Position + 1;
                    }

                    var lastRequest = (song.LastTimeRequested.HasValue) 
                        ? MathFunctions.GetCooldownMinutes(song.LastTimeRequested.Value) : 100;

                    if (isModOwner || lastRequest > 60)
                    {
                        var updateSong = await TableStorageService.RetrieveEntity<AllSongs>(song.PartitionKey, song.RowKey, Constants.AllSongsTableName);
                        updateSong.Counter = song.Counter0 + 1;
                        updateSong.LastTimeRequested = DateTime.UtcNow;
                        await TableStorageService.MergeEntity(updateSong, Constants.AllSongsTableName);

                        var newSongQueue = new AzureTableSong()
                        {
                            PartitionKey = song.PartitionKey,
                            RowKey = song.RowKey,
                            Title = song.OriginalTitle,
                            OriginalGame = song.OriginalGame,
                            LowerCaseTitle = song.Title,
                            Channel = song.Channel,
                            VideoId = song.YouTubeLink.Replace("https://youtu.be/", string.Empty),
                            YouTubeLink = song.YouTubeLink,
                            Duration = song.Duration,
                            TotalTime = 0,
                            RecentlyAdded = song.RecentlyAdded,
                            RequestedBy = userName,
                            RequestedById = channel,
                            Counter = song.Counter0 + 1,
                            Likes = song.Likes0,
                            Position = position,
                            LastTimeRequested = song.LastTimeRequested
                        };

                        await TableStorageService.InsertEntity(newSongQueue, Constants.QueueTableName);
                        return RedirectToAction("Index", "SongsQueue", new { ac = $"The song was added to the queue in position {position}.", type = "success" });
                    }
                    else
                        return RedirectToAction("Index", "SongsQueue", new { ac = $"Error. That song is in cooldown. Please wait {60 - lastRequest} minutes to request it again.", type = "danger" });
                }
                else
                    return RedirectToAction("Index", "SongsQueue", new { ac = "Error. The song is currently playing.", type = "danger" });
            }
            else
                return RedirectToAction("Index", "SongsQueue", new { ac = "Error. The song does not exist.", type = "danger" });
        }
    }
}