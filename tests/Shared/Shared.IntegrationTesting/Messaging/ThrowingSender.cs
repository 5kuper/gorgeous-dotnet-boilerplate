using MediatR;

namespace Shared.IntegrationTesting.Messaging;

public sealed class ThrowingSender : ISender
{
    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This sender is only registered for endpoint metadata tests.");
    }

    public Task Send<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        throw new NotSupportedException("This sender is only registered for endpoint metadata tests.");
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This sender is only registered for endpoint metadata tests.");
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This sender is only registered for endpoint metadata tests.");
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This sender is only registered for endpoint metadata tests.");
    }
}
