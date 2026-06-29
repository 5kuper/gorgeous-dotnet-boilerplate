using MediatR;
using Gorgeous.Abstractions.Results;

namespace Shared.AppModel.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
