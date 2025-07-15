using BlogManagementApp.Models;
using System.ComponentModel.DataAnnotations;
namespace BlogManagementApp;

public class Comment
{ 
    public int Id { get; set; }
    [Required,StringLength(100)]
    public string Name { get; set; }
    [Required,EmailAddress]
    public string Email{ get; set; }
    [Required]
    public string Text { get; set; }

    public DateTime PostedOn { get; set; }

    public int BlogPostId { get; set; }

    public BlogPost? BlogPost { get; set; }


}
