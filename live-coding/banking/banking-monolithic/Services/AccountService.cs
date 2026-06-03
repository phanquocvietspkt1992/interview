using banking_monolithic.Models;
using banking_monolithic.Repositories;

namespace banking_monolithic.Services
{
    public class AccountService(AccountRepository accountRepository)
    {
        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await accountRepository.GetAllAsync();
        }

        public async Task<Account> OpenAccount(Guid customerId, AccountType type, decimal initialDeposit = 0)
        {
            var customer = accountRepository.GetByIdAsync(customerId) ?? throw new KeyNotFoundException($"Customer {customerId} not found.");

            var account = new Account
            {
                Id = Guid.NewGuid(),
                CustomerId = customerId,
                Type = type,
                Balance = initialDeposit,
                AccountNumber = GenerateAccountNumber(),
            };

            accountRepository.AddAsync(account);
            accountRepository.SaveAsync();
            return account;
        }

        public async Task<Account> GetAccountAsync(Guid id)
       => await accountRepository.GetByIdAsync(id)
           ?? throw new KeyNotFoundException($"Account {id} not found.");

        public async Task<Account> GetCustomerAccountsAsync(Guid customerId)
            => await accountRepository.GetByIdAsync(customerId);

        private static string GenerateAccountNumber()
            => $"ACC{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
    }
}
