using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViviStreamWebsite.Models;
using ViviStreamWebsite.Services;
using ViviStreamWebsite.Helpers;
using X.PagedList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;

namespace ViviStreamWebsite.Controllers
{
    public class PlaylistController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        IConfiguration configuration;

        public PlaylistController(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            this.configuration = configuration;
            this.userManager = userManager;
        }

        public async Task<ViewResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int? pageSize, string currentSort, bool? checkNew)
        {
            ViewBag.CurrentSort = sortOrder ?? currentSort;
            sortOrder = ViewBag.CurrentSort;

            ViewBag.SortTitle = (string.IsNullOrEmpty(sortOrder) ? "title_desc" : "");
            ViewBag.SortDuration = ((sortOrder == "duration") ? "duration_desc" : "duration");
            ViewBag.SortCounter = ((sortOrder == "counter") ? "counter_desc" : "counter");
            ViewBag.SortArtist = ((sortOrder == "artist") ? "artist_desc" : "artist");
            ViewBag.SortNumber = ((sortOrder == "number") ? "number_desc" : "number");

            var currentPageSize = pageSize.HasValue ? pageSize.Value : 10;
            ViewBag.CurrentPageSize = currentPageSize;

            if (!string.IsNullOrWhiteSpace(searchString))
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            ViewBag.CheckNew = checkNew;

            var playlist = await TableStorageService.RetrieveAllEntities<AllSongs>(Constants.AllSongsTableName);

            var source = string.IsNullOrWhiteSpace(searchString)
                ? playlist
                : EasyCustomSearch.SearchSong(searchString, playlist);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var num = -1;

                if (int.TryParse(searchString, out num))
                {
                    var song = playlist.Where(x => x.RowKey == searchString || x.RowKey == num.ToString("D4"));
                    if (song != null)
                    {
                        if (!source.Any(x => x.RowKey == searchString || x.RowKey == num.ToString("D4")))
                            source = source.Union(song);
                    }
                }
            }

            if (checkNew.HasValue)
                if (checkNew.Value)
                    source = source.Where(x => x.Game == "NEW" || x.RecentlyAdded == "✓");

            switch (sortOrder)
            {
                case "title_desc":
                    source = source.OrderByDescending(x => x.OriginalTitle);
                    break;
                case "duration":
                    source = source.OrderBy(x => x.Duration).ThenBy(x => x.OriginalTitle);
                    break;
                case "duration_desc":
                    source = source.OrderByDescending(x => x.Duration).ThenBy(x => x.OriginalTitle);
                    break;
                case "counter":
                    source = source.OrderBy(x => x.Counter).ThenBy(x => x.OriginalTitle);
                    break;
                case "counter_desc":
                    source = source.OrderByDescending(x => x.Counter).ThenBy(x => x.OriginalTitle);
                    break;
                case "artist":
                    source = source.OrderBy(x => x.Channel).ThenBy(x => x.OriginalTitle);
                    break;
                case "artist_desc":
                    source = source.OrderByDescending(x => x.Channel).ThenBy(x => x.OriginalTitle);
                    break;
                case "number":
                    source = source.OrderBy(x => x.RowKey);
                    break;
                case "number_desc":
                    source = source.OrderByDescending(x => x.RowKey);
                    break;
                default:
                    // no va para respetar la busqueda source = source.OrderBy(x => x.OriginalTitle);
                    break;
            }

            var pageNumber = page ?? 1;
            return View(new PageSongs() { AllSongs = source.ToPagedList(pageNumber, currentPageSize) });
        }

        public async Task<ActionResult> Details(string pk, string rk)
        {
            var song = await TableStorageService.RetrieveEntity<AllSongs>(pk, rk, Constants.AllSongsTableName);

            if (song != null)
                return View(song);
            else
                return RedirectToAction("Index", "Playlist", new { ac = "The song doesn't exist!", type = "danger" });
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> GetSongInfo(IFormCollection collection, string pk = "", string rk = "")
        {
            var newSong = new NewSong();

            if (!string.IsNullOrWhiteSpace(collection["YTLink"]))
            {
                var ytLink = collection["YTLink"].ToString();
                var song = await YouTubeApiService.GetSongInfo(configuration, ytLink);
                var songExists = false;

                var youTubeLink = collection["YTLink"].ToString();
                var videoID = YouTubeApiService.GetYouTubeVideoID(youTubeLink);

                var table = TableStorageService.ConnectToTable(Constants.AllSongsTableName);
                TableContinuationToken tableContinuationToken = null;

                do
                {
                    var tableQuerySegment = await table.ExecuteQuerySegmentedAsync(new TableQuery<AllSongs>(), tableContinuationToken);
                    var songs = tableQuerySegment.Results;
                    songExists = songs.Any(x => x.YouTubeLink.Contains(videoID) && x.RowKey != rk);
                    tableContinuationToken = tableQuerySegment.ContinuationToken;
                } while (tableContinuationToken != null && !songExists);

                if (!songExists)
                {
                    newSong.OriginalGame = song.OriginalGame;
                    newSong.OriginalTitle = song.OriginalTitle;
                    newSong.YouTubeLink = song.YouTubeLink;
                    newSong.Channel = song.Channel;
                    newSong.Duration = song.Duration;
                    newSong.YTLink = ytLink;
                    newSong.PartitionKey = pk;
                    newSong.RowKey = rk;

                    //TODO: tags para dos casos: new y edit
                }
                else
                    return RedirectToAction("Index", "Playlist", new { ac = "The YouTube video URL already exists in the playlist!", type = "danger" });
            }

            if (string.IsNullOrWhiteSpace(pk) || string.IsNullOrWhiteSpace(rk))
                return View("Create", newSong);
            else
                return View("Edit", newSong);
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Create(IFormCollection collection, AllSongs song)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var youTubeLink = collection["YouTubeLink"].ToString();

                    if (!string.IsNullOrWhiteSpace(youTubeLink))
                    {
                        var table = TableStorageService.ConnectToTable(Constants.AllSongsTableName);
                        TableContinuationToken tableContinuationToken = null;
                        int maxKey = 0;
                        var songExists = false;

                        do
                        {
                            var tableQuerySegment = await table.ExecuteQuerySegmentedAsync(new TableQuery<AllSongs>(), tableContinuationToken);
                            var songs = tableQuerySegment.Results;

                            var max = songs.Max(x => x.RowKey);
                            maxKey = Math.Max(maxKey, int.Parse(max));

                            tableContinuationToken = tableQuerySegment.ContinuationToken;
                            songExists = songs.Any(x => x.YouTubeLink == youTubeLink);
                        }
                        while (tableContinuationToken != null && !songExists);

                        if (!songExists)
                        {

                            var i = (maxKey + 1).ToString("D4");
                            string title = collection["OriginalTitle"];
                            string game = collection["OriginalGame"];

                            var newSong = new AllSongs()
                            {
                                RowKey = i,
                                OriginalGame = game,
                                Game = game.ToLower(),
                                OriginalTitle = title,
                                PartitionKey = StringFunctions.ReplaceChars(game.ToLower()),
                                Title = StringFunctions.ReplaceChars(title.ToLower()),
                                Channel = collection["Channel"],
                                Duration = collection["Duration"],
                                YouTubeLink = youTubeLink,
                                LastTimeRequested = null,
                                RecentlyAdded = "✓",
                                Counter = 0,
                                Likes = 0
                            };

                            await TableStorageService.InsertEntity(newSong, Constants.AllSongsTableName);

                            //TODO: add tags from webpage

                            return RedirectToAction("Index", "Playlist", new { ac = "The song was successfully added!", type = "success" });
                        }
                        else
                        {
                            return RedirectToAction("Index", "Playlist", new { ac = "The YouTube video URL already exists in the playlist!", type = "danger" });
                        }
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return View();
                }
            }
            catch (Exception)
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Edit(string pk, string rk, IFormCollection collection)
        {
            try
            {
                string title = collection["OriginalTitle"];
                string game = collection["OriginalGame"];

                var song = await TableStorageService.RetrieveEntity<AllSongs>(pk, rk, Constants.AllSongsTableName);

                song.OriginalGame = game;
                song.Game = game.ToLower();
                song.OriginalTitle = title;
                song.Title = StringFunctions.ReplaceChars(title.ToLower());
                song.Channel = collection["Channel"];
                song.Duration = collection["Duration"];
                song.YouTubeLink = collection["YouTubeLink"];
                song.RecentlyAdded = "✓";

                await TableStorageService.MergeEntity(song, Constants.AllSongsTableName);

                //TODO: add tag songs

                return RedirectToAction("Index", "Playlist", new { ac = "The song was successfully edited!", type = "success" });
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Delete(string pk, string rk, IFormCollection collection)
        {
            try
            {
                var song = await TableStorageService.RetrieveEntity<AllSongs>(pk, rk, Constants.AllSongsTableName);
                await TableStorageService.DeleteEntity(song, Constants.AllSongsTableName);

                return RedirectToAction("Index", "Playlist", new { ac = "The song was successfully removed!", type = "success" });
            }
            catch
            {
                return View();
            }
        }

        // Si se ocupa, para mostrar la vista al ingresar a Create
        [Authorize(Roles = "Moderator,Owner")]
        public ActionResult Create()
        {
            return View();
        }

        // Si se ocupa, para mostrar la vista al ingresar a Edit
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Edit(string pk, string rk)
        {
            var song = await TableStorageService.RetrieveEntity<AllSongs>(pk, rk, Constants.AllSongsTableName);

            if (song != null)
            {
                var editSong = new NewSong()
                {
                    PartitionKey = song.PartitionKey,
                    RowKey = song.RowKey,
                    Channel = song.Channel,
                    Duration = song.Duration,
                    OriginalGame = song.OriginalGame,
                    OriginalTitle = song.OriginalTitle,
                    YouTubeLink = song.YouTubeLink
                };

                //TODO: Get tags for this song

                return View(editSong);
            }
            else
                return RedirectToAction("Index", "Playlist", new { ac = "The song doesn't exist!", type = "danger" });
        }

        // Si se ocupa, para mostrar la vista al ingresar a Delete
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Delete(string pk, string rk)
        {
            return await RetrieveEntity(pk, rk);
        }

        public async Task<ActionResult> RetrieveEntity(string pk, string rk)
        {
            var song = await TableStorageService.RetrieveEntity<AllSongs>(pk, rk, Constants.AllSongsTableName);

            if (song != null)
                return View(song);
            else
                return RedirectToAction("Index", "Playlist", new { ac = "The song doesn't exist!", type = "danger" });
        }

        public async Task<ActionResult> RequestSong(string pk, string rk)
        {
            var tableSong = TableStorageService.ConnectToTable("CurrentSong");
            var queryCS = await tableSong.ExecuteQuerySegmentedAsync(new TableQuery<AzureTableSong>(), null);
            var currentSong = queryCS.Results.FirstOrDefault();

            if (currentSong.RowKey != rk)
            {
                var song = await TableStorageService.RetrieveEntity<AllSongs>(pk, rk, Constants.AllSongsTableName);

                if (song != null)
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
                                    return RedirectToAction("Index", "Playlist", new { ac = "Error. You already have two song requests on the queue.", type = "danger" });
                            }
                            else
                            {
                                var userExists = queueSongs.Any(x => x.RequestedById == channel);
                                if (userExists)
                                    return RedirectToAction("Index", "Playlist", new { ac = "Error. You already have a song request on the queue.", type = "danger" });
                            }
                        }

                        var songExists = queueSongs.Any(x => x.RowKey == rk);
                        if (songExists)
                            return RedirectToAction("Index", "Playlist", new { ac = $"Error. That song is already on the queue.", type = "danger" });

                        queueSongs = queueSongs.OrderBy(x => x.Position).ToList();
                        position = queueSongs.Last().Position + 1;
                    }

                    var lastRequest = (song.LastTimeRequested.HasValue)
                        ? MathFunctions.GetCooldownMinutes(song.LastTimeRequested.Value) : 100;

                    if (isModOwner || lastRequest > 60)
                    {
                        var updateSong = new AllSongs()
                        {
                            PartitionKey = song.PartitionKey,
                            RowKey = song.RowKey,
                            Counter = song.Counter0 + 1,
                            LastTimeRequested = DateTime.UtcNow
                        };

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
                        return RedirectToAction("Index", "Playlist", new { ac = $"Error. That song is in cooldown. Please wait {60 - lastRequest} minutes to request it again.", type = "danger" });
                }

                return RedirectToAction("Index", "Playlist", new { ac = "Error. The song does not exist.", type = "danger" });
            }
            else
                return RedirectToAction("Index", "Playlist", new { ac = "Error. The song is currently playing.", type = "danger" });
        }
    }
}