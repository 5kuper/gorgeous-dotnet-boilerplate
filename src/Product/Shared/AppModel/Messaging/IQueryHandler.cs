using MediatR;
using Gorgeous.Abstractions.Results;

namespace Shared.AppModel.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
