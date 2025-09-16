using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfoPoint.Areas.LearningWalks.Controllers
{
    [Area("LearningWalks")]
    [Authorize]
    public class LearningWalksController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}