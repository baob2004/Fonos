namespace Fonos.API.DTOs.Books
{
    public record BookUpdateDto(string Title, string Description, string CoverImageUrl, decimal Price, Guid AuthorId, Guid CategoryId);
}
