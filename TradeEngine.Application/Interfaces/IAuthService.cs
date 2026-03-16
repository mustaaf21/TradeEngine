using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeEngine.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string userName, string password);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
        Task RevokeTokenAsync(Guid accountId);
    }

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public Guid? AccountId { get; set; }
        public string? Error { get; set; }
    }
}
