namespace IdentityService.Domain.Events;

public record CustomerRegisteredEvent(Guid CustomerId, string Email, string FullName) : DomainEvent;
public record CustomerUpdatedEvent(Guid CustomerId) : DomainEvent;
public record CustomerKycVerifiedEvent(Guid CustomerId) : DomainEvent;
public record CustomerKycRejectedEvent(Guid CustomerId, string Reason) : DomainEvent;
