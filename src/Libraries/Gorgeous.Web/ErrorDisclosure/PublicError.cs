using Gorgeous.Abstractions.Results;

namespace Gorgeous.Web.ErrorDisclosure;

public sealed record PublicError(string Code, string Message, ErrorType Type);
