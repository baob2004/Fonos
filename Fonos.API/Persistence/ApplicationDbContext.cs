using Fonos.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Fonos.API.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Book> Books => Set<Book>();
        public DbSet<Author> Authors => Set<Author>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Chapter> Chapters => Set<Chapter>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<UserBook> UserBooks => Set<UserBook>();
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(builder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseAsyncSeeding(async (context, _, cancellationToken) =>
                {
                    // 1. Seed Category
                    var category = await context.Set<Category>().FirstOrDefaultAsync(c => c.Name == "Tâm linh & Triết học", cancellationToken);
                    if (category == null)
                    {
                        category = new Category { Id = Guid.NewGuid(), Name = "Tâm linh & Triết học", Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow };
                        await context.Set<Category>().AddAsync(category, cancellationToken);
                    }

                    // 2. Seed Author
                    var author = await context.Set<Author>().FirstOrDefaultAsync(a => a.Name == "Nguyên Phong", cancellationToken);
                    if (author == null)
                    {
                        author = new Author { Id = Guid.NewGuid(), Name = "Nguyên Phong", Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow };
                        await context.Set<Author>().AddAsync(author, cancellationToken);
                    }
                    await context.SaveChangesAsync(cancellationToken);

                    // 3. Seed Book
                    var book = await context.Set<Book>().FirstOrDefaultAsync(b => b.Title == "Muôn Kiếp Nhân Sinh", cancellationToken);
                    if (book == null)
                    {
                        book = new Book
                        {
                            Id = Guid.NewGuid(),
                            Title = "Muôn Kiếp Nhân Sinh",
                            Description = "Một hành trình khám phá về luật nhân quả và luân hồi.",
                            CoverImageUrl = "https://example.com/muon-kiep-nhan-sinh.jpg",
                            Price = 150000,
                            AuthorId = author.Id,
                            CategoryId = category.Id,
                            Created = DateTimeOffset.UtcNow,
                            LastModified = DateTimeOffset.UtcNow
                        };
                        await context.Set<Book>().AddAsync(book, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        // 4. Seed Chapters cho cuốn sách vừa tạo
                        if (!await context.Set<Chapter>().AnyAsync(c => c.BookId == book.Id, cancellationToken))
                        {
                            await context.Set<Chapter>().AddRangeAsync(new List<Chapter>
                            {
                        new Chapter
                        {
                            Id = Guid.NewGuid(),
                            BookId = book.Id,
                            Title = "Chương 1: Sự thức tỉnh",
                            OrderNumber = 1,
                            ContentText = "Nội dung chương 1...",
                            Status = AudioStatus.Completed,
                            AudioUrl = "/audios/muon-kiep-nhan-sinh-c1.mp3",
                            DurationInSeconds = 600,
                            Created = DateTimeOffset.UtcNow,
                            LastModified = DateTimeOffset.UtcNow
                        },
                        new Chapter
                        {
                            Id = Guid.NewGuid(),
                            BookId = book.Id,
                            Title = "Chương 2: Luân hồi",
                            OrderNumber = 2,
                            ContentText = "Nội dung chương 2...",
                            Status = AudioStatus.Pending,
                            Created = DateTimeOffset.UtcNow,
                            LastModified = DateTimeOffset.UtcNow
                        }
                            }, cancellationToken);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                })
                .UseSeeding((context, _) =>
                {
                    var categoryExists = context.Set<Category>().Any(c => c.Name == "Tâm linh & Triết học");
                    if (!categoryExists)
                    {
                        var category = new Category { Id = Guid.NewGuid(), Name = "Tâm linh & Triết học", Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow };
                        context.Set<Category>().Add(category);
                        context.SaveChanges();
                    }
                });
        }
    }
}
