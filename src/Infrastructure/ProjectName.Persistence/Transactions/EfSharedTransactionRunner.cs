using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.BuildingBlocks.Core.Results;

namespace ProjectName.Persistence.Transactions;

internal sealed class EfSharedTransactionRunner
{
    public async Task<Result<TResponse>> ExecuteAsync<TResponse>(
        DbContext primaryContext,
        IReadOnlyCollection<DbContext> joinedContexts,
        Func<CancellationToken, Task<Result<TResponse>>> operation,
        CancellationToken ct = default)
    {
        var connection = primaryContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(ct);
        }

        await using var transaction = await primaryContext.Database.BeginTransactionAsync(ct);

        foreach (var joinedContext in joinedContexts)
        {
            await joinedContext.Database.UseTransactionAsync(transaction.GetDbTransaction(), ct);
        }

        try
        {
            var result = await operation(ct);

            if (result.IsFailure)
            {
                await transaction.RollbackAsync(ct);
                return result;
            }

            await transaction.CommitAsync(ct);

            return result;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
