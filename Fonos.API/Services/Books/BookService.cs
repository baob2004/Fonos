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

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _dbContext.Books.Include(b=>b.Author).Include(b=>b.Category).Select(b => new BookDto(b.Id, b.Title, b.Description, b.CoverImageUrl, b.Price, b.Author.Name!, b.Category.Name)).ToListAsync();
            return books;
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
