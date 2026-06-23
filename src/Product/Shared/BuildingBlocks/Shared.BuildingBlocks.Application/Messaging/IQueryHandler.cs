using MediatR;
using Shared.BuildingBlocks.Core.Results;

namespace Shared.BuildingBlocks.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
