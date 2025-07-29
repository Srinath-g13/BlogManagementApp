using BlogManagementApp.Data;
using BlogManagementApp.Models;
using BlogManagementApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementApp.Controllers
{
    public class BlogPostsController : Controller
    {
        private readonly BlogManagementDBContext _Context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public BlogPostsController(BlogManagementDBContext context, IWebHostEnvironment environment, IConfiguration configuration)
        {
            _Context = context;
            _environment = environment;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index(String? searchTitle, int? SearchCategoryId, int? PageNumber)
        {
            try
            {
                int pageSize = _configuration.GetValue<int?>("Pagination:PageSize") ?? 10;
                var categories = await _Context.Categories.OrderBy(c => c.Name).ToListAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Name");

                var Postsquery = _Context.BlogPosts.Include(b => b.Author).Include(b => b.Category).AsQueryable();
                if (!string.IsNullOrEmpty(searchTitle))
                {
                    Postsquery = Postsquery.Where(b => b.Title.Contains(searchTitle));
                }
                if (SearchCategoryId.HasValue && SearchCategoryId.Value != 0)
                {
                    Postsquery = Postsquery.Where(b => b.CategoryId == SearchCategoryId.Value);
                }
                Postsquery = Postsquery.OrderByDescending(b => b.PublishedOn);
                int TotalPosts = await Postsquery.CountAsync();

                int TotalPages = (int)Math.Ceiling(TotalPosts / (double)pageSize);
                TotalPages = TotalPages < 1 ? 1 : TotalPages;

                PageNumber = PageNumber.HasValue && PageNumber.Value > 0 ? PageNumber.Value : 1;
                PageNumber = PageNumber > TotalPages ? TotalPages : PageNumber;


                var posts = await Postsquery
                .Skip((PageNumber.Value - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

                var viewModel = new BlogPostIndexViewModel
                {
                    Posts = posts,
                    CurrentPage = PageNumber.Value,
                    TotalPages = TotalPages,
                    SearchTitle = searchTitle,
                    SearchCategoryId = SearchCategoryId ?? 0,
                };


                return View(viewModel);
            }

            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Unable to load blog posts. Please try again later.";
                return View("Error");

            }
        }
        [HttpGet]
        [Route("/blog/{slug}")]
        public async Task<IActionResult> Details(String slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                ViewBag.ErrorMessage = "Slug not provided";
                return View("Error");
            }
            try
            {
                var BlogPost = await _Context.BlogPosts.Include(b => b.Author).Include(b => b.Category).Include(b => b.Comments).FirstOrDefaultAsync(m => m.Slug == slug);
                if (BlogPost == null)
                {
                    ViewBag.ErrorMessage = "Blog post not found.";
                    return View("Error");
                }
                BlogPost.Views = BlogPost.Views + 1;
                await _Context.SaveChangesAsync();

                ViewBag.MetaDescription = BlogPost.MetaDescription;
                ViewBag.MetaKeyword = BlogPost.MetaKeyword;
                ViewBag.Title = BlogPost.MetaTitle ?? BlogPost.Title;
                var viewModel = new BlogPostDetailsViewModel
                {
                    BlogPost = BlogPost,
                    Comment = new Comment()
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An error occurred while loading the blog post details.";
                return View("Error");
            }
            return View();
        }
        [HttpGet("/Categories/{id}/Posts")]
        public async Task<IActionResult> PostsByCategory(int Id, int? PageNumber)
        {
            var category = await _Context.Categories.FindAsync(Id);
            if (category == null)
            {
                ViewBag.ErrorMessage = "Invalid Category";
                return View("Error");
            }
            int pagesize = _configuration.GetValue<int?>("pagination:PageSize") ?? 10;
            var postquery = _Context.BlogPosts.Where(b => b.CategoryId == Id).Include(b => b.Author).Include(b => b.Category).OrderByDescending(b => b.PublishedOn).AsQueryable();
            int totalposts = await postquery.CountAsync();
            int totalpages = (int)Math.Ceiling(totalposts / (double)pagesize);
            totalpages = totalpages < 1 ? 1 : totalpages;
            PageNumber = PageNumber.HasValue && PageNumber.Value > 0 ? PageNumber.Value : 1;
            PageNumber = PageNumber > totalpages ? totalpages : PageNumber;
            var post = await postquery.Skip((PageNumber.Value - 1) * pagesize).Take(pagesize).ToListAsync();
            var viewModel = new CategoryPostsViewsModel
            {
                Posts = post,
                CurrentPage = PageNumber.Value,
                TotalPage = totalpages,
                CategoryName = category.Name,
                CategoryId = category.Id
            };
            return View("CategoryPosts", viewModel);
        }
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Author = await _Context.Authors.ToListAsync();
                ViewBag.Categories = await _Context.Categories.OrderBy(c => c.Name).ToListAsync();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Unable to load the create blog post form. Please try again later.";
                return View("Error");
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPost blogPost, IFormFile? FeaturedImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    blogPost.FeaturedImage = await UploadFeaturedImageAsync(FeaturedImage) ?? blogPost.FeaturedImage;
                    blogPost.Slug = blogPost.Slug;
                    blogPost.Slug = string.IsNullOrEmpty(blogPost.Slug) ? await GenerateSlugAsync(blogPost.Title) : blogPost.Slug;

                    if (await _Context.BlogPosts.AnyAsync(b => b.Slug == blogPost.Slug))
                    {
                        ModelState.AddModelError("Slug", "The slug must be unique");
                    }
                    else
                    {
                        _Context.Add(blogPost);
                        await _Context.SaveChangesAsync();
                        TempData["Success Message"] = "Blog Post Has Been Successfully Added";
                        return RedirectToAction(nameof(Index));

                    }

                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "An error occurred while creating the blog post.";
                    return View("Error");
                }
            }
            ViewBag.Categories = await _Context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Authors = await _Context.Authors.ToListAsync();
            return View(blogPost);
        }

        public async Task<IActionResult> Edit(int? Id)
        {
            if (Id == null)
            {
                ViewBag.ErrorMessage = "Blog post ID is missing.";
                return View("Error");
            }
            try
            {
                var BlogPost = await _Context.BlogPosts.FindAsync(Id);
                if (BlogPost == null)
                {
                    ViewBag.ErrorMessage = "Blogpost not found";
                    return View("Error");
                }
                ViewBag.Categories = await _Context.Categories.OrderBy(c => c.Name).ToListAsync();
                ViewBag.Authors = await _Context.Authors.ToListAsync();
                return View(BlogPost);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An Error Occured While Loading The Edit BlogPost Form";
                return View("Error");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, BlogPost blogPost, IFormFile? FeaturedImage)
        {
            if (Id != blogPost.Id)
            {
                return NotFound("BlogPost Id mismatch");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var ExistingPost = await _Context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(b => b.Id == Id);
                    if (ExistingPost == null)
                    {
                        return NotFound("Blog not found");
                    }
                    if (FeaturedImage != null && FeaturedImage.Length > 0)
                    {
                        blogPost.FeaturedImage = await UploadFeaturedImageAsync(FeaturedImage);
                    }
                    else
                    {
                        blogPost.FeaturedImage = ExistingPost.FeaturedImage;
                    }
                    blogPost.Slug = String.IsNullOrEmpty(blogPost.Slug) ? await GenerateSlugAsync(blogPost.Title) : blogPost.Slug;
                    if (await _Context.BlogPosts.AnyAsync(b => b.Slug == blogPost.Slug && b.Id != blogPost.Id))
                    {
                        ModelState.AddModelError("Slug", "The slug Must Be Unique");
                    }
                    else
                    {
                        _Context.Update(blogPost);
                        await _Context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Blog Has Been Successfully Updated";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "There is an error while updating the blog";
                    return View("Error");

                }
            }
            ViewBag.Categories = await _Context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.Authors = await _Context.Authors.ToListAsync();
            return View(blogPost);
        }
        public async Task<IActionResult> Delete(int? Id)
        {
            if (Id == null)
            {
                ViewBag.ErrorMessage = "Blog post ID is missing.";
                return View("Error");
            }
            try
            {
                var BlogPost = await _Context.BlogPosts.Include(b => b.Author).FirstOrDefaultAsync(m => m.Id == Id);
                if (BlogPost == null)
                {
                    ViewBag.ErrorMessage = "BlogPost Not Found.";
                    return View("Error");
                }
                return View(BlogPost);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "An Error Occured While Loading The Post For Delection";
                return View("Error");
            }
        }

        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int Id)
        {
            try
            {
                var BlogPost = await _Context.BlogPosts.FindAsync(Id);
                if(BlogPost != null)
                {
                    _Context.BlogPosts.Remove(BlogPost);
                    await _Context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "The BlogPost Has Been Successfully Deleted";
                }
                return RedirectToAction(nameof(Index));
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = "There has been some error while deleting the post";
                return View("Error");
            }
        }
        public async Task<string> UploadFeaturedImageAsync(IFormFile FeaturedImage)
        {
            if(FeaturedImage != null && FeaturedImage.Length >0) {

                var UploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(UploadsFolder))
                {
                    Directory.CreateDirectory(UploadsFolder);
                }
                var UniqueFileName = Guid.NewGuid().ToString() + "_" + FeaturedImage.FileName;
                var Filepath = Path.Combine(UploadsFolder, UniqueFileName);
                using (var filestream = new FileStream(Filepath, FileMode.Create))
                {
                    await FeaturedImage.CopyToAsync(filestream);
                }
                return "/Uploads"+UniqueFileName;
            }
            return null;
        }
        private async Task<string> GenerateSlugAsync(string title)
        {
            var Slug = System.Text.RegularExpressions.Regex.Replace(title.ToLowerInvariant(), @"\s+", "-").Trim();

            var UniqueSlug = Slug;
            int counter = 1;
            while (await _Context.BlogPosts.AnyAsync(b => b.Slug == UniqueSlug))
            {
                UniqueSlug = $"{Slug}-{counter++}";
            }
            return UniqueSlug;
        }
    }
}
           



    

