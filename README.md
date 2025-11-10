# DotNetApp-MockBD

Una aplicación de blog completa construida con .NET 8, ASP.NET Core Web API y Blazor WebAssembly. Esta versión utiliza una base de datos simulada en memoria para fines de desarrollo y pruebas.

## Descripción

DotNetApp-MockBD es una aplicación de blog que permite a los usuarios registrarse, iniciar sesión, crear publicaciones, dar "me gusta" a las publicaciones y gestionar contenido. La aplicación consta de una API RESTful backend y una interfaz de usuario frontend construida con Blazor.

**Características principales:**
- Autenticación JWT
- Gestión de usuarios
- Creación y gestión de publicaciones
- Sistema de "me gusta" para publicaciones
- API RESTful documentada con Swagger
- Base de datos simulada en memoria (no persistente)
- Soporte para Docker

## Arquitectura

La aplicación está dividida en los siguientes proyectos:

- **BlogApi**: API RESTful ASP.NET Core
- **BlogData**: Biblioteca de clases con modelos y servicios de datos
- **BlogWeb**: Aplicación Blazor WebAssembly

## Requisitos Previos

- .NET 8.0 SDK
- Docker (opcional, para ejecución en contenedores)
- Navegador web moderno

## Instalación y Ejecución

### Opción 1: Ejecutar con Docker (Recomendado)

1. Asegúrese de tener Docker instalado y ejecutándose.
2. Navegue al directorio raíz del proyecto.
3. Ejecute el comando:

```bash
docker-compose up --build
```

4. La API estará disponible en `http://localhost:5001`
5. La aplicación web estará disponible en `http://localhost:3000`
6. La documentación Swagger estará disponible en `http://localhost:5001/swagger`

### Opción 2: Ejecutar localmente

1. Restaure las dependencias:

```bash
dotnet restore
```

2. Ejecute la API:

```bash
cd BlogApi
dotnet run
```

3. En otra terminal, ejecute la aplicación web:

```bash
cd BlogWeb
dotnet run
```

4. La API estará disponible en `https://localhost:5001` (o el puerto configurado)
5. La aplicación web estará disponible en `https://localhost:5000` (o el puerto configurado)

## Configuración

La aplicación utiliza configuración basada en archivos `appsettings.json`. Los ajustes importantes incluyen:

- **JwtSettings**: Configuración para tokens JWT
  - `SecretKey`: Clave secreta para firmar tokens
  - `Issuer`: Emisor del token
  - `Audience`: Audiencia del token

## Documentación de la API

La API proporciona endpoints para gestionar usuarios, publicaciones y "me gusta". Todos los endpoints requieren autenticación JWT excepto los de registro e inicio de sesión.

### Endpoints de Autenticación (`/api/auth`)

#### `POST /api/auth/register`
Registra un nuevo usuario en el sistema.

**Parámetros de entrada:**
- `userName` (string): Nombre de usuario (2-50 caracteres, solo letras y números)
- `password` (string): Contraseña (mínimo 3 caracteres)

**Salida:**
```json
{
  "token": "jwt_token_here",
  "user": {
    "id": "user_id",
    "email": "user@test.com",
    "userName": "username"
  }
}
```

**Códigos de respuesta:**
- 200: Registro exitoso
- 400: Datos inválidos o usuario ya existe

#### `POST /api/auth/login`
Inicia sesión de un usuario existente.

**Parámetros de entrada:**
- `userName` (string): Nombre de usuario
- `password` (string): Contraseña

**Salida:**
```json
{
  "token": "jwt_token_here",
  "user": {
    "id": "user_id",
    "email": "user@test.com",
    "userName": "username"
  }
}
```

**Códigos de respuesta:**
- 200: Inicio de sesión exitoso
- 401: Credenciales inválidas

#### `POST /api/auth/logout`
Cierra la sesión del usuario actual.

**Autenticación:** Requerida (Bearer Token)

**Salida:**
```json
{
  "message": "Logged out successfully"
}
```

#### `GET /api/auth/me`
Obtiene información del usuario actualmente autenticado.

**Autenticación:** Requerida (Bearer Token)

**Salida:**
```json
{
  "id": "user_id",
  "email": "user@test.com",
  "userName": "username"
}
```

### Endpoints de Publicaciones (`/api/posts`)

#### `GET /api/posts`
Obtiene todas las publicaciones, opcionalmente filtradas por búsqueda.

**Parámetros de consulta:**
- `search` (string, opcional): Término de búsqueda en título o contenido

**Salida:**
```json
[
  {
    "id": 1,
    "title": "Título de la publicación",
    "content": "Contenido de la publicación",
    "createdAt": "2023-01-01T00:00:00Z",
    "updatedAt": null,
    "authorId": "user_id",
    "author": {
      "id": "user_id",
      "userName": "username",
      "email": "user@test.com"
    },
    "authorName": "username",
    "likes": [...],
    "likesCount": 5
  }
]
```

#### `GET /api/posts/{id}`
Obtiene una publicación específica por ID.

**Parámetros de ruta:**
- `id` (int): ID de la publicación

