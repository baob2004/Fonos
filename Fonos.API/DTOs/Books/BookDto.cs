namespace Fonos.API.DTOs.Books
{
    public record BookDto(Guid Id, string Title, string Description, string CoverImageUrl, decimal Price, string AuthorName, string CategoryName);
}
