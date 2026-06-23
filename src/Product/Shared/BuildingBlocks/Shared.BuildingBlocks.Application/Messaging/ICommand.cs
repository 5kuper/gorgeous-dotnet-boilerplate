using MediatR;
using Shared.BuildingBlocks.Core.Results;

namespace Shared.BuildingBlocks.Application.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
