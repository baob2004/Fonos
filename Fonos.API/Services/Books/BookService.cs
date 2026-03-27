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
            var author = await _dbContext.Authors.FindAsync(command.AuthorId)
                         ?? throw new KeyNotFoundException("Invalid Author Id");
            var category = await _dbContext.Categories.FindAsync(command.CategoryId)
                           ?? throw new KeyNotFoundException("Invalid Category Id");

            string coverPath = "/images/default-book.png"; 

            // Xử lý Upload file ảnh bìa
            if (command.CoverImageFile != null && command.CoverImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(command.CoverImageFile.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await command.CoverImageFile.CopyToAsync(stream);
                }
                coverPath = $"/images/{fileName}";
            }

            var book = Book.Create(command.Title, command.Description, coverPath, command.Price, command.AuthorId, command.CategoryId);

            await _dbContext.Books.AddAsync(book);
            await _dbContext.SaveChangesAsync();

            return new BookDto(book.Id, book.Title, book.Description, book.CoverImageUrl, book.Price, author.Name, category.Name, 0);
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

            var query = _dbContext.Books.Include(b=>b.Author).Include(b=>b.Category).Include(b=>b.Chapters).AsNoTracking().AsQueryable();

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
                .Select(b => new BookDto(b.Id, b.Title,b.Description,b.CoverImageUrl,b.Price,b.Author.Name,b.Category.Name, b.Chapters.Sum(c => c.DurationInSeconds) / 60))
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
            var book = await _dbContext.Books.Include(b => b.Author).Include(b => b.Category).Include(b=>b.Chapters).FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) throw new KeyNotFoundException("Invalid Book Id");

            return new BookDto(book.Id, book.Title, book.Description, book.CoverImageUrl, book.Price, book.Author.Name!, book.Category.Name!, book.Chapters.Sum(c => c.DurationInSeconds) / 60);
        }

        public async Task UpdateBookAsync(Guid id, BookUpdateDto command)
        {
            var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id);
            if (book == null) throw new KeyNotFoundException("Không tìm thấy sách với Id này.");

            var authorExists = await _dbContext.Authors.AnyAsync(a => a.Id == command.AuthorId);
            if (!authorExists) throw new KeyNotFoundException("Mã tác giả không hợp lệ.");

            var categoryExists = await _dbContext.Categories.AnyAsync(a => a.Id == command.CategoryId);
            if (!categoryExists) throw new KeyNotFoundException("Mã thể loại không hợp lệ.");

            string finalCoverUrl = book.CoverImageUrl; 

            if (command.CoverImageFile != null && command.CoverImageFile.Length > 0)
            {
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(command.CoverImageFile.FileName)}";
                var filePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await command.CoverImageFile.CopyToAsync(stream);
                }

                if (!string.IsNullOrEmpty(book.CoverImageUrl) && book.CoverImageUrl.StartsWith("/images/"))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.CoverImageUrl.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                finalCoverUrl = $"/images/{fileName}";
            }

            book.Update(
                command.Title,
                command.Description,
                finalCoverUrl, 
                command.Price,
                command.AuthorId,
                command.CategoryId
            );

            await _dbContext.SaveChangesAsync();
        }

        public async Task<PagedResponse<BookDto>> GetPurchasedBooksAsync(string userId, QueryFilter filter, CancellationToken cancellationToken = default)
        {
            var pageNumber = Math.Max(1, filter.PageNumber);
            var pageSize = Math.Clamp(filter.PageSize, 1, 50);

            var query = _dbContext.UserBooks
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Book)
                    .ThenInclude(b => b.Author)
                .Include(ub => ub.Book.Category)
                .Include(ub => ub.Book.Chapters)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(ub =>
                    ub.Book.Title.ToLower().Contains(search) ||
                    ub.Book.Author.Name.ToLower().Contains(search));
            }

            var totalRecords = await query.CountAsync(cancellationToken);

            query = query.OrderByDescending(ub => ub.PurchaseDate);

            var books = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(ub => new BookDto(
                    ub.Book.Id,
                    ub.Book.Title,
                    ub.Book.Description,
                    ub.Book.CoverImageUrl,
                    ub.Book.Price,
                    ub.Book.Author.Name,
                    ub.Book.Category.Name,
                    ub.Book.Chapters.Sum(c => c.DurationInSeconds) / 60
                ))
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

        public async Task<bool> CheckOwnershipAsync(string userId, Guid bookId)
        {
            return await _dbContext.UserBooks
                .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId);
        }
    }
}