**Salida:** Objeto de publicación (ver arriba)

**Códigos de respuesta:**
- 200: Publicación encontrada
- 404: Publicación no encontrada

#### `POST /api/posts`
Crea una nueva publicación.

**Autenticación:** Requerida (Bearer Token)

**Parámetros de entrada:**
- `title` (string): Título (3-100 caracteres)
- `content` (string): Contenido (10-5000 caracteres)

**Salida:** Objeto de publicación creado

**Códigos de respuesta:**
- 201: Publicación creada
- 400: Datos inválidos
- 401: No autenticado

#### `PUT /api/posts/{id}`
Actualiza una publicación existente.

**Parámetros de ruta:**
- `id` (int): ID de la publicación

**Parámetros de entrada:** Ver POST /api/posts

**Salida:** Objeto de publicación actualizado

**Códigos de respuesta:**
- 204: Actualización exitosa
- 400: Datos inválidos
- 404: Publicación no encontrada

#### `DELETE /api/posts/{id}`
Elimina una publicación.

**Parámetros de ruta:**
- `id` (int): ID de la publicación

**Códigos de respuesta:**
- 204: Eliminación exitosa
- 404: Publicación no encontrada

### Endpoints de "Me gusta" (`/api/likes`)

#### `GET /api/likes/post/{postId}`
Obtiene todos los "me gusta" para una publicación específica.

**Parámetros de ruta:**
- `postId` (int): ID de la publicación

**Salida:**
```json
[
  {
    "id": 1,
    "postId": 1,
    "userId": "user_id",
    "createdAt": "2023-01-01T00:00:00Z",
    "post": {...},
    "user": {...}
  }
]
```

#### `POST /api/likes/post/{postId}`
Da "me gusta" a una publicación.

**Autenticación:** Requerida (Bearer Token)

**Parámetros de ruta:**
- `postId` (int): ID de la publicación

**Salida:**
```json
{
  "message": "Post liked successfully"
}
```

**Códigos de respuesta:**
- 200: "Me gusta" agregado
- 400: Ya dio "me gusta" o datos inválidos
- 404: Publicación no encontrada

#### `DELETE /api/likes/post/{postId}`
Quita el "me gusta" de una publicación.

**Autenticación:** Requerida (Bearer Token)

**Parámetros de ruta:**
- `postId` (int): ID de la publicación

**Salida:**
```json
{
  "message": "Post unliked successfully"
}
```

**Códigos de respuesta:**
- 200: "Me gusta" removido
- 404: "Me gusta" no encontrado

#### `GET /api/likes/post/{postId}/status`
Verifica si el usuario actual dio "me gusta" a una publicación.

**Autenticación:** Requerida (Bearer Token)

**Parámetros de ruta:**
- `postId` (int): ID de la publicación

**Salida:**
```json
{
  "hasLiked": true
}
```

## Modelos de Datos

### User
```csharp
public class User
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

### Post
```csharp
public class Post
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? AuthorId { get; set; }
    public User? Author { get; set; }
    public string? AuthorName { get; set; }
    public ICollection<Like> Likes { get; set; }
    public int LikesCount { get; }
}
```

### Like
```csharp
public class Like
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Post Post { get; set; }
    public User User { get; set; }
}
```

## Servicios de Datos

La aplicación utiliza un servicio de datos simulado (`JsonDataService`) que mantiene los datos en memoria. Los métodos principales incluyen:

### Gestión de Publicaciones
- `GetPostsAsync(string? search)`: Obtiene publicaciones con filtro opcional
- `GetPostByIdAsync(int id)`: Obtiene publicación por ID
- `CreatePostAsync(Post post)`: Crea nueva publicación
- `UpdatePostAsync(int id, Post post)`: Actualiza publicación
- `DeletePostAsync(int id)`: Elimina publicación

### Gestión de "Me gusta"
- `GetLikesForPostAsync(int postId)`: Obtiene "me gusta" para una publicación
- `CreateLikeAsync(int postId, string userId)`: Crea "me gusta"
- `DeleteLikeAsync(int postId, string userId)`: Elimina "me gusta"
- `HasUserLikedAsync(int postId, string userId)`: Verifica si usuario dio "me gusta"

### Gestión de Usuarios
- `GetUsersAsync()`: Obtiene todos los usuarios
- `GetUserByIdAsync(string id)`: Obtiene usuario por ID
- `GetUserByNameAsync(string userName)`: Obtiene usuario por nombre
- `CreateUserAsync(User user)`: Crea nuevo usuario
- `ValidateUserPasswordAsync(string userName, string password)`: Valida contraseña
- `UserExistsAsync(string userName)`: Verifica si usuario existe

## Pruebas

Para ejecutar las pruebas:

```bash
dotnet test
```

## Despliegue

La aplicación está configurada para despliegue con Docker. Los archivos `Dockerfile.api` y `Dockerfile.web` definen las imágenes para la API y la aplicación web respectivamente.

## Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## Soporte

Para soporte, por favor abre un issue en el repositorio de GitHub.
