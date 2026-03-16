using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeEngine.Domain.Entities;

namespace TradeEngine.Infrastructure.Persistence
{
    public class TradeEngineDbContext : DbContext
    {
        public TradeEngineDbContext(DbContextOptions<TradeEngineDbContext> options)
            : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<LedgerEntry> LedgerEntries { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Account>()
                .Property(a => a.FrozenBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Account>()
                .HasIndex(a => a.UserName)
                .IsUnique();

            modelBuilder.Entity<Account>()
                .Property(a => a.PasswordHash)
                .IsRequired();

            modelBuilder.Entity<Account>()
                .Property(a => a.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Order>()
                .Property(o => o.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.StopPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.IdempotencyKey)
                .IsUnique()
                .HasFilter("[IdempotencyKey] IS NOT NULL");

            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.AccountId, o.Status });

            modelBuilder.Entity<Order>()
                .HasIndex(o => new { o.AssetSymbol, o.Side, o.Status, o.Price });

            modelBuilder.Entity<Trade>()
                .Property(t => t.ExecutionPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Trade>()
                .HasIndex(t => t.AccountId);

            modelBuilder.Entity<Trade>()
                .HasIndex(t => t.OrderId);

            modelBuilder.Entity<Position>()
                .Property(p => p.AverageCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Position>()
                .Property(p => p.RowVersion)
                .IsRowVersion();

            modelBuilder.Entity<Position>()
                .HasIndex(p => new { p.AccountId, p.AssetSymbol })
                .IsUnique();

            modelBuilder.Entity<LedgerEntry>()
                .Property(l => l.Debit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LedgerEntry>()
                .Property(l => l.Credit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LedgerEntry>()
                .HasIndex(l => l.AccountId);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(r => r.AccountId);
        }
    }
}
