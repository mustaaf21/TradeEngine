using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeEngine.Domain.Entities;

namespace TradeEngine.Application.Interfaces
{
    public interface IAccountService
    {
        Task<Guid> CreateAccountAsync(string ownerName, string username, string password, string email);
        Task DepositAsync(Guid accountId, decimal amount);
        Task<Account?> GetAccountAsync(Guid accountId);
        Task<Account?> GetAccountByUsernameAsync(string userName);
        bool VerifyPassword(string password, string passwordHash);
    }
}
