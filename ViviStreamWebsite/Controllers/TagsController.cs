using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;
using ViviStreamWebsite.Models;
using ViviStreamWebsite.Services;
using X.PagedList;
using ViviStreamWebsite.Helpers;

namespace ViviStreamWebsite.Controllers
{
    public class TagsController : Controller
    {
        public async Task<ViewResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int? pageSize, string currentSort)
        {
            ViewBag.CurrentSort = sortOrder ?? currentSort;
            sortOrder = ViewBag.CurrentSort;

            ViewBag.SortName = (string.IsNullOrEmpty(sortOrder) ? "name_desc" : "");
            ViewBag.SortEmoji = ((sortOrder == "emoji") ? "emoji_desc" : "emoji");
            ViewBag.SortDescription = ((sortOrder == "description") ? "description_desc" : "description");

            var currentPageSize = pageSize.HasValue ? pageSize.Value : 10;
            ViewBag.CurrentPageSize = currentPageSize;

            if (!string.IsNullOrWhiteSpace(searchString))
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var tags = await TableStorageService.RetrieveAllEntities<Tags>(Constants.TagsTableName);

            var source = string.IsNullOrWhiteSpace(searchString)
                ? tags
                : tags.Where(x => x.Name.Contains(searchString) || x.Description.Contains(searchString));

            switch (sortOrder)
            {
                case "name_desc":
                    source = source.OrderByDescending(x => x.Name);
                    break;
                case "emoji":
                    source = source.OrderBy(x => x.Emoji).ThenBy(x => x.Name);
                    break;
                case "emoji_desc":
                    source = source.OrderByDescending(x => x.Emoji).ThenBy(x => x.Name);
                    break;
                case "description":
                    source = source.OrderBy(x => x.Description).ThenBy(x => x.Name);
                    break;
                case "description_desc":
                    source = source.OrderByDescending(x => x.Description).ThenBy(x => x.Name);
                    break;
                default:
                    source = source.OrderBy(x => x.Name);
                    break;
            }

            int pageNumber = page ?? 1;
            return View(new PageTags() { Tags = source.ToPagedList(pageNumber, currentPageSize) });
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Create(IFormCollection collection, Tags tag)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrWhiteSpace(collection["Name"]))
                    {
                        var tags = new Tags()
                        {
                            PartitionKey = Guid.NewGuid().ToString(),
                            RowKey = "1",
                            Name = collection["Name"],
                            Emoji = collection["Emoji"],
                            Description = collection["Description"]
                        };

                        await TableStorageService.InsertEntity(tags, Constants.TagsTableName);

                        return RedirectToAction("Index", "Tags", new { ac = "The tag was successfully added!", type = "success" });
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
                var tag = await TableStorageService.RetrieveEntity<Tags>(pk, rk, Constants.TagsTableName);

                tag.Name = collection["Name"];
                tag.Emoji = collection["Emoji"];
                tag.Description = collection["Description"];

                await TableStorageService.ReplaceEntity(tag, Constants.TagsTableName);
                return RedirectToAction("Index", "Tags", new { ac = "The tag was successfully edited!", type = "success" });
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
                var tag = await TableStorageService.RetrieveEntity<Tags>(pk, rk, Constants.TagsTableName);
                await TableStorageService.DeleteEntity(tag, Constants.TagsTableName);

                return RedirectToAction("Index", "Tags", new { ac = "The tag was successfully removed!", type = "success" });
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

        public async Task<ActionResult> Details(string pk, string rk, string tag)
        {
            if (!string.IsNullOrWhiteSpace(pk) && !string.IsNullOrWhiteSpace(rk))
            {
                return await RetrieveEntity(pk, rk);
            }
            else
            {
                return RedirectToAction("Index", "Tags", new { ac = "The tag doesn't exist!", type = "danger" });
            }
        }

        // Si se ocupa, para mostrar la vista al ingresar a Edit
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Edit(string pk, string rk)
        {
            return await RetrieveEntity(pk, rk);
        }

        // Si se ocupa, para mostrar la vista al ingresar a Delete
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Delete(string pk, string rk)
        {
            return await RetrieveEntity(pk, rk);
        }

        public async Task<ActionResult> RetrieveEntity(string pk, string rk)
        {
            var timer = await TableStorageService.RetrieveEntity<Tags>(pk, rk, Constants.TagsTableName);

            if (timer != null)
                return View(timer);
            else
                return RedirectToAction("Index", "Tags", new { ac = "The tag doesn't exist!", type = "danger" });
        }
    }
}