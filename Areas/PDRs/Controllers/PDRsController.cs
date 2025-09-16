using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfoPoint.Areas.PDRs.Controllers
{
    [Area("PDRs")]
    [Authorize]
    public class PDRsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}