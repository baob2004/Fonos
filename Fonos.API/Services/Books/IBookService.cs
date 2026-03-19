using Fonos.API.Common;
using Fonos.API.DTOs.Books;

namespace Fonos.API.Services.Books
{
    public interface IBookService
    {
        Task<PagedResponse<BookDto>> GetAllBooksAsync(QueryFilter filter, CancellationToken cancellationToken = default);
        Task<BookDto?> GetBookAsync(Guid id);
        Task<BookDto> CreateBookAsync(BookCreateDto command);
        Task UpdateBookAsync(Guid id, BookUpdateDto command);
        Task DeleteBookAsync(Guid id);
    }
}
