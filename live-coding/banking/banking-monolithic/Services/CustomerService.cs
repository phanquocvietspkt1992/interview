using banking_monolithic.Models;
using banking_monolithic.Repositories;

namespace banking_monolithic.Services
{
    public class CustomerService(CustomerRepository customerRepository)
    {
        public async Task<Customer> RegistryAsync(string name, string email, string phone)
        {
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                FullName = name,
                Email = email,
                PhoneNumber = phone
            };
            await customerRepository.AddAsync(customer);
            await customerRepository.SaveAsync();
            return customer;
        }

        public async Task<Customer> GetCustomerAsync(Guid id) 
            => await customerRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Customer {id} not found.");

        public async Task<List<Customer>> GetAllCustomersAsync() 
            => await customerRepository.GetAllAsync();    

    }
}
