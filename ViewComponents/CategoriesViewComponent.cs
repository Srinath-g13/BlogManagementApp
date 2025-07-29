using BlogManagementApp.Data;
using BlogManagementApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementApp.ViewComponents
{
    public class CategoriesViewComponent:ViewComponent
    {
        private readonly BlogManagementDBContext _Context;

        public CategoriesViewComponent(BlogManagementDBContext Context)
        {
            _Context = Context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            List<Category> categories= await _Context.Categories.OrderBy(c=>c.Name).ToListAsync();

            return View(categories);
        }
    }
}
