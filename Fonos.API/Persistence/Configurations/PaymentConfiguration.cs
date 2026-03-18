using Fonos.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fonos.API.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Status)
           .HasConversion<string>()
           .HasMaxLength(50);

            builder.Property(p => p.TransactionId).IsRequired();

            builder.Property(p => p.PaymentMethod).IsRequired();

            builder.Property(p => p.Amount).HasPrecision(18, 2);

            builder.HasOne(p => p.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
