using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ViviStreamWebsite.Models;
using ViviStreamWebsite.Services;
using ViviStreamWebsite.Helpers;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace ViviStreamWebsite.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var info = await TableStorageService.RetrieveEntity<StreamInfo>(Constants.StreamPartitionKey, "1", Constants.StreamInfoTableName);

            if (info != null)
                return View(info);
            else
                return View(new StreamInfo());
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
