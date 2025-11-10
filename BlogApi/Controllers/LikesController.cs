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

    /// <summary>
    /// Obtiene todos los "me gusta" para una publicaciรณn especรญfica.
    /// </summary>
    /// <param name="postId">ID de la publicaciรณn.</param>
    /// <returns>Lista de likes para la publicaciรณn.</returns>
    /// <response code="200">Likes obtenidos exitosamente.</response>
    [HttpGet("post/{postId}")]
    public async Task<ActionResult<IEnumerable<Like>>> GetLikesForPost(int postId)
    {
        var likes = await _dataService.GetLikesForPostAsync(postId);
        return likes;
    }

    /// <summary>
    /// Da "me gusta" a una publicaciรณn.
    /// </summary>
    /// <param name="postId">ID de la publicaciรณn a la que dar like.</param>
    /// <returns>Mensaje de confirmaciรณn.</returns>
    /// <response code="200">"Me gusta" agregado exitosamente.</response>
    /// <response code="400">ID invรกlido, ya dio like, o es su propia publicaciรณn.</response>
    /// <response code="401">Usuario no autenticado.</response>
    /// <response code="404">Publicaciรณn no encontrada.</response>
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

    /// <summary>
    /// Quita el "me gusta" de una publicaciรณn.
    /// </summary>
    /// <param name="postId">ID de la publicaciรณn de la que quitar like.</param>
    /// <returns>Mensaje de confirmaciรณn.</returns>
    /// <response code="200">"Me gusta" removido exitosamente.</response>
    /// <response code="401">Usuario no autenticado.</response>
    /// <response code="404">"Me gusta" no encontrado.</response>
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

    /// <summary>
    /// Verifica si el usuario actual dio "me gusta" a una publicación.
    /// </summary>
    /// <param name="postId">ID de la publicación.</param>
    /// <returns>Estado del like (HasLiked: true/false).</returns>
    /// <response code="200">Estado obtenido exitosamente.</response>
    /// <response code="401">Usuario no autenticado.</response>
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
