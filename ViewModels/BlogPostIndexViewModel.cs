using BlogManagementApp.Models;

namespace BlogManagementApp.ViewModels
{
    public class BlogPostIndexViewModel
    {
        public List<BlogPost>? Posts { get; set; }

        public int? CurrentPage { get; set; }

        public int? TotalPages { get; set; }

        public string? SearchTitle { get; set; }

        public int? SearchCategoryId { get; set; }

    }
}
