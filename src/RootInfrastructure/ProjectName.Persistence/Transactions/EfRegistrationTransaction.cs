using Auth.Application.Ports.Registration;
using Auth.Infrastructure.Persistence;
using Gorgeous.Abstractions.Results;
using Users.Infrastructure.Persistence;

namespace ProjectName.Persistence.Transactions;

internal sealed class EfRegistrationTransaction(
    UsersDbContext usersDbContext,
    AuthDbContext authDbContext,
    EfSharedTransactionRunner transactionRunner) : IRegistrationTransaction
{
    public Task<Result<TResponse>> ExecuteAsync<TResponse>(
        Func<CancellationToken, Task<Result<TResponse>>> operation,
        CancellationToken ct = default)
        => transactionRunner.ExecuteAsync(usersDbContext, [authDbContext], operation, ct);
}
