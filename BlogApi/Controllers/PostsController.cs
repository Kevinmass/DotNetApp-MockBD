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

    /// <summary>
    /// Obtiene todas las publicaciones, opcionalmente filtradas por búsqueda.
    /// </summary>
    /// <param name="search">Término de búsqueda opcional en título o contenido.</param>
    /// <returns>Lista de publicaciones con sus autores y likes.</returns>
    /// <response code="200">Publicaciones obtenidas exitosamente.</response>
    /// <response code="500">Error interno del servidor.</response>
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

    /// <summary>
    /// Obtiene una publicación específica por ID.
    /// </summary>
    /// <param name="id">ID de la publicación.</param>
    /// <returns>Publicación con sus likes incluidos.</returns>
    /// <response code="200">Publicación encontrada.</response>
    /// <response code="404">Publicación no encontrada.</response>
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

    /// <summary>
    /// Crea una nueva publicación.
    /// </summary>
    /// <param name="post">Datos de la publicación a crear.</param>
    /// <returns>Publicación creada.</returns>
    /// <response code="201">Publicación creada exitosamente.</response>
    /// <response code="400">Datos inválidos.</response>
    /// <response code="401">Usuario no autenticado.</response>
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

    /// <summary>
    /// Actualiza una publicación existente.
    /// </summary>
    /// <param name="id">ID de la publicación a actualizar.</param>
    /// <param name="post">Datos actualizados de la publicación.</param>
    /// <returns>NoContent si la actualización es exitosa.</returns>
    /// <response code="204">Publicación actualizada exitosamente.</response>
    /// <response code="400">Datos inválidos o ID no coincide.</response>
    /// <response code="404">Publicación no encontrada.</response>
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

    /// <summary>
    /// Elimina una publicación por ID.
    /// </summary>
    /// <param name="id">ID de la publicación a eliminar.</param>
    /// <returns>NoContent si la eliminación es exitosa.</returns>
    /// <response code="204">Publicación eliminada exitosamente.</response>
    /// <response code="404">Publicación no encontrada.</response>
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
