using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TradeEngine.Application.Interfaces;
using TradeEngine.Application.DTOs;

namespace TradeEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            var accountId = await _accountService.CreateAccountAsync(
                request.OwnerName, 
                request.UserName, 
                request.Password, 
                request.Email);
            return Ok(new { accountId, message = "Account created successfully." });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            var account = await _accountService.GetAccountAsync(id);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            return Ok(new
            {
                id = account.Id,
                ownerName = account.OwnerName,
                userName = account.UserName,
                email = account.Email,
                balance = account.Balance,
                frozenBalance = account.FrozenBalance,
                availableBalance = account.AvailableBalance,
                isActive = account.IsActive,
                createdAt = account.CreatedAt
            });
        }

        [Authorize]
        [HttpPost("{id}/deposit")]
        public async Task<IActionResult> Deposit(Guid id, [FromBody] DepositRequest request)
        {
            await _accountService.DepositAsync(id, request.Amount);
            return Ok(new { message = "Deposit successful.", amount = request.Amount });
        }
    }

    public class DepositRequest
    {
        public decimal Amount { get; set; }
    }
}
