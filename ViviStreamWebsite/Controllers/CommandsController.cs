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
    public class CommandsController : Controller
    {
        public async Task<ViewResult> Index(string sortOrder, string currentFilter, string searchString, int? page, int? pageSize, string currentSort)
        {
            ViewBag.CurrentSort = sortOrder ?? currentSort;
            sortOrder = ViewBag.CurrentSort;

            ViewBag.SortCommand = (string.IsNullOrEmpty(sortOrder) ? "command_desc" : "");
            ViewBag.SortStatus = ((sortOrder == "status") ? "status_desc" : "status");
            ViewBag.SortMessage = ((sortOrder == "message") ? "message_desc" : "message");
            ViewBag.SortUserlevel = ((sortOrder == "userlevel") ? "userlevel_desc" : "userlevel");
            
            var currentPageSize = pageSize.HasValue ? pageSize.Value : 10;
            ViewBag.CurrentPageSize = currentPageSize;

            if (!string.IsNullOrWhiteSpace(searchString))
                page = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var commands = FileService.ReadCommands();
            var customCommands = await TableStorageService.RetrieveAllEntities<CustomCommands>(Constants.CustomCommandsTableName);
            commands = commands.Union(customCommands).ToList();

            var source = string.IsNullOrWhiteSpace(searchString)
                ? commands
                : commands.Where(x => x.Command.Contains(searchString) || x.Message.Contains(searchString));

            switch (sortOrder)
            {
                case "command_desc":
                    source = source.OrderByDescending(x => x.Command);
                    break;
                case "status":
                    source = source.OrderBy(x => x.IsEnabled).ThenBy(x => x.Command);
                    break;
                case "status_desc":
                    source = source.OrderByDescending(x => x.IsEnabled).ThenBy(x => x.Command);
                    break;
                case "message":
                    source = source.OrderBy(x => x.Message).ThenBy(x => x.Command);
                    break;
                case "message_desc":
                    source = source.OrderByDescending(x => x.Message).ThenBy(x => x.Command);
                    break;
                case "userlevel":
                    source = source.OrderBy(x => x.Userlevel).ThenBy(x => x.Command);
                    break;
                case "userlevel_desc":
                    source = source.OrderByDescending(x => x.Userlevel).ThenBy(x => x.Command);
                    break;
                default:
                    source = source.OrderBy(x => x.Command);
                    break;
            }

            int pageNumber = page ?? 1;
            return View(new PageCommands() { Commands = source.ToPagedList(pageNumber, currentPageSize) });
        }

        [HttpPost]
        [Authorize(Roles = "Moderator,Owner")]
        public async Task<ActionResult> Create(IFormCollection collection, CustomCommands cmd)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (!string.IsNullOrWhiteSpace(collection["Command"]))
                    {
                        var customCommands = new CustomCommands()
                        {
                            PartitionKey = Guid.NewGuid().ToString(),
                            RowKey = "1",
                            Alias = collection["Alias"],
                            Command = collection["Command"],
                            Cooldown = int.Parse(collection["Cooldown"]),
                            Message = collection["Message"],
                            Userlevel = collection["Userlevel"],
                            Description = collection["Description"],
                            IsEnabled = bool.Parse(collection["IsEnabled"])
                        };

                        await TableStorageService.InsertEntity(customCommands, Constants.CustomCommandsTableName);

                        return RedirectToAction("Index", "Commands", new { ac = "The command was successfully added!", type = "success" });
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
                var command = await TableStorageService.RetrieveEntity<CustomCommands>(pk, rk, Constants.CustomCommandsTableName);

                command.Alias = collection["Alias"];
                command.Command = collection["Command"];
                command.Cooldown = int.Parse(collection["Cooldown"]);
                command.Message = collection["Message"];
                command.Userlevel = collection["Userlevel"];
                command.Description = collection["Description"];
                command.IsEnabled = bool.Parse(collection["IsEnabled"]);

                await TableStorageService.ReplaceEntity(command, Constants.CustomCommandsTableName);
                return RedirectToAction("Index", "Commands", new { ac = "The command was successfully edited!", type = "success" });
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
                var command = await TableStorageService.RetrieveEntity<CustomCommands>(pk, rk, Constants.CustomCommandsTableName);
                await TableStorageService.DeleteEntity(command, Constants.CustomCommandsTableName);

                return RedirectToAction("Index", "Commands", new { ac = "The command was successfully removed!", type = "success" });
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

        public async Task<ActionResult> Details(string pk, string rk, string command)
        {
            if (!string.IsNullOrWhiteSpace(pk) && !string.IsNullOrWhiteSpace(rk))
            {
                return await RetrieveEntity(pk, rk);
            }
            else
            {
                var commands = FileService.ReadCommands();
                var query = commands.Where(x => x.Command == command).FirstOrDefault();

                if (query != null)
                    return View(query as CustomCommands);
                else
                    return RedirectToAction("Index", "Commands", new { ac = "The command doesn't exist!", type = "danger" });
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
            var timer = await TableStorageService.RetrieveEntity<CustomCommands>(pk, rk, Constants.CustomCommandsTableName);

            if (timer != null)
                return View(timer);
            else
                return RedirectToAction("Index", "Commands", new { ac = "The command doesn't exist!", type = "danger" });
        }
    }
}