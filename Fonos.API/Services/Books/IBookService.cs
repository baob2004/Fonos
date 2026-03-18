using Fonos.API.DTOs.Books;

namespace Fonos.API.Services.Books
{
    public interface IBookService
    {
        Task<IEnumerable<BookDto>> GetAllBooksAsync();
        Task<BookDto?> GetBookAsync(Guid id);
        Task<BookDto> CreateBookAsync(BookCreateDto command);
        Task UpdateBookAsync(Guid id, BookUpdateDto command);
        Task DeleteBookAsync(Guid id);
    }
}
