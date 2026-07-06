using MediatR;

namespace Shared.IntegrationTesting.Messaging;

public sealed class TestSender : ISender
{
    private readonly object? response;
    private readonly bool hasResponse;

    private TestSender(object? response, bool hasResponse)
    {
        this.response = response;
        this.hasResponse = hasResponse;
    }

    public static TestSender Throwing() => new(response: null, hasResponse: false);

    public static TestSender Returning(object response) => new(response, hasResponse: true);

    public Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult((TResponse)GetResponse());
    }

    public Task Send<TRequest>(
        TRequest request,
        CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        throw new NotSupportedException("This sender is only registered for request/response endpoint tests.");
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<object?>(GetResponse());
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This sender is only registered for request/response endpoint tests.");
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("This sender is only registered for request/response endpoint tests.");
    }

    private object GetResponse()
    {
        if (!hasResponse)
        {
            throw new NotSupportedException("This sender is only registered for endpoint tests.");
        }

        return response!;
    }
}
