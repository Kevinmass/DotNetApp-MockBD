using System.Collections.Generic;
using System.Threading.Tasks;
using BlogData.Models;

namespace BlogData.Services;

public interface IDataService
{
    // Posts
    Task<List<Post>> GetPostsAsync(string? search = null);
    Task<Post?> GetPostByIdAsync(int id);
    Task<Post> CreatePostAsync(Post post);
    Task<Post?> UpdatePostAsync(int id, Post post);
    Task<bool> DeletePostAsync(int id);

    // Likes
    Task<List<Like>> GetLikesForPostAsync(int postId);
    Task<Like?> CreateLikeAsync(int postId, string userId);
    Task<bool> DeleteLikeAsync(int postId, string userId);
    Task<bool> HasUserLikedAsync(int postId, string userId);

    // Users (simplified from IdentityUser for mocking)
    Task<List<User>> GetUsersAsync();
    Task<User?> GetUserByIdAsync(string id);
    Task<User?> GetUserByNameAsync(string userName);
    Task<User> CreateUserAsync(User user);
    Task<bool> ValidateUserPasswordAsync(string userName, string password);
    Task<bool> UserExistsAsync(string userName);
}
