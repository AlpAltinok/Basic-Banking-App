using BankaApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankaApp.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IdempotencyKey)
            .HasMaxLength(100);

        builder.HasIndex(x => x.IdempotencyKey);


        builder.HasOne(x => x.FromWallet)
            .WithMany(x => x.OutgoingTransactions)
            .HasForeignKey(x => x.FromWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ToWallet)
            .WithMany(x => x.IncomingTransactions)
            .HasForeignKey(x => x.ToWalletId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
