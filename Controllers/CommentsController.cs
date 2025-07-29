using BlogManagementApp.Data;
using BlogManagementApp.Models;
using BlogManagementApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementApp.Controllers
{
    public class CommentsController : Controller
    {
        private readonly BlogManagementDBContext _Context;
        public CommentsController(BlogManagementDBContext Context)
        {
            _Context = Context;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int BlogPostId,Comment comment)
        {
            if (ModelState.IsValid) {
                comment.BlogPostId = BlogPostId;
                comment.PostedOn = DateTime.UtcNow;
                _Context.Comments.Add(comment);
                await _Context.SaveChangesAsync();
                var post = await _Context.BlogPosts.FindAsync(BlogPostId);
                return RedirectToAction("Details", "BlogPosts", new { slug = post?.Slug });
            }
            var BlogPost = await _Context.BlogPosts.Include(b=>b.Author).Include(b=>b.Comments).FirstOrDefaultAsync(b=>b.Id == BlogPostId);
            if (BlogPost == null)
            {
                ViewBag.ErrorMessage = "Blog Post Not Found";
                return View("Error");
            }
            var viewModel = new BlogPostDetailsViewModel
            {
                BlogPost = BlogPost,
                Comment = comment
            };
            return View("../BlogPosts/Details", viewModel);
        }
}
