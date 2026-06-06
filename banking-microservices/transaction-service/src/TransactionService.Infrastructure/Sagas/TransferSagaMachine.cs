using BuildingBlocks.Messaging.IntegrationEvents;
using MassTransit;
using Microsoft.Extensions.Logging;
using TransactionService.Domain.Repositories;
using TransactionService.Infrastructure.Messaging;

namespace TransactionService.Infrastructure.Sagas;

/// <summary>
/// Orchestration Saga for fund transfers.
///
/// Flow (happy path):
///   Initial → [DebitAccount] → DebitingAccount
///   DebitingAccount → [AccountDebited] → ProcessingPayment → [ProcessPayment]
///   ProcessingPayment → [PaymentProcessed] → CreditingAccount → [CreditAccount]
///   CreditingAccount → [AccountCredited] → Completed
///
/// Compensation flow:
///   DebitingAccount → [AccountDebitFailed] → Failed
///   ProcessingPayment → [PaymentFailed] → CompensatingDebit → [ReverseDebit]
///   CompensatingDebit → [AccountDebitReversed] → Failed
///
/// Why Orchestration (not Choreography)?
///   - Single place to see the full flow and compensations
///   - Easier to add steps (e.g. fraud check) without changing other services
///   - State is persisted — survives service restarts
/// </summary>
public class TransferSagaMachine : MassTransitStateMachine<TransferSagaState>
{
    public State DebitingAccount { get; private set; } = default!;
    public State ProcessingPayment { get; private set; } = default!;
    public State CreditingAccount { get; private set; } = default!;
    public State CompensatingDebit { get; private set; } = default!;
    public State Completed { get; private set; } = default!;
    public State Failed { get; private set; } = default!;

    public Event<TransferSagaStarted> TransferStarted { get; private set; } = default!;
    public Event<AccountDebitedEvent> AccountDebited { get; private set; } = default!;
    public Event<AccountDebitFailedEvent> AccountDebitFailed { get; private set; } = default!;
    public Event<PaymentProcessedEvent> PaymentProcessed { get; private set; } = default!;
    public Event<PaymentFailedEvent> PaymentFailed { get; private set; } = default!;
    public Event<AccountCreditedEvent> AccountCredited { get; private set; } = default!;
    public Event<AccountDebitReversedEvent> AccountDebitReversed { get; private set; } = default!;

    public TransferSagaMachine()
    {
        InstanceState(x => x.CurrentState);

        // Correlate all events back to the saga instance via CorrelationId
        Event(() => TransferStarted, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => AccountDebited, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => AccountDebitFailed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentProcessed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => PaymentFailed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => AccountCredited, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));
        Event(() => AccountDebitReversed, x => x.CorrelateById(ctx => ctx.Message.CorrelationId));

        Initially(
            When(TransferStarted)
                .Then(ctx =>
                {
                    ctx.Saga.TransactionId = ctx.Message.TransactionId;
                    ctx.Saga.FromAccountId = ctx.Message.FromAccountId;
                    ctx.Saga.ToAccountId = ctx.Message.ToAccountId;
                    ctx.Saga.Amount = ctx.Message.Amount;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(ctx => ctx.Init<DebitAccountCommand>(new DebitAccountCommand(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.TransactionId,
                    ctx.Saga.FromAccountId,
                    ctx.Saga.Amount)))
                .TransitionTo(DebitingAccount));

        During(DebitingAccount,
            When(AccountDebited)
                .Then(ctx => ctx.Saga.UpdatedAt = DateTime.UtcNow)
                .PublishAsync(ctx => ctx.Init<ProcessPaymentCommand>(new ProcessPaymentCommand(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.TransactionId,
                    ctx.Saga.FromAccountId,
                    ctx.Saga.ToAccountId,
                    ctx.Saga.Amount)))
                .TransitionTo(ProcessingPayment),

            When(AccountDebitFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .TransitionTo(Failed)
                .Finalize());

        During(ProcessingPayment,
            When(PaymentProcessed)
                .Then(ctx =>
                {
                    ctx.Saga.PaymentId = ctx.Message.PaymentId;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(ctx => ctx.Init<CreditAccountCommand>(new CreditAccountCommand(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.TransactionId,
                    ctx.Saga.ToAccountId,
                    ctx.Saga.Amount)))
                .TransitionTo(CreditingAccount),

            When(PaymentFailed)
                .Then(ctx =>
                {
                    ctx.Saga.FailureReason = ctx.Message.Reason;
                    ctx.Saga.UpdatedAt = DateTime.UtcNow;
                })
                .PublishAsync(ctx => ctx.Init<ReverseDebitCommand>(new ReverseDebitCommand(
                    ctx.Saga.CorrelationId,
                    ctx.Saga.TransactionId,
                    ctx.Saga.FromAccountId,
                    ctx.Saga.Amount)))
                .TransitionTo(CompensatingDebit));

        During(CreditingAccount,
            When(AccountCredited)
                .Then(ctx => ctx.Saga.UpdatedAt = DateTime.UtcNow)
                .TransitionTo(Completed)
                .Finalize());

        During(CompensatingDebit,
            When(AccountDebitReversed)
                .Then(ctx => ctx.Saga.UpdatedAt = DateTime.UtcNow)
                .TransitionTo(Failed)
                .Finalize());

        SetCompletedWhenFinalized();
    }
}
