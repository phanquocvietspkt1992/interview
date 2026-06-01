using BankingApp.Models;
using BankingApp.Repositories;

namespace BankingApp.Services;

public class CustomerService(CustomerRepository customerRepo)
{
    public async Task<Customer> RegisterAsync(string fullName, string email, string phone)
    {
        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            Phone = phone
        };

        await customerRepo.AddAsync(customer);
        await customerRepo.SaveAsync();

        return customer;
    }

    public async Task<Customer> GetCustomerAsync(Guid id)
        => await customerRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Customer {id} not found.");

    public async Task<List<Customer>> GetAllAsync()
        => await customerRepo.GetAllAsync();
}
