using BankingApp.Models;
using BankingApp.Repositories;

namespace BankingApp.Services;

public class TransferService(AccountRepository accountRepo, TransactionRepository txRepo)
{
    public async Task TransferAsync(string fromNumber, string toNumber, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        var sender = await accountRepo.GetByNumberAsync(fromNumber)
            ?? throw new KeyNotFoundException($"Account {fromNumber} not found.");

        var receiver = await accountRepo.GetByNumberAsync(toNumber)
            ?? throw new KeyNotFoundException($"Account {toNumber} not found.");

        if (sender.Status != AccountStatus.Active)
            throw new InvalidOperationException("Sender account is not active.");

        if (receiver.Status != AccountStatus.Active)
            throw new InvalidOperationException("Receiver account is not active.");

        if (sender.Balance < amount)
            throw new InvalidOperationException("Insufficient funds.");

        // --- NOTE: this is the monolith problem ---
        // All of this happens in one DB transaction.
        // In microservices, Account and Transaction are separate services —
        // you CANNOT wrap them in a single DbContext.SaveChanges() anymore.
        sender.Balance -= amount;
        receiver.Balance += amount;

        var debit = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = sender.Id,
            RelatedAccountId = receiver.Id,
            Amount = amount,
            Type = TransactionType.Debit,
            Description = $"Transfer to {toNumber}"
        };

        var credit = new Transaction
        {
            Id = Guid.NewGuid(),
            AccountId = receiver.Id,
            RelatedAccountId = sender.Id,
            Amount = amount,
            Type = TransactionType.Credit,
            Description = $"Transfer from {fromNumber}"
        };

        await txRepo.AddAsync(debit);
        await txRepo.AddAsync(credit);
        await txRepo.SaveAsync();
    }
}
