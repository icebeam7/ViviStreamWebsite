using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViviStreamWebsite.Models;
using ViviStreamWebsite.Services;
using X.PagedList;
using ViviStreamWebsite.Helpers;

namespace ViviStreamWebsite.Controllers
{
    [Authorize(Roles = "Moderator,Owner")]
    public class BotTimersController : Controller
    {
        public async Task<ViewResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int? pageSize, string currentSort)
        {
            ViewBag.CurrentSort = sortOrder ?? currentSort;
            sortOrder = ViewBag.CurrentSort;

            ViewBag.SortName = (string.IsNullOrEmpty(sortOrder) ? "name_desc" : "");
            ViewBag.SortStatus = ((sortOrder == "status") ? "status_desc" : "status");

            var currentPageSize = pageSize.HasValue ? pageSize.Value : 10;
            ViewBag.CurrentPageSize = currentPageSize;

            if (!string.IsNullOrWhiteSpace(searchString))
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var botTimers = await TableStorageService.RetrieveAllEntities<BotTimers>(Constants.BotTimersTableName);

            var source = string.IsNullOrWhiteSpace(searchString)
                ? botTimers
                : botTimers.Where(x => x.Name.Contains(searchString) || x.Message.Contains(searchString));

            switch (sortOrder)
            {
                case "name_desc":
                    source = source.OrderByDescending(x => x.Name);
                    break;
                case "status":
                    source = source.OrderBy(x => x.Status).ThenBy(x => x.Name);
                    break;
                case "status_desc":
                    source = source.OrderByDescending(x => x.Status).ThenBy(x => x.Name);
                    break;
                default:
                    source = source.OrderBy(x => x.Name);
                    break;
            }
            
            int pageNumber = page ?? 1;
            return View(new PageTimers() { Bot_Timers = source.ToPagedList(pageNumber, currentPageSize) });
        }

        [HttpPost]
        public async Task<ActionResult> Create(IFormCollection collection)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var message = collection["Message"].ToString();

                    if (message.Length > 200)
                        message = message.Substring(0, 200);

                    var botTimers = new BotTimers()
                    {
                        PartitionKey = Guid.NewGuid().ToString(),
                        RowKey = "1",
                        Name = collection["Name"],
                        Message = message,
                        Interval = int.Parse(collection["Interval"]),
                        ChatLines = int.Parse(collection["ChatLines"]),
                        Alias = collection["Alias"],
                        Status = bool.Parse(collection["Status"]),
                        IsVisible = bool.Parse(collection["IsVisible"])
                    };

                    await TableStorageService.InsertEntity(botTimers, Constants.BotTimersTableName);
                    return RedirectToAction("Index", "BotTimers", new { ac = "Timer successfully created!", type = "success" });
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
        public async Task<ActionResult> Edit(string pk, string rk, IFormCollection collection)
        {
            try
            {
                var message = collection["Message"].ToString();

                if (message.Length > 200)
                    message = message.Substring(0, 200);

                var timer = await TableStorageService.RetrieveEntity<BotTimers>(pk, rk, Constants.BotTimersTableName);
                timer.Name = collection["Name"];
                timer.Message = message;
                timer.Interval = int.Parse(collection["Interval"]);
                timer.ChatLines = int.Parse(collection["ChatLines"]);
                timer.Alias = collection["Alias"];
                timer.Status = bool.Parse(collection["Status"]);
                timer.IsVisible = bool.Parse(collection["IsVisible"]);

                await TableStorageService.ReplaceEntity(timer, Constants.BotTimersTableName);
                return RedirectToAction("Index", "BotTimers", new { ac = "The timer was successfully edited!", type = "success" });
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string pk, string rk, IFormCollection collection)
        {
            try
            {
                var timer = await TableStorageService.RetrieveEntity<BotTimers>(pk, rk, Constants.BotTimersTableName);
                await TableStorageService.DeleteEntity(timer, Constants.BotTimersTableName);

                return RedirectToAction("Index", "BotTimers", new { ac = "The timer was successfully removed!", type = "success" });
            }
            catch
            {
                return View();
            }
        }

        // Si se ocupa, para mostrar la vista al ingresar a Create
        public ActionResult Create()
        {
            return View();
        }

        public async Task<ActionResult> Details(string pk, string rk)
        {
            return await RetrieveEntity(pk, rk);
        }

        // Si se ocupa, para mostrar la vista al ingresar a Edit
        public async Task<ActionResult> Edit(string pk, string rk)
        {
            return await RetrieveEntity(pk, rk);
        }

        // Si se ocupa, para mostrar la vista al ingresar a Delete
        public async Task<ActionResult> Delete(string pk, string rk)
        {
            return await RetrieveEntity(pk, rk);
        }

        public async Task<ActionResult> RetrieveEntity(string pk, string rk)
        {
            var timer = await TableStorageService.RetrieveEntity<BotTimers>(pk, rk, Constants.BotTimersTableName);

            if (timer != null)
                return View(timer);
            else
                return RedirectToAction("Index", "BotTimers", new { ac = "The timer doesn't exist!", type = "danger" });
        }
    }
}