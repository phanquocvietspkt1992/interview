using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TransactionService.Infrastructure.Sagas;

public class TransferSagaStateMap : SagaClassMap<TransferSagaState>
{
    protected override void Configure(EntityTypeBuilder<TransferSagaState> entity, ModelBuilder model)
    {
        entity.ToTable("TransferSagaStates");
        entity.HasKey(s => s.CorrelationId);

        entity.Property(s => s.CurrentState).HasMaxLength(64).IsRequired();
        entity.Property(s => s.FailureReason).HasMaxLength(500);
        entity.Property(s => s.Amount).HasPrecision(18, 4);

        entity.HasIndex(s => s.TransactionId);
    }
}
