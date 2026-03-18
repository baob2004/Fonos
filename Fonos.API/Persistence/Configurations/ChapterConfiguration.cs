using Fonos.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fonos.API.Persistence.Configurations
{
    public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
    {
        public void Configure(EntityTypeBuilder<Chapter> builder)
        {
            builder.ToTable("Chapters");

            builder.HasKey(a => a.Id);

            builder.Property(c => c.OrderNumber).IsRequired();

            builder.Property(c => c.Title).IsRequired().HasMaxLength(100);

            builder.Property(c => c.DurationInSeconds).IsRequired();

            builder.HasOne(c => c.Book)
                .WithMany(b=>b.Chapters)
                .HasForeignKey(c=>c.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasMaxLength(50);
        }
    }
}
