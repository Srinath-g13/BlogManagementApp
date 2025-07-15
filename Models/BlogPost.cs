using System.ComponentModel.DataAnnotations;

namespace BlogManagementApp.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        [Required,StringLength(200)]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        public string? FeaturedImage { get; set; }
        [StringLength(200)]
        public string? MetaTitle { get; set; }
        [StringLength(250)]
        public string? MetaDescription { get; set; }
        [StringLength(200)]
        public string? MetaKeyword { get; set; }

        [StringLength(200)]

        public string? Slug { get; set; }  
        public int Views { get; set; }

        public DateTime PublishedOn { get; set; } = DateTime.UtcNow;

        public DateTime? ModifiedOn { get; set; } = DateTime.UtcNow;

        [Required]
        public int? AuthorId { get; set; }
        public Author? Author { get; set; }

        [Required]

        public int? CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Comment>? Comments { get; set; }  
    }
}
