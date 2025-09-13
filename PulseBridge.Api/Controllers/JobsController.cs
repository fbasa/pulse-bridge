using Microsoft.AspNetCore.Mvc;

namespace PulseBridge.Api.Controllers
{
    public class JobsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
