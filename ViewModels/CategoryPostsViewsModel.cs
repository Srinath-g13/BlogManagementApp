using BlogManagementApp.Models;

namespace BlogManagementApp.ViewModels
{
    public class CategoryPostsViewsModel
    {
        public List<BlogPost> Posts { get; set; }

        public int? CurrentPage { get; set; }

        public int? TotalPage { get; set; }

        public string? CategoryName { get; set; }

        public int? CategoryId { get; set; }
    }
}
