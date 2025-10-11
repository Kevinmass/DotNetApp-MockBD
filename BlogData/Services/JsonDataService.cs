using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogData.Models;
using BlogData.Services;
using System.Text.Json;

namespace BlogData.Services;

public class JsonDataService : IDataService
{
    private readonly List<Post> _posts = new();
    private readonly List<Like> _likes = new();
    private readonly List<User> _users = new();
    private int _nextPostId = 1;
    private int _nextLikeId = 1;

    public JsonDataService()
    {
        // Initialize with sample data
        InitializeSampleData();
    }

    private void InitializeSampleData()
    {
        // Sample users
        var user1 = new User { Id = "user1", UserName = "johndoe", Email = "johndoe@test.com", PasswordHash = "password123", CreatedAt = DateTime.UtcNow };
        var user2 = new User { Id = "user2", UserName = "janesmith", Email = "janesmith@test.com", PasswordHash = "password123", CreatedAt = DateTime.UtcNow };
        _users.Add(user1);
        _users.Add(user2);

        // Sample posts
        var post1 = new Post
        {
            Id = _nextPostId++,
            Title = "Welcome to the Blog",
            Content = "This is the first post on our blog. We're excited to share our thoughts and ideas with you!",
            AuthorId = user1.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        _posts.Add(post1);

        var post2 = new Post
        {
            Id = _nextPostId++,
            Title = "Getting Started with .NET",
            Content = "Today we're going to explore the basics of .NET development and best practices for building robust applications.",
            AuthorId = user2.Id,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _posts.Add(post2);

        var post3 = new Post
        {
            Id = _nextPostId++,
            Title = "Modern Web Development",
            Content = "Web development has evolved significantly. Let's discuss the latest trends and technologies that are shaping the industry.",
            AuthorId = user1.Id,
            CreatedAt = DateTime.UtcNow.AddHours(-5)
        };
        _posts.Add(post3);

        // Sample likes
        var like1 = new Like { Id = _nextLikeId++, PostId = post1.Id, UserId = user2.Id, CreatedAt = DateTime.UtcNow.AddHours(-1), User = user2 };
        _likes.Add(like1);
        var like2 = new Like { Id = _nextLikeId++, PostId = post2.Id, UserId = user1.Id, CreatedAt = DateTime.UtcNow.AddHours(-2), User = user1 };
        _likes.Add(like2);
    }

    // Posts
    public Task<List<Post>> GetPostsAsync(string? search = null)
    {
        var query = _posts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                    p.Content.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var posts = query.OrderByDescending(p => p.CreatedAt).ToList();

        // Load authors for posts (frontend expects Author property)
        foreach (var post in posts)
        {
            if (!string.IsNullOrEmpty(post.AuthorId))
            {
                var author = _users.FirstOrDefault(u => u.Id == post.AuthorId);
                if (author != null)
                {
                    post.Author = author;
                    post.AuthorName = author.UserName;
                }
            }
        }

        return Task.FromResult(posts);
    }

    public Task<Post?> GetPostByIdAsync(int id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(post);
    }

    public Task<Post> CreatePostAsync(Post post)
    {
        post.Id = _nextPostId++;
        post.CreatedAt = DateTime.UtcNow;
        _posts.Add(post);
        return Task.FromResult(post);
    }

    public Task<Post?> UpdatePostAsync(int id, Post post)
    {
        var existingPost = _posts.FirstOrDefault(p => p.Id == id);
        if (existingPost == null) return Task.FromResult<Post?>(null);

        existingPost.Title = post.Title;
        existingPost.Content = post.Content;
        existingPost.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Post?>(existingPost);
    }

    public Task<bool> DeletePostAsync(int id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post == null) return Task.FromResult(false);

        _posts.Remove(post);
        // Remove associated likes
        _likes.RemoveAll(l => l.PostId == id);
        return Task.FromResult(true);
    }

    // Likes
    public Task<List<Like>> GetLikesForPostAsync(int postId)
    {
        var likes = _likes.Where(l => l.PostId == postId).ToList();
        return Task.FromResult(likes);
    }

    public Task<Like?> CreateLikeAsync(int postId, string userId)
    {
        // Check if post exists
        if (!_posts.Any(p => p.Id == postId)) return Task.FromResult<Like?>(null);

        // Check if user has already liked
        if (_likes.Any(l => l.PostId == postId && l.UserId == userId)) return Task.FromResult<Like?>(null);

        var like = new Like
        {
            Id = _nextLikeId++,
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _likes.Add(like);
        return Task.FromResult<Like?>(like);
    }

    public Task<bool> DeleteLikeAsync(int postId, string userId)
    {
        var like = _likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);
        if (like == null) return Task.FromResult(false);

        _likes.Remove(like);
        return Task.FromResult(true);
    }

    public Task<bool> HasUserLikedAsync(int postId, string userId)
    {
        var hasLiked = _likes.Any(l => l.PostId == postId && l.UserId == userId);
        return Task.FromResult(hasLiked);
    }

    // Users
    public Task<List<User>> GetUsersAsync()
    {
        return Task.FromResult(_users.ToList());
    }

    public Task<User?> GetUserByIdAsync(string id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByNameAsync(string userName)
    {
        var user = _users.FirstOrDefault(u => u.UserName == userName);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(User user)
    {
        user.Id = Guid.NewGuid().ToString();
        user.CreatedAt = DateTime.UtcNow;
        _users.Add(user);
        return Task.FromResult(user);
    }

    public Task<bool> ValidateUserPasswordAsync(string userName, string password)
    {
        var user = _users.FirstOrDefault(u => u.UserName == userName);
        if (user == null) return Task.FromResult(false);

        // Simple password validation (in real app, use proper hashing)
        return Task.FromResult(user.PasswordHash == password);
    }

    public Task<bool> UserExistsAsync(string userName)
    {
        var exists = _users.Any(u => u.UserName == userName);
        return Task.FromResult(exists);
    }
}
