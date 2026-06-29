using Gorgeous.Abstractions.Results;

namespace Users.Contracts.Verification;

public interface IUsersEmailVerification
{
    Task<Result> ConfirmEmailAsync(Guid userPublicId, CancellationToken ct = default);
}

