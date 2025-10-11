namespace BlogData.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Author relationship
    public string? AuthorId { get; set; }
    public User? Author { get; set; } // Using mock User instead of IdentityUser
    public string? AuthorName { get; set; } // For mock data compatibility

    // Likes relationship (computed count)
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public int LikesCount => Likes.Count;
}
