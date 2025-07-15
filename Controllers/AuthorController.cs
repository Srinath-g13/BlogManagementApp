using BlogManagementApp.Data;
using Microsoft.AspNetCore.Mvc;

namespace BlogManagementApp.Controllers
{
    public class AuthorController:Controller
    {
        private readonly BlogManagementDBContext? _blogManagementDB;

        public AuthorController(BlogManagementDBContext blogManegamentDB)
        {
           _blogManagementDB = blogManegamentDB;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
    }
}  

