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

namespace ViviStreamWebsite.Controllers
{
    [Authorize]
    public class MyPlaylistController : Controller
    {
        public async Task<ViewResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int? pageSize, string currentSort)
        {
            ViewBag.CurrentSort = sortOrder ?? currentSort;
            sortOrder = ViewBag.CurrentSort;

            ViewBag.SortTitle = (string.IsNullOrEmpty(sortOrder) ? "title_desc" : "");
            ViewBag.SortDuration = ((sortOrder == "duration") ? "duration_desc" : "duration");
            ViewBag.SortArtist = ((sortOrder == "artist") ? "artist_desc" : "artist");

            var currentPageSize = pageSize.HasValue ? pageSize.Value : 10;
            ViewBag.CurrentPageSize = currentPageSize;

            if (!string.IsNullOrWhiteSpace(searchString))
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var playlist = new List<MySongs>();

            var table = TableStorageService.ConnectToTable(Constants.MySongsTableName);
            TableContinuationToken tableContinuationToken = null;

            var channel = CookieService.Get(Request, Constants.ChannelCookieName);

            var filter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, channel);
            var query = new TableQuery<MySongs>().Where(filter);

            do
            {
                var tableQuerySegment = await table.ExecuteQuerySegmentedAsync(query, tableContinuationToken);
                playlist.AddRange(tableQuerySegment.Results);
                tableContinuationToken = tableQuerySegment.ContinuationToken;
            }
            while (tableContinuationToken != null);

            var source = string.IsNullOrWhiteSpace(searchString)
                ? playlist
                //: playlist.Where(x => x.Title.Contains(searchString));
                : EasyCustomSearch.SearchSong(searchString, playlist);

            switch (sortOrder)
            {
                case "title_desc":
                    source = source.OrderByDescending(x => x.Title);
                    break;
                case "duration":
                    source = source.OrderBy(x => x.Duration).ThenBy(x => x.Title);
                    break;
                case "duration_desc":
                    source = source.OrderByDescending(x => x.Duration).ThenBy(x => x.Title);
                    break;
                case "artist":
                    source = source.OrderBy(x => x.Channel).ThenBy(x => x.Title);
                    break;
                case "artist_desc":
                    source = source.OrderByDescending(x => x.Channel).ThenBy(x => x.Title);
                    break;
                default:
                    source = source.OrderBy(x => x.Title);
                    break;
            }

            var pageNumber = page ?? 1;
            return View(new MyPageSongs() { MySongs = source.ToPagedList(pageNumber, currentPageSize) });
        }

        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string pk, string rk)
        {
            try
            {
                var song = await TableStorageService.RetrieveEntity<MySongs>(pk, rk, Constants.MySongsTableName);
                await TableStorageService.DeleteEntity(song, Constants.MySongsTableName);

                return RedirectToAction("Index", "MyPlaylist", new { ac = "The song was successfully removed!", type = "success" });
            }
            catch
            {
                return View();
            }
        }

        public async Task<ActionResult> Details(string pk, string rk)
        {
            var song = await TableStorageService.RetrieveEntity<MySongs>(pk, rk, Constants.MySongsTableName);

            if (song != null)
                return View(song);
            else
                return RedirectToAction("Index", "MyPlaylist", new { ac = "The song doesn't exist!", type = "danger" });
        }
    }
}