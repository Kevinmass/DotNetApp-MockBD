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

    /// <summary>
    /// Obtiene todas las publicaciones, opcionalmente filtradas por búsqueda.
    /// </summary>
    /// <param name="search">Término de búsqueda opcional en título o contenido.</param>
    /// <returns>Lista de publicaciones ordenadas por fecha de creación descendente.</returns>
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

    /// <summary>
    /// Obtiene una publicación por su ID.
    /// </summary>
    /// <param name="id">ID de la publicación.</param>
    /// <returns>Publicación encontrada o null si no existe.</returns>
    public Task<Post?> GetPostByIdAsync(int id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(post);
    }

    /// <summary>
    /// Crea una nueva publicación.
    /// </summary>
    /// <param name="post">Datos de la publicación a crear.</param>
    /// <returns>Publicación creada con ID asignado.</returns>
    public Task<Post> CreatePostAsync(Post post)
    {
        post.Id = _nextPostId++;
        post.CreatedAt = DateTime.UtcNow;
        _posts.Add(post);
        return Task.FromResult(post);
    }

    /// <summary>
    /// Actualiza una publicación existente.
    /// </summary>
    /// <param name="id">ID de la publicación a actualizar.</param>
    /// <param name="post">Datos actualizados de la publicación.</param>
    /// <returns>Publicación actualizada o null si no se encontró.</returns>
    public Task<Post?> UpdatePostAsync(int id, Post post)
    {
        var existingPost = _posts.FirstOrDefault(p => p.Id == id);
        if (existingPost == null) return Task.FromResult<Post?>(null);

        existingPost.Title = post.Title;
        existingPost.Content = post.Content;
        existingPost.UpdatedAt = DateTime.UtcNow;

        return Task.FromResult<Post?>(existingPost);
    }

    /// <summary>
    /// Elimina una publicación por ID.
    /// </summary>
    /// <param name="id">ID de la publicación a eliminar.</param>
    /// <returns>True si se eliminó exitosamente, false si no se encontró.</returns>
    public Task<bool> DeletePostAsync(int id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);
        if (post == null) return Task.FromResult(false);

        _posts.Remove(post);
        // Remove associated likes
        _likes.RemoveAll(l => l.PostId == id);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Obtiene todos los likes para una publicación específica.
    /// </summary>
    /// <param name="postId">ID de la publicación.</param>
    /// <returns>Lista de likes para la publicación.</returns>
    public Task<List<Like>> GetLikesForPostAsync(int postId)
    {
        var likes = _likes.Where(l => l.PostId == postId).ToList();
        return Task.FromResult(likes);
    }

    /// <summary>
    /// Crea un nuevo like para una publicación.
    /// </summary>
    /// <param name="postId">ID de la publicación.</param>
    /// <param name="userId">ID del usuario que da like.</param>
    /// <returns>Like creado o null si ya existe o la publicación no existe.</returns>
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

    /// <summary>
    /// Elimina un like de una publicación para un usuario específico.
    /// </summary>
    /// <param name="postId">ID de la publicación.</param>
    /// <param name="userId">ID del usuario.</param>
    /// <returns>True si se eliminó exitosamente, false si no se encontró.</returns>
    public Task<bool> DeleteLikeAsync(int postId, string userId)
    {
        var like = _likes.FirstOrDefault(l => l.PostId == postId && l.UserId == userId);
        if (like == null) return Task.FromResult(false);

        _likes.Remove(like);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Verifica si un usuario ha dado like a una publicación.
    /// </summary>
    /// <param name="postId">ID de la publicación.</param>
    /// <param name="userId">ID del usuario.</param>
    /// <returns>True si el usuario ha dado like, false en caso contrario.</returns>
    public Task<bool> HasUserLikedAsync(int postId, string userId)
    {
        var hasLiked = _likes.Any(l => l.PostId == postId && l.UserId == userId);
        return Task.FromResult(hasLiked);
    }

    /// <summary>
    /// Obtiene todos los usuarios.
    /// </summary>
    /// <returns>Lista de todos los usuarios.</returns>
    public Task<List<User>> GetUsersAsync()
    {
        return Task.FromResult(_users.ToList());
    }

    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <returns>Usuario encontrado o null si no existe.</returns>
    public Task<User?> GetUserByIdAsync(string id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    /// <summary>
    /// Obtiene un usuario por su nombre de usuario.
    /// </summary>
    /// <param name="userName">Nombre de usuario.</param>
    /// <returns>Usuario encontrado o null si no existe.</returns>
    public Task<User?> GetUserByNameAsync(string userName)
    {
        var user = _users.FirstOrDefault(u => u.UserName == userName);
        return Task.FromResult(user);
    }

    /// <summary>
    /// Crea un nuevo usuario.
    /// </summary>
    /// <param name="user">Datos del usuario a crear.</param>
    /// <returns>Usuario creado con ID asignado.</returns>
    public Task<User> CreateUserAsync(User user)
    {
        user.Id = Guid.NewGuid().ToString();
        user.CreatedAt = DateTime.UtcNow;
        _users.Add(user);
        return Task.FromResult(user);
    }

    /// <summary>
    /// Valida la contraseña de un usuario.
    /// </summary>
    /// <param name="userName">Nombre de usuario.</param>
    /// <param name="password">Contraseña a validar.</param>
    /// <returns>True si la contraseña es correcta, false en caso contrario.</returns>
    public Task<bool> ValidateUserPasswordAsync(string userName, string password)
    {
        var user = _users.FirstOrDefault(u => u.UserName == userName);
        if (user == null) return Task.FromResult(false);

        // Simple password validation (in real app, use proper hashing)
        return Task.FromResult(user.PasswordHash == password);
    }

    /// <summary>
    /// Verifica si un usuario existe por nombre de usuario.
    /// </summary>
    /// <param name="userName">Nombre de usuario a verificar.</param>
    /// <returns>True si el usuario existe, false en caso contrario.</returns>
    public Task<bool> UserExistsAsync(string userName)
    {
        var exists = _users.Any(u => u.UserName == userName);
        return Task.FromResult(exists);
    }
}
