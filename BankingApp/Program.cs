using BankingApp.Data;
using BankingApp.Models;
using BankingApp.Repositories;
using BankingApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BankDbContext>(opt =>
    opt.UseInMemoryDatabase("BankDB"));

builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<TransactionRepository>();

builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<TransferService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed two customers with accounts so you can test transfers immediately
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();

    var alice = new Customer
    {
        Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
        FullName = "Alice",
        Email = "alice@bank.com",
        Phone = "111"
    };
    var bob = new Customer
    {
        Id = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001"),
        FullName = "Bob",
        Email = "bob@bank.com",
        Phone = "222"
    };

    db.Customers.AddRange(alice, bob);

    var aliceAcc = new Account
    {
        Id = Guid.NewGuid(),
        AccountNumber = "ACC-ALICE",
        Balance = 1000,
        Type = AccountType.Checking,
        CustomerId = alice.Id
    };
    var bobAcc = new Account
    {
        Id = Guid.NewGuid(),
        AccountNumber = "ACC-BOB",
        Balance = 500,
        Type = AccountType.Checking,
        CustomerId = bob.Id
    };

    db.Accounts.AddRange(aliceAcc, bobAcc);
    db.SaveChanges();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
