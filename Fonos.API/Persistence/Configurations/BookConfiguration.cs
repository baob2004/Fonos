using Fonos.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fonos.API.Persistence.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.ToTable("Books");

            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(b => b.Description)
                   .IsRequired()
                   .HasMaxLength(3000);

            builder.Property(b => b.CoverImageUrl).IsRequired();
            
            builder.Property(b => b.Price).HasPrecision(18, 2);
        
            builder.HasOne(b => b.Author)
                    .WithMany(b => b.Books)
                    .HasForeignKey(b => b.AuthorId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Category)
                    .WithMany(b => b.Books)
                    .HasForeignKey(b => b.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(b => b.Title);
            builder.HasIndex(b => b.Price);
            builder.HasIndex(b => b.AuthorId);
            builder.HasIndex(b => b.CategoryId);
        }
    }
}
