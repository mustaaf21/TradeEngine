using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TradeEngine.Application.Interfaces;
using TradeEngine.Domain.Entities;
using TradeEngine.Infrastructure.Persistence;

namespace TradeEngine.Infrastructure.Service
{
    public class AuthService : IAuthService
    {
        private readonly TradeEngineDbContext _context;
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;

        public AuthService(TradeEngineDbContext context, IAccountService accountService, IConfiguration configuration)
        {
            _context = context;
            _accountService = accountService;
            _configuration = configuration;
        }

        public async Task<AuthResult> LoginAsync(string userName, string password)
        {
            var account = await _accountService.GetAccountByUsernameAsync(userName);

            if (account == null)
            {
                return new AuthResult { Success = false, Error = "Invalid username or password." };
            }

            if (!account.IsActive)
            {
                return new AuthResult { Success = false, Error = "Account is deactivated." };
            }

            if (!_accountService.VerifyPassword(password, account.PasswordHash))
            {
                return new AuthResult { Success = false, Error = "Invalid username or password." };
            }

            var accessToken = GenerateAccessToken(account);
            var refreshToken = await GenerateRefreshTokenAsync(account.Id);

            return new AuthResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
                AccountId = account.Id
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (token == null || !token.IsActive)
            {
                return new AuthResult { Success = false, Error = "Invalid or expired refresh token." };
            }

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.Id == token.AccountId);

            if (account == null || !account.IsActive)
            {
                return new AuthResult { Success = false, Error = "Account not found or deactivated." };
            }

            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            var newAccessToken = GenerateAccessToken(account);
            var newRefreshToken = await GenerateRefreshTokenAsync(account.Id);

            await _context.SaveChangesAsync();

            return new AuthResult
            {
                Success = true,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
                AccountId = account.Id
            };
        }

        public async Task RevokeTokenAsync(Guid accountId)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.AccountId == accountId && !t.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private string GenerateAccessToken(Account account)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "TradeEngineDefaultSecretKey123456789012";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "TradeEngine";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "TradeEngineUsers";

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, account.UserName),
                new Claim(JwtRegisteredClaimNames.Email, account.Email),
                new Claim("accountId", account.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpirationMinutes()),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(Guid accountId)
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Token = Convert.ToBase64String(randomBytes),
                ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenExpirationDays()),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        private int GetAccessTokenExpirationMinutes()
        {
            var value = _configuration["Jwt:AccessTokenExpirationMinutes"];
            return int.TryParse(value, out var minutes) ? minutes : 15;
        }

        private int GetRefreshTokenExpirationDays()
        {
            var value = _configuration["Jwt:RefreshTokenExpirationDays"];
            return int.TryParse(value, out var days) ? days : 7;
        }
    }
}
