using BankingApp.Models;
using BankingApp.Repositories;

namespace BankingApp.Services;

public class AccountService(AccountRepository accountRepo, CustomerRepository customerRepo)
{
    public async Task<Account> OpenAccountAsync(Guid customerId, AccountType type, decimal initialDeposit = 0)
    {
        var customer = await customerRepo.GetByIdAsync(customerId)
            ?? throw new KeyNotFoundException($"Customer {customerId} not found.");

        var account = new Account
        {
            Id = Guid.NewGuid(),
            AccountNumber = GenerateAccountNumber(),
            Balance = initialDeposit,
            Type = type,
            CustomerId = customerId
        };

        await accountRepo.AddAsync(account);
        await accountRepo.SaveAsync();

        return account;
    }

    public async Task<Account> GetAccountAsync(Guid id)
        => await accountRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Account {id} not found.");

    public async Task<List<Account>> GetCustomerAccountsAsync(Guid customerId)
        => await accountRepo.GetByCustomerAsync(customerId);

    private static string GenerateAccountNumber()
        => $"ACC{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
}
