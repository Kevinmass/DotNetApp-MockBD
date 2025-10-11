using BlogData.Models;
using BlogData.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LikesController : ControllerBase
{
    private readonly IDataService _dataService;

    public LikesController(IDataService dataService)
    {
        _dataService = dataService;
    }

    // GET: api/likes/post/5 - Get likes for a specific post
    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<Like>>> GetLikesForPost(int postId)
    {
        var likes = await _dataService.GetLikesForPostAsync(postId);
        return likes;
    }

    // POST: api/likes/post/5 - Like a post
    [HttpPost("post/{postId}")]
    [Authorize]
    public async Task<IActionResult> LikePost(int postId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized("User not authenticated");
        }

        if (postId <= 0)
            return BadRequest("Invalid post ID");

        // Check if post exists
        var post = await _dataService.GetPostByIdAsync(postId);
        if (post == null)
        {
            return NotFound("Post not found");
        }

        if (post.AuthorId == userId)
            return BadRequest("You cannot like your own post");

        // Check if user already liked this post
        var hasLiked = await _dataService.HasUserLikedAsync(postId, userId);
        if (hasLiked)
        {
            return BadRequest("You have already liked this post");
        }

        // Create new like
        var like = await _dataService.CreateLikeAsync(postId, userId);
        if (like == null)
        {
            return BadRequest("Failed to create like");
        }

        return Ok(new { Message = "Post liked successfully" });
    }

    // DELETE: api/likes/post/5 - Unlike a post
    [HttpDelete("post/{postId}")]
    [Authorize]
    public async Task<IActionResult> UnlikePost(int postId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _dataService.DeleteLikeAsync(postId, userId);
        if (!result)
        {
            return NotFound("Like not found");
        }

        return Ok(new { Message = "Post unliked successfully" });
    }

    // GET: api/likes/post/5/status - Check if current user liked a post
    [HttpGet("post/{postId}/status")]
    [Authorize]
    public async Task<IActionResult> GetLikeStatus(int postId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var hasLiked = await _dataService.HasUserLikedAsync(postId, userId);
        return Ok(new { HasLiked = hasLiked });
    }
}
