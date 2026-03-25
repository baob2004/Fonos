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
                    // 1. Seed Categories (5 mục)
                    var catNames = new[] { "Kinh doanh", "Kỹ năng sống", "Văn học", "Thiếu nhi", "Lịch sử" };
                    foreach (var name in catNames)
                    {
                        if (!await context.Set<Category>().AnyAsync(c => c.Name == name, cancellationToken))
                        {
                            await context.Set<Category>().AddAsync(Category.Create(name), cancellationToken);
                        }
                    }
                    await context.SaveChangesAsync(cancellationToken);

                    // 2. Seed Authors (5 mục)
                    var authorsData = new[] {
                (N: "Dale Carnegie", A: "https://example.com/dale.jpg"),
                (N: "Paulo Coelho", A: "https://example.com/paulo.jpg"),
                (N: "Haruki Murakami", A: "https://example.com/haruki.jpg"),
                (N: "Nguyễn Nhật Ánh", A: "https://example.com/nhatanh.jpg"),
                (N: "Yuval Noah Harari", A: "https://example.com/yuval.jpg")
                    };
                    foreach (var auth in authorsData)
                    {
                        if (!await context.Set<Author>().AnyAsync(a => a.Name == auth.N, cancellationToken))
                        {
                            await context.Set<Author>().AddAsync(Author.Create(auth.N, auth.A), cancellationToken);
                        }
                    }
                    await context.SaveChangesAsync(cancellationToken);

                    // 3. Seed Books & Chapters (5 mục)
                    var businessCat = await context.Set<Category>().FirstAsync(c => c.Name == "Kinh doanh", cancellationToken);
                    var daleAuthor = await context.Set<Author>().FirstAsync(a => a.Name == "Dale Carnegie", cancellationToken);

                    var bookNames = new[] { "Đắc Nhân Tâm", "Nhà Giả Kim", "Rừng Na Uy", "Sapiens", "Mắt Biếc" };
                    foreach (var title in bookNames)
                    {
                        var bookExists = await context.Set<Book>().AnyAsync(b => b.Title == title, cancellationToken);
                        if (!bookExists)
                        {
                            var newBook = Book.Create(title, $"Mô tả cho {title}", "https://example.com/cover.jpg", 120000, daleAuthor.Id, businessCat.Id);
                            await context.Set<Book>().AddAsync(newBook, cancellationToken);
                            await context.SaveChangesAsync(cancellationToken);

                            // Seed Chapters cho mỗi sách
                            var ch1 = Chapter.Create(newBook.Id, 1, "Chương 1: Khởi đầu", "Nội dung...");
                            ch1.SetAudio($"/audios/{title}-c1.mp3", 600);
                            await context.Set<Chapter>().AddAsync(ch1, cancellationToken);
                        }
                    }
                    await context.SaveChangesAsync(cancellationToken);
                })
                .UseSeeding((context, _) =>
                {
                    // CẦN THIẾT: Logic đồng bộ tương tự như trên để tránh lỗi
                    var catNames = new[] { "Kinh doanh", "Kỹ năng sống", "Văn học", "Thiếu nhi", "Lịch sử" };
                    foreach (var name in catNames)
                    {
                        if (!context.Set<Category>().Any(c => c.Name == name))
                        {
                            context.Set<Category>().Add(Category.Create(name));
                        }
                    }
                    context.SaveChanges();

                    var authorName = "Dale Carnegie";
                    if (!context.Set<Author>().Any(a => a.Name == authorName))
                    {
                        context.Set<Author>().Add(Author.Create(authorName, "https://example.com/dale.jpg"));
                        context.SaveChanges();
                    }

                    var cat = context.Set<Category>().First(c => c.Name == "Kinh doanh");
                    var auth = context.Set<Author>().First(a => a.Name == authorName);

                    if (!context.Set<Book>().Any(b => b.Title == "Đắc Nhân Tâm"))
                    {
                        var book = Book.Create("Đắc Nhân Tâm", "Mô tả", "url", 120000, auth.Id, cat.Id);
                        context.Set<Book>().Add(book);
                        context.SaveChanges();
                    }
                });
        }
    }
}
