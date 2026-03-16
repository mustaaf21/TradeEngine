using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TradeEngine.Application.Interfaces;
using TradeEngine.Domain.Entities;
using TradeEngine.Domain.Exceptions;
using TradeEngine.Infrastructure.Persistence;


namespace TradeEngine.Infrastructure.Service
{
    public class AccountService : IAccountService
    {
        private readonly TradeEngineDbContext _context;
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100000;

        public AccountService(TradeEngineDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAccountAsync(string ownerName, string userName, string password, string email)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentException("Username cannot be empty.");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Password cannot be empty.");
            }

            var exists = await _context.Accounts.AnyAsync(a => a.UserName == userName);

            if (exists)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            var account = new Account
            {
                Id = Guid.NewGuid(),
                OwnerName = ownerName,
                UserName = userName,
                PasswordHash = HashPassword(password),
                Email = email,
                Balance = 0,
                FrozenBalance = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return account.Id;
        }

        public async Task DepositAsync(Guid accountId, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be greater than 0.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            var account = await _context.Accounts.SingleOrDefaultAsync(a => a.Id == accountId);

            if (account == null)
            {
                throw new AccountNotFoundException();
            }

            account.AddBalance(amount);

            var ledgerEntry = new LedgerEntry
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Debit = 0,
                Credit = amount,
                Description = "Deposit",
                CreatedAt = DateTime.UtcNow
            };

            _context.LedgerEntries.Add(ledgerEntry);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }

        public async Task<Account?> GetAccountAsync(Guid accountId)
        {
            return await _context.Accounts.SingleOrDefaultAsync(a => a.Id == accountId);
        }

        public async Task<Account?> GetAccountByUsernameAsync(string userName)
        {
            return await _context.Accounts.SingleOrDefaultAsync(a => a.UserName == userName);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var parts = passwordHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var computedHash = pbkdf2.GetBytes(HashSize);

            return CryptographicOperations.FixedTimeEquals(hash, computedHash);
        }

        private static string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(HashSize);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }
    }
}
