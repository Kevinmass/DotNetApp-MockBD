namespace BlogData.Models;

public class Like
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
}
