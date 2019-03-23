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
    [Authorize(Roles = "Moderator,Owner")]
    public class QuickEditController : Controller
    {
        private UserManager<ApplicationUser> userManager;
        IConfiguration configuration;

        public QuickEditController(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            this.configuration = configuration;
            this.userManager = userManager;
        }

        public async Task<ViewResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int? pageSize, string currentSort, bool? checkNew)
        {
            ViewBag.CurrentSort = sortOrder ?? currentSort;
            sortOrder = ViewBag.CurrentSort;

            ViewBag.SortTitle = (string.IsNullOrEmpty(sortOrder) ? "title_desc" : "");
            ViewBag.SortGame = ((sortOrder == "game") ? "game_desc" : "game");
            ViewBag.SortNumber = ((sortOrder == "number") ? "number_desc" : "number");

            var currentPageSize = pageSize.HasValue ? pageSize.Value : 10;
            ViewBag.CurrentPageSize = currentPageSize;

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                if (!page.HasValue)
                    page = 1;
            }
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
                case "game":
                    source = source.OrderBy(x => x.OriginalGame).ThenBy(x => x.OriginalTitle);
                    break;
                case "game_desc":
                    source = source.OrderByDescending(x => x.OriginalGame).ThenBy(x => x.OriginalTitle);
                    break;
                case "number":
                    source = source.OrderBy(x => x.RowKey);
                    break;
                case "number_desc":
                    source = source.OrderByDescending(x => x.RowKey);
                    break;
                default:
                    //no va para respetar la busqueda source = source.OrderBy(x => x.OriginalTitle);
                    break;
            }

            var pageNumber = page ?? 1;
            ViewBag.Page = pageNumber;
            return View(new PageSongs() { AllSongs = source.ToPagedList(pageNumber, currentPageSize) });
        }

        public async Task<ActionResult> Save(IFormCollection collection, int items, string sortOrder, string currentFilter, string searchString, int? page, int? pageSize, string currentSort, bool? checkNew)
        {
            var ac = string.Empty;
            var type = string.Empty;

            try
            {
                var table = TableStorageService.ConnectToTable(Constants.AllSongsTableName);

                for (int i = 0; i < items; i++)
                {
                    string pk = collection[$"pk_{i}"];
                    string rk = collection[$"rk_{i}"];
                    string title = collection[$"Title_{i}"];
                    string game = collection[$"Game_{i}"];

                    var operation = TableOperation.Retrieve<AllSongs>(pk, rk);

                    var song = (AllSongs)(await table.ExecuteAsync(operation)).Result;
                    song.ETag = "*";
                    song.OriginalTitle = title;
                    song.Title = StringFunctions.ReplaceChars(title.ToLower());
                    song.OriginalGame = game;
                    song.Game = game.ToLower();

                    var replace = TableOperation.Merge(song);
                    await table.ExecuteAsync(replace);
                }

                ac = "The playlist was successfully edited!";
                type = "success";

            }
            catch (Exception ex)
            {
                ac = "There was an error updating the entries!";
                type = "danger";
            }

            // falta mandar los parametros
            //It is recommended you return the view is ModelState is not valid so that the user can correct any errors.
            return RedirectToAction("Index", "QuickEdit", new { ac, type, sortOrder, currentFilter, searchString, page, pageSize, currentSort, checkNew });
        }
    }
}