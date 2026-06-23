using MediatR;
using Shared.BuildingBlocks.Core.Results;

namespace Shared.BuildingBlocks.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
