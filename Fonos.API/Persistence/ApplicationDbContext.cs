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
            optionsBuilder.UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                // 1. Đảm bảo có Category và Author trước
                var category = await context.Set<Category>().FirstOrDefaultAsync(c => c.Name == "Tâm linh & Triết học", cancellationToken);
                if (category == null)
                {
                    category = Category.Create("Tâm linh & Triết học");
                    await context.Set<Category>().AddAsync(category, cancellationToken);
                }

                var author = await context.Set<Author>().FirstOrDefaultAsync(a => a.Name == "Nguyên Phong", cancellationToken);
                if (author == null)
                {
                    author = Author.Create("Nguyên Phong", "https://example.com/author.jpg");
                    await context.Set<Author>().AddAsync(author, cancellationToken);
                }

                await context.SaveChangesAsync(cancellationToken);

                // 2. Seed Sách nếu bảng Book đang trống
                if (!await context.Set<Book>().AnyAsync(cancellationToken))
                {
                    var books = GetSeedBooks(author.Id, category.Id);
                    await context.Set<Book>().AddRangeAsync(books, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);

                    // 3. Seed nhanh mỗi cuốn 1 chương để Bảo test Frontend
                    foreach (var b in books)
                    {
                        var chapter = Chapter.Create(b.Id, 1, "Lời mở đầu", "Nội dung khởi đầu của tác phẩm...");
                        await context.Set<Chapter>().AddAsync(chapter, cancellationToken);
                    }
                    await context.SaveChangesAsync(cancellationToken);
                }
            });
        }
        private static List<Book> GetSeedBooks(Guid authorId, Guid categoryId) =>
        [
            Book.Create("Muôn Kiếp Nhân Sinh - Tập 2", "Hành trình thức tỉnh tiếp theo tại Atlantis và Ai Cập.", "https://fonos.vn/images/mkns2.jpg", 168000, authorId, categoryId),
            Book.Create("Bên Rặng Tuyết Sơn", "Những bí ẩn tâm linh vùng Himalaya.", "https://fonos.vn/images/brts.jpg", 95000, authorId, categoryId),
            Book.Create("Hoa Sen Trên Tuyết", "Sự chuyển hóa tâm thức giữa đời thường.", "https://fonos.vn/images/hstt.jpg", 115000, authorId, categoryId),
            Book.Create("Hành Trình Về Phương Đông", "Ghi chép của các nhà khoa học Anh tại Ấn Độ.", "https://fonos.vn/images/htvpd.jpg", 125000, authorId, categoryId),
            Book.Create("Dấu Chân Trên Cát", "Câu chuyện về nhân quả từ thời Ai Cập cổ đại.", "https://fonos.vn/images/dctc.jpg", 145000, authorId, categoryId)
        ];
    }
}
