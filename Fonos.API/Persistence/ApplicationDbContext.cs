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
                        category = Category.Create("Tâm linh & Triết học");
                        await context.Set<Category>().AddAsync(category, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    // 2. Seed Author
                    var author = await context.Set<Author>().FirstOrDefaultAsync(a => a.Name == "Nguyên Phong", cancellationToken);
                    if (author == null)
                    {
                        author = Author.Create("Nguyên Phong", "https://example.com/avatar-nguyen-phong.jpg");
                        await context.Set<Author>().AddAsync(author, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);
                    }

                    // 3. Seed Book (Sử dụng hàm Create mới có AuthorId và CategoryId)
                    var book = await context.Set<Book>().FirstOrDefaultAsync(b => b.Title == "Muôn Kiếp Nhân Sinh", cancellationToken);
                    if (book == null)
                    {
                        book = Book.Create(
                            "Muôn Kiếp Nhân Sinh",
                            "Hành trình khám phá luật nhân quả.",
                            "https://example.com/muon-kiep-nhan-sinh.jpg",
                            150000,
                            author.Id,
                            category.Id
                        );

                        await context.Set<Book>().AddAsync(book, cancellationToken);
                        await context.SaveChangesAsync(cancellationToken);

                        // 4. Seed Chapters
                        if (!await context.Set<Chapter>().AnyAsync(c => c.BookId == book.Id, cancellationToken))
                        {
                            var ch1 = Chapter.Create(book.Id, 1, "Chương 1: Thức tỉnh", "Nội dung...");
                            ch1.SetAudio("/audios/mkns-c1.mp3", 600); // Đánh dấu hoàn thành luôn

                            var ch2 = Chapter.Create(book.Id, 2, "Chương 2: Luân hồi", "Nội dung...");

                            await context.Set<Chapter>().AddRangeAsync(new[] { ch1, ch2 }, cancellationToken);
                            await context.SaveChangesAsync(cancellationToken);
                        }
                    }
                })
                .UseSeeding((context, _) =>
                {
                    // Logic đồng bộ tương tự nếu cần
                    if (!context.Set<Category>().Any(c => c.Name == "Tâm linh & Triết học"))
                    {
                        var category = Category.Create("Tâm linh & Triết học");
                        context.Set<Category>().Add(category);
                        context.SaveChanges();
                    }
                });
        }
    }
}
