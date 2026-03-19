using Fonos.API.Common;
using Fonos.API.DTOs.Books;
using Fonos.API.Models;
using Fonos.API.Persistence;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fonos.API.Services.Books
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _dbContext;
        public BookService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<BookDto> CreateBookAsync(BookCreateDto command)
        {
            //Validate
            var author = await _dbContext.Authors.FindAsync(command.AuthorId)
                             ?? throw new KeyNotFoundException("Invalid Author Id");

            var category = await _dbContext.Categories.FindAsync(command.CategoryId)
                         ?? throw new KeyNotFoundException("Invalid Category Id");

            //Create
            var book = Book.Create(command.Title, command.Description, command.CoverImageUrl, command.Price, command.AuthorId, command.CategoryId);

            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();

            return new BookDto(book.Id, book.Title, book.Description, book.CoverImageUrl, book.Price, author.Name, category.Name);
        }

        public async Task DeleteBookAsync(Guid id)
        {
            var book = await _dbContext.Books.FindAsync(id);
            if (book == null) throw new KeyNotFoundException("Invalid Book Id");

            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<PagedResponse<BookDto>> GetAllBooksAsync(QueryFilter filter, CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);

            var query = _dbContext.Books.Include(b=>b.Author).Include(b=>b.Category).AsNoTracking().AsQueryable();

            // 1. Apply search filter (reduces the dataset)
            query = query.ApplySearch(filter.Search);

            // 2. Count total records AFTER filtering, BEFORE pagination
            var totalRecords = await query.CountAsync(cancellationToken);

            // 3. Apply sorting (default to Title if not specified)
            query = query.ApplySort(
                string.IsNullOrWhiteSpace(filter.SortBy) ? "Created" : filter.SortBy);

            // 4. Apply pagination and project to DTOs
            var books = await query
                .ApplyPagination(pageNumber, pageSize)
                .Select(b => new BookDto(b.Id, b.Title,b.Description,b.CoverImageUrl,b.Price,b.Author.Name,b.Category.Name))
                .ToListAsync(cancellationToken);

            return new PagedResponse<BookDto>
            {
                Data = books,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };
        }

        public async Task<BookDto?> GetBookAsync(Guid id)
        {
            var book = await _dbContext.Books.Include(b => b.Author).Include(b => b.Category).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) throw new KeyNotFoundException("Invalid Book Id");

            return new BookDto(book.Id, book.Title, book.Description, book.CoverImageUrl, book.Price, book.Author.Name!, book.Category.Name!);
        }

        public async Task UpdateBookAsync(Guid id, BookUpdateDto command)
        {            
            //Validate
            var authorExists = await _dbContext.Authors.AnyAsync(a => a.Id == command.AuthorId);
            if (!authorExists) throw new KeyNotFoundException("Invalid Author Id");

            var categoryExists = await _dbContext.Categories.AnyAsync(a => a.Id == command.CategoryId);
            if (!categoryExists) throw new KeyNotFoundException("Invalid Category Id");

            //Update
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) throw new KeyNotFoundException("Invalid Book Id");

            book.Update(command.Title, command.Description, command.CoverImageUrl, command.Price, command.AuthorId, command.CategoryId);
            await _dbContext.SaveChangesAsync();
        }
    }
}
