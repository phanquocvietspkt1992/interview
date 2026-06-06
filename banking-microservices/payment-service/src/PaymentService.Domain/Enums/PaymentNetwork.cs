namespace PaymentService.Domain.Enums;

public enum PaymentNetwork
{
    Swift,
    Ach,
    Sepa,
    Domestic,
    Internal   // for intra-bank transfers (saga-driven)
}
