
using BlogManagementApp.Service;
using Microsoft.AspNetCore.Mvc;

namespace BlogManagementApp.Controllers
{
    public class CommentController : Controller
    {
        private readonly IBlackListService _blackListService;

        public CommentController(IBlackListService blackListService)
        {
            _blackListService = blackListService;
        }

        public object BlogPost { get; internal set; }

        [HttpPost]
        public IActionResult PostComment(String comment)
        {
            if (_blackListService.ContainsBlackListedWord(comment))
            {
                ModelState.AddModelError("comment", "Your Comment Contains Inappropriate Words");
                return View();
            }
            return RedirectToAction("Index");
        }
    }
}
