using BlogManagementApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogManagementApp.Data
{
    public class BlogManagementDBContext : DbContext
    {
        public BlogManagementDBContext(DbContextOptions<BlogManagementDBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BlogPost>()
                .HasIndex(b => b.Slug).IsUnique();

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.BlogPost)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
            public DbSet<Author> Authors { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Category> Categories { get; set; }


    }
}
