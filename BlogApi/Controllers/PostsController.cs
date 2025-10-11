using BlogData.Models;
using BlogData.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IDataService _dataService;

    public PostsController(IDataService dataService)
    {
        _dataService = dataService;
    }

    // GET: api/posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetPosts([FromQuery] string? search = null)
    {
        try
        {
            var posts = await _dataService.GetPostsAsync(search);

            // Load likes for each post
            foreach (var post in posts)
            {
                var likes = await _dataService.GetLikesForPostAsync(post.Id);
                post.Likes = likes;
            }

            return posts;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in GetPosts: {ex.ToString()}");
            return StatusCode(500, $"Internal server error: {ex.Message}, Inner: {ex.InnerException?.Message}");
        }
    }

    // GET: api/posts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Post>> GetPost(int id)
    {
        var post = await _dataService.GetPostByIdAsync(id);

        if (post == null)
        {
            return NotFound();
        }

        // Load likes
        var likes = await _dataService.GetLikesForPostAsync(id);
        post.Likes = likes;

        return post;
    }

    // POST: api/posts
    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Post>> CreatePost(Post post)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized("User not authenticated");
        }

        if (string.IsNullOrWhiteSpace(post.Title))
            return BadRequest("Title cannot be null or empty");

        if (post.Title.Length < 3)
            return BadRequest("Title must be at least 3 characters long");

        if (post.Title.Length > 100)
            return BadRequest("Title cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(post.Content))
            return BadRequest("Content cannot be null or empty");

        if (post.Content.Length < 10)
            return BadRequest("Content must be at least 10 characters long");

        if (post.Content.Length > 5000)
            return BadRequest("Content cannot exceed 5000 characters");

        post.AuthorId = userId;
        post.CreatedAt = DateTime.UtcNow;
        post.UpdatedAt = null;

        var createdPost = await _dataService.CreatePostAsync(post);

        return CreatedAtAction(nameof(GetPost), new { id = createdPost.Id }, createdPost);
    }

    // PUT: api/posts/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, Post post)
    {
        if (id != post.Id)
        {
            return BadRequest();
        }

        if (string.IsNullOrWhiteSpace(post.Title))
            return BadRequest("Title cannot be null or empty");

        if (post.Title.Length < 3)
            return BadRequest("Title must be at least 3 characters long");

        if (post.Title.Length > 100)
            return BadRequest("Title cannot exceed 100 characters");

        if (string.IsNullOrWhiteSpace(post.Content))
            return BadRequest("Content cannot be null or empty");

        if (post.Content.Length < 10)
            return BadRequest("Content must be at least 10 characters long");

        if (post.Content.Length > 5000)
            return BadRequest("Content cannot exceed 5000 characters");

        var updatedPost = await _dataService.UpdatePostAsync(id, post);
        if (updatedPost == null)
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/posts/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
        var result = await _dataService.DeletePostAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
}
