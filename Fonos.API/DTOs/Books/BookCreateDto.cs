namespace Fonos.API.DTOs.Books
{
    public record BookCreateDto(string Title, string Description, string CoverImageUrl, decimal Price, Guid AuthorId, Guid CategoryId);
}
