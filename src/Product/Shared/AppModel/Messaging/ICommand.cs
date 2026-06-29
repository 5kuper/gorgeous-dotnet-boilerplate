using MediatR;
using Gorgeous.Abstractions.Results;

namespace Shared.AppModel.Messaging;

public interface ICommand : IRequest<Result>;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>;
