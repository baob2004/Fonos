using Fonos.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fonos.API.Persistence.Configurations
{
    public class UserBookConfiguration : IEntityTypeConfiguration<UserBook>
    {
        public void Configure(EntityTypeBuilder<UserBook> builder)
        {
            builder.ToTable("UserBooks");

            // Composite primary key
            builder.HasKey(ub => new { ub.BookId, ub.UserId });

            builder.HasOne(ub => ub.User)
               .WithMany(u => u.PurchasedBooks)
               .HasForeignKey(ub => ub.UserId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ub => ub.Book)
               .WithMany(b => b.UserBooks)
               .HasForeignKey(ub => ub.BookId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
