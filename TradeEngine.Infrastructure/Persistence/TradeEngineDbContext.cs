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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Account>()
                .Property(a => a.FrozenBalance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Trade>()
                .Property(t => t.ExecutionPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LedgerEntry>()
                .Property(l => l.Debit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LedgerEntry>()
                .Property(l => l.Credit)
                .HasPrecision(18, 2);
        }
    }
}
