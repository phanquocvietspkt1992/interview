using IdentityService.Domain.Enums;
using IdentityService.Domain.Events;
using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.Entities;

public class Customer
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Phone { get; private set; } = default!;
    public string? NationalId { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<DomainEvent> _domainEvents = [];
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();

    public string FullName => $"{FirstName} {LastName}";

    private Customer() { }

    public static Customer Register(string firstName, string lastName, string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new DomainException("First name is required");
        if (string.IsNullOrWhiteSpace(lastName)) throw new DomainException("Last name is required");
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email is required");
        if (!email.Contains('@')) throw new DomainException("Invalid email format");
        if (string.IsNullOrWhiteSpace(phone)) throw new DomainException("Phone is required");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.ToLowerInvariant().Trim(),
            Phone = phone.Trim(),
            KycStatus = KycStatus.Pending,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        customer.Raise(new CustomerRegisteredEvent(customer.Id, customer.Email, customer.FullName));
        return customer;
    }

    public void Update(string firstName, string lastName, string phone)
    {
        if (!IsActive) throw new DomainException("Cannot update an inactive customer");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Phone = phone.Trim();
        UpdatedAt = DateTime.UtcNow;

        Raise(new CustomerUpdatedEvent(Id));
    }

    public void VerifyKyc(string nationalId)
    {
        if (KycStatus == KycStatus.Verified) throw new DomainException("Customer is already KYC verified");
        if (string.IsNullOrWhiteSpace(nationalId)) throw new DomainException("National ID is required for KYC");

        NationalId = nationalId.Trim();
        KycStatus = KycStatus.Verified;
        UpdatedAt = DateTime.UtcNow;

        Raise(new CustomerKycVerifiedEvent(Id));
    }

    public void RejectKyc(string reason)
    {
        if (KycStatus == KycStatus.Verified) throw new DomainException("Cannot reject an already verified customer");

        KycStatus = KycStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;

        Raise(new CustomerKycRejectedEvent(Id, reason));
    }

    public void Deactivate()
    {
        if (!IsActive) throw new DomainException("Customer is already inactive");
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private void Raise(DomainEvent e) => _domainEvents.Add(e);
}
